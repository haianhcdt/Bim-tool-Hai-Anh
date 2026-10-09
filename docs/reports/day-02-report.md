# Báo cáo Ngày 2: Tạo công cụ Working 3D View & Kiểm chứng Hot Reload

- **Dự án:** BIM-Tool-WorkingView
- **Học viên thực hiện:** Nguyễn Tôn Hải Anh (BVN)
- **Phiên bản Revit mục tiêu:** 2019 (Target Framework: `net47`)
- **Ngày thực hiện:** 2026-10-06

---

## 1. Công cụ làm gì
- **Tên công cụ:** Tạo View 3D làm việc trên Project Browser (Working 3D View).
- **Vấn đề giải quyết:** Giúp kỹ sư tự động tạo góc nhìn 3D cá nhân để thao tác mà không làm xáo trộn các View in ấn hồ sơ chính thức của dự án.
- **Nghiệp vụ thực hiện:**
  - Quét danh sách `View3D` trong mô hình để kiểm tra sự tồn tại của View `3D_Working_HaiAnh`.
  - **Quy tắc chống trùng lặp (Idempotent):** Nếu View đã tồn tại, công cụ kích hoạt mở View đó (`UIDocument.ActiveView = existingView`) và thông báo, tuyệt đối không tạo thêm bản sao `Copy 1`, `Copy 2`.
  - Nếu View chưa tồn tại: Mở `Transaction("Tạo 3D View làm việc")`, tạo mới bằng `View3D.CreateIsometric`, đặt tên `3D_Working_HaiAnh`, thiết lập mức hiển thị `DetailLevel.Fine` và kiểu hiển thị `DisplayStyle.ShadingWithEdges`.
  - Gán nhãn nhận diện vào tham số của View (`VIEW_DESCRIPTION`).
  - Tự động kích hoạt hiển thị View mới trên màn hình làm việc.
- **Phạm vi an toàn:** Thao tác an toàn trong Transaction, hỗ trợ Undo (Ctrl+Z) sạch sẽ; không can thiệp, không sửa đổi hay xóa bất kỳ phần tử hình học mô hình nào.

---

## 2. Quá trình Build và Unit Test
- **Biên dịch mã nguồn (Build):**
  - Sử dụng lệnh `dotnet build` với các tham số chuẩn từ `resolve-profile.ps1`:
    - `-p:RevitVersion=2019`
    - `-p:RevitTargetFramework=net47`
    - `-p:RevitApiPath="C:\Program Files\Autodesk\Revit 2019\RevitAPI.dll"`
    - `-p:RevitApiUiPath="C:\Program Files\Autodesk\Revit 2019\RevitAPIUI.dll"`
  - Biên dịch tách biệt 2 thành phần theo kiến trúc Hot Reload:
    - `DSCons.Revit.HotReloadHost.dll`: Vỏ nạp trung gian cho Revit (0 Error, 0 Warning).
    - `DSCons.Revit.Starter.dll`: Thư viện chứa logic nghiệp vụ và giao diện công cụ (0 Error, 0 Warning).
- **Kiểm thử phần lõi (Unit Test):**
  - Chạy `dotnet test` kiểm thử thư viện `DSCons.Revit.Starter.Core.Tests`.
  - Kết quả: **5/5 bài kiểm tra Đạt (Passed)**:
    - 2 test kiểm tra quy tắc làm sạch ký tự tên (`NameRulesTests`).
    - 3 test kiểm tra logic chuẩn hóa và sinh tên View làm việc (`WorkingViewNamingTests`).

---

## 3. Nhật ký học viên tự chạy trong Revit 2019
Theo ghi nhận từ học viên trong phiên làm việc:
1. **Chạy lần 1 (Kiểm tra tạo View):**
   - Học viên đã mở Revit 2019, mở mô hình dự án mẫu.
   - Học viên đã bấm nút **Working 3D View** trên tab Ribbon **`BIM TOOL`** và xác nhận đã chạy xong lần 1.
2. **Chạy lần 2 (Kiểm chứng Hot Reload):**
   - Sau khi AI thay đổi câu thông báo trong code và chạy `dotnet build` lại DLL dưới nền, học viên giữ nguyên Revit đang mở (không tắt phần mềm).
   - Học viên đã bấm lại nút **Working 3D View** trên thanh Ribbon.

---

## 4. Kết quả kiểm chứng Hot Reload
- **Tình trạng:** **Đã chạy được thành công.**
- **Bằng chứng thực tế:** Học viên xác nhận đã nhìn thấy trực tiếp dòng chữ mới xuất hiện trong hộp thoại TaskDialog:
  `"[Hot Reload ⚡] View 3D làm việc '3D_Working_HaiAnh' đã có sẵn trong dự án! Đã tự động chuyển góc nhìn sang View này (Cập nhật không cần tắt Revit)."`
- Điều này chứng minh vỏ `HotReloadHost` đã nạp lại byte array của file DLL mới mà không bị Revit khóa file trên ổ đĩa.

---

## 5. Điều còn vướng / Ghi chú cho giai đoạn tiếp theo
- **Tên View cố định:** Tên View hiện tại được gắn mặc định cho người dùng (`3D_Working_HaiAnh`), chưa có giao diện hộp thoại để người dùng tự gõ tên tùy biến khi bấm nút.
- **Chưa mở rộng sang Mặt bằng tầng (Floor Plan):** Phiên bản MVP Ngày 2 mới tập trung vào View 3D Isometric. Việc tạo hàng loạt mặt bằng làm việc theo từng Level sẽ được cân nhắc ở giai đoạn nâng cao với giao diện WPF.
- **Phân loại Project Browser:** Công cụ hiện gán nhận diện vào `VIEW_DESCRIPTION`. Nếu các dự án thực tế tại doanh nghiệp dùng Shared Parameter riêng để gom nhóm Browser Organization (ví dụ `Sub-Discipline`), cần cấu hình thêm mapping tham số tương ứng.
