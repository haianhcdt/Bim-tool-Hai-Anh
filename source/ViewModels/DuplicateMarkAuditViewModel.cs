using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

/// <summary>
/// Chi tiết một cấu kiện bị trùng mã Mark
/// </summary>
public sealed class DuplicateMarkElementDetail
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;
    public string IdString => Id?.ToString() ?? string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string LevelName { get; set; } = string.Empty;
    public string Mark { get; set; } = string.Empty;
}

/// <summary>
/// Đại diện cho 1 nhóm đối tượng có chung mã Mark (Trùng lặp)
/// </summary>
public sealed class DuplicateMarkGroupItem
{
    public int Index { get; set; }
    public string Mark { get; set; } = string.Empty;
    public int DuplicateCount => Elements.Count;
    public string CategoryNames { get; set; } = string.Empty;
    public string FamilyAndTypes { get; set; } = string.Empty;
    public string LevelNames { get; set; } = string.Empty;
    public string ElementIdsString => string.Join(", ", Elements.Select(e => e.IdString));

    public List<ElementId> ElementIdList => Elements.Select(e => e.Id).ToList();
    public List<DuplicateMarkElementDetail> Elements { get; set; } = new();
}

/// <summary>
/// ViewModel kiểm toán và quản lý đối tượng trùng thông số Mark trong mô hình
/// </summary>
public sealed class DuplicateMarkAuditViewModel : ViewModelBase
{
    private readonly UIDocument _uidoc;
    private readonly Document _doc;

    private int _totalScannedCount;
    private int _totalWithMarkCount;
    private int _duplicateGroupsCount;
    private int _totalDuplicateElementsCount;
    private bool _isClean;
    private string _statusMessage = "Đang quét mô hình...";
    private bool _isBusy;
    private DuplicateMarkGroupItem? _selectedGroup;

    public DuplicateMarkAuditViewModel(UIDocument uidoc)
    {
        _uidoc = uidoc ?? throw new ArgumentNullException(nameof(uidoc));
        _doc = uidoc.Document;

        StudentName = StudentBranding.StudentName;
        Company = StudentBranding.Company;
        DocumentTitle = string.IsNullOrWhiteSpace(_doc.Title) ? "Mô hình hiện hành" : _doc.Title;

        // Khởi tạo lệnh
        RefreshCommand = new RelayCommand(_ => ScanModel(), _ => !IsBusy);
        SelectInRevitCommand = new RelayCommand(_ => SelectInRevit(), _ => SelectedGroup != null);
        AutoFixCommand = new RelayCommand(_ => AutoFixAllDuplicates(), _ => CanAutoFix);
        ExportReportCommand = new RelayCommand(_ => ExportReport(), _ => !IsBusy);
        CloseCommand = new RelayCommand(_ => Close());

        // Quét ngay khi khởi tạo
        ScanModel();
    }

    // --- Branding & Document info ---
    public string StudentName { get; }
    public string Company { get; }
    public string DocumentTitle { get; }

    public ObservableCollection<DuplicateMarkGroupItem> DuplicateGroups { get; } = new();

    public int TotalScannedCount
    {
        get => _totalScannedCount;
        set => SetProperty(ref _totalScannedCount, value);
    }

    public int TotalWithMarkCount
    {
        get => _totalWithMarkCount;
        set => SetProperty(ref _totalWithMarkCount, value);
    }

    public int DuplicateGroupsCount
    {
        get => _duplicateGroupsCount;
        set => SetProperty(ref _duplicateGroupsCount, value);
    }

    public int TotalDuplicateElementsCount
    {
        get => _totalDuplicateElementsCount;
        set => SetProperty(ref _totalDuplicateElementsCount, value);
    }

    public bool IsClean
    {
        get => _isClean;
        set
        {
            if (SetProperty(ref _isClean, value))
            {
                OnPropertyChanged(nameof(StatusBadgeText));
                OnPropertyChanged(nameof(StatusBadgeColor));
                OnPropertyChanged(nameof(CanAutoFix));
            }
        }
    }

    public string StatusBadgeText => IsClean
        ? "🟢 MÔ HÌNH SẠCH · 0 ĐỐI TƯỢNG TRÙNG MÃ MARK"
        : $"🔴 PHÁT HIỆN {TotalDuplicateElementsCount:N0} ĐỐI TƯỢNG TRÙNG MÃ MARK ({DuplicateGroupsCount:N0} NHÓM)";

    public string StatusBadgeColor => IsClean ? "#16A34A" : "#DC2626";

    public bool CanAutoFix => !IsBusy && DuplicateGroupsCount > 0;

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
                RaiseCanExecute();
            }
        }
    }

    public DuplicateMarkGroupItem? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                SelectInRevitCommand.RaiseCanExecuteChanged();
            }
        }
    }

    // --- Commands ---
    public RelayCommand RefreshCommand { get; }
    public RelayCommand SelectInRevitCommand { get; }
    public RelayCommand AutoFixCommand { get; }
    public RelayCommand ExportReportCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public void Close()
    {
        RequestClose?.Invoke();
    }

    private void RaiseCanExecute()
    {
        RefreshCommand.RaiseCanExecuteChanged();
        SelectInRevitCommand.RaiseCanExecuteChanged();
        AutoFixCommand.RaiseCanExecuteChanged();
        ExportReportCommand.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(CanAutoFix));
    }

    /// <summary>
    /// Quét toàn bộ mô hình và lọc ra các đối tượng có mã Mark trùng lặp
    /// </summary>
    public void ScanModel()
    {
        IsBusy = true;
        StatusMessage = "Đang quét toàn bộ cấu kiện trong mô hình...";

        try
        {
            DuplicateGroups.Clear();

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            var markMap = new Dictionary<string, List<DuplicateMarkElementDetail>>(StringComparer.OrdinalIgnoreCase);
            int scanned = 0;
            int withMark = 0;

            foreach (var el in collector)
            {
                scanned++;

                // Bỏ qua view, sketch, symbols nội bộ không phải physical element
                if (el.Category == null) continue;

                var markParam = el.get_Parameter(BuiltInParameter.ALL_MODEL_MARK) ?? el.LookupParameter("Mark");
                if (markParam == null || !markParam.HasValue) continue;

                string val = markParam.AsString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                val = val.Trim();
                withMark++;

                string catName = el.Category?.Name ?? "Không rõ";
                string typeName = string.Empty;
                string familyName = string.Empty;

                var typeId = el.GetTypeId();
                if (typeId != null && typeId != ElementId.InvalidElementId)
                {
                    var typeEl = _doc.GetElement(typeId);
                    if (typeEl != null)
                    {
                        typeName = typeEl.Name;
                        if (typeEl is FamilySymbol fs && fs.Family != null)
                        {
                            familyName = fs.Family.Name;
                        }
                    }
                }

                string levelName = "Không rõ";
                var levelParam = el.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM)
                                 ?? el.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM)
                                 ?? el.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                if (levelParam != null && levelParam.HasValue)
                {
                    var lvlId = levelParam.AsElementId();
                    if (lvlId != null && lvlId != ElementId.InvalidElementId)
                    {
                        var lvl = _doc.GetElement(lvlId) as Level;
                        if (lvl != null) levelName = lvl.Name;
                    }
                }

                var detail = new DuplicateMarkElementDetail
                {
                    Id = el.Id,
                    CategoryName = catName,
                    FamilyName = familyName,
                    TypeName = typeName,
                    LevelName = levelName,
                    Mark = val
                };

                if (!markMap.TryGetValue(val, out var list))
                {
                    list = new List<DuplicateMarkElementDetail>();
                    markMap[val] = list;
                }
                list.Add(detail);
            }

            TotalScannedCount = scanned;
            TotalWithMarkCount = withMark;

            // Lọc các nhóm có từ 2 phần tử trở lên
            var duplicates = markMap
                .Where(kvp => kvp.Value.Count > 1)
                .OrderByDescending(kvp => kvp.Value.Count)
                .ThenBy(kvp => kvp.Key)
                .ToList();

            int groupIndex = 1;
            int totalDupElements = 0;

            foreach (var kvp in duplicates)
            {
                var elements = kvp.Value;
                totalDupElements += elements.Count;

                var catSummary = string.Join(", ", elements.Select(e => e.CategoryName).Distinct());
                var famSummary = string.Join("; ", elements.Select(e => string.IsNullOrWhiteSpace(e.FamilyName) ? e.TypeName : $"{e.FamilyName}: {e.TypeName}").Distinct().Take(3));
                var lvlSummary = string.Join(", ", elements.Select(e => e.LevelName).Distinct());

                var groupItem = new DuplicateMarkGroupItem
                {
                    Index = groupIndex++,
                    Mark = kvp.Key,
                    CategoryNames = catSummary,
                    FamilyAndTypes = famSummary,
                    LevelNames = lvlSummary,
                    Elements = elements
                };

                DuplicateGroups.Add(groupItem);
            }

            DuplicateGroupsCount = DuplicateGroups.Count;
            TotalDuplicateElementsCount = totalDupElements;
            IsClean = DuplicateGroupsCount == 0;

            StatusMessage = IsClean
                ? $"Quét hoàn tất: Kiểm tra {scanned:N0} cấu kiện ({withMark:N0} có Mark) · KHÔNG CÓ MARK TRÙNG."
                : $"Quét hoàn tất: Phát hiện {totalDupElements:N0} cấu kiện bị trùng mã Mark trong {DuplicateGroupsCount:N0} nhóm.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi quét Mark: " + ex.Message;
            Log.Write("ScanModel Mark error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Chọn và Zoom tới các đối tượng của nhóm Mark đang chọn trong view Revit hiện hành
    /// </summary>
    public void SelectInRevit()
    {
        if (SelectedGroup == null || SelectedGroup.ElementIdList.Count == 0) return;

        try
        {
            _uidoc.Selection.SetElementIds(SelectedGroup.ElementIdList);
            _uidoc.ShowElements(SelectedGroup.ElementIdList);
            StatusMessage = $"Đã chọn {SelectedGroup.ElementIdList.Count} đối tượng có mã Mark '{SelectedGroup.Mark}' trên Revit.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Không thể highlight đối tượng trên Revit: " + ex.Message;
        }
    }

    /// <summary>
    /// Tự động sửa trùng lặp mã Mark bằng cách thêm hậu tố ElementId
    /// </summary>
    public void AutoFixAllDuplicates()
    {
        if (DuplicateGroups.Count == 0)
        {
            TaskDialog.Show("Kiểm Tra Mark", "Không có đối tượng nào bị trùng mã Mark để xử lý.");
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang tự động chuẩn hóa các mã Mark bị trùng...";

        try
        {
            int fixedCount = 0;
            using (var t = new Transaction(_doc, "Tự động phân biệt mã Mark trùng lặp"))
            {
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                opts.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(opts);

                t.Start();

                foreach (var group in DuplicateGroups)
                {
                    foreach (var detail in group.Elements)
                    {
                        var el = _doc.GetElement(detail.Id);
                        if (el == null) continue;

                        var markParam = el.get_Parameter(BuiltInParameter.ALL_MODEL_MARK) ?? el.LookupParameter("Mark");
                        if (markParam != null && !markParam.IsReadOnly)
                        {
                            string prefix = "M";
                            if (el.Category != null)
                            {
                                string catName = el.Category.Name;
                                if (catName.IndexOf("Plumbing", StringComparison.OrdinalIgnoreCase) >= 0) prefix = "PLB";
                                else if (catName.IndexOf("Mechanical", StringComparison.OrdinalIgnoreCase) >= 0) prefix = "MEC";
                                else if (catName.IndexOf("Pipe", StringComparison.OrdinalIgnoreCase) >= 0) prefix = "PIP";
                                else if (catName.IndexOf("Duct", StringComparison.OrdinalIgnoreCase) >= 0) prefix = "DCT";
                                else if (catName.IndexOf("Electrical", StringComparison.OrdinalIgnoreCase) >= 0) prefix = "ELE";
                            }

                            string newMark = $"{prefix}-{group.Mark}-{el.Id}";
                            markParam.Set(newMark);
                            fixedCount++;
                        }
                    }
                }

                _doc.Regenerate();
                t.Commit();
            }

            ScanModel();

            var dialog = new TaskDialog("BIM TOOL · Xử Lý Trùng Mark")
            {
                MainInstruction = $"Đã phân biệt thành công {fixedCount:N0} đối tượng trùng mã Mark!",
                MainContent = $"Mô hình: {_doc.Title}\n" +
                              $"Toàn bộ các mã Mark trùng đã được bổ sung mã định danh duy nhất theo Category và Element ID.\n" +
                              $"Hiện tại số lượng đối tượng trùng Mark đã về 0.\n\n" +
                              $"Bạn có thể nhấn Ctrl+Z trong Revit để hoàn tác bất cứ lúc nào.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            dialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi tự động sửa mã Mark: " + ex.Message;
            Log.Write("AutoFixAllDuplicates error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Xuất báo cáo chi tiết đối tượng trùng Mark ra file CSV và mở file
    /// </summary>
    public void ExportReport()
    {
        try
        {
            var reportsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSCons", "Reports");
            Directory.CreateDirectory(reportsDir);

            var filePath = Path.Combine(reportsDir, $"Bao_Cao_Trung_Mark_{_doc.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            var sb = new StringBuilder();
            sb.AppendLine("STT,Ma_Mark,So_Luong_Trung,Danh_Muc_Category,Ten_Cau_Kien_Family_Type,Tang_Level,Element_IDs");

            foreach (var g in DuplicateGroups)
            {
                string cat = $"\"{g.CategoryNames.Replace("\"", "\"\"")}\"";
                string fam = $"\"{g.FamilyAndTypes.Replace("\"", "\"\"")}\"";
                string lvl = $"\"{g.LevelNames.Replace("\"", "\"\"")}\"";
                string ids = $"\"{g.ElementIdsString.Replace("\"", "\"\"")}\"";

                sb.AppendLine($"{g.Index},{g.Mark},{g.DuplicateCount},{cat},{fam},{lvl},{ids}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            StatusMessage = $"Đã xuất báo cáo CSV tại: {filePath}";

            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusMessage = "Không thể xuất file báo cáo: " + ex.Message;
        }
    }
}
