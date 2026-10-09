# Quy tắc và Ngữ cảnh AI cho Dự án BIM-Tool-WorkingView

## 1. Nguyên tắc cốt lõi bắt buộc
- **ĐỌC TRƯỚC KHI LÀM:** Mỗi khi mở phiên chat mới hoặc chuyển máy tính, AI PHẢI ĐỌC ngay các file trong `.agents/`:
  1. `PRD.md` (Yêu cầu sản phẩm & thông tin dự án)
  2. `IMPLEMENTATION_PLAN.md` (Kế hoạch tổng thể)
  3. `IMPLEMENTATION_STATUS.md` (Trạng thái build/test/runtime thực tế)
  4. `TASKS.md` (Danh sách công việc & tiến độ)
  5. `chat-history.md` (Lịch sử các quyết định, lỗi đã fix và nơi dừng lại)
- **TỰ ĐỘNG CẬP NHẬT:** Trong suốt phiên làm việc, sau mỗi quyết định quan trọng, hoàn thành task hoặc sửa lỗi, AI PHẢI cập nhật ngược lại vào thư mục `.agents/` để phiên chat tiếp theo luôn nắm bắt được 100% tiến độ mà người dùng không cần giải thích lại.
- **AN TOÀN MÔ HÌNH:** Không tự ý ghi model khi chưa có bước Preview và xác nhận của học viên. Mọi thao tác ghi phải gom trong 1 Transaction để hoàn tác sạch bằng `Ctrl + Z`.
- **KIẾN TRÚC DỰ ÁN:** Tuân thủ Hot Reload (`DSCons.Revit.HotReloadHost.dll` là vỏ bọc, `DSCons.Revit.Starter.dll` là thư viện sản phẩm nạp động qua byte array). Sửa command/logic không cần khởi động lại Revit; chỉ khi thêm nút mới vào Ribbon mới cần đóng Revit để build lại Host.
- **CHUẨN UI BỘ KIT:** Giao diện xem trước phải là WPF Window chuẩn MVVM (kế thừa `Theme.xaml`, màu branding `#1466B8`). Dùng `DynamicResource` cho các tài nguyên theme. TaskDialog chỉ dùng cho cảnh báo/xác nhận cực ngắn.