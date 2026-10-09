# TOOL BRIEF - Công cụ 2: Ghi chú Tag hàng loạt trên View (Batch Tag MEP Elements)

## Bối cảnh Revit

- Người dùng thực tế: Kỹ sư Cơ Điện / BIM Manager (Nguyễn Tấn Hải Anh - BVN)
- Phiên bản Revit: 2019 (.NET Framework 4.7)
- Bộ môn MEP: Cơ Điện (Trọng tâm MVP: Ducts - Ống gió, mở rộng: Pipes, Equipment)
- Model/file test: `HA_MODEL_STUDY.rvt`
- Phạm vi: Active View (Mặt bằng Floor Plan hoặc Mặt cắt Section đang mở)
- Loại Model: Standalone và Workshared

## UI và branding

- Màu chủ đạo đã xác nhận: `#1466B8` (Xanh dương đậm)
- Tên tab Ribbon: `BIM TOOL`
- Panel: `Revit API Kit` (hoặc `Annotation Tools`)
- Tên nút Ribbon: `Batch Tag`
- Pictogram: Biểu tượng thẻ ghi chú / Tag nhãn MEP (32x32 và 16x16)
- Giao diện: Direct Command với TaskDialog tóm tắt kết quả (hoặc WPF mini-dialog tùy chọn tham số leader)

## Nghiệp vụ

- Vấn đề đang làm tay: Kỹ sư phải bấm từng đoạn ống gió để gắn tag kích thước, rất mất thời gian trên các hệ thống lớn (hơn 700 đoạn ống gió), dễ bị sót hoặc bị gắn đè nhiều tag lên cùng 1 ống.
- Đầu vào: 
  - Kích hoạt nút `Batch Tag` trên Ribbon khi đang mở một View.
  - Category đối tượng cần tag: Mặc định MVP là `Ducts` (Ống gió).
  - Vị trí đặt tag: Trung điểm đoạn ống (`LocationCurve.Evaluate(0.5, true)`).
  - Leader: Mặc định `false` (đặt tag nằm ngay tâm ống).
- Quy tắc xử lý:
  1. Lấy danh sách toàn bộ phần tử thuộc Category mục tiêu đang hiển thị trong `ActiveView` (`FilteredElementCollector(doc, activeView.Id)`).
  2. Thu thập danh sách `IndependentTag` hiện có trong `ActiveView` để trích xuất `TaggedLocalElementId`.
  3. Lấy FamilySymbol của Tag tương ứng (ví dụ: `Duct Tag`). Nếu chưa có Family Tag trong model -> Rollback và báo người dùng nạp Family Tag.
  4. Lọc bỏ các phần tử **đã có tag** trong View (đảm bảo tính lũy thừa / Idempotency).
  5. Mở Transaction: `"BIM TOOL - Batch Tag Elements"`.
  6. Với mỗi phần tử chưa có tag:
     - Lấy đường tâm `LocationCurve`, tính trung điểm `XYZ midPoint = curve.Evaluate(0.5, true)`.
     - Tạo `IndependentTag.Create(doc, activeView.Id, new Reference(element), false, TagMode.TM_AD_NAME_TAG, TagOrientation.Horizontal, midPoint)`.
  7. Hoàn tất `Transaction.Commit()`.
  8. Hiển thị thông báo `TaskDialog`: Tổng số đối tượng tìm thấy, số tag đã tạo mới, số đối tượng đã bỏ qua vì có sẵn tag.

## Quy tắc chạy lại không tạo trùng (Idempotency)

- Kiểm tra sự tồn tại của tag gắn vào phần tử trong View trước khi tạo.
- Bấm lệnh nhiều lần trên cùng 1 View chỉ xử lý các phần tử mới bổ sung, tuyệt đối không tạo tag trùng đè nét.

## An toàn và hoàn tác

- Toàn bộ thao tác ghi gói gọn trong 1 Transaction duy nhất -> Hoàn tác tức thì bằng `Ctrl + Z` để gỡ sạch toàn bộ tag vừa tạo.
- Guard Clause: Không thực thi nếu Active View không hỗ trợ Annotation (ví dụ 3D view chưa khóa hướng hoặc View Schedule/Sheet).

## Tiêu chuẩn nghiệm thu (Test Cases)

- Ca thử đạt 1 (Tạo mới): Mở View có các đoạn ống gió chưa tag -> Bấm nút -> Toàn bộ ống được gắn tag chuẩn tại tâm ống.
- Ca thử đạt 2 (Chống trùng): Bấm nút lần thứ 2 -> Không sinh thêm tag nào, thông báo báo 0 tag mới.
- Ca thử hoàn tác: Bấm Ctrl + Z -> Tất cả tag vừa tạo bị xóa sạch, model nguyên vẹn.
- Ca thử ngoại lệ: Dự án chưa load Family Tag -> Báo thông báo rõ ràng, không văng Revit.