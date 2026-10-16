# TÀI LIỆU HƯỚNG DẪN CÀI ĐẶT VÀ SỬ DỤNG
## BỘ CÔNG CỤ: BIM TOOL - HẢI ANH (MEP Optimization & Delivery Suite)

- **Tác giả:** Nguyễn Tôn Hải Anh (BVN - BIM Manager)
- **Email:** haianhcdt@gmail.com | **Điện thoại:** 0947823273
- **Phiên bản phát hành:** `v1.2.0` (Multi-Version: Revit 2019, 2021, 2022, 2023, 2024)
- **File cài đặt:** `BIM-Tool-WorkingView-Setup-1.2.0.exe`
- **Mã định danh Add-in (AddInId):** `A24532C5-ACDE-4946-8414-D8EE406FA589` | **VendorId:** `DSCS`

---

## 1. GIỚI THIỆU TRỌN BỘ CÔNG CỤ (8 TÍNH NĂNG TÍCH HỢP)

Bộ công cụ **BIM TOOL - Hải Anh** là Add-in chuyên dụng tích hợp trực tiếp vào Autodesk Revit, hỗ trợ các kỹ sư Cơ Điện (MEP) và nhóm BIM tối ưu hóa quy trình triển khai bản vẽ và bàn giao hồ sơ mô hình:

1. **Working 3D View (Tạo View 3D cá nhân):**
   - Tự động kiểm tra và khởi tạo View 3D làm việc riêng biệt (`3D_Working_HaiAnh`) trên Project Browser.
   - Cấu hình chuẩn hiển thị: `Fine` (Chi tiết cao), `ShadingWithEdges` (Đổ bóng rõ cạnh), giúp thao tác không gian MEP mà không can thiệp vào các View in ấn chính thức của dự án.

2. **Batch Tag (Gắn nhãn ống gió hàng loạt):**
   - Quét toàn bộ hệ thống ống gió (`Ducts`) trong View hiện hành.
   - Tự động tính toán tâm hình học và gắn thẻ ghi chú (`Duct Tags`) kích thước hàng loạt.
   - Tích hợp bộ lọc thông minh chống gắn nhãn trùng lặp hoặc đè lên nhau.

3. **Align Tags (Căn chỉnh thẻ ghi chú thẳng hàng):**
   - Hỗ trợ căn gióng hàng loạt thẻ tag đang chọn theo phương ngang (cùng tọa độ Y) hoặc phương dọc (cùng tọa độ X).
   - Tích hợp cửa sổ giao diện xem trước (WPF MVVM Preview Window) hiển thị danh sách tag, tọa độ gốc, tọa độ mới và độ lệch $\Delta$ trước khi áp dụng.

4. **Dọn Dẹp File Bàn Giao (Model Delivery Cleanup Suite):**
   - **Xóa CAD Link**: Gỡ bỏ sạch sẽ toàn bộ liên kết file CAD ngoài đĩa.
   - **Xóa Revit Link**: Gỡ bỏ liên kết kiến trúc / kết cấu khi cần xuất file MEP độc lập.
   - **Dọn CAD Import**: Xóa các bản vẽ chèn trực tiếp và làm sạch mục Imported Categories trong Visibility/Graphics.
   - **Triệt tiêu Warnings**: Đưa cảnh báo về 0, làm mờ nút Warnings trên tab Manage mà **bảo toàn nguyên vẹn 100% đối tượng và hệ thống MEP**.
   - **Purge Unused**: Dọn sạch các Family, Type, Material rác không dùng để nén dung lượng file tối đa khi gửi đối tác.
   - **1-Click All-in-One**: Dọn dẹp toàn diện chỉ với 1 cú click chuột (hỗ trợ `Ctrl + Z` hoàn tác).

5. **Cảnh Báo Audit & Bảng Thống Kê Trùng Mark (Duplicate Mark Audit):**
   - Quét toàn bộ mô hình và kiểm toán chất lượng tham số Mark.
   - Bảng giao diện WPF trực quan: Thống kê số lượng, hiển thị Category, Family & Type, Level, Element IDs.
   - Hỗ trợ chọn/zoom đối tượng trực tiếp trên Revit, tự động sửa trùng lặp và xuất báo cáo CSV.

6. **Dọn Dẹp 47k Đường Nét Center Line Rác (Clean Center Lines):**
   - Quét tìm và xóa bỏ hàng chục ngàn đường Center Line/Model Line thừa sinh ra khi convert hoặc explode CAD.
   - Bộ lọc an toàn tuyệt đối: Không bao giờ xóa ống (`MEPCurve`) hay phụ kiện/thiết bị (`FamilyInstance`).
   - Giúp giảm từ 30 MB – 50 MB dung lượng file và tăng tốc xoay 3D đáng kể.

7. **Tối Ưu & Xóa Hệ Thống Ống Rỗng (Optimize Piping Systems):**
   - Tự động quét và giải phóng các hệ thống ống rỗng (Piping Systems có 0 cấu kiện) do Revit tự nhân bản làm nặng cơ sở dữ liệu.
   - Cam kết bảo toàn 100% hình ảnh phối cảnh, vật liệu và hệ thống thực tế.

8. **About Me (Thông tin & Thương hiệu):**
   - Hộp thoại giao diện WPF hiển thị thông tin chuyên gia, chức vụ, đơn vị công tác và liên hệ hỗ trợ kỹ thuật.

---

## 2. YÊU CẦU ĐỘI CHẠY (System Requirements)
- **Hệ điều hành:** Windows 10 hoặc Windows 11 (64-bit).
- **Phần mềm máy chủ:** Autodesk Revit đã được cài đặt trên máy tính (hỗ trợ **2019, 2021, 2022, 2023, 2024**).
- **Nền tảng .NET:** .NET Framework 4.7.2 / 4.8.

---

## 3. HƯỚNG DẪN CÀI ĐẶT

### Cách 1: Cài đặt bằng File Thực thi (.exe - Khuyên dùng)
1. Đảm bảo đã **lưu file và ĐÓNG HOÀN TOÀN REVIT**.
2. Chạy file bộ cài đặt: **`BIM-Tool-WorkingView-Setup-1.2.0.exe`**.
3. Bộ cài đặt sẽ tự động dò tìm các phiên bản Revit có trên máy tính của bạn và thông báo rõ ràng trên màn hình.
4. Bấm **Next** ➔ **Install** ➔ **Finish**.
5. Mở Autodesk Revit, nhìn lên thanh Ribbon bạn sẽ thấy xuất hiện tab mới: **`BIM TOOL`** với đầy đủ các công cụ sẵn sàng sử dụng.

### Cách 2: Cài đặt tự động bằng PowerShell (1-Click Setup)
1. Đóng hoàn toàn Revit.
2. Mở thư mục **`powershell-setup/`**.
3. Nhấp chuột phải vào **`Install-All-Versions.ps1`** ➔ Chọn **Run with PowerShell**.
4. Script sẽ tự động nhận diện tất cả các phiên bản Revit và nạp file add-in tương ứng.

---

## 4. HƯỚNG DẪN GỠ CÀI ĐẶT (Uninstall)
- **Qua Windows Settings:** Vào `Settings ➔ Apps & Features` (hoặc `Installed Apps`), tìm **BIM Tool - Hai Anh** và chọn **Uninstall**.
- **Qua PowerShell:** Chạy file `powershell-setup/Uninstall-All-Versions.ps1` để dọn dẹp sạch sẽ toàn bộ manifest và DLL khỏi mọi phiên bản Revit.

---

## 5. THÔNG TIN XÁC THỰC BẢN PHÁT HÀNH

| Thuộc tính | Giá trị xác thực |
|---|---|
| **Tên tệp** | `BIM-Tool-WorkingView-Setup-1.2.0.exe` |
| **Phiên bản** | `1.2.0` |
| **Dung lượng** | 2.148.920 bytes (~2,05 MB) |
| **Mã băm SHA-256** | `00A2A0679B3DF7E22346B034ED35E0026353F5BEC00A569DD849DC57FBCD197C` |
| **Phạm vi hỗ trợ** | Autodesk Revit 2019, 2021, 2022, 2023, 2024 |
| **Trạng thái kiểm thử** | 20/20 Unit Tests PASS (100%), 0 Warnings, 0 Errors |