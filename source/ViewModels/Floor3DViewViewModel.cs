using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class Floor3DLevelItem : ViewModelBase
{
    private bool _isSelected = true;
    private string _levelCode = string.Empty;

    public Level Level { get; }
    public string Name => Level.Name;
    public double ElevationFeet => Level.Elevation;
    public double ElevationMm => Math.Round(Level.Elevation * 304.8);
    public string ElevationDisplay => $"{ElevationMm:+#,##0;-#,##0;0} mm";
    public bool IsTopLevel { get; set; }
    public Level? NextLevel { get; set; }

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

    public string LevelCode
    {
        get => _levelCode;
        set => SetProperty(ref _levelCode, value);
    }

    public event Action? SelectionChanged;

    public Floor3DLevelItem(Level level, bool isTopLevel, Level? nextLevel)
    {
        Level = level ?? throw new ArgumentNullException(nameof(level));
        IsTopLevel = isTopLevel;
        NextLevel = nextLevel;
        LevelCode = AutoDetectLevelCode(level.Name);
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

public sealed class Floor3DViewViewModel : ViewModelBase
{
    private readonly UIDocument _uiDoc;
    private readonly Document _doc;

    private string _selectedUser = "User1";
    private string _statusMessage = "Sẵn sàng tạo View 3D theo tầng.";
    private bool _isBusy;

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
                OnPropertyChanged(nameof(PreviewViewNames));
            }
        }
    }

    public ObservableCollection<Floor3DLevelItem> Levels { get; } = new();

    public int SelectedLevelCount => Levels.Count(x => x.IsSelected);
    public bool CanExecute => SelectedLevelCount > 0 && !IsBusy;

    public string PreviewViewNames
    {
        get
        {
            var first = Levels.FirstOrDefault(x => x.IsSelected);
            return first != null ? $"{SelectedUser}_{first.LevelCode}_3D ..." : $"{SelectedUser}_L01_3D";
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanExecute));
                ExecuteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand SelectAllCommand { get; }
    public RelayCommand DeselectAllCommand { get; }
    public RelayCommand ExecuteCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public Floor3DViewViewModel(UIDocument uiDoc)
    {
        _uiDoc = uiDoc ?? throw new ArgumentNullException(nameof(uiDoc));
        _doc = uiDoc.Document;

        SelectAllCommand = new RelayCommand(_ => SetAllLevels(true));
        DeselectAllCommand = new RelayCommand(_ => SetAllLevels(false));
        ExecuteCommand = new RelayCommand(_ => ExecuteCreate3DViews(), _ => CanExecute);
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

        LoadLevels();
    }

    private void SetAllLevels(bool selected)
    {
        foreach (var lvl in Levels)
        {
            lvl.IsSelected = selected;
        }
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        OnPropertyChanged(nameof(SelectedLevelCount));
        OnPropertyChanged(nameof(CanExecute));
        OnPropertyChanged(nameof(PreviewViewNames));
        ExecuteCommand.RaiseCanExecuteChanged();
    }

    private void LoadLevels()
    {
        Levels.Clear();

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

            var item = new Floor3DLevelItem(lvl, isTop, next);
            item.SelectionChanged += UpdateSelectionState;
            Levels.Add(item);
        }

        UpdateSelectionState();
    }

    private void ExecuteCreate3DViews()
    {
        var selectedLevels = Levels.Where(x => x.IsSelected).ToList();
        if (!selectedLevels.Any()) return;

        IsBusy = true;
        StatusMessage = "Đang tạo các View 3D theo tầng...";

        try
        {
            // 1. Tìm ViewFamilyType cho 3D View
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

            // 2. Xác định phạm vi mặt bằng (X, Y) từ Grids hoặc Model
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

            double margin = 2000.0 / 304.8; // 2000 mm bao ra ngoài Grid
            if (minX >= maxX || minY >= maxY)
            {
                // Fallback nếu không có Grid
                minX = -100.0; maxX = 100.0;
                minY = -100.0; maxY = 100.0;
            }
            else
            {
                minX -= margin;
                maxX += margin;
                minY -= margin;
                maxY += margin;
            }

            // Lấy tất cả view 3D hiện có trong dự án
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
                    string targetName = $"{SelectedUser}_{lvlItem.LevelCode}_3D";

                    // Chiều cao tầng:
                    // Z min = cao độ sàn tầng hiện hành
                    // Z max = cao độ sàn tầng trên kế tiếp (hoặc +3500mm nếu là tầng mái)
                    double zMin = lvlItem.ElevationFeet;
                    double zMax = lvlItem.NextLevel != null
                        ? lvlItem.NextLevel.Elevation
                        : lvlItem.ElevationFeet + (3500.0 / 304.8);

                    // Nếu có mặt bằng của tầng này có CropBox riêng, ưu tiên lấy X,Y theo CropBox đó
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

                    // Xây dựng Section Box cho View 3D
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

                    // Bật hộp cắt Section Box và gán BoundingBox
                    view3d.IsSectionBoxActive = true;
                    view3d.SetSectionBox(sectionBox);
                }

                trans.Commit();
            }

                        StatusMessage = $"Hoàn tất: Đã tạo mới {createdCount} View, cập nhật {updatedCount} View 3D theo tầng.";

            string dialogMsg = $"Đã hoàn thành tạo View 3D theo tầng:\n\n"
                + $"• Người dùng: {SelectedUser}\n"
                + $"• Số View 3D tạo mới: {createdCount}\n"
                + $"• Số View 3D cập nhật: {updatedCount}\n\n"
                + "Toàn bộ các View 3D đã được bật Section Box và đóng khung chuẩn theo thông số mặt bằng.";

            TaskDialog.Show("Tạo View 3D Theo Tầng", dialogMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khi tạo View 3D: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}