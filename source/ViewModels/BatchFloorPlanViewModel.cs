using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class BatchFloorPlanUserOption
{
    public string Code { get; }
    public string DisplayName { get; }

    public BatchFloorPlanUserOption(string code, string name)
    {
        Code = code;
        DisplayName = $"{code} - {name}";
    }
}

public sealed class BatchFloorPlanLevelItem : ViewModelBase
{
    public Level Level { get; }
    public string Name { get; }
    public double ElevationFeet => Level.Elevation;
    public double ElevationMm => Math.Round(Level.Elevation * 304.8);
    public string ElevationDisplay => $"{ElevationMm:+#,##0;-#,##0;0} mm";
    public bool IsTopLevel { get; set; }

    private string _levelCode = string.Empty;
    public string LevelCode
    {
        get => _levelCode;
        set => SetProperty(ref _levelCode, value);
    }

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public BatchFloorPlanLevelItem(Level level, bool isTopLevel)
    {
        Level = level ?? throw new ArgumentNullException(nameof(level));
        Name = level.Name;
        IsTopLevel = isTopLevel;
        LevelCode = AutoDetectLevelCode(Name);
    }

    public static string AutoDetectLevelCode(string levelName)
    {
        if (string.IsNullOrWhiteSpace(levelName)) return "L01";
        var clean = levelName.Trim().ToUpperInvariant();

        // Basement: B1, B01, BASEMENT 1, HẦM 1 -> LB1, LB2...
        var bm = Regex.Match(clean, @"(?:LB|B|BASEMENT|HẦM|HAM)\s*0*(\d+)");
        if (bm.Success) return $"LB{bm.Groups[1].Value}";

        // Tum / Rooftop -> LRF
        if (clean.Contains("ROOFTOP") || clean.Contains("TUM")) return "LRF";

        // Mái / Roof -> LRO
        if (clean.Contains("ROOF") || clean.Contains("MÁI") || clean.Contains("MAI")) return "LRO";

        // Tầng chuẩn: 1FL, FL01, TẦNG 1, LEVEL 1, 10F, v.v. -> L01..L99
        var lm = Regex.Match(clean, @"(?:TẦNG|TANG|LEVEL|FL|L)?\s*0*(\d+)\s*(?:FL|F|TẦNG|TANG)?");
        if (lm.Success && int.TryParse(lm.Groups[1].Value, out int lvlNum) && lvlNum > 0)
        {
            return $"L{lvlNum:D2}";
        }

        return clean.Length <= 4 ? clean : clean.Substring(0, 4);
    }
}

public sealed class BatchFloorPlanSystemItem : ViewModelBase
{
    private string _code = string.Empty;
    public string Code
    {
        get => _code;
        set
        {
            if (SetProperty(ref _code, value))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string EnglishName { get; }
    public string VietnameseName { get; }
    public string DisplayText => $"{Code} — {EnglishName}";
    public string Subtitle => VietnameseName;

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public BatchFloorPlanSystemItem(string code, string english, string vietnamese, bool isSelected = true)
    {
        _code = code;
        EnglishName = english;
        VietnameseName = vietnamese;
        IsSelected = isSelected;
    }
}

public sealed class BatchFloorPlanViewModel : ViewModelBase
{
    private readonly UIDocument _uidoc;
    private readonly Document _doc;

    public string StudentName { get; }
    public string Company { get; }
    public string WindowTitle { get; }
    public string DocumentTitle { get; }

    public ObservableCollection<BatchFloorPlanUserOption> Users { get; } = new();
    private BatchFloorPlanUserOption _selectedUser;
    public BatchFloorPlanUserOption SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                UpdatePreviewCount();
            }
        }
    }

    public ObservableCollection<BatchFloorPlanLevelItem> Levels { get; } = new();
    public ObservableCollection<BatchFloorPlanSystemItem> MepSystems { get; } = new();

    private string _statusMessage = "Sẵn sàng tạo View mặt bằng";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _previewSummary = "Dự kiến: 0 view";
    public string PreviewSummary
    {
        get => _previewSummary;
        set => SetProperty(ref _previewSummary, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ExecuteCreateViewsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand SelectAllLevelsCommand { get; }
    public RelayCommand UnselectAllLevelsCommand { get; }
    public RelayCommand SelectAllSystemsCommand { get; }
    public RelayCommand UnselectAllSystemsCommand { get; }
    public RelayCommand ExecuteCreateViewsCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public BatchFloorPlanViewModel(UIDocument uidoc)
    {
        _uidoc = uidoc ?? throw new ArgumentNullException(nameof(uidoc));
        _doc = uidoc.Document;

        StudentName = StudentBranding.StudentName;
        Company = StudentBranding.Company;
        WindowTitle = "BIM TOOL · Tạo Mặt Bằng MEP Tự Động (Batch Floor Plans)";
        DocumentTitle = string.IsNullOrWhiteSpace(_doc.Title) ? "Mô hình hiện hành" : _doc.Title;

        // 1. Nạp danh sách Users
        Users.Add(new BatchFloorPlanUserOption("User1", "Người dùng 1"));
        Users.Add(new BatchFloorPlanUserOption("User2", "Người dùng 2"));
        Users.Add(new BatchFloorPlanUserOption("User3", "Người dùng 3"));
        Users.Add(new BatchFloorPlanUserOption("User4", "Người dùng 4"));
        Users.Add(new BatchFloorPlanUserOption("User5", "Người dùng 5"));
        _selectedUser = Users[0];

        // 2. Nạp danh sách 13 hệ thống MEP chuẩn
        InitializeMepSystems();

        // 3. Nạp danh sách Level từ active document
        InitializeLevels();

        // 4. Khởi tạo Commands
        SelectAllLevelsCommand = new RelayCommand(_ => SetAllLevels(true));
        UnselectAllLevelsCommand = new RelayCommand(_ => SetAllLevels(false));
        SelectAllSystemsCommand = new RelayCommand(_ => SetAllSystems(true));
        UnselectAllSystemsCommand = new RelayCommand(_ => SetAllSystems(false));
        ExecuteCreateViewsCommand = new RelayCommand(_ => CreateViews(), _ => !IsBusy && HasSelection());
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

        UpdatePreviewCount();
    }

    private void InitializeMepSystems()
    {
        var systems = new[]
        {
            ("CB", "Combine System", "Tất cả hệ thống"),
            ("EL", "Lighting System", "Hệ thống chiếu sáng"),
            ("EG", "GPO System", "Hệ thống ổ cắm điện"),
            ("EP", "Power Supply System", "Hệ thống cấp nguồn"),
            ("EU", "Utility Electrical System", "Hệ thống điện phụ trợ"),
            ("FA", "Fire Alarm System", "Hệ thống quản lý tòa nhà"),
            ("ET", "Telephone-Data & Access Control System", "Hệ thống mạng-điện thoại & an ninh điện tử"),
            ("FI", "Water Fire Fighting System", "Hệ thống chữa cháy bằng nước"),
            ("HS", "Water Supply System", "Hệ thống cấp nước"),
            ("HD", "Drainage System", "Hệ thống thoát nước"),
            ("MV", "Ducting System", "Hệ thống ống gió"),
            ("MP", "Chiller, Gas & Drain Water System", "Hệ thống ống nước lạnh, ống gas & nước ngưng"),
            ("ME", "Pressurization & Smoke Extraction System", "Hệ thống tạo áp và hút khói")
        };

        foreach (var (code, eng, vi) in systems)
        {
            var item = new BatchFloorPlanSystemItem(code, eng, vi, true);
            item.PropertyChanged += (_, _) => UpdatePreviewCount();
            MepSystems.Add(item);
        }
    }

    private void InitializeLevels()
    {
        var rawLevels = new FilteredElementCollector(_doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        if (rawLevels.Count == 0) return;

        var highestElevation = rawLevels.Last().Elevation;

        foreach (var lvl in rawLevels)
        {
            bool isTop = Math.Abs(lvl.Elevation - highestElevation) < 0.001;
            var item = new BatchFloorPlanLevelItem(lvl, isTop);
            item.PropertyChanged += (_, _) => UpdatePreviewCount();
            Levels.Add(item);
        }
    }

    private void SetAllLevels(bool selected)
    {
        foreach (var l in Levels) l.IsSelected = selected;
        UpdatePreviewCount();
    }

    private void SetAllSystems(bool selected)
    {
        foreach (var s in MepSystems) s.IsSelected = selected;
        UpdatePreviewCount();
    }

    private bool HasSelection()
    {
        return Levels.Any(l => l.IsSelected) && MepSystems.Any(s => s.IsSelected);
    }

    private void UpdatePreviewCount()
    {
        int lvlCount = Levels.Count(l => l.IsSelected);
        int sysCount = MepSystems.Count(s => s.IsSelected);
        int total = lvlCount * sysCount;
        PreviewSummary = $"Dự kiến: {total} view mặt bằng ({lvlCount} tầng × {sysCount} hệ MEP) — {SelectedUser?.Code}";
        ExecuteCreateViewsCommand.RaiseCanExecuteChanged();
    }

    private void CreateViews()
    {
        if (!HasSelection())
        {
            StatusMessage = "Vui lòng chọn ít nhất một Tầng và một Hệ thống MEP.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang khởi tạo các View mặt bằng...";

        try
        {
            // Tìm ViewFamilyType cho FloorPlan
            var floorPlanType = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.FloorPlan);

            if (floorPlanType == null)
            {
                StatusMessage = "Lỗi: Không tìm thấy Floor Plan ViewFamilyType trong mô hình.";
                IsBusy = false;
                return;
            }

            // Thu thập ViewPlan hiện có để kiểm tra trùng tên
            var existingViews = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => v.ViewType == ViewType.FloorPlan)
                .ToDictionary(v => v.Name, v => v, StringComparer.OrdinalIgnoreCase);

            var selectedLevels = Levels.Where(l => l.IsSelected).ToList();
            var selectedSystems = MepSystems.Where(s => s.IsSelected).ToList();
            var userCode = SelectedUser?.Code ?? "User1";

            int createdCount = 0;
            int updatedCount = 0;
            var errorList = new List<string>();

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
                        var targetViewName = $"{userCode}_{lvlCode}_{sysCode}";

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

                            // Áp dụng chuẩn View Range
                            ApplyViewRange(targetView, lvlItem.IsTopLevel);
                        }
                        catch (Exception ex)
                        {
                            errorList.Add($"{targetViewName}: {ex.Message}");
                        }
                    }
                }

                trans.Commit();
            }

            string resultMsg = $"Hoàn tất: Tạo mới {createdCount} view, cập nhật {updatedCount} view sẵn có.";
            if (errorList.Count > 0)
            {
                resultMsg += $" Có {errorList.Count} view cảnh báo.";
            }

            StatusMessage = resultMsg;
            TaskDialog.Show("Tạo View Mặt Bằng MEP", resultMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
            Log.Write("BatchFloorPlanViewModel CreateViews Error: " + ex);
            TaskDialog.Show("Lỗi Tạo Mặt Bằng", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cài đặt View Range chuẩn:
    /// - Top: Level Above - Offset: 0 mm (với tầng đỉnh fallback Top: Current - Offset: 3000 mm)
    /// - Cut plane: Associated Level - Offset: 1200 mm
    /// - Bottom: Associated Level - Offset: 0 mm
    /// - View Depth: Associated Level - Offset: 0 mm
    /// </summary>
    private static void ApplyViewRange(ViewPlan viewPlan, bool isTopLevel)
    {
        try
        {
            var planRange = viewPlan.GetViewRange();
            if (planRange == null) return;

            // Chuyển đổi mm sang Feet (1 foot = 304.8 mm)
            const double cutPlaneOffsetFeet = 1200.0 / 304.8;
            const double fallbackTopOffsetFeet = 3000.0 / 304.8;

            // Thứ tự an toàn để không vi phạm quy tắc Top >= Cut >= Bottom >= Depth trong Revit:
            // 1. Hạ Bottom & Depth về 0 trước
            planRange.SetLevelId(PlanViewPlane.ViewDepthPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.ViewDepthPlane, 0.0);

            planRange.SetLevelId(PlanViewPlane.BottomClipPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.BottomClipPlane, 0.0);

            // 2. Thiết lập Top Clip Plane
            if (!isTopLevel)
            {
                try
                {
                    planRange.SetLevelId(PlanViewPlane.TopClipPlane, PlanViewRange.LevelAbove);
                    planRange.SetOffset(PlanViewPlane.TopClipPlane, 0.0);
                }
                catch
                {
                    planRange.SetLevelId(PlanViewPlane.TopClipPlane, PlanViewRange.Current);
                    planRange.SetOffset(PlanViewPlane.TopClipPlane, fallbackTopOffsetFeet);
                }
            }
            else
            {
                planRange.SetLevelId(PlanViewPlane.TopClipPlane, PlanViewRange.Current);
                planRange.SetOffset(PlanViewPlane.TopClipPlane, fallbackTopOffsetFeet);
            }

            // 3. Thiết lập Cut Plane: Current - Offset 1200 mm
            planRange.SetLevelId(PlanViewPlane.CutPlane, PlanViewRange.Current);
            planRange.SetOffset(PlanViewPlane.CutPlane, cutPlaneOffsetFeet);

            // 4. Áp dụng lại cho View
            viewPlan.SetViewRange(planRange);
        }
        catch (Exception ex)
        {
            Log.Write($"Không thể áp dụng View Range cho view {viewPlan.Name}: {ex.Message}");
        }
    }
}
