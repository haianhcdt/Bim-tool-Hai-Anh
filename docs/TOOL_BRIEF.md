# TOOL BRIEF — Tạo View 3D làm việc trên Project Browser (Working 3D View)

## Bối cảnh Revit

- Người dùng thực tế: Kỹ sư Cơ Điện / BIM Manager (Nguyễn Tôn Hải Anh - BVN)
- Phiên bản Revit: 2019
- Bộ môn MEP: Cơ Điện tổng hợp (HVAC, Plumbing, Electrical, Fire Protection)
- Model/file test: File Revit MEP mẫu (me_basic_sample_project.rvt hoặc file bản sao)
- Phạm vi: Active Document (Toàn bộ tài liệu đang mở)
- Workshared model: Hỗ trợ cả Local/Standalone và Central Model (chỉ tạo View cục bộ)

## UI và branding (khi tool có Ribbon hoặc WPF)

- Màu chủ đạo đã được học viên xác nhận: #1466B8 (Xanh dương đậm)
- Tên tab Ribbon đã được học viên xác nhận: BIM TOOL
- Panel: Revit API Kit (hoặc View Tools)
- Tên nút Ribbon: Working View
- Pictogram theo nghĩa chức năng tool: Biểu tượng khối hộp không gian 3D / Isometric View (32x32 và 16x16 vector)
- Giao diện: Direct Command (Lệnh thực thi trực tiếp, thông báo kết quả nhanh qua TaskDialog)

## Nghiệp vụ

- Vấn đề đang làm tay: Mỗi khi vào dự án hoặc chuyển phân khu, kỹ sư phải tự nhấn Default 3D View ({3D}), đổi tên thành view riêng để không đè lên view của người khác, chỉnh Detail Level sang Fine, chỉnh Visual Style sang Shaded, rồi gán tham số phân loại để View nằm đúng nhóm  Working Views trong cây thư mục Project Browser.
- Đầu vào: Nhấn nút Working View trên thanh Ribbon.
- Quy tắc xử lý:
  1. Quét kiểm tra toàn bộ danh sách View3D trong mô hình (bỏ qua View Template).
  2. Kiểm tra tên View dự kiến: 3D_Working_HaiAnh.
  3. Nếu ĐÃ TỒN TẠI: Kích hoạt view đó (uiDoc.ActiveView = existingView), hiện thông báo ngắn gọn.
  4. Nếu CHƯA TỒN TẠI:
     - Lấy ViewFamilyType của loại 3D (ViewFamily.ThreeDimensional).
     - Khởi tạo Transaction với tên Tạo 3D View làm việc.
     - Tạo view mới bằng View3D.CreateIsometric(doc, viewFamilyTypeId).
     - Đổi tên thành 3D_Working_HaiAnh.
     - Cấu hình hiển thị: DetailLevel = ViewDetailLevel.Fine, DisplayStyle = DisplayStyle.Shading.
     - Gán giá trị nhận diện vào tham số Comments hoặc tham số phân loại Browser nếu có giá trị là Working View.
     - Hoàn tất Transaction.Commit().
     - Chuyển góc nhìn của người dùng sang View vừa tạo (uiDoc.ActiveView = newView).
- Kết quả đầu ra: View 3D_Working_HaiAnh xuất hiện trên Project Browser và được mở sẵn trên màn hình.
- Quy tắc chạy lại không tạo trùng: Kiểm tra theo tên chính xác trước khi tạo; nếu đã có thì chỉ mở lên (Activate), không sinh thêm Copy 1, Copy 2.
- Family/Type/Parameter/Connector bắt buộc: ViewFamilyType (loại ThreeDimensional), Built-in Parameter VIEW_NAME.
- Giới hạn MVP và phần chưa làm:
  - MVP (Ngày 2): Tạo đúng 1 View 3D Isometric chuẩn cá nhân, tự cấu hình hiển thị và mở ngay.
  - Phần chưa làm (Để dành): Cửa sổ WPF chọn Level để tạo hàng loạt mặt bằng làm việc (Working Floor Plans).

## An toàn và kiểm thử

- Có preview trước khi ghi model: Không cần thiết (thao tác tạo View mới độc lập).
- Có Transaction/Rollback: Có (using (Transaction t = new Transaction(doc, Create Working 3D View))).
- Điều kiện phải dừng trước khi ghi model: Document ở chế độ Read-Only hoặc không tìm thấy kiểu ViewFamilyType 3D.
- Ca test đạt:
  - Lần 1: Mô hình chưa có 3D_Working_HaiAnh -> Bấm nút -> View được tạo chuẩn, mở trên màn hình.
  - Lần 2: Mô hình đã có 3D_Working_HaiAnh -> Bấm nút -> Không tạo thêm, tự động kích hoạt chuyển đến View này.
- Ca test không hợp lệ: Dự án bị khóa quyền sửa đổi view hoặc file template bị cấm đổi tên.
- Tiêu chí nghiệm thu định lượng: Thời gian thực thi < 0.5s; không gây lỗi cảnh báo trùng lặp của Revit; hoàn tác (Ctrl+Z) xóa sạch view vừa tạo mà không ảnh hưởng mô hình.
- Phạm vi dữ liệu được phép sửa: Chỉ tạo thêm 1 đối tượng View3D mới, tuyệt đối không chỉnh sửa đối tượng hình học MEP/Kiến trúc/Kết cấu.
- Không chạy trên model dự án thật trước khi đạt test mẫu.
