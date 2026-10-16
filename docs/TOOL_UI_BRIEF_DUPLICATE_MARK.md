# Tool UI Brief: Bảng Thống Kê & Kiểm Tra Trùng Thông Số Mark

## 1. Việc hoàn tất
Kỹ sư kiểm tra toàn bộ mô hình xem có cấu kiện nào đang bị trùng thông số `Mark` hay không, xem danh sách trực quan dạng bảng với đầy đủ Category, Family & Type, Level, ElementId, có thể chọn đối tượng trực tiếp trên Revit và tự động sửa trùng lặp chỉ với 1 cú click.

## 2. Luồng ngắn nhất
1. Kỹ sư mở công cụ `Dọn Dẹp File` trên Ribbon `BIM TOOL` và bấm nút **`[🔍 Kiểm Tra Trùng Mark]`** (hoặc chạy từ menu Audit).
2. Cửa sổ WPF hiển thị ngay lập tức bảng thống kê phân loại theo từng mã Mark bị trùng, tổng số đối tượng và tình trạng an toàn.
3. Kỹ sư xem bảng, click chọn dòng để Revit tự động highlight cấu kiện trong không gian 3D/Mặt bằng, hoặc bấm **`[🔧 Tự Động Sửa Trùng]`** / **`[📄 Xuất Báo Cáo]`**.

## 3. Quyết định giao diện
- Bề mặt chọn: **WPF table/workbench** (`DuplicateMarkAuditWindow`).
- Lý do: Bắt buộc dùng WPF Window theo chuẩn Kit khi hiển thị bảng dữ liệu, không dùng TaskDialog vì cần hiển thị DataGrid đa cột, tương tác highlight cấu kiện trong Revit và xuất báo cáo.
- Màu chủ đạo: `#1466B8` (Theo branding kỹ sư Nguyễn Tôn Hải Anh · BVN).
- Bố cục: Gồm thanh Header thương hiệu, cụm KPI cards (Tổng quét, Có Mark, Số nhóm trùng, Tổng đối tượng trùng), bảng DataGrid tương tác và thanh công cụ tác vụ phía dưới.

## 4. Trạng thái và hành động

| Trạng thái | Người dùng nhìn thấy | Primary action |
|---|---|---|
| Khởi tạo | Đang quét các đối tượng có tham số Mark... | Disable nút tác vụ |
| Không có lỗi trùng | Badge xanh "🟢 MÔ HÌNH SẠCH - 0 ĐỐI TƯỢNG TRÙNG MARK", bảng trống | Xuất báo cáo xác nhận sạch |
| Phát hiện trùng Mark | Badge đỏ "🔴 PHÁT HIỆN {N} ĐỐI TƯỢNG TRÙNG MARK", bảng hiển thị chi tiết | `Tự Động Sửa Trùng` / `Chọn trong Revit` |
| Đang sửa | Thanh tiến trình và thông báo xử lý | Disable các nút thao tác |
| Hoàn tất sửa | Badge chuyển sang xanh, bảng cập nhật 0 đối tượng trùng | Đóng hoặc xem lại |

## 5. Wireframe chữ

```text
+-----------------------------------------------------------------------------------+
| BIM TOOL · BẢNG THỐNG KÊ & KIỂM TRA TRÙNG THÔNG SỐ MARK      (BVN - Nguyễn Tôn Hải Anh)|
| Mô hình: HH2-CETEC-BAS-MEP-ZZ-v24.rvt                                             |
+-----------------------------------------------------------------------------------+
| [ Tổng quét: 15.420 ]  [ Có Mark: 3.250 ]  [ Nhóm trùng: 0 ]  [ Đối tượng trùng: 0 ]|
+-----------------------------------------------------------------------------------+
| 🟢 TRẠNG THÁI: MÔ HÌNH HOÀN TOÀN SẠCH - KHÔNG CÓ ĐỐI TƯỢNG NÀO TRÙNG MÃ MARK       |
+-----------------------------------------------------------------------------------+
| STT | Mã Mark | Số lượng | Danh mục (Category) | Tên Family & Type | Tầng | Element IDs |
|-----+---------+----------+---------------------+-------------------+------+-------------|
| (Danh sách các đối tượng nếu có hoặc thông báo trống)                             |
+-----------------------------------------------------------------------------------+
| [👁️ Xem trên Revit]  [🔧 Tự Động Sửa Trùng]  [📄 Xuất Báo Cáo CSV]  [🔄 Quét Lại] [Đóng]|
+-----------------------------------------------------------------------------------+
```

## 6. Boundary kỹ thuật
- Modal WPF Window có `Owner = revitMainWindowHandle`.
- Revit read/write đi qua: `IExternalCommand` (sử dụng Transaction an toàn khi sửa, ReadOnly khi chỉ kiểm toán).
- Hỗ trợ cả Revit 2019 đến Revit 2024+.
