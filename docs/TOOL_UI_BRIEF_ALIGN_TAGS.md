# Tool UI Brief — Căn chỉnh thẳng hàng Thẻ ghi chú MEP (Align Tags)

## 1. Việc hoàn tất

Người dùng sẽ **căn các thẻ ghi chú (Tag) đã chọn thẳng hàng nhau theo phương ngang hoặc phương dọc** sau khi xem trước bảng tọa độ và bấm nút **Căn chỉnh**.

## 2. Luồng ngắn nhất

1. Người dùng chọn từ 2 thẻ ghi chú (Tag) trở lên trên mặt bằng/mặt cắt trong Revit.
2. Bấm nút **Align Tags** trên Ribbon tab `BIM TOOL`.
3. Hộp thoại WPF hiện lên hiển thị bảng xem trước (Preview) tọa độ hiện tại và tọa độ sau khi gióng hàng:
   - Chọn phương gióng: Ngang (cùng Y) hoặc Dọc (cùng X).
   - Chọn mốc chuẩn: Theo Tag đầu tiên chọn hoặc Lấy trung bình.
4. Bấm **Căn chỉnh thẳng hàng** -> Cập nhật vị trí các Tag trong 1 Transaction duy nhất. Hoàn tác nhanh bằng `Ctrl + Z`.

## 3. Quyết định giao diện

- Bề mặt chọn: **WPF form compact có bảng xem trước (Preview)**.
- Giao diện kế thừa `Theme.xaml` và mã màu thương hiệu `#1466B8` (Nguyễn Tôn Hải Anh).
- Lý do chọn WPF Window modal: Người dùng cần kiểm tra sự chênh lệch tọa độ trước khi quyết định di chuyển hàng loạt, tránh làm nhảy nét tag sang vùng bản vẽ khác.
- Phần chọn trực tiếp trong Revit: Người dùng quét chọn các đối tượng `IndependentTag` trước khi gọi lệnh.

## 4. Trạng thái và hành động

| Trạng thái | Người dùng nhìn thấy | Primary action |
|---|---|---|
| Chưa chọn đủ | Cảnh báo ngắn: "Vui lòng chọn từ 2 tag trở lên" | Đóng thông báo, quay lại Revit chọn |
| Hợp lệ (Sẵn sàng) | Danh sách Tag, tọa độ hiện tại, tọa độ mới và độ dịch chuyển $\Delta$ | **Căn chỉnh (N tag)** |
| Đang tính toán | Bảng tự động cập nhật ngay khi đổi RadioButton | **Căn chỉnh (N tag)** |
| Thành công | Tag đã thẳng hàng trên mặt bằng, thông báo xác nhận | Đóng cửa sổ |

## 5. Wireframe chữ

```text
+--------------------------------------------------------------+
| [Icon] BIM TOOL | CĂN CHỈNH THẲNG HÀNG NHÃN MEP (TAGS)       |
+--------------------------------------------------------------+
| Hướng căn chỉnh:                                             |
|   (*) Căn theo phương ngang (cùng Y)   ( ) Căn theo phương dọc (cùng X) |
| Mốc tham chiếu:                                              |
|   (*) Theo Tag đầu tiên                ( ) Lấy vị trí trung bình    |
+--------------------------------------------------------------+
| Xem trước vị trí (Preview):                                  |
| [ ID Tag  | Loại Tag | Vị trí hiện tại | Vị trí dự kiến | Độ lệch Δ ] |
| [ 120451  | Duct Tag | X: 12.5, Y: 8.3 | X: 12.5, Y: 8.0| Δ: 0.3m   ] |
| [ 120452  | Duct Tag | X: 16.2, Y: 7.9 | X: 16.2, Y: 8.0| Δ: 0.1m   ] |
+--------------------------------------------------------------+
|                              [Căn chỉnh thẳng hàng]   [Hủy/Đóng] |
+--------------------------------------------------------------+
```

## 6. Boundary kỹ thuật

- **Kiểu cửa sổ:** Modal (`ShowDialog()`), là con của Revit MainWindow qua `WindowInteropHelper`.
- **Revit read/write:** 
  - Đọc selection và tọa độ hiện tại trong `IExternalCommand.Execute`.
  - Ghi model trong duy nhất 1 Transaction (`"BIM TOOL - Align Tags"`) sau khi người dùng bấm nút chính.
- **An toàn hoàn tác:** Hỗ trợ `Ctrl + Z` một bước phục hồi toàn bộ vị trí cũ.