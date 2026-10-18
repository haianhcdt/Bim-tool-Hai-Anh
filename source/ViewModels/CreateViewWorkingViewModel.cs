using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class CreateViewWorkingViewModel : ViewModelBase
{
    private readonly UIDocument _uiDoc;
    private readonly Document _doc;

    private string _selectedUser = "User1";
    private string _statusMessage = "Sẵn sàng khởi tạo View làm việc.";
    private bool _isBusy;

    public string DocumentTitle => string.IsNullOrWhiteSpace(_doc.Title) ? "Revit Project" : _doc.Title;

    public ObservableCollection<string> Users { get; } = new()
    {
        "User1",
        "User2",
        "User3",
        "User4",
        "User5"
    };

    public string SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                OnPropertyChanged(nameof(UserFolder1));
                OnPropertyChanged(nameof(UserPrefix));
                OnPropertyChanged(nameof(TargetVerName));
                OnPropertyChanged(nameof(TargetHozName));
                OnPropertyChanged(nameof(PlanPreviewName));
                OnPropertyChanged(nameof(Floor3DPreviewName));
            }
        }
    }

    /// <summary>
    /// Giá trị điền vào MN_FOLDER 1 (quy chuẩn "User 1" .. "User 5")
    /// </summary>
    public string UserFolder1
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SelectedUser)) return "User 1";
            var match = Regex.Match(SelectedUser.Trim(), @"^User\s*(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"User {match.Groups[1].Value}";
            }
            return SelectedUser.Trim();
        }
    }

    /// <summary>
    /// Tiền tố tên View chuẩn (không dấu cách: User1 .. User5)
    /// </summary>
    public string UserPrefix
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SelectedUser)) return "User1";
            var match = Regex.Match(SelectedUser.Trim(), @"^User\s*(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"User{match.Groups[1].Value}";
            }
            return SelectedUser.Trim().Replace(" ", "");
        }
    }

    public string TargetVerName => $"{UserPrefix}_Ver";
    public string TargetHozName => $"{UserPrefix}_Hoz";
    public string PlanPreviewName => $"{UserPrefix}_L01_CB";
    public string Floor3DPreviewName => $"{UserPrefix}_L01_3D";

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                CreateFloorPlansCommand.RaiseCanExecuteChanged();
                CreateSectionsCommand.RaiseCanExecuteChanged();
                Create3DViewsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // =========================================================================
    // 1. MẶT BẰNG MEP (FLOOR PLANS)
    // =========================================================================
    public ObservableCollection<BatchFloorPlanLevelItem> PlanLevels { get; } = new();
    public ObservableCollection<BatchFloorPlanSystemItem> PlanSystems { get; } = new();

    public int SelectedPlanLevelCount => PlanLevels.Count(x => x.IsSelected);
    public int SelectedPlanSystemCount => PlanSystems.Count(x => x.IsSelected);
    public int EstimatedPlanCount => SelectedPlanLevelCount * SelectedPlanSystemCount;

    public bool CanCreatePlans => EstimatedPlanCount > 0 && !IsBusy;
    public string CreatePlansButtonText => EstimatedPlanCount > 0
        ? $"🚀 TẠO MẶT BẰNG MEP ({EstimatedPlanCount} VIEWS)"
        : "🚀 TẠO MẶT BẰNG MEP";

    public RelayCommand SelectAllPlanLevelsCommand { get; }
    public RelayCommand UnselectAllPlanLevelsCommand { get; }
    public RelayCommand SelectAllPlanSystemsCommand { get; }
    public RelayCommand UnselectAllPlanSystemsCommand { get; }
    public RelayCommand CreateFloorPlansCommand { get; }

    // =========================================================================
    // 2. MẶT CẮT MEP (SECTIONS)
    // =========================================================================
    private bool _sectionCreateVer = true;
    private bool _sectionCreateHoz = true;

    public bool SectionCreateVer
    {
        get => _sectionCreateVer;
        set
        {
            if (SetProperty(ref _sectionCreateVer, value))
            {
                OnPropertyChanged(nameof(CanCreateSections));
                CreateSectionsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool SectionCreateHoz
    {
        get => _sectionCreateHoz;
        set
        {
            if (SetProperty(ref _sectionCreateHoz, value))
            {
                OnPropertyChanged(nameof(CanCreateSections));
                CreateSectionsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanCreateSections => (SectionCreateVer || SectionCreateHoz) && !IsBusy;
    public RelayCommand CreateSectionsCommand { get; }

    // =========================================================================
    // 3. 3D TẦNG MEP (FLOOR 3D VIEWS)
    // =========================================================================
    public ObservableCollection<Floor3DLevelItem> Floor3DLevels { get; } = new();

    public int Selected3DLevelCount => Floor3DLevels.Count(x => x.IsSelected);
    public bool CanCreate3DViews => Selected3DLevelCount > 0 && !IsBusy;
    public string Create3DViewsButtonText => Selected3DLevelCount > 0
        ? $"🚀 TẠO VIEW 3D THEO TẦNG ({Selected3DLevelCount} TẦNG)"
        : "🚀 TẠO VIEW 3D THEO TẦNG";

    public RelayCommand SelectAll3DLevelsCommand { get; }
    public RelayCommand UnselectAll3DLevelsCommand { get; }
    public RelayCommand Create3DViewsCommand { get; }

    public RelayCommand CloseCommand { get; }
    public event Action? RequestClose;

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================
    public CreateViewWorkingViewModel(UIDocument uiDoc)
    {
        _uiDoc = uiDoc ?? throw new ArgumentNullException(nameof(uiDoc));
        _doc = uiDoc.Document;

        // Commands Mặt Bằng
        SelectAllPlanLevelsCommand = new RelayCommand(_ => SetAllPlanLevels(true));
        UnselectAllPlanLevelsCommand = new RelayCommand(_ => SetAllPlanLevels(false));
        SelectAllPlanSystemsCommand = new RelayCommand(_ => SetAllPlanSystems(true));
        UnselectAllPlanSystemsCommand = new RelayCommand(_ => SetAllPlanSystems(false));
        CreateFloorPlansCommand = new RelayCommand(_ => ExecuteCreateFloorPlans(), _ => CanCreatePlans);

        // Commands Mặt Cắt
        CreateSectionsCommand = new RelayCommand(_ => ExecuteCreateSections(), _ => CanCreateSections);

        // Commands 3D Tầng
        SelectAll3DLevelsCommand = new RelayCommand(_ => SetAll3DLevels(true));
        UnselectAll3DLevelsCommand = new RelayCommand(_ => SetAll3DLevels(false));
        Create3DViewsCommand = new RelayCommand(_ => ExecuteCreate3DViews(), _ => CanCreate3DViews);

        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

        InitializePlanSystems();
        InitializeLevels();
    }

    private void InitializePlanSystems()
    {
        PlanSystems.Clear();
        PlanSystems.Add(new BatchFloorPlanSystemItem("CB", "Combine System", "Tất cả hệ thống cơ điện"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("EL", "Lighting System", "Hệ thống chiếu sáng"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("EG", "GPO System", "Hệ thống ổ cắm điện"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("EP", "Power Supply System", "Hệ thống cấp nguồn"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("EU", "Utility Electrical System", "Hệ thống điện phụ trợ"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("FA", "Fire Alarm System", "Hệ thống báo cháy & BMS"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("ET", "Telephone-Data System", "Mạng, thoại & an ninh điện tử"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("FI", "Water Fire Fighting System", "Hệ thống chữa cháy bằng nước"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("HS", "Water Supply System", "Hệ thống cấp nước sinh hoạt"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("HD", "Drainage System", "Hệ thống thoát nước"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("MV", "Ducting System", "Hệ thống ống gió"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("MP", "Chiller, Gas & Drain Water System", "Ống nước lạnh, gas & nước ngưng"));
        PlanSystems.Add(new BatchFloorPlanSystemItem("ME", "Pressurization & Smoke Extraction", "Hệ thống tạo áp và hút khói"));

        foreach (var sys in PlanSystems)
        {
            sys.PropertyChanged += (_, __) => UpdatePlanState();
        }
    }

    private void InitializeLevels()
    {
        PlanLevels.Clear();
        Floor3DLevels.Clear();

        var levels = new FilteredElementCollector(_doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        for (int i = 0; i < levels.Count; i++)
        {
            var lvl = levels[i];
            bool isTop = (i == levels.Count - 1);
            Level? next = isTop ? null : levels[i + 1];

            var planItem = new BatchFloorPlanLevelItem(lvl, isTop);
            planItem.PropertyChanged += (_, __) => UpdatePlanState();
            PlanLevels.Add(planItem);

            var item3d = new Floor3DLevelItem(lvl, isTop, next);
            item3d.SelectionChanged += Update3DState;
            Floor3DLevels.Add(item3d);
        }

        UpdatePlanState();
        Update3DState();
    }

    private void SetAllPlanLevels(bool sel)
    {
        foreach (var l in PlanLevels) l.IsSelected = sel;
        UpdatePlanState();
    }

    private void SetAllPlanSystems(bool sel)
    {
        foreach (var s in PlanSystems) s.IsSelected = sel;
        UpdatePlanState();
    }

    private void UpdatePlanState()
    {
        OnPropertyChanged(nameof(SelectedPlanLevelCount));
        OnPropertyChanged(nameof(SelectedPlanSystemCount));
        OnPropertyChanged(nameof(EstimatedPlanCount));
        OnPropertyChanged(nameof(CanCreatePlans));
        OnPropertyChanged(nameof(CreatePlansButtonText));
        CreateFloorPlansCommand.RaiseCanExecuteChanged();
    }

    private void SetAll3DLevels(bool sel)
    {
        foreach (var l in Floor3DLevels) l.IsSelected = sel;
        Update3DState();
    }

    private void Update3DState()
    {
        OnPropertyChanged(nameof(Selected3DLevelCount));
        OnPropertyChanged(nameof(CanCreate3DViews));
        OnPropertyChanged(nameof(Create3DViewsButtonText));
        Create3DViewsCommand.RaiseCanExecuteChanged();
    }

    // =========================================================================
    // EXECUTE 1: TẠO MẶT BẰNG MEP
    // =========================================================================
    private void ExecuteCreateFloorPlans()
    {
        if (!CanCreatePlans) return;
        IsBusy = true;
        StatusMessage = "Đang tạo hàng loạt View mặt bằng MEP...";

        try
        {
            var selectedLevels = PlanLevels.Where(x => x.IsSelected).ToList();
            var selectedSystems = PlanSystems.Where(x => x.IsSelected).ToList();

            var floorPlanType = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.FloorPlan);

            if (floorPlanType == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy FloorPlan ViewFamilyType trong mô hình.";
                IsBusy = false;
                return;
            }

            var existingViews = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate)
                .ToDictionary(v => v.Name, v => v, StringComparer.OrdinalIgnoreCase);

            int createdCount = 0;
            int updatedCount = 0;

            using (var trans = new Transaction(_doc, "Tạo Mặt Bằng MEP Tự Động"))
            {
                trans.Start();

                foreach (var lvlItem in selectedLevels)
                {
                    var level = lvlItem.Level;
                    var lvlCode = string.IsNullOrWhiteSpace(lvlItem.LevelCode)
                        ? BatchFloorPlanLevelItem.AutoDetectLevelCode(level.Name)
                        : lvlItem.LevelCode.Trim();

                    foreach (var sysItem in selectedSystems)
                    {
                        var sysCode = string.IsNullOrWhiteSpace(sysItem.Code) ? "SYS" : sysItem.Code.Trim();
                        var targetViewName = $"{UserPrefix}_{lvlCode}_{sysCode}";

                        try
                        {
                            ViewPlan targetView;
                            if (existingViews.TryGetValue(targetViewName, out var existing))
                            {
                                targetView = existing;
                                updatedCount++;
                            }
                            else
                            {
                                targetView = ViewPlan.Create(_doc, floorPlanType.Id, level.Id);
                                targetView.Name = targetViewName;
                                existingViews[targetViewName] = targetView;
                                createdCount++;
                            }

                            ApplyViewRange(targetView, lvlItem.IsTopLevel);

                            // Gán thư mục chuẩn theo thứ tự:
                            // 1. MN_FOLDER: WORKING
                            // 2. MN_FOLDER 1: User 1 .. User 5
                            // 3. MN_FOLDER 2: L01 .. LRO
                            SetViewFolderParameters(targetView, "WORKING", UserFolder1, lvlCode);
                        }
                        catch (Exception ex)
                        {
                            Log.Write($"Lỗi khi tạo view '{targetViewName}': {ex.Message}");
                        }
                    }
                }

                trans.Commit();
            }

            string resultMsg = $"Hoàn tất tạo mặt bằng MEP:\n\n"
                + $"• Tạo mới thành công: {createdCount} View\n"
                + $"• Cập nhật thành công: {updatedCount} View\n"
                + $"• Người dùng: {UserFolder1}\n"
                + $"• Cấu trúc thư mục (Project Browser):\n"
                + $"   - MN_FOLDER: WORKING\n"
                + $"   - MN_FOLDER 1: {UserFolder1}\n"
                + $"   - MN_FOLDER 2: {string.Join(", ", selectedLevels.Select(l => l.LevelCode).Distinct())}\n"
                + $"• Chuẩn View Range: Top Level Above (0mm), Cut 1200mm, Bottom 0mm, Depth 0mm";

            StatusMessage = $"Đã tạo mới {createdCount} View, cập nhật {updatedCount} View mặt bằng.";
            TaskDialog.Show("Tạo View Mặt Bằng MEP", resultMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
            TaskDialog.Show("Lỗi Tạo Mặt Bằng", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void ApplyViewRange(ViewPlan plan, bool isTopLevel)
    {
        try
        {
            var planRange = plan.GetViewRange();
            double cutOffset = 1200.0 / 304.8;
            double zeroOffset = 0.0;

            if (isTopLevel)
            {
                double topOffset = 3000.0 / 304.8;
                planRange.SetLevelId(PlanViewPlane.TopClipPlane, PlanViewRange.Current);
                planRange.SetOffset(PlanViewPlane.TopClipPlane, topOffset);
            }
            else
            {
                planRange.SetLevelId(PlanViewPlane.TopClipPlane, PlanViewRange.LevelAbove);
                planRange.SetOffset(PlanViewPlane.TopClipPlane, zeroOffset);
            }

            planRange.SetLevelId(PlanViewPlane.CutPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.CutPlane, cutOffset);

            planRange.SetLevelId(PlanViewPlane.BottomClipPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.BottomClipPlane, zeroOffset);

            planRange.SetLevelId(PlanViewPlane.ViewDepthPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.ViewDepthPlane, zeroOffset);

            plan.SetViewRange(planRange);
        }
        catch (Exception ex)
        {
            Log.Write($"ApplyViewRange Error on '{plan.Name}': {ex.Message}");
        }
    }

    // =========================================================================
    // EXECUTE 2: TẠO MẶT CẮT MEP
    // =========================================================================
    private void ExecuteCreateSections()
    {
        if (!CanCreateSections) return;
        IsBusy = true;
        StatusMessage = "Đang tính toán tâm công trình và tạo mặt cắt...";

        try
        {
            var sectionVft = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Section);

            if (sectionVft == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy loại mặt cắt (Section ViewFamilyType) trong dự án.";
                IsBusy = false;
                return;
            }

            var levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            double zMin = levels.Any() ? levels.First().Elevation - (1000.0 / 304.8) : -10.0;
            double zMax = levels.Any() ? levels.Last().Elevation + (3000.0 / 304.8) : 100.0;
            double sectionHeight = zMax - zMin;

            var allGrids = new FilteredElementCollector(_doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            double maxGridLength = 0;
            double minGridX = double.MaxValue, maxGridX = double.MinValue;
            double minGridY = double.MaxValue, maxGridY = double.MinValue;

            foreach (var grid in allGrids)
            {
                if (grid.Curve != null)
                {
                    double len = grid.Curve.Length;
                    if (len > maxGridLength) maxGridLength = len;

                    XYZ p0 = grid.Curve.GetEndPoint(0);
                    XYZ p1 = grid.Curve.GetEndPoint(1);

                    minGridX = Math.Min(minGridX, Math.Min(p0.X, p1.X));
                    maxGridX = Math.Max(maxGridX, Math.Max(p0.X, p1.X));
                    minGridY = Math.Min(minGridY, Math.Min(p0.Y, p1.Y));
                    maxGridY = Math.Max(maxGridY, Math.Max(p0.Y, p1.Y));
                }
            }

            XYZ centerWorld;
            double lengthHoz;
            double lengthVer;
            double extraLength = 3000.0 / 304.8; // Dài hơn Grid dài nhất + 3000 mm

            if (allGrids.Any() && minGridX < maxGridX && minGridY < maxGridY)
            {
                double centerX = (minGridX + maxGridX) * 0.5;
                double centerY = (minGridY + maxGridY) * 0.5;
                centerWorld = new XYZ(centerX, centerY, zMin);

                double dimX = maxGridX - minGridX;
                double dimY = maxGridY - minGridY;

                double baseMax = Math.Max(maxGridLength, Math.Max(dimX, dimY));
                lengthHoz = baseMax + extraLength;
                lengthVer = baseMax + extraLength;
            }
            else
            {
                centerWorld = new XYZ(0, 0, zMin);
                lengthHoz = 200.0;
                lengthVer = 200.0;
            }

            // Thuật toán né Grid
            double safeTolerance = 300.0 / 304.8;
            double shiftStep = 500.0 / 304.8;

            XYZ centerHoz = new XYZ(centerWorld.X, centerWorld.Y, zMin);
            XYZ centerVer = new XYZ(centerWorld.X, centerWorld.Y, zMin);

            foreach (var grid in allGrids)
            {
                if (grid.Curve is Line line)
                {
                    XYZ p0 = line.GetEndPoint(0);
                    XYZ p1 = line.GetEndPoint(1);

                    if (Math.Abs(p0.X - p1.X) < 0.05)
                    {
                        double gridX = (p0.X + p1.X) * 0.5;
                        if (Math.Abs(centerVer.X - gridX) <= safeTolerance)
                        {
                            centerVer = new XYZ(centerVer.X + shiftStep, centerVer.Y, centerVer.Z);
                        }
                    }

                    if (Math.Abs(p0.Y - p1.Y) < 0.05)
                    {
                        double gridY = (p0.Y + p1.Y) * 0.5;
                        if (Math.Abs(centerHoz.Y - gridY) <= safeTolerance)
                        {
                            centerHoz = new XYZ(centerHoz.X, centerHoz.Y + shiftStep, centerHoz.Z);
                        }
                    }
                }
            }

            double farClipOffset = 1000.0 / 304.8;
            var results = new List<string>();

            using (var trans = new Transaction(_doc, "Tạo Mặt Cắt MEP Làm Việc"))
            {
                trans.Start();

                if (SectionCreateVer)
                {
                    string verName = TargetVerName;
                    var boxVer = CreateSectionBox(centerVer, lengthVer, sectionHeight, farClipOffset, isVertical: true);
                    CreateOrUpdateSection(verName, sectionVft.Id, boxVer, farClipOffset, results);
                }

                if (SectionCreateHoz)
                {
                    string hozName = TargetHozName;
                    var boxHoz = CreateSectionBox(centerHoz, lengthHoz, sectionHeight, farClipOffset, isVertical: false);
                    CreateOrUpdateSection(hozName, sectionVft.Id, boxHoz, farClipOffset, results);
                }

                trans.Commit();
            }

            StatusMessage = "Thành công: " + string.Join(" | ", results);

            string dialogMsg = "Đã hoàn thành tạo mặt cắt làm việc:\n\n"
                + string.Join("\n", results)
                + "\n\n• Cấu trúc thư mục (Project Browser):"
                + $"\n   - MN_FOLDER: WORKING"
                + $"\n   - MN_FOLDER 1: {UserFolder1}"
                + "\n   - MN_FOLDER 2: COM"
                + "\n• Chiều sâu nhìn (Far Clip): 1000 mm"
                + "\n• Chiều dài: Dài hơn Grid dài nhất (+3000 mm)"
                + "\n• Chiều cao: Kéo qua toàn bộ các tầng trong dự án"
                + "\n• Vị trí: Căn giữa công trình (Tự động né trục Grid)";

            TaskDialog.Show("Tạo Mặt Cắt MEP", dialogMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khi tạo mặt cắt: {ex.Message}";
            TaskDialog.Show("Lỗi Tạo Mặt Cắt", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private BoundingBoxXYZ CreateSectionBox(XYZ origin, double length, double height, double farClipOffset, bool isVertical)
    {
        var box = new BoundingBoxXYZ();
        var transform = Transform.Identity;
        transform.Origin = origin;

        if (isVertical)
        {
            transform.BasisX = new XYZ(0, 1, 0);
            transform.BasisY = new XYZ(0, 0, 1);
            transform.BasisZ = transform.BasisX.CrossProduct(transform.BasisY);
        }
        else
        {
            transform.BasisX = new XYZ(1, 0, 0);
            transform.BasisY = new XYZ(0, 0, 1);
            transform.BasisZ = transform.BasisX.CrossProduct(transform.BasisY);
        }

        box.Transform = transform;
        box.Min = new XYZ(-length * 0.5, 0, 0);
        box.Max = new XYZ(length * 0.5, height, farClipOffset);

        return box;
    }

    private void CreateOrUpdateSection(string viewName, ElementId vftId, BoundingBoxXYZ box, double farClipOffset, List<string> results)
    {
        var existingSection = new FilteredElementCollector(_doc)
            .OfClass(typeof(ViewSection))
            .Cast<ViewSection>()
            .FirstOrDefault(v => string.Equals(v.Name, viewName, StringComparison.OrdinalIgnoreCase));

        ViewSection section;
        if (existingSection != null)
        {
            existingSection.CropBox = box;
            SetFarClipOffset(existingSection, farClipOffset);
            section = existingSection;
            results.Add($"• Cập nhật '{viewName}'");
        }
        else
        {
            var newSection = ViewSection.CreateSection(_doc, vftId, box);
            newSection.Name = viewName;
            SetFarClipOffset(newSection, farClipOffset);
            section = newSection;
            results.Add($"• Tạo mới '{viewName}'");
        }

        // Gán thư mục chuẩn theo thứ tự:
        // 1. MN_FOLDER: WORKING
        // 2. MN_FOLDER 1: User 1 .. User 5
        // 3. MN_FOLDER 2: COM (Mặt cắt chung)
        SetViewFolderParameters(section, "WORKING", UserFolder1, mnFolder2: "COM");
    }

    private static void SetFarClipOffset(ViewSection section, double offsetFeet)
    {
        var param = section.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
        if (param != null && !param.IsReadOnly) param.Set(offsetFeet);

        var clipActiveParam = section.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
        if (clipActiveParam != null && !clipActiveParam.IsReadOnly) clipActiveParam.Set(1);
    }

    // =========================================================================
    // EXECUTE 3: TẠO VIEW 3D THEO TẦNG
    // =========================================================================
    private void ExecuteCreate3DViews()
    {
        var selectedLevels = Floor3DLevels.Where(x => x.IsSelected).ToList();
        if (!selectedLevels.Any()) return;

        IsBusy = true;
        StatusMessage = "Đang tạo các View 3D theo tầng...";

        try
        {
            var view3dType = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

            if (view3dType == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy loại View 3D trong dự án.";
                IsBusy = false;
                return;
            }

            var allGrids = new FilteredElementCollector(_doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var grid in allGrids)
            {
                if (grid.Curve != null)
                {
                    XYZ p0 = grid.Curve.GetEndPoint(0);
                    XYZ p1 = grid.Curve.GetEndPoint(1);
                    minX = Math.Min(minX, Math.Min(p0.X, p1.X));
                    maxX = Math.Max(maxX, Math.Max(p0.X, p1.X));
                    minY = Math.Min(minY, Math.Min(p0.Y, p1.Y));
                    maxY = Math.Max(maxY, Math.Max(p0.Y, p1.Y));
                }
            }

            double margin = 2000.0 / 304.8; // 2000 mm
            if (minX >= maxX || minY >= maxY)
            {
                minX = -100.0; maxX = 100.0;
                minY = -100.0; maxY = 100.0;
            }
            else
            {
                minX -= margin; maxX += margin;
                minY -= margin; maxY += margin;
            }

            var existing3DViews = new FilteredElementCollector(_doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(v => !v.IsTemplate)
                .ToDictionary(v => v.Name, v => v, StringComparer.OrdinalIgnoreCase);

            int createdCount = 0;
            int updatedCount = 0;

            using (var trans = new Transaction(_doc, "Tạo View 3D MEP Theo Tầng"))
            {
                trans.Start();

                foreach (var lvlItem in selectedLevels)
                {
                    string targetName = $"{UserPrefix}_{lvlItem.LevelCode}_3D";

                    double zMin = lvlItem.ElevationFeet;
                    double zMax = lvlItem.NextLevel != null
                        ? lvlItem.NextLevel.Elevation
                        : lvlItem.ElevationFeet + (3500.0 / 304.8);

                    double curMinX = minX, curMaxX = maxX, curMinY = minY, curMaxY = maxY;
                    var matchingPlan = new FilteredElementCollector(_doc)
                        .OfClass(typeof(ViewPlan))
                        .Cast<ViewPlan>()
                        .FirstOrDefault(vp => !vp.IsTemplate && vp.GenLevel != null && vp.GenLevel.Id == lvlItem.Level.Id && vp.CropBoxActive && vp.CropBox != null);

                    if (matchingPlan != null && matchingPlan.CropBox != null)
                    {
                        var cb = matchingPlan.CropBox;
                        XYZ p0 = cb.Transform.OfPoint(cb.Min);
                        XYZ p1 = cb.Transform.OfPoint(cb.Max);
                        curMinX = Math.Min(p0.X, p1.X) - margin;
                        curMaxX = Math.Max(p0.X, p1.X) + margin;
                        curMinY = Math.Min(p0.Y, p1.Y) - margin;
                        curMaxY = Math.Max(p0.Y, p1.Y) + margin;
                    }

                    var sectionBox = new BoundingBoxXYZ
                    {
                        Transform = Transform.Identity,
                        Min = new XYZ(curMinX, curMinY, zMin),
                        Max = new XYZ(curMaxX, curMaxY, zMax)
                    };

                    View3D view3d;
                    if (existing3DViews.TryGetValue(targetName, out var existing))
                    {
                        view3d = existing;
                        updatedCount++;
                    }
                    else
                    {
                        view3d = View3D.CreateIsometric(_doc, view3dType.Id);
                        view3d.Name = targetName;
                        existing3DViews[targetName] = view3d;
                        createdCount++;
                    }

                    view3d.IsSectionBoxActive = true;
                    view3d.SetSectionBox(sectionBox);

                    // Gán thư mục chuẩn theo thứ tự:
                    // 1. MN_FOLDER: WORKING
                    // 2. MN_FOLDER 1: User 1 .. User 5
                    // 3. MN_FOLDER 2: L01 .. LRO
                    SetViewFolderParameters(view3d, "WORKING", UserFolder1, lvlItem.LevelCode);
                }

                trans.Commit();
            }

            StatusMessage = $"Hoàn tất: Đã tạo mới {createdCount} View, cập nhật {updatedCount} View 3D theo tầng.";

            string dialogMsg = $"Đã hoàn thành tạo View 3D theo tầng:\n\n"
                + $"• Người dùng: {UserFolder1}\n"
                + $"• Cấu trúc thư mục (Project Browser):\n"
                + $"   - MN_FOLDER: WORKING\n"
                + $"   - MN_FOLDER 1: {UserFolder1}\n"
                + $"   - MN_FOLDER 2: {string.Join(", ", selectedLevels.Select(l => l.LevelCode))}\n"
                + $"• Số View 3D tạo mới: {createdCount}\n"
                + $"• Số View 3D cập nhật: {updatedCount}\n\n"
                + "Toàn bộ các View 3D đã được bật Section Box và đóng khung chuẩn theo thông số mặt bằng.";

            TaskDialog.Show("Tạo View 3D Theo Tầng", dialogMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khi tạo View 3D: {ex.Message}";
            TaskDialog.Show("Lỗi Tạo View 3D", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // =========================================================================
    // HELPER: GÁN THƯ MỤC PROJECT BROWSER (MN_FOLDER, MN_FOLDER 1, MN_FOLDER 2)
    // =========================================================================
    /// <summary>
    /// Gán thư mục phân cấp cây Project Browser theo quy định bắt buộc:
    /// Thứ tự:
    /// 1. MN_FOLDER: WORKING
    /// 2. MN_FOLDER 1: User 1 .. User 5
    /// 3. MN_FOLDER 2: L01 .. LRO (Nếu mnFolder2 null/rỗng thì không điền giá trị)
    /// </summary>
    private static void SetViewFolderParameters(View view, string mnFolder, string mnFolder1, string? mnFolder2)
    {
        if (view == null) return;

        // Thứ tự 1: MN_FOLDER
        SetStringParam(view, "MN_FOLDER", mnFolder);

        // Thứ tự 2: MN_FOLDER 1 (và dự phòng MN_SUB-FOLDER 1 nếu template dự án dùng tên này)
        SetStringParam(view, "MN_FOLDER 1", mnFolder1);
        SetStringParam(view, "MN_SUB-FOLDER 1", mnFolder1);

        // Thứ tự 3: MN_FOLDER 2 (và dự phòng MN_SUB-FOLDER 2 nếu template dự án dùng tên này)
        // Nếu mnFolder2 có giá trị thì điền; nếu null hoặc rỗng thì không cần điền giá trị.
        if (!string.IsNullOrWhiteSpace(mnFolder2))
        {
            SetStringParam(view, "MN_FOLDER 2", mnFolder2);
            SetStringParam(view, "MN_SUB-FOLDER 2", mnFolder2);
        }
    }

    private static void SetStringParam(View view, string paramName, string? value)
    {
        if (view == null || string.IsNullOrEmpty(paramName)) return;

        try
        {
            var pList = view.GetParameters(paramName);
            if (pList != null && pList.Count > 0)
            {
                foreach (var p in pList)
                {
                    if (p != null && !p.IsReadOnly && p.StorageType == StorageType.String)
                    {
                        p.Set(value ?? string.Empty);
                    }
                }
                return;
            }

            var param = view.LookupParameter(paramName);
            if (param != null && !param.IsReadOnly && param.StorageType == StorageType.String)
            {
                param.Set(value ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            Log.Write($"Lỗi khi gán tham số '{paramName}' cho view '{view.Name}': {ex.Message}");
        }
    }
}