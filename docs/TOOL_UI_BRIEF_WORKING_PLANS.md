# Tool UI Brief — Quản lý & Tạo Mặt Bằng Làm Việc Cá Nhân (Working Plans)

## 1. Việc hoàn tất

Người dùng sẽ **tự động tạo một hoặc nhiều View Mặt bằng làm việc cá nhân (FL_Working_HaiAnh_{LevelName}) độc lập với hồ sơ dự án** bằng cách chọn các Tầng (Level) trên cửa sổ giao diện WPF và bấm nút **Tạo Mặt Bằng Làm Việc**.

## 2. Luồng ngắn nhất

1. Người dùng bấm nút **Working Views** trên Ribbon tab BIM TOOL.
2. Cửa sổ WPF **Quản Lý View Làm Việc Cá Nhân** hiển thị:
   - Danh sách tất cả các Tầng (Level) trong dự án kèm cao độ và trạng thái (Đã tạo / Chưa tạo).
   - Mặc định tích chọn Tầng của View hiện hành (hoặc người dùng bấm 'Chọn tất cả' / tích chọn các tầng mong muốn).
3. Người dùng bấm nút **⚡ Tạo Mặt Bằng Làm Việc (N tầng)**.
4. Add-in tạo các ViewPlan tương ứng trong 1 Transaction duy nhất (BIM TOOL - Tạo Mặt Bằng Làm Việc):
   - Đặt tên theo chuẩn cá nhân: FL_Working_HaiAnh_{LevelName}.
   - Cấu hình hiển thị chuẩn MEP: Detail Level = Fine, Display Style = ShadingWithEdges.
   - Nếu chọn 1 tầng, tự động kích hoạt chuyển sang View đó trên màn hình.
   - Chống tạo trùng lặp (Idempotent): Nếu view đã có, báo rõ trạng thái mà không gây lỗi.
5. Người dùng cũng có thể nhanh chóng bấm nút **🌐 Tạo / Mở View 3D Cá Nhân** ngay trên cùng giao diện.

## 3. Quyết định giao diện

- **Bề mặt chọn:** WPF Window (Modal dialog) chuẩn MVVM.
- **Mã màu thương hiệu:** #1466B8 (Nguyễn Tôn Hải Anh - BVN).
- **Theme:** Kế thừa Theme.xaml với bo góc control 5px, card 7px, typography Segoe UI.
- **Vì sao dùng WPF Window:** Người dùng cần xem danh sách các Level trong dự án, kiểm tra tầng nào đã có mặt bằng làm việc và linh hoạt chọn tạo 1 hoặc nhiều tầng cùng lúc.
- **Phần chọn trực tiếp trong Revit:** Tự động nhận diện Level từ Active View làm gợi ý ban đầu.

## 4. Trạng thái và hành động

| Trạng thái | Người dùng nhìn thấy | Primary action |
|---|---|---|
| Chưa mở mô hình | Cảnh báo: "Không có mô hình Revit nào đang mở" | Đóng thông báo |
| Khởi tạo | Đọc toàn bộ Levels trong mô hình, kiểm tra view đã tồn tại | Sẵn sàng thao tác |
| Chưa chọn tầng nào | Nút bấm mờ (Disabled), tooltip "Vui lòng chọn ít nhất 1 tầng" | Tích chọn checkbox tầng |
| Đã chọn >= 1 tầng | Nút bấm sáng: "⚡ Tạo Mặt Bằng Làm Việc (N tầng)" | **Thực hiện tạo view** |
| Đang tạo | Thanh trạng thái cập nhật tiến độ | Đang xử lý |
| Thành công | Thông báo số lượng view đã tạo, cập nhật trạng thái trong bảng | Mở view / Đóng |

## 5. Wireframe chữ

`	ext
+--------------------------------------------------------------------------+
| [Icon] BIM TOOL | QUẢN LÝ VIEW LÀM VIỆC CÁ NHÂN (WORKING VIEWS)           |
+--------------------------------------------------------------------------+
| [Thẻ 1: VIEW 3D LÀM VIỆC]                                                |
| View 3D cá nhân: 3D_Working_HaiAnh [Đã có sẵn trong mô hình]             |
|                                        [🌐 Kích hoạt / Mở 3D View]       |
+--------------------------------------------------------------------------+
| [Thẻ 2: MẶT BẰNG LÀM VIỆC (WORKING FLOOR PLANS)]                         |
| Chọn tầng cần tạo mặt bằng làm việc cá nhân:                             |
| [X] Chọn tất cả    [ ] Bỏ chọn                     Quy tắc: FL_Working_HaiAnh_{Level} |
| +----------------------------------------------------------------------+ |
| | [ ] | Tên Tầng (Level) | Cao Độ (Elevation) | Trạng Thái           | |
| |-----|------------------|--------------------|----------------------| |
| | [x] | Level 1          | 0.000 m            | Chưa có              | |
| | [x] | Level 2          | 3.900 m            | Chưa có              | |
| | [ ] | Roof             | 7.800 m            | Đã có (Sẵn sàng mở)  | |
| +----------------------------------------------------------------------+ |
| Tùy chọn hiển thị: Chi tiết: [Fine v]  |  Kiểu: [Shaded with Edges v]    |
|                                                                          |
|                                 [⚡ Tạo Mặt Bằng Làm Việc (2 tầng)]      |
+--------------------------------------------------------------------------+
| Sẵn sàng tạo mặt bằng làm việc cá nhân...                     [ Đóng ]   |
+--------------------------------------------------------------------------+
`

## 6. Boundary kỹ thuật

- **Modal Window:** Đảm bảo toàn vẹn dữ liệu trong suốt quá trình người dùng chọn tầng và tạo view.
- **Revit Read/Write:** Chạy trong IExternalCommand chuẩn với Transaction an toàn.
- **Unit Testing:** Kiểm thử bộ sinh tên WorkingViewNaming và thuật toán lọc Level trong DSCons.Revit.Starter.Core.Tests.