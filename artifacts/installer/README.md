# TÀI LIỆU HƯỚNG DẪN CÀI ĐẶT VÀ SỬ DỤNG
## BỘ CÔNG CỤ: BIM TOOL - HẢI ANH (BIM-Tool-WorkingView)

- **Tác giả:** Nguyễn Tấn Hải Anh (BVN - BIM Manager)
- **Email:** haianhcdt@gmail.com | **Điện thoại:** 0947823273
- **Phiên bản phát hành:** `v1.0.0`
- **File cài đặt:** `BIM-Tool-WorkingView-Setup-1.0.0.exe`
- **Mã định danh Add-in:** `A24532C5-ACDE-4946-8414-D8EE406FA589` | **VendorId:** `DSCS`

---

## 1. GIỚI THIỆU CÔNG CỤ (Công cụ làm gì?)
Bộ công cụ **BIM TOOL - Hải Anh** là Add-in chuyên dụng tích hợp trực tiếp vào Autodesk Revit, hỗ trợ các kỹ sư Cơ Điện (MEP) và nhóm BIM tối ưu hóa quy trình triển khai bản vẽ kỹ thuật:

1. **Working 3D View (Tạo View 3D cá nhân):**
   - Tự động kiểm tra và khởi tạo View 3D làm việc riêng biệt (`3D_Working_HaiAnh`) trên Project Browser.
   - Tự động cấu hình chuẩn hiển thị: `DetailLevel.Fine` (Chi tiết cao), `DisplayStyle.ShadingWithEdges` (Đổ bóng rõ cạnh), giúp quan sát không gian MEP mà không can thiệp vào các View in ấn chính thức của dự án.
2. **Batch Tag (Gắn nhãn ống gió hàng loạt):**
   - Quét toàn bộ hệ thống ống gió (`Ducts`) trong View hiện hành.
   - Tự động tính toán tâm hình học và gắn thẻ ghi chú (`Duct Tags`) kích thước hàng loạt.
   - Tích hợp bộ lọc thông minh: Tự động đối chiếu các tag đã có sẵn để loại trừ, chống gắn nhãn trùng lặp đè lên nhau.
3. **Align Tags (Căn chỉnh thẻ ghi chú thẳng hàng):**
   - Hỗ trợ căn gióng hàng loạt thẻ tag đang chọn theo phương ngang (cùng tọa độ Y) hoặc phương dọc (cùng tọa độ X).
   - Tích hợp cửa sổ giao diện xem trước (WPF MVVM Preview Window) hiển thị danh sách tag, tọa độ cũ, tọa độ mới và độ lệch $\Delta$ trước khi áp dụng.
   - Tự động tính khoảng cách an toàn tối thiểu (`minSpacing = 2.5 ft`) nhằm triệt tiêu hiện tượng đè chữ khi các thẻ tag nằm sát nhau.
   - Toàn bộ thao tác ghi vào mô hình được gom trong đúng 1 Transaction duy nhất (`Ctrl + Z` hoàn tác tức thì).
4. **About Me (Thông tin & Thương hiệu):**
   - Hộp thoại giao diện WPF hiển thị thông tin chuyên gia, chức vụ, đơn vị công tác và liên hệ hỗ trợ kỹ thuật.

---

## 2. YÊU CẦU ĐỂ CHẠY (System Requirements)
- **Hệ điều hành:** Windows 10 hoặc Windows 11 (64-bit).
- **Phần mềm máy chủ:** Autodesk Revit đã được cài đặt trên máy tính (hỗ trợ bản 2019, 2021, 2022).
- **Nền tảng .NET:**
  - Với Revit 2019: Yêu cầu .NET Framework 4.7 trở lên.
  - Với Revit 2021 & 2022: Yêu cầu .NET Framework 4.8 trở lên.
- **Quyền hạn người dùng:** Quyền Quản trị viên (Administrator) khi chạy bộ cài đặt để ghi file cấu hình vào `%ProgramData%`.

---

## 3. HƯỚNG DẪN CÀI ĐẶT (Cách cài)

### Bước 1: Kiểm tra tính toàn vẹn file (Đối chiếu mã SHA-256)
Trước khi cài đặt, bạn nên kiểm tra mã băm SHA-256 của file cài đặt để đảm bảo file tải về nguyên vẹn và không bị can thiệp:
1. Mở cửa sổ **PowerShell** tại thư mục chứa file bộ cài.
2. Chạy câu lệnh sau:
   ```powershell
   Get-FileHash .\BIM-Tool-WorkingView-Setup-1.0.0.exe -Algorithm SHA256
   ```
3. Đối chiếu kết quả với mã băm chính thức:
   ```text
   70E8B761E48349AD894A8B417F657B3E47EB68C1D5E5FA56317E0D516E53CA80
   ```
   *(Nếu hai chuỗi ký tự trùng khớp hoàn toàn, file an toàn 100% để cài đặt).*

### Bước 2: Xử lý cảnh báo Windows SmartScreen (Lưu ý quan trọng)
Do đây là gói cài đặt phát hành nội bộ của kỹ sư và chưa mua chứng chỉ số thương mại (Code Signing Certificate), hệ thống bảo vệ **Windows SmartScreen** có thể xuất hiện hộp thoại màu xanh với dòng chữ:
> *"Windows protected your PC / Microsoft Defender SmartScreen prevented an unrecognized app from starting."*

**Cách xử lý an toàn:**
- Nhấp chuột vào liên kết **"More info"** (Xem thêm thông tin).
- Nhấp nút **"Run anyway"** (Vẫn chạy) để tiếp tục mở trình cài đặt.

### Bước 3: Tiến hành cài đặt
1. Hãy **đóng phần mềm Autodesk Revit** trước khi tiến hành cài đặt.
2. Chạy file `BIM-Tool-WorkingView-Setup-1.0.0.exe` với quyền Administrator.
3. Tại màn hình **Ready to Install**, bộ cài sẽ tự động hiển thị danh sách các phiên bản Revit được phát hiện trên máy tính của bạn (Revit 2019, 2021 hoặc 2022).
4. Nhấn **Install** để hoàn tất cài đặt tự động.

---

## 4. HƯỚNG DẪN SỬ DỤNG (Cách dùng)
1. Khởi động phần mềm **Autodesk Revit**.
2. Trên thanh thanh Ribbon, nhấp vào thẻ mới có tên: **`BIM TOOL`** (nhóm panel `Revit API Kit`).
3. Sử dụng các chức năng:
   - **Working 3D View:** Nhấp nút để lập tức mở hoặc tạo View 3D cá nhân `3D_Working_HaiAnh`.
   - **Batch Tag:** Mở một mặt bằng ống gió bất kỳ -> Nhấp nút **Batch Tag** -> Kiểm tra kết quả gắn nhãn tự động trên màn hình.
   - **Align Tags:** Dùng chuột quét chọn từ 2 thẻ Tag trở lên trong mặt bằng -> Nhấp nút **Align Tags** -> Hộp thoại xem trước hiện ra:
     - Chọn hướng: *Căn ngang (Cùng tọa độ Y)* hoặc *Căn dọc (Cùng tọa độ X)*.
     - Chọn mốc gióng: *Theo tag đầu tiên* hoặc *Trung bình tọa độ*.
     - Xem trước bảng tọa độ mới và độ lệch $\Delta$ -> Nhấp **Thực hiện căn chỉnh**.
   - **About Me:** Nhấp nút để tra cứu thông tin tác giả và phiên bản công cụ.

---

## 5. HƯỚNG DẪN GỠ CÀI ĐẶT (Cách gỡ)

### Cách 1: Gỡ bỏ tự động qua Windows (Khuyên dùng)
1. Đóng phần mềm Autodesk Revit.
2. Mở **Windows Settings** (`Windows + I`) > chọn **Apps** > **Installed apps** (hoặc mở *Control Panel > Programs and Features*).
3. Tìm kiếm ứng dụng có tên: **`BIM Tool - Hai Anh - Revit MEP Tools (Uninstall)`**.
4. Chọn **Uninstall** -> Nhấn **Yes** để xác nhận. Trình gỡ cài đặt sẽ tự động quét và xóa sạch toàn bộ file nhị phân và file `.addin` trong tất cả các thư mục Revit.

### Cách 2: Gỡ bỏ thủ công
Nếu không dùng trình gỡ cài đặt, truy cập thư mục:
`C:\ProgramData\Autodesk\Revit\Addins\<Năm_Revit>\`
Xóa 2 mục:
- File manifest: `DSCons.Revit.Starter.addin`
- Thư mục chương trình: `BIM-Tool-WorkingView\`

---

## 6. BẢNG MA TRẬN PHIÊN BẢN REVIT (Audit Matrix)

| Phiên bản Revit | Build | Unit Test | Đã chạy thử trong Revit thực tế | Ghi chú kỹ thuật |
|---|---|---|---|---|
| **Revit 2019** | **ĐẠT** (`net47`) | **20/20 PASS** | **ĐÃ CHẠY THỬ** (2026-10-09, Model `HA_MODEL_STUDY.rvt`, View `Typical Room WSHP`) | Phiên bản chính; đã kiểm nghiệm thực tế và hoàn thiện thuật toán. |
| **Revit 2021** | **ĐẠT** (`net48`) | **20/20 PASS** | **CHƯA KIỂM CHỨNG** | Build sạch; đóng gói sẵn trong bộ cài; chưa runtime-test thực tế. |
| **Revit 2022** | **ĐẠT** (`net48`) | **20/20 PASS** | **CHƯA KIỂM CHỨNG** | Build sạch (1 warning obsolete); đóng gói sẵn trong bộ cài; chưa runtime-test. |
| **Revit 2023** | **LỖI** | **20/20 PASS** (Core) | **CHƯA KIỂM CHỨNG** | Lỗi biên dịch: Autodesk gỡ bỏ `IndependentTag.TaggedLocalElementId` trên bản 2023+. |
| **Revit 2024** | **LỖI** | **20/20 PASS** (Core) | **CHƯA KIỂM CHỨNG** | Lỗi biên dịch: Cùng nguyên nhân breaking change API như Revit 2023. |
| **Revit 2026** | **LỖI** | **20/20 PASS** (Core) | **CHƯA KIỂM CHỨNG** | Lỗi biên dịch: Cùng nguyên nhân breaking change API như Revit 2023. |

---

## 7. GIỚI HẠN & CÁC LỖI THƯỜNG GẶP (Troubleshooting)

### Giới hạn công cụ
- Bộ cài đặt hiện tại chỉ phân phối nhị phân cho **Revit 2019, Revit 2021 và Revit 2022**. Các phiên bản từ 2023 trở lên hiện chưa hỗ trợ trong bản phát hành này.
- Công cụ `Align Tags` bắt buộc người dùng phải quét chọn trước **tối thiểu 2 thẻ ghi chú** (`IndependentTag`). Nếu chưa chọn đối tượng hoặc chọn sai loại phần tử, công cụ sẽ hiển thị thông báo nhắc nhở và không thực hiện lệnh.
- Công cụ `Batch Tag` hiện được lập trình tối ưu cho danh mục Ống gió (`Ducts`) trong Active View.

### Xử lý sự cố thường gặp
1. **Mở Revit không thấy tab `BIM TOOL` trên Ribbon:**
   - *Nguyên nhân:* Revit đang mở trong lúc bạn chạy bộ cài đặt, hoặc bộ cài chưa được cấp quyền Administrator.
   - *Khắc phục:* Đóng hoàn toàn Revit, nhấp chuột phải vào file `.exe` chọn **Run as administrator** để cài lại, sau đó mở lại Revit.
2. **Hộp thoại báo không tìm thấy tag khi căn chỉnh:**
   - *Nguyên nhân:* Bạn đang chọn các phần tử mô hình (ống gió, phụ kiện) thay vì chọn thẻ tag chữ.
   - *Khắc phục:* Giữ phím `Ctrl` hoặc quét chuột chọn chính xác các thẻ ghi chú kích thước cần gióng hàng trước khi bấm lệnh.
3. **Mã SHA-256 không trùng khớp khi kiểm tra:**
   - *Nguyên nhân:* File cài đặt bị lỗi gián đoạn đường truyền trong quá trình sao chép/tải về.
   - *Khắc phục:* Tải lại bộ cài đặt và đối chiếu lại mã hash trước khi thực thi.