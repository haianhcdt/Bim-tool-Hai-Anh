# Kế hoạch triển khai (Implementation Plan) — BIM-Tool-WorkingView

## Giai đoạn 1: Khởi tạo & Định danh (Ngày 1)
- [x] Thiết lập workspace dự án tại `Projects/BIM-Tool-WorkingView`.
- [x] Cá nhân hóa branding qua `personalize-starter.ps1` (Chủ sở hữu: Nguyễn Tôn Hải Anh, màu `#1466B8`, tab `BIM TOOL`).
- [x] Tạo kho tài liệu ý tưởng `docs/IDEAS.md` phân loại 10 bài toán tự động hóa MEP.

## Giai đoạn 2: Công cụ Tạo View làm việc (Ngày 2)
- [x] Viết `WorkingViewNaming.cs` trong `Core` và unit test 5/5 passed.
- [x] Viết `CreateWorkingViewCommand.cs` tự động sinh hoặc kích hoạt View 3D cá nhân `3D_Working_HaiAnh`.
- [x] Đăng ký nút `Working 3D View` (icon vector `view3d`).
- [x] Cài đặt manifest phát triển, chạy thử nghiệm trên Revit 2019 và kiểm chứng thành công Hot Reload.

## Giai đoạn 3: Công cụ Gắn Tag hàng loạt (Ngày 3)
- [x] Xây dựng thư viện Core: `BatchTagCandidate`, `BatchTagFilter`, `BatchTagResult` và unit test đạt 11/11 passed.
- [x] Lập `docs/TOOL_BRIEF_2.md` và `docs/TEST_PLAN_2.md`.
- [x] Viết lệnh `BatchTagCommand.cs` gắn tag tự động cho ống gió trên mặt bằng, kiểm tra chống trùng `alreadyTaggedIds`.
- [x] Kết nối Revit MCP thành công, đọc snapshot model `HA_MODEL_STUDY.rvt` và chạy ca kiểm thử thực tế trên mặt bằng `Typical Room WSHP` (gắn 14 tag, bấm lần 2 báo 0 tag mới).

## Giai đoạn 4: Nâng cấp Công cụ Căn chỉnh Tag thẳng hàng (Ngày 4 — Hiện tại)
- [x] Khảo sát kho ý tưởng, chốt nâng cấp bổ sung chức năng `Align Tags` vào bộ công cụ `Batch Tag`.
- [x] Lập hồ sơ [docs/TOOL_UI_BRIEF_ALIGN_TAGS.md](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/docs/TOOL_UI_BRIEF_ALIGN_TAGS.md) theo chuẩn Kit.
- [x] Viết thuật toán [TagAlignmentCalculator.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/DSCons.Revit.Starter.Core/BatchTag/TagAlignmentCalculator.cs) và bổ sung 6 unit test nâng tổng số test lên 17/17 passed.
- [x] Bổ sung icon vector `align` chuyên biệt vào [SemanticRibbonIconFactory.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/Infrastructure/SemanticRibbonIconFactory.cs).
- [x] Lập và chạy script [scripts/audit-ribbon-icons.ps1](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/scripts/audit-ribbon-icons.ps1) đạt 100% tiêu chuẩn Ribbon cho cả 4 nút.
- [x] Xây dựng giao diện WPF [AlignTagsWindow.xaml](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/Views/AlignTagsWindow.xaml) và [AlignTagsViewModel.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/ViewModels/AlignTagsViewModel.cs) có bảng xem trước (Preview DataGrid).
- [x] Viết lệnh [AlignTagsCommand.cs](file:///c:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/source/Commands/AlignTagsCommand.cs) gói trong 1 Transaction duy nhất (`"BIM TOOL - Align Tags"`).
- [x] Đăng ký `AlignTagsProxyCommand` vào Hot Reload Host.
- [x] Khắc phục lỗi runtime `StaticResourceExtension` tại dòng 96 bằng cách chuyển sang `DynamicResource PrimaryButton`. Build lại và nạp tức thì qua Hot Reload.

## Giai đoạn 5: Định hướng kế tiếp (Ngày 5+)
- [ ] Mở rộng `Batch Tag` gắn đồng thời cho Pipes (Ống nước chữa cháy/cấp thoát), Conduits và Equipment.
- [ ] Triển khai Ý tưởng số 3 trong `IDEAS.md`: Rà soát đầu nối hở hệ thống (`Check Connectors`) — chế độ chỉ đọc an toàn.
- [ ] Nghiên cứu thuật toán xoay hướng tag theo vector góc nghiêng của ống gió chạy chéo.