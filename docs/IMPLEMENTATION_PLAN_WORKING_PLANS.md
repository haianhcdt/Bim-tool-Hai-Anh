# Kế hoạch Triển khai: Nâng cấp Bộ Công cụ Tạo View Làm Việc (Mặt bằng & 3D)

- **Mục tiêu:** Nâng cấp công cụ Working View từ tạo View 3D đơn lẻ thành Hộp thoại Quản lý View làm việc trung tâm theo Phương án A, hỗ trợ tạo Mặt bằng làm việc cá nhân hàng loạt theo Level.
- **Tác giả:** Nguyễn Tôn Hải Anh (BVN)
- **Tài liệu giao diện:** [docs/TOOL_UI_BRIEF_WORKING_PLANS.md](file:///C:/Users/DELL/OneDrive/02.STUDY/12.REVIT%20AI/Projects/BIM-Tool-WorkingView/docs/TOOL_UI_BRIEF_WORKING_PLANS.md)

---

## Giai đoạn 1: Lõi nghiệp vụ & Unit Tests (Core Library)
1. **Mở rộng WorkingViewNaming.cs:**
   - Bổ sung hàm tạo tên mặt bằng: BuildWorkingPlanName(string levelName, string? userName = null) -> FL_Working_HaiAnh_{LevelName}.
   - Bổ sung hàm tạo tên mặt cắt dự kiến: BuildWorkingSectionName(string suffix, string? userName = null) -> SEC_Working_HaiAnh_{suffix}.
   - Đảm bảo tương thích ngược 100% với hàm BuildWorkingViewName() cũ.
2. **Bổ sung Unit Tests:**
   - Cập nhật WorkingViewNamingTests.cs kiểm tra đầy đủ các ca: Tên tầng chuẩn, Tên tầng có khoảng trắng/kí tự đặc biệt, Tên tầng rỗng, và kiểm tra tính nhất quán tiền tố FL_Working_HaiAnh_.
   - Chạy dotnet test đảm bảo 100% tests PASS.

---

## Giai đoạn 2: Xây dựng Giao diện WPF MVVM (UI Layer)
1. **ViewModel (WorkingViewsViewModel.cs):**
   - Lớp model LevelItemViewModel: LevelId, LevelName, ElevationFormatted, IsSelected, AlreadyExists, ExistingViewId.
   - Properties: Levels (ObservableCollection), SelectAllLevels, SelectedLevelsCount, CanCreatePlans, StatusMessage, Is3DViewExisting.
   - Commands:
     - CreateWorkingPlansCommand: Tạo các mặt bằng làm việc cho các tầng được tick chọn.
     - CreateOrOpenWorking3DCommand: Tạo hoặc chuyển nhanh sang 3D cá nhân.
     - OpenExistingPlanCommand: Chuyển góc nhìn sang mặt bằng đã có.
     - ToggleSelectAllCommand: Chọn tất cả / Bỏ chọn tất cả.
     - CloseCommand: Đóng cửa sổ.
2. **Giao diện XAML (WorkingViewsWindow.xaml & .xaml.cs):**
   - Thiết kế chuẩn theo Theme.xaml, mã màu #1466B8.
   - Bố cục 2 thẻ trực quan: Thẻ 3D View cá nhân (nhỏ gọn) và Thẻ Bảng danh sách Tầng (chính).
   - DataGrid / ListView hiển thị Level Name, Elevation, Trạng thái (Pill xanh/xám), CheckBox tương tác mượt mà.

---

## Giai đoạn 3: Tích hợp Command & Ribbon (Integration Layer)
1. **Command (WorkingViewsCommand.cs):**
   - Thay thế/nâng cấp CreateWorkingViewCommand.cs bằng WorkingViewsCommand.cs.
   - Mở cửa sổ WorkingViewsWindow (Modal, gán Owner = Revit MainWindowHandle).
   - Thực thi Transaction tạo ViewPlan bằng ViewPlan.Create(doc, floorPlanType.Id, level.Id) kèm cấu hình DetailLevel = Fine, DisplayStyle = ShadingWithEdges.
2. **Ribbon UI (App.cs):**
   - Cập nhật text nút: "Working\nViews" (hoặc "Working\nView").
   - Cập nhật ToolTip mô tả đầy đủ cả Mặt bằng và 3D View.

---

## Giai đoạn 4: Kiểm thử, Biên dịch Đa phiên bản & Đóng gói (Testing & Delivery)
1. Chạy toàn bộ Unit Tests dotnet test.
2. Biên dịch Release cho cả 5 phiên bản: Revit 2019, 2021, 2022, 2023, 2024.
3. Cập nhật nhị phân vào rtifacts/Revit<Year>/.
4. Biên dịch lại bộ cài Inno Setup BIM-Tool-WorkingView-Setup-1.2.0.exe (hoặc nâng lên 1.3.0).