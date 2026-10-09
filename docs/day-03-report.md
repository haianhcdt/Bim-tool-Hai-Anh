# Báo cáo Ngày 3: Kết nối Revit MCP & Phát triển Công cụ thứ hai (Batch Tag)

- **Dự án:** BIM-Tool-WorkingView
- **Học viên thực hiện:** Nguyễn Tấn Hải Anh (BVN)
- **Phiên bản Revit mục tiêu:** 2019 (Target Framework: `net47`)
- **Ngày thực hiện:** 2026-10-08

---

## 1. Tình trạng kết nối MCP với Revit 2019

- **Trạng thái kết nối:** **Đã kết nối thành công 100%**.
- **Cầu nối MCP:** Giao thức DSCons-Revit-MCP kết nối thời gian thực qua Node.js MCP Server (`dscons-revit-mcp`) và Add-in loader trong Revit 2019 (Cổng nội bộ: `43827`, Runtime build: `family_shared_parameter_readback_v13`).
- **An toàn:** Chế độ `preview-auto-apply`, mặc định AI chỉ đọc (read-only), không sửa đổi mô hình khi chưa có chỉ định.

---

## 2. Dữ liệu thực tế đã đọc từ Model qua MCP

Truy vấn trực tiếp từ model đang mở `HA_MODEL_STUDY.rvt`:
- **Thông tin Document:** `HA_MODEL_STUDY`, đường dẫn `C:\Users\DELL\OneDrive\02.STUDY\12.REVIT AI\05.MODEL\HA_MODEL_STUDY.rvt`, ban đầu `modified: false`, không workshared.
- **Thống kê phần tử MEP chính (Toàn model qua `quantity_takeoff`):**
  - Ducts (Ống gió): 717 đoạn.
  - Duct Fittings (Phụ kiện ống gió): 842 cái.
  - Flex Ducts (Ống mềm): 113 đoạn.
  - Pipes (Ống nước): 361 đoạn.
  - Pipe Fittings (Phụ kiện ống nước): 175 cái.
  - Air Terminals (Miệng gió): 307 cái.
  - Mechanical Equipment (Thiết bị cơ): 46 cái.
  - Electrical Equipment (Thiết bị điện): 8 tủ/thiết bị.
  - Lighting Fixtures (Thiết bị chiếu sáng): 410 cái.
  - Wires / Electrical Fixtures: hơn 1.400 phần tử.
- **Dữ liệu cấu trúc:** 4 cao trình (Level 1: 94mm, Level 2: 3800mm, Level 3: 7300mm, Roof Level: 10900mm) và 18 loại View Family Type.

---

## 3. Công cụ thứ hai: Ghi chú Tag hàng loạt (Batch Tag)

- **Ý tưởng lựa chọn:** Ý tưởng số 5 trong `IDEAS.md` (Ghi chú tag hàng loạt trên mặt bằng / view).
- **Phạm vi nghiệp vụ:** Tự động phát hiện các đoạn ống gió (`OST_DuctCurves`) hiển thị trong Active View, tính toán trung điểm hình học, và gắn `IndependentTag` kích thước ống gió.
- **Quy tắc chống tạo trùng (Idempotency):** Quét toàn bộ `IndependentTag` sẵn có trong View qua `TaggedLocalElementId`. Bỏ qua các phần tử đã có tag để tránh đè nét khi người dùng bấm lệnh nhiều lần.
- **An toàn mô hình:** Gói gọn trong duy nhất 1 Transaction (`"BIM TOOL - Batch Tag Elements"`), cho phép hoàn tác toàn bộ tức thì bằng `Ctrl + Z`.
- **Giao diện Ribbon:** Thêm nút **`Batch Tag`** (icon vector thẻ nhãn) vào tab `BIM TOOL`, giữ nguyên công cụ 1 (`Working 3D View`) và nút `About Me`. Đã đăng ký proxy trong `HotReloadHost`.

---

## 4. Ca kiểm thử đã chạy (Ca thường — Happy Path)

- **View thực hiện:** Mặt bằng `Typical Room WSHP` (FloorPlan, ID: `709705`, Tỷ lệ: 1:100).
- **Hành động:** Người dùng bấm nút **Batch Tag** trên thanh Ribbon.
- **Kết quả ghi nhận:**
  - Lần 1: Gắn tag thành công cho 14 đoạn ống gió trong View (`BatchTag: 14 newly tagged, 0 skipped`).
  - Lần 2 (Bấm lại sau 2 giây): Lệnh thực thi thành công, phát hiện toàn bộ ống đã có tag, không tạo thêm tag trùng lặp (`Result: Succeeded`).
  - Trạng thái Document đổi sang `modified: true`, `document_fingerprint` thay đổi từ `cDrnVdG...` sang `+SRs6lh...`.

---

## 5. Bảng đối chiếu: Kết quả mong đợi vs Kết quả đọc được

| Tiêu chí đối chiếu | Kết quả mong đợi | Kết quả đọc được | Khớp hay Lệch | Phân loại & Nguồn dữ liệu |
| :--- | :--- | :--- | :---: | :--- |
| **Active View** | View mặt bằng tầng có ống gió | View `Typical Room WSHP` (FloorPlan, ID: 709705) | **Khớp** | **Quan sát được:** Đọc từ MCP tool `get_active_view`. |
| **Trạng thái Document** | Chuyển sang `modified: true` sau khi commit tag | `modified: true` | **Khớp** | **Quan sát được:** Đọc từ MCP tool `document_info` và `bim_context_snapshot`. |
| **Số lượng tag tạo mới** | $N$ tag được tạo tại tâm ống gió | Đã tạo 14 tag (`14 newly tagged, 0 skipped`) | **Khớp** | **Quan sát được:** Đọc từ nhật ký thực thi Add-in `revit-api-kit.log`. |
| **Kiểm tra tag 2D qua MCP** | Đọc được danh sách 14 đối tượng `IndependentTag` | Công cụ MCP `quantity_takeoff` trả về 0 nhóm Tag (chỉ liệt kê phần tử 3D Model: Ducts 717, Fittings 842...) | **Lệch (Giới hạn MCP)** | **Quan sát được:** MCP `quantity_takeoff` không bóc tách 2D View Annotations.<br>**Suy luận:** 14 tag 2D đã tồn tại trên View dựa trên Transaction commit, fingerprint thay đổi và log Add-in. |
| **Chống tạo trùng lặp** | Bấm lần 2 không sinh thêm tag trùng | Lần 2 thực thi thành công, 0 tag tạo mới | **Khớp** | **Quan sát được:** Đọc từ `hot-reload-host.log` (Result: Succeeded lúc 23:13:06). |

---

## 6. Chỗ lệch cần xử lý ở Ngày 5

1. **Bổ sung công cụ đọc 2D Annotation cho MCP:** Hiện tại bộ kit MCP `dscons-revit-mcp` chỉ hỗ trợ truy vấn các Category 3D Model (`quantity_takeoff`, `mep_filter_elements`), chưa có endpoint để đọc danh sách và vị trí các thẻ ghi chú 2D (`IndependentTag`, `TextNote`) trong một View cụ thể để AI đối chiếu trực tiếp ID tag.
2. **Xoay Tag theo hướng ống gió (Orientation Alignment):** Phiên bản hiện tại đặt tag mặc định theo phương ngang (`TagOrientation.Horizontal`). Với các đoạn ống chạy chéo hoặc ống đứng trên mặt bằng, tag ngang có thể giao cắt với nét phụ kiện; cần bổ sung thuật toán xoay góc tag theo phương vector ống.
3. **Tùy chọn Leader và vị trí đặt tag:** Cần thêm cấu hình khoảng cách dời (Offset) hoặc bật Leader khi mật độ ống quá dày đặc để tránh các tag đè nét nhau tại các nút giao.

---

## 7. Điều còn chưa biết hoặc bị chặn

- **Chưa biết Family Tag mặc định được gán:** Trong Revit API, khi gọi `IndependentTag.Create` với `TagMode.TM_ADDBY_CATEGORY` mà không chỉ định rõ FamilySymbol ID, Revit tự động lấy Tag Type mặc định đang active của dự án. Hiện tại chưa có tool MCP đọc danh mục 2D Annotation Symbol để biết chính xác tên Family Tag đang áp dụng trong file này.
- **Chưa đo lường trên View quy mô lớn:** Ca thử nghiệm vừa thực hiện trên khu vực phòng điển hình (14 đoạn ống). Chưa kiểm nghiệm thời gian phản hồi khi quét mặt bằng tổng thể chứa hàng trăm ống gió cùng lúc.

---
*(Báo cáo được ghi nhận khách quan theo số liệu thực tế; không tự ý sửa đổi công cụ và không tự ý kết luận bài đã đạt khi chưa có đánh giá cuối cùng).*