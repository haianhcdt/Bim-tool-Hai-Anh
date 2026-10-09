# Product Requirements Document (PRD) — BIM-Tool-WorkingView

## 1. Thông tin chung dự án
- **Tên dự án:** BIM-Tool-WorkingView
- **Chủ sở hữu (Branding):** Nguyễn Tôn Hải Anh
- **Đơn vị / Chức vụ:** BIM Manager — BVN
- **Email:** haianhcdt@gmail.com | **Điện thoại:** 0947823273
- **Phiên bản Revit mục tiêu:** Autodesk Revit 2019 (Target Framework: `net47`, .NET Framework 4.7)
- **Model kiểm thử mẫu:** `HA_MODEL_STUDY.rvt`
- **Giao diện Ribbon:** Tab `BIM TOOL`, Panel `Revit API Kit`
- **Màu chủ đạo (Branding Accent):** `#1466B8` (Xanh dương đậm)
- **AddInId:** `75B89104-E715-4D6C-9A5A-4F8A6B290A81` (Manifest) / `A24532C5-ACDE-4946-8414-D8EE406FA589` (Starter)
- **VendorId:** `DSCS`

---

## 2. Mục tiêu sản phẩm
Xây dựng bộ công cụ Add-in đa chức năng (Multi-tool) cho kỹ sư Cơ Điện (MEP) trên nền tảng Revit 2019, giải quyết các tác vụ lặp đi lặp lại trong quy trình triển khai mô hình và trình bày bản vẽ kỹ thuật:
1. Tự động hóa tạo góc nhìn 3D làm việc cá nhân, không can thiệp vào các View in ấn chính thức.
2. Tự động tính toán tâm ống và gắn Tag kích thước hàng loạt, chống đè nét lặp lại.
3. Hỗ trợ căn gióng thẳng hàng các nhãn Tag trên mặt bằng/mặt cắt với cửa sổ xem trước (Preview), đảm bảo tính thẩm mỹ bản vẽ kỹ thuật.
4. Nền tảng mở rộng sẵn sàng cho các công cụ Dựng hình MEP từ CAD, Rà soát đầu nối hở và Bóc tách khối lượng.

---

## 3. Danh mục công cụ hiện có (Tính đến Ngày 4)

### Công cụ 1: Tạo View 3D làm việc (`Working 3D View`) — Ngày 2
- **Lệnh:** `CreateWorkingViewCommand` (Proxy: `CreateWorkingViewProxyCommand`)
- **Icon:** `view3d` (Khối lập phương isometric 3D)
- **Nghiệp vụ:** Kiểm tra View `3D_Working_HaiAnh`. Nếu đã có thì mở ra; nếu chưa có thì tạo mới View3D isometric, cấu hình `DetailLevel.Fine`, `DisplayStyle.ShadingWithEdges`, gán tham số phân loại và kích hoạt ngay.

### Công cụ 2: Gắn Tag hàng loạt (`Batch Tag`) — Ngày 3
- **Lệnh:** `BatchTagCommand` (Proxy: `BatchTagProxyCommand`)
- **Icon:** `tag` (Thẻ nhãn có lỗ xỏ)
- **Nghiệp vụ:** Quét toàn bộ ống gió (`Ducts`) trong Active View, tính toán trung điểm hình học, đối chiếu danh sách tag sẵn có (`TaggedLocalElementId`) để loại trừ chống đè nét, tạo `IndependentTag` trong duy nhất 1 Transaction.

### Công cụ 3: Căn chỉnh Tag thẳng hàng (`Align Tags`) — Ngày 4 (MỚI)
- **Lệnh:** `AlignTagsCommand` (Proxy: `AlignTagsProxyCommand`)
- **Icon:** `align` (Trục gióng thẳng đứng và các thẻ tag thẳng tắp)
- **Giao diện:** WPF Modal Window [AlignTagsWindow.xaml](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/Views/AlignTagsWindow.xaml) kế thừa `#1466B8` qua `WindowTheme`.
- **Nghiệp vụ:** Tiếp nhận selection từ 2 tag trở lên, hiển thị bảng xem trước (Preview DataGrid) tọa độ hiện tại, tọa độ sau khi gióng và độ lệch $\Delta$. Cho phép chọn căn ngang (cùng Y) hoặc căn dọc (cùng X), mốc theo tag đầu tiên hoặc trung bình. Ghi vào model trong 1 Transaction duy nhất (`"BIM TOOL - Align Tags"`), hỗ trợ Ctrl+Z hoàn tác 1 lần.

### Công cụ 4: Hồ sơ cá nhân (`About Me`)
- **Lệnh:** `AboutMeCommand` (Proxy: `AboutMeProxyCommand`)
- **Icon:** `about` (Hình tượng chân dung)
- **Giao diện:** Hộp thoại WPF modal hiển thị thông tin chuyên gia, đơn vị, liên hệ và nhận diện thương hiệu.

---

## 4. Ràng buộc kỹ thuật & Kiến trúc
1. **Kiến trúc Hot Reload:**
   - `DSCons.Revit.HotReloadHost.dll`: Assembly mỏng duy nhất được nạp vào tiến trình Revit lúc khởi động.
   - `DSCons.Revit.Starter.dll`: Chứa toàn bộ nghiệp vụ và UI, được Host nạp động qua byte array (`File.ReadAllBytes`) mỗi khi người dùng click lệnh. Sửa code C#/WPF trong Product DLL không cần tắt Revit.
2. **Kiến trúc Thư viện lõi (Core Library):**
   - `DSCons.Revit.Starter.Core`: Độc lập hoàn toàn với Revit API, chứa các thuật toán thuần túy (`TagAlignmentCalculator`, `BatchTagFilter`, `BatchTagCandidate`, `WorkingViewNaming`) giúp viết Unit Test độc lập và tự động hóa CI/CD.
3. **An toàn mô hình:**
   - Mọi thay đổi ghi model phải nằm trong 1 Transaction duy nhất có tên rõ ràng.
   - Có bước xem trước (Preview) và cho phép người dùng Hủy / Esc trước khi commit.