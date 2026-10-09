# Nhật ký làm việc (Chat History Summary) — BIM-Tool-WorkingView

---

## Tóm tắt phiên làm việc: Ngày 4 (2026-10-09)

### 1. Ngữ cảnh ban đầu
- **Học viên:** Nguyễn Tôn Hải Anh (BVN) — Email: haianhcdt@gmail.com | ĐT: 0947823273
- **Dự án:** `BIM-Tool-WorkingView`, mục tiêu Revit 2019 (`net47`), mô hình `HA_MODEL_STUDY.rvt`.
- **Tình trạng trước phiên:** Đã có 2 công cụ chính trên thanh Ribbon `BIM TOOL`:
  1. `Working 3D View` (Tạo góc nhìn 3D cá nhân - Ngày 2).
  2. `Batch Tag` (Tự động gắn tag kích thước ống gió - Ngày 3).
  3. `About Me` (Hộp thoại thông tin cá nhân).

---

### 2. Chuỗi yêu cầu và Quyết định đã duyệt

#### Bước 1: Khám phá ý tưởng nâng cấp Multi-Tool
- **Yêu cầu:** Đọc `IDEAS.md` và mã nguồn, đề xuất 5–7 chức năng mở rộng theo 4 hướng:
  (1) Đối nghịch, (2) Hàng loạt, (3) Nối tiếp, (4) Kiểm tra & báo cáo.
- **Kết quả:** AI phân tích và đưa ra 7 chức năng thiết thực; đề xuất ưu tiên các bước an toàn, có xem trước và hỗ trợ hoàn tác.

#### Bước 2: Chốt bài toán Ngày 4 — Căn chỉnh Tag thẳng hàng (`Align Tags`)
- **Quyết định của học viên:** Nâng cấp công cụ `Batch Tag` bằng chức năng **Căn chỉnh các tag thẳng hàng với nhau (`Align Tags`)**.
- **Yêu cầu kỹ thuật ngặt nghèo:**
  - Có bước Xem trước (Preview) qua cửa sổ **WPF Window** chuẩn MVVM theo `templates/TOOL_UI_BRIEF.md` và `skills/wpf-mvvm` (không dùng WinForms, TaskDialog chỉ cho cảnh báo rất ngắn).
  - Gom toàn bộ hành động ghi vào **MỘT Transaction** duy nhất để `Ctrl + Z` hoàn tác cả lần.
  - Báo lỗi bằng lời dễ hiểu, ghi log không chứa dữ liệu nhạy cảm.
  - Rà soát cả 4 nút Ribbon (`About Me`, `Working 3D View`, `Batch Tag`, `Align Tags`), bổ sung vector pictogram riêng cho từng nút (16x16 và 32x32), dùng đúng màu `#1466B8`.
  - Giữ nguyên ID, class name, AddInId, logic cũ.
  - Đăng ký lệnh trong Hot Reload Host.

#### Bước 3: Triển khai mã nguồn & Kiểm thử Core
- **Tài liệu UI:** Lập [docs/TOOL_UI_BRIEF_ALIGN_TAGS.md](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/docs/TOOL_UI_BRIEF_ALIGN_TAGS.md).
- **Core Library:** Tạo [TagAlignmentCalculator.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/DSCons.Revit.Starter.Core/BatchTag/TagAlignmentCalculator.cs) hỗ trợ cả Căn ngang (cùng Y) và Căn dọc (cùng X), mốc Tag đầu tiên hoặc Trung bình.
- **Unit Tests:** Tạo [TagAlignmentTests.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/DSCons.Revit.Starter.Core.Tests/TagAlignmentTests.cs). Chạy `dotnet test` đạt **17/17 tests Passed (100%)**.
- **Vector Icons & Audit:** Bổ sung vector `align` vào `SemanticRibbonIconFactory.cs`. Tạo và chạy script `scripts/audit-ribbon-icons.ps1` -> **ĐẠT 100% TIÊU CHUẨN RIBBON & ICON**.
- **Giao diện WPF:** Tạo `AlignTagsWindow.xaml` (DataGrid xem trước tọa độ & độ lệch $\Delta$) và `AlignTagsViewModel.cs`.
- **Command:** Tạo `AlignTagsCommand.cs` bọc trong 1 Transaction `"BIM TOOL - Align Tags"`. Đăng ký `AlignTagsProxyCommand` vào `HotReloadHost/App.cs`.

#### Bước 4: Đóng Revit & Refresh Hot Reload Host
- Học viên đóng Revit -> AI biên dịch sạch sẽ toàn bộ giải pháp cho Revit 2019:
  - `DSCons.Revit.HotReloadHost.dll`: Build Succeeded (0 Error, 0 Warning).
  - `DSCons.Revit.Starter.dll`: Build Succeeded (0 Error, 0 Warning).

#### Bước 5: Chẩn đoán và Khắc phục lỗi XAML Runtime
- **Hiện tượng:** Khi học viên mở Revit và chạy lệnh, hộp thoại Revit báo lỗi:  
  `Lỗi khi căn chỉnh tag: 'Provide value on 'System.Windows.StaticResourceExtension' threw an exception.' Line number '96' and line position '25'.`
- **Nguyên nhân:** Dòng 96 trong `AlignTagsWindow.xaml` dùng `{StaticResource PrimaryButton}`. Do `Theme.xaml` được nạp động sau khi XAML khởi tạo qua `WindowTheme.Apply(this)`, `StaticResource` đòi hỏi tài nguyên ngay tại thời điểm parse nên sinh lỗi.
- **Xử lý:** Học viên xác nhận -> AI đổi thành `{DynamicResource PrimaryButton}` (giống [AboutMeWindow.xaml](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/Views/AboutMeWindow.xaml#L172)).
- **Nạp nóng (Hot Reload):** Build lại `DSCons.Revit.Starter.dll` đạt 0 Error, 0 Warning. File DLL được nạp tức thì vào phiên Revit đang mở mà học viên không cần khởi động lại Revit.

---

### 3. Đồng bộ GitHub & Vị trí dừng hiện tại
- **Kho lưu trữ từ xa (GitHub):** https://github.com/haianhcdt/Bim-tool-Hai-Anh
- **Trạng thái Git:** Đã khởi tạo Git repository, cấu hình `.gitignore` loại bỏ toàn bộ file nhị phân (`bin/`, `obj/`, `*.dll`) và mô hình Revit (`*.rvt`, `*.rfa`, `*.dwg`).
- **Lịch sử Commit:**
  - `c55643b`: `feat: Khoi tao du an BIM-Tool-WorkingView - Hoan thanh 4 cong cu (Working View, Batch Tag, Align Tags, About Me)`
  - `cdb6bf8`: `docs: cap nhat chat-history.md ve trang thai day kho GitHub`
- **Kết quả Push:** Đã đẩy thành công 100% lên nhánh `main` của kho riêng tư trên GitHub. Nhánh cục bộ đồng bộ hoàn toàn với `origin/main` (`Up to date`).
- **Trạng thái mã nguồn:** Bộ 4 công cụ (Working View, Batch Tag, Align Tags, About Me) và tài liệu `.agents/` hoàn chỉnh.

### 2026-10-09 - Ngày 5: Điều tra lỗi trùng đè tag (Align Tags Overlap)
- **Điểm lưu Git:** Đã tạo annotated tag checkpoint-ngay-5-pre-fix và đẩy lên GitHub.
- **Khảo sát MCP (Chỉ-đọc):** Active View Typical Room WSHP (709705), Document HA_MODEL_STUDY, 2 thẻ tag đang chọn 877737 và 877740 (Category Duct Tags, Class IndependentTag).
- **Tài liệu BUG_REPORT.md:** Đã lập theo mẫu chuẩn Bộ Kit, phân tách rõ nguyên nhân có căn cứ trong TagAlignmentCalculator.cs và suy luận kỹ thuật, đề xuất Phương án A (giãn cách tự động minSpacing), chuẩn bị 2 ca kiểm thử (ca lỗi và ca bình thường).
- **Trạng thái:** Dừng chờ học viên phê duyệt trước khi sửa mã nguồn.

#### Kết quả xử lý Ngày 5: Sửa lỗi trùng đè tag khi căn chỉnh
- **Học viên:** Xác nhận thực hiện sửa lỗi.
- **Sửa mã nguồn lõi:** Cập nhật TagAlignmentCalculator.cs hỗ trợ minSpacing và thuật toán phân tách va chạm chống đè chữ; AlignTagsViewModel.cs áp dụng mặc định minSpacing: 2.5 feet.
- **Kiểm thử tự động:** Bổ sung 3 unit tests mới trong TagAlignmentTests.cs. Toàn bộ 20/20 unit tests PASS (0 lỗi).
- **Biên dịch Revit 2019 (net47):** DSCons.Revit.Starter.dll build thành công 0 Error, 0 Warning. Sẵn sàng nạp nóng qua Hot Reload vào Revit.
- **Tài liệu:** Đã cập nhật BUG_REPORT.md sang trạng thái RESOLVED.

#### Khắc phục lỗi runtime MethodNotFound khi nạp nóng
- **Hiện tượng:** Khi học viên bấm Align Tags trên Revit 2019, Revit báo lỗi: Method not found: TagAlignmentCalculator.CalculateAlignment. 
- **Nguyên nhân:** Do Revit đang chạy đã nạp assembly Starter.Core.dll từ trước (chỉ có chữ ký 3 tham số). Khi đổi chữ ký thành 4 tham số, CLR không nạp đè assembly cùng identity trong AppDomain nên sinh lỗi MissingMethodException.
- **Xử lý:**
  1. Tạo TagAlignmentEngine.cs nội bộ bên trong assembly DSCons.Revit.Starter.dll để nạp nóng 100% độc lập, không phụ thuộc vào assembly Core cũ trong bộ nhớ Revit.
  2. Bổ sung overload 3 tham số tương thích ngược trong TagAlignmentCalculator.cs (Core).
  3. Build lại DSCons.Revit.Starter.dll đạt 0 Error, 0 Warning.
- **Kiểm thử:** 20/20 Unit Tests PASS. File DLL mới nạp tức thì qua Hot Reload.
