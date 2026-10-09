# Danh sách công việc (Tasks Ledger) — BIM-Tool-WorkingView

## Đã hoàn thành (Done)
- [x] **[Setup]** Khởi tạo dự án, thiết lập profile branding `Nguyễn Tôn Hải Anh` (màu `#1466B8`, tab `BIM TOOL`).
- [x] **[Tool 1]** Lập `TOOL_BRIEF.md` cho công cụ `Working 3D View`.
- [x] **[Tool 1]** Triển khai `CreateWorkingViewCommand.cs` và `WorkingViewNaming.cs`.
- [x] **[Tool 1]** Kiểm chứng Hot Reload thành công trên Revit 2019 (Báo cáo: `docs/reports/day-02-report.md`).
- [x] **[Tool 2]** Lập `TOOL_BRIEF_2.md` và `TEST_PLAN_2.md` cho công cụ `Batch Tag`.
- [x] **[Tool 2]** Triển khai `BatchTagCommand.cs`, `BatchTagCandidate.cs`, `BatchTagFilter.cs`.
- [x] **[Tool 2]** Kết nối MCP Revit, kiểm thử gắn 14 tag trên mặt bằng `Typical Room WSHP`, kiểm chứng chống trùng (Báo cáo: `docs/reports/day-03-report.md`).
- [x] **[Tool 3 - Ngày 4]** Phân tích 7 gợi ý nâng cấp multi-tool theo 4 hướng và chốt tính năng `Align Tags`.
- [x] **[Tool 3 - Ngày 4]** Lập hồ sơ UI `docs/TOOL_UI_BRIEF_ALIGN_TAGS.md`.
- [x] **[Tool 3 - Ngày 4]** Lập trình thuật toán Core `TagAlignmentCalculator.cs` và 6 unit test mới (17/17 passed).
- [x] **[Tool 3 - Ngày 4]** Thiết kế vector icon `align` và chạy kiểm toán `scripts/audit-ribbon-icons.ps1` đạt 100%.
- [x] **[Tool 3 - Ngày 4]** Xây dựng WPF Window xem trước `AlignTagsWindow.xaml` và `AlignTagsViewModel.cs`.
- [x] **[Tool 3 - Ngày 4]** Triển khai lệnh `AlignTagsCommand.cs` trong 1 Transaction hoàn tác Ctrl+Z.
- [x] **[Tool 3 - Ngày 4]** Đăng ký Host proxy `AlignTagsProxyCommand`, học viên đóng Revit và build thành công Hot Reload Host.
- [x] **[Tool 3 - Ngày 4]** Chẩn đoán và sửa lỗi XAML `StaticResourceExtension` dòng 96 thành `DynamicResource PrimaryButton`, hot-reload trực tiếp vào Revit.

## Công việc hiện tại (In Progress)
- [ ] Học viên thử nghiệm thực tế nút `Align Tags` trên mặt bằng `Typical Room WSHP` trong Revit 2019.
- [ ] Ghi nhận báo cáo nghiệm thu Ngày 4 (`docs/reports/day-04-report.md`).

## Công việc kế tiếp (Backlog / Ngày 5+)
- [ ] Mở rộng `Batch Tag` gắn tag cho Category `Pipes` và `Conduits`.
- [ ] Nghiên cứu thuật toán xoay góc nghiêng cho tag theo hướng ống gió xiên.
- [ ] Triển khai công cụ Rà soát đầu nối hở hệ thống (`Check Connectors`) theo Ý tưởng số 3 trong `IDEAS.md`.