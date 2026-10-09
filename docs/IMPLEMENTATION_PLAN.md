# IMPLEMENTATION PLAN — Tạo View 3D làm việc trên Project Browser (Working 3D View)

> Kế hoạch triển khai dạng checklist. NGUỒN CHÂN LÝ về tiến độ.
> AI dựa vào `- [ ]` / `- [x]` để biết đang ở đâu (khôi phục ngữ cảnh sau /compact).
> Cứ 3–5 task: cập nhật trạng thái + ghi "Nhật ký tiến độ".

## Nguyên tắc
- Làm tuần tự, mỗi task nhỏ & kiểm chứng được.
- Sinh code build được ngay (kèm using/namespace, phiên bản Revit 2019, dependencies).
- Build/compile sau mỗi thay đổi; luôn try-catch/log; sửa TRỰC TIẾP file trong thư mục `source/`.
- DỪNG ở **[CHECKPOINT]** để xin duyệt.

## Giai đoạn 1 — Môi trường & Khung cấu trúc
- [x] Tạo workspace dự án riêng biệt tại `Projects/BIM-Tool-WorkingView`
- [x] Sao chép bộ Starter đã cá nhân hóa vào thư mục `source/` làm điểm xuất phát
- [x] Khởi tạo hồ sơ `docs/IDEAS.md`, `docs/TOOL_BRIEF.md` và `docs/IMPLEMENTATION_PLAN.md`
- [x] **[CHECKPOINT]** Xác nhận phạm vi, đầu vào, đầu ra và phiên bản đích Revit 2019 với học viên trước khi code

## Giai đoạn 2 — Tính năng lõi (Revit API)
- [x] Tạo class Command `CreateWorkingViewCommand.cs` trong `source/Commands/` implement `IExternalCommand`
- [x] Viết logic kiểm tra View3D đã tồn tại theo tên `3D_Working_HaiAnh` để chống tạo trùng
- [x] Viết logic tìm `ViewFamilyType` (ThreeDimensional) và tạo View3D bằng `View3D.CreateIsometric`
- [x] Thiết lập thuộc tính hiển thị: `DetailLevel.Fine`, `DisplayStyle.ShadingWithEdges`, gán tham số phân loại
- [x] Tích hợp mở tự động View vừa tạo qua `UIDocument.ActiveView`
- [x] Đóng gói Transaction an toàn với try-catch và hoàn tác (Undo) sạch sẽ

## Giai đoạn 3 — Giao diện Ribbon & Kiểm thử
- [x] Đăng ký nút `Working View` vào Ribbon Tab `BIM TOOL` trong `source/App.cs` (giữ nguyên nút About Me & Hello Revit)
- [x] Đăng ký lệnh đại diện `CreateWorkingViewProxyCommand` và nút `Working View` trong `DSCons.Revit.HotReloadHost` theo skill hot-reload
- [x] Gán icon 3D Isometric chuẩn vector (`view3d`)
- [x] Build dự án cho Revit 2019 (cả Host và Product DLL) đạt 0 Error, 0 Warning
- [x] Chạy unit tests phần lõi đạt 5/5 bài kiểm tra (100% Passed)
- [x] **[CHECKPOINT]** Cài đặt manifest phát triển trỏ tới Hot Reload Host của dự án
- [x] Học viên tự mở Revit 2019 kiểm thử thực tế trên model mẫu (chạy xong lần 1)
- [x] Kiểm chứng thành công tính năng Hot Reload (cập nhật thông báo mà không tắt Revit)
- [x] Lưu hồ sơ báo cáo tổng kết Ngày 2 (`docs/reports/day-02-report.md`)

## Nhật ký tiến độ
- [2026-10-06] Đã xong: Hoàn thành toàn bộ quy trình Ngày 2: Lập Tool Brief, code công cụ Working 3D View, build & test đạt 100%, cài đặt manifest add-in riêng cho dự án, học viên đã tự chạy trên Revit 2019 và kiểm chứng thành công Hot Reload, đã lưu báo cáo day-02-report.md | Đang làm: Sẵn sàng cho Ngày 3 | Vướng: Không có
