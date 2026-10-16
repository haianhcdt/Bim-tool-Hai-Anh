using System;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Core.Views;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class LevelItemViewModel : ViewModelBase
{
    private bool _isSelected;
    private bool _alreadyExists;

    public ElementId LevelId { get; }
    public string LevelName { get; }
    public double ElevationFeet { get; }
    public string ElevationFormatted { get; }
    public string TargetPlanName { get; }
    public ElementId? ExistingViewId { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                SelectionChanged?.Invoke();
            }
        }
    }

    public bool AlreadyExists
    {
        get => _alreadyExists;
        set
        {
            if (SetProperty(ref _alreadyExists, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusBrush));
                OnPropertyChanged(nameof(StatusBadgeBackground));
            }
        }
    }

    public string StatusText => AlreadyExists ? "Đã có sẵn" : "Chưa tạo";
    public string StatusBrush => AlreadyExists ? "#15803D" : "#64748B";
    public string StatusBadgeBackground => AlreadyExists ? "#DCFCE7" : "#F1F5F9";

    public event Action? SelectionChanged;

    public LevelItemViewModel(
        ElementId levelId,
        string levelName,
        double elevationFeet,
        string targetPlanName,
        bool alreadyExists,
        ElementId? existingViewId)
    {
        LevelId = levelId;
        LevelName = levelName;
        ElevationFeet = elevationFeet;
        TargetPlanName = targetPlanName;
        _alreadyExists = alreadyExists;
        ExistingViewId = existingViewId;

        double meters = elevationFeet * 0.3048;
        ElevationFormatted = (meters >= 0 ? "+" : "") + meters.ToString("0.000") + " m";
    }
}

public sealed class WorkingViewsViewModel : ViewModelBase
{
    private readonly UIDocument _uiDoc;
    private readonly Document _doc;
    private string _statusMessage = "Sẵn sàng tạo View làm việc cá nhân.";
    private bool _isBusy;
    private bool _is3DViewExisting;
    private Autodesk.Revit.DB.View? _pendingActiveView;

    public ObservableCollection<LevelItemViewModel> Levels { get; } = new();

    public string OwnerName { get; } = "HaiAnh";
    public string Target3DViewName => WorkingViewNaming.BuildWorking3DViewName(OwnerName);

    public Autodesk.Revit.DB.View? PendingActiveView => _pendingActiveView;

    public bool Is3DViewExisting
    {
        get => _is3DViewExisting;
        private set
        {
            if (SetProperty(ref _is3DViewExisting, value))
            {
                OnPropertyChanged(nameof(Status3DText));
                OnPropertyChanged(nameof(Status3DBrush));
                OnPropertyChanged(nameof(Button3DText));
            }
        }
    }

    public string Status3DText => Is3DViewExisting ? "Đã có trong dự án" : "Chưa tạo";
    public string Status3DBrush => Is3DViewExisting ? "#15803D" : "#64748B";
    public string Button3DText => Is3DViewExisting ? "🌐 Mở 3D_Working_HaiAnh" : "⚡ Tạo mới 3D_Working_HaiAnh";

    public int SelectedCount => Levels.Count(x => x.IsSelected);
    public bool CanCreatePlans => SelectedCount > 0 && !IsBusy;
    public string CreateButtonText => SelectedCount > 0
        ? $"⚡ Tạo Mặt Bằng Làm Việc ({SelectedCount} tầng)"
        : "⚡ Tạo Mặt Bằng Làm Việc";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                CreateWorkingPlansCommand.RaiseCanExecuteChanged();
                CreateOrOpenWorking3DCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand CreateWorkingPlansCommand { get; }
    public RelayCommand CreateOrOpenWorking3DCommand { get; }
    public RelayCommand SelectAllCommand { get; }
    public RelayCommand DeselectAllCommand { get; }
    public RelayCommand OpenExistingPlanCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public WorkingViewsViewModel(UIDocument uiDoc)
    {
        _uiDoc = uiDoc ?? throw new ArgumentNullException(nameof(uiDoc));
        _doc = uiDoc.Document;

        CreateWorkingPlansCommand = new RelayCommand(_ => CreateWorkingPlans(), _ => CanCreatePlans);
        CreateOrOpenWorking3DCommand = new RelayCommand(_ => CreateOrOpenWorking3D(), _ => !IsBusy);
        SelectAllCommand = new RelayCommand(_ => SetAllSelection(true));
        DeselectAllCommand = new RelayCommand(_ => SetAllSelection(false));
        OpenExistingPlanCommand = new RelayCommand(p => OpenExistingPlan(p as LevelItemViewModel));
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

        ScanLevelsAndViews();
    }

    private void ScanLevelsAndViews()
    {
        Levels.Clear();

        // 1. Kiểm tra 3D View
        var existing3D = new FilteredElementCollector(_doc)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .FirstOrDefault(v => !v.IsTemplate && string.Equals(v.Name, Target3DViewName, StringComparison.OrdinalIgnoreCase));
        Is3DViewExisting = (existing3D != null);

        // 2. Thu thập danh sách ViewPlan đang có
        var existingPlans = new FilteredElementCollector(_doc)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(v => !v.IsTemplate)
            .ToList();

        // 3. Thu thập danh sách Level
        var levels = new FilteredElementCollector(_doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        ElementId? currentLevelId = _uiDoc.ActiveView.GenLevel?.Id;

        foreach (var lvl in levels)
        {
            string expectedPlanName = WorkingViewNaming.BuildWorkingPlanName(lvl.Name, OwnerName);
            var matchedView = existingPlans.FirstOrDefault(
                p => string.Equals(p.Name, expectedPlanName, StringComparison.OrdinalIgnoreCase));

            bool exists = matchedView != null;
            var item = new LevelItemViewModel(
                lvl.Id,
                lvl.Name,
                lvl.Elevation,
                expectedPlanName,
                exists,
                matchedView?.Id);

            // Mặc định tick chọn tầng của View hiện hành nếu chưa có mặt bằng làm việc
            if (currentLevelId != null && lvl.Id == currentLevelId)
            {
                item.IsSelected = true;
            }

            item.SelectionChanged += OnLevelSelectionChanged;
            Levels.Add(item);
        }

        // Nếu không có tầng nào được chọn mặc định, chọn tầng đầu tiên
        if (SelectedCount == 0 && Levels.Count > 0)
        {
            Levels[0].IsSelected = true;
        }

        UpdateCounters();
    }

    private void SetAllSelection(bool isSelected)
    {
        foreach (var lvl in Levels)
        {
            lvl.IsSelected = isSelected;
        }
        UpdateCounters();
    }

    private void OnLevelSelectionChanged()
    {
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(CanCreatePlans));
        OnPropertyChanged(nameof(CreateButtonText));
        CreateWorkingPlansCommand.RaiseCanExecuteChanged();
    }

    public void CreateWorkingPlans()
    {
        var targets = Levels.Where(x => x.IsSelected).ToList();
        if (targets.Count == 0) return;

        IsBusy = true;
        try
        {
            // Tìm kiểu ViewFamilyType cho FloorPlan
            var viewFamilyType = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.FloorPlan);

            if (viewFamilyType == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy kiểu View Mặt Bằng (ViewFamily.FloorPlan) trong dự án.";
                return;
            }

            int createdCount = 0;
            int existingCount = 0;
            ViewPlan? lastView = null;

            using (var tx = new Transaction(_doc, "BIM TOOL - Tạo Mặt Bằng Làm Việc"))
            {
                tx.Start();

                foreach (var item in targets)
                {
                    if (item.AlreadyExists && item.ExistingViewId != null)
                    {
                        existingCount++;
                        lastView = _doc.GetElement(item.ExistingViewId) as ViewPlan;
                        continue;
                    }

                    try
                    {
                        var newPlan = ViewPlan.Create(_doc, viewFamilyType.Id, item.LevelId);
                        newPlan.Name = item.TargetPlanName;
                        newPlan.DetailLevel = ViewDetailLevel.Fine;
                        newPlan.DisplayStyle = DisplayStyle.ShadingWithEdges;

                        var descParam = newPlan.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                        if (descParam != null && !descParam.IsReadOnly)
                        {
                            descParam.Set("Working Plan - Nguyễn Tôn Hải Anh");
                        }

                        item.AlreadyExists = true;
                        item.ExistingViewId = newPlan.Id;
                        lastView = newPlan;
                        createdCount++;
                    }
                    catch (Exception ex)
                    {
                        Log.Write($"Lỗi khi tạo mặt bằng {item.TargetPlanName}: {ex.Message}");
                    }
                }

                tx.Commit();
            }

            if (createdCount > 0)
            {
                StatusMessage = $"Đã tạo thành công {createdCount} mặt bằng làm việc cá nhân!";
                if (targets.Count == 1 && lastView != null)
                {
                    _pendingActiveView = lastView;
                    RequestClose?.Invoke();
                }
            }
            else if (existingCount > 0)
            {
                StatusMessage = $"Các tầng đã chọn đều đã có mặt bằng làm việc sẵn có ({existingCount} tầng).";
                if (targets.Count == 1 && lastView != null)
                {
                    _pendingActiveView = lastView;
                    RequestClose?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi tạo mặt bằng: " + ex.Message;
            Log.Write("CreateWorkingPlans error: " + ex);
        }
        finally
        {
            IsBusy = false;
            UpdateCounters();
        }
    }

    public void CreateOrOpenWorking3D()
    {
        IsBusy = true;
        try
        {
            var existing3D = new FilteredElementCollector(_doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && string.Equals(v.Name, Target3DViewName, StringComparison.OrdinalIgnoreCase));

            if (existing3D != null)
            {
                _pendingActiveView = existing3D;
                StatusMessage = $"Đã kích hoạt View 3D '{Target3DViewName}'!";
                RequestClose?.Invoke();
                return;
            }

            var viewFamilyType = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFamilyType == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy kiểu View 3D trong dự án.";
                return;
            }

            using (var tx = new Transaction(_doc, "BIM TOOL - Tạo 3D View Làm Việc"))
            {
                tx.Start();
                var new3D = View3D.CreateIsometric(_doc, viewFamilyType.Id);
                new3D.Name = Target3DViewName;
                new3D.DetailLevel = ViewDetailLevel.Fine;
                new3D.DisplayStyle = DisplayStyle.ShadingWithEdges;

                var descParam = new3D.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                if (descParam != null && !descParam.IsReadOnly)
                {
                    descParam.Set("Working View - Nguyễn Tôn Hải Anh");
                }

                tx.Commit();

                _pendingActiveView = new3D;
                Is3DViewExisting = true;
                StatusMessage = $"Đã tạo thành công View 3D '{Target3DViewName}'!";
                RequestClose?.Invoke();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi xử lý 3D View: " + ex.Message;
            Log.Write("CreateOrOpenWorking3D error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OpenExistingPlan(LevelItemViewModel? item)
    {
        if (item?.ExistingViewId == null) return;
        var view = _doc.GetElement(item.ExistingViewId) as Autodesk.Revit.DB.View;
        if (view != null)
        {
            _pendingActiveView = view;
            RequestClose?.Invoke();
        }
    }
}