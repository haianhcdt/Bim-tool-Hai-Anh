# Tool UI Brief — About Me của Revit API Programming Kit

## 1. Việc hoàn tất

Người dùng sẽ xem nhanh danh tính và thông tin liên hệ của chủ sở hữu bộ công
cụ sau khi bấm **About Me** trên Ribbon.

## 2. Luồng ngắn nhất

1. Bấm About Me.
2. Đọc hồ sơ, chuyên môn và liên hệ.
3. Bấm Đóng hoặc Esc để trở lại Revit.

## 3. Quyết định giao diện

- Bề mặt chọn: **WPF Window modal chỉ đọc**.
- Mẫu chính: Single Action Form theo bố cục hồ sơ hai cột; không có mẫu phụ.
- Không dùng TaskDialog vì thông tin liên hệ và giới thiệu dài không thể đọc tốt
  trong một thông báo ngắn.
- Không có phần chọn trong Revit, không có input và không ghi model.

## 4. Trạng thái và hành động

| Trạng thái | Người dùng nhìn thấy | Primary action |
|---|---|---|
| Mở hợp lệ | Branding, hồ sơ, liên hệ | Đóng |
| Thiếu email tùy chọn | Hiện “Chưa cập nhật” | Đóng |
| Đóng | Quay lại Revit, không đổi model | Esc / Đóng |

## 5. Wireframe chữ

```text
[Header: thương hiệu | họ tên, vai trò | initials]
[Hồ sơ chuyên môn + thẻ công ty + nhãn] [Liên hệ theo từng dòng]
[Footer thương hiệu]                                      [Đóng]
```

## 6. Boundary kỹ thuật

- Modal; là con của cửa sổ Revit.
- Revit read/write: không có. ViewModel chỉ đọc StudentBranding.
- Test: build Revit 2023/net48 và 2025/net8; kiểm tra chuỗi dài, Esc và DPI
  100/125/150% trong Revit trên model copy.
