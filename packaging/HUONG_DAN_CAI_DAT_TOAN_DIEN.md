# HƯỚNG DẪN CÀI ĐẶT & PHÁT HÀNH BỘ CÔNG CỤ BIM TOOL (MULTI-VERSION)

**Tác giả:** Nguyễn Tôn Hải Anh  
**Chức danh:** BIM Manager (BVN)  
**Phiên bản add-in:** v1.5.1  
**Phạm vi hỗ trợ:** Autodesk Revit 2019, 2021, 2022, 2023, 2024  

---

## 1. Cấu trúc thư mục gói đóng gói (`packaging/`)

```text
packaging/
├── Install-All-Versions.ps1         # Script cài đặt tự động cho TẤT CẢ phiên bản phát hiện được
├── Uninstall-All-Versions.ps1       # Script gỡ bỏ sạch sẽ mọi phiên bản
├── Setup_BIMTool_MultiVersion.iss   # Kịch bản Inno Setup đóng gói thành file .exe cài đặt
├── HUONG_DAN_CAI_DAT_TOAN_DIEN.md   # Tài liệu hướng dẫn này
│
├── Revit2019/                      # Gói phát hành độc lập cho Revit 2019
│   ├── BIMTool.Revit2019.addin
│   ├── BIMTool/ (chứa DLLs)
│   ├── Install-BIMTool-2019.ps1
│   ├── Uninstall-BIMTool-2019.ps1
│   └── Setup_BIMTool_Revit2019.iss
│
├── Revit2021/                      # Gói phát hành độc lập cho Revit 2021
├── Revit2022/                      # Gói phát hành độc lập cho Revit 2022
├── Revit2023/                      # Gói phát hành độc lập cho Revit 2023
└── Revit2024/                      # Gói phát hành độc lập cho Revit 2024
```

---

## 2. Hướng dẫn cài đặt nhanh cho người dùng cuối (End-User)

### Cách 1: Cài đặt tự động bằng PowerShell (Khuyên dùng - 1 Click)
1. Đảm bảo đã **lưu file và đóng hoàn toàn Autodesk Revit**.
2. Nhấp chuột phải vào file **`Install-All-Versions.ps1`** trong thư mục `packaging/`.
3. Chọn **Run with PowerShell**.
4. Script sẽ tự động nhận diện tất cả các phiên bản Revit (2019, 2021, 2022, 2023, 2024) có trên máy và sao chép đúng file tương ứng vào thư mục Addins của người dùng (`%APPDATA%\Autodesk\Revit\Addins\`).
5. Khi màn hình hiện thông báo màu xanh **CÀI ĐẶT THÀNH CÔNG**, quá trình hoàn tất!

*(Nếu bạn chỉ muốn cài cho riêng một phiên bản cụ thể, hãy vào thư mục con của năm đó (ví dụ `Revit2024/`) và chạy file `Install-BIMTool-2024.ps1`).*

---

### Cách 2: Đóng gói thành file Installer độc lập (.exe bằng Inno Setup)
1. Cài đặt phần mềm miễn phí [Inno Setup 6](https://jrsoftware.org/isdl.php) trên máy.
2. Mở file **`Setup_BIMTool_MultiVersion.iss`** bằng Inno Setup Compiler.
3. Bấm menu **Build** ➔ **Compile** (hoặc nhấn phím `Ctrl + F9`).
4. File bộ cài đặt dạng thực thi **`Setup_BIMTool_MultiVersion_2019_2024.exe`** sẽ được tạo tại thư mục `Output\`.
5. Bạn có thể gửi trực tiếp file `.exe` này cho các kỹ sư trong công ty hoặc đối tác để họ cài đặt như một phần mềm Windows thông thường.

---

## 3. Cách gỡ cài đặt (Uninstall)

- Chạy file **`Uninstall-All-Versions.ps1`** bằng PowerShell.
- Toàn bộ file manifest `.addin` và thư mục `BIMTool` trong các thư mục Addins sẽ được dọn sạch hoàn toàn, không để lại file rác.
