# TEST PLAN — Batch Tag MEP Elements (Công cụ 2)

## Môi trường kiểm thử

- **Phiên bản Revit:** Autodesk Revit 2019
- **Profile / TFM active:** `net47` (.NET Framework 4.7)
- **Model mẫu:** `HA_MODEL_STUDY.rvt`
- **Đường dẫn model:** `C:\Users\DELL\OneDrive\02.STUDY\12.REVIT AI\05.MODEL\HA_MODEL_STUDY.rvt`
- **Dữ liệu đối tượng:** 717 đoạn ống gió (`OST_DuctCurves`), các hệ thống cấp/hồi/hút (`Mechanical Supply Air`, `Exhaust`, `Return`).
- **Family / Preset đầu vào:** `Duct Tag` chuẩn hệ thống MEP của Revit.

---

## Cấp 1 — Build & Kiểm thử đơn vị (Core Unit Tests)

- [x] Build đúng phiên bản Revit 2019 (`net47`) không có cảnh báo hay lỗi.
- [x] Core unit test đạt 11/11 tests (Kiểm thử bộ lọc `BatchTagFilter`, tọa độ `BatchTagCandidate`, và định dạng `BatchTagResult`).
- [x] Không ghi đè manifest ngoài thư mục dự án trong bước build.

---

## Cấp 2 — Revit Integration Test (3 Ca kiểm tra chính)

### 1. Ca thường (Happy Path — Gắn tag hàng loạt trên mặt bằng)
- **Mục đích:** Gắn tag tự động cho toàn bộ các đoạn ống gió chưa có tag trên mặt bằng cơ điện.
- **Dữ liệu vào:**
  - View: Mặt bằng cơ điện Tầng 1 (`Floor Plan: 1 - Mech` hoặc `Level 1`).
  - Đối tượng: Các đoạn ống gió (`Ducts`) hiển thị trong View chưa có `IndependentTag`.
- **Kết quả mong đợi:**
  1. Tạo đúng $N$ thẻ `IndependentTag` mới tại tâm các đoạn ống gió.
  2. Hộp thoại `TaskDialog` báo: *"Đã gắn tag thành công cho N đối tượng ống gió!"*.
  3. Bấm **`Ctrl + Z`**: Thu hồi sạch toàn bộ $N$ tag vừa tạo trong 1 bước duy nhất.
- **Cách kiểm tra bằng MCP (Chỉ đọc):**
  - Gọi `document_info` và `get_active_view` trước khi chạy để ghi nhận `title`, `view_id`, `modified = false`.
  - Gọi `mep_filter_elements(categories: ["Ducts"], level_name: "Level 1")` để đối chiếu số lượng ống gió trên tầng.
  - Sau khi chạy: Gọi `document_info` để xác nhận `modified = true` (Transaction đã commit thành công).

---

### 2. Ca khó (Edge Case — Chống tạo trùng lặp khi chạy lại / Idempotency)
- **Mục đích:** Đảm bảo khi bấm lệnh nhiều lần hoặc trong View đã có sẵn tag thì không bao giờ tạo tag đè nét lên nhau.
- **Dữ liệu vào:**
  - View: Cùng mặt bằng ở Ca 1 (đã chạy qua Ca 1) hoặc mặt bằng hỗn hợp có $M$ ống đã có tag và $K$ ống mới vẽ thêm.
  - Đối tượng: Hỗn hợp ống đã có tag và chưa có tag.
- **Kết quả mong đợi:**
  1. Quét nhận diện chính xác $M$ ống đã có tag trong View (`TaggedLocalElementId`).
  2. Bỏ qua hoàn toàn $M$ ống cũ, chỉ tạo thêm đúng $K$ tag mới cho các ống chưa có.
  3. Nếu $K = 0$: Báo *"Tất cả M đối tượng ống gió trong View đều đã có tag từ trước. Không tạo thêm tag để tránh đè nét."* (Tạo thêm: 0 tag trùng lặp).
- **Cách kiểm tra bằng MCP (Chỉ đọc):**
  - Gọi `bim_context_snapshot` để lưu context snapshot của View.
  - Sau khi chạy lần 2: Gọi lại `quantity_takeoff` hoặc kiểm tra trạng thái document để xác nhận không phát sinh phần tử rác hay tag đè nét ngoài dự kiến.
  - Server bridge phản hồi bình thường, không có lỗi runtime.

---

### 3. Ca sai / Ngoại lệ (Negative Case — Guard Clause)
- **Mục đích:** Kiểm tra cơ chế tự bảo vệ khi người dùng chạy tool ở View không hợp lệ (View 3D chưa khóa hướng nhìn hoặc View không hỗ trợ annotation).
- **Dữ liệu vào:**
  - View: View 3D mặc định `{3D}` đang ở trạng thái chưa khóa hướng nhìn (`IsLocked = false`).
  - Đối tượng: Toàn bộ ống gió nhìn thấy trong không gian 3D.
- **Kết quả mong đợi:**
  1. Tool phát hiện vi phạm Guard Clause, **chặn ngay lập tức** trước khi mở Transaction.
  2. Hiện `TaskDialog` nhắc nhở: *"View 3D chưa được khóa hướng nhìn (Orientation Locked). Vui lòng khóa View 3D hoặc chuyển sang Mặt bằng/Mặt cắt để gắn tag."*
  3. Không làm crash ứng dụng, số tag tạo mới: 0.
- **Cách kiểm tra bằng MCP (Chỉ đọc):**
  - Trước và sau khi bấm nút: Gọi `document_info` xác nhận trạng thái `modified` không bị thay đổi bất thường.
  - Gọi `system_status` để xác nhận kết nối Revit - AI vẫn thông suốt (`document_open: true`, không bị văng tiến trình Revit).

---

## Cấp 3 — Nghiệm thu dự án (Project Acceptance)

- [ ] Thực hiện nghiệm thu thực tế trên model `HA_MODEL_STUDY.rvt`.
- [ ] Xác nhận giao diện hiển thị đúng icon thẻ nhãn vector trên tab Ribbon `BIM TOOL`.
- [ ] Kỹ sư Cơ điện xác nhận kết quả gắn tag ngay ngắn tại tâm ống và quy tắc chống trùng hoạt động chuẩn xác.