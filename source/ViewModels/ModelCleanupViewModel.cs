using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.ViewModels;

/// <summary>
/// ViewModel quản lý dữ liệu và 5 chức năng dọn dẹp file bàn giao cho kỹ sư:
/// 1. Purge file để gửi đi (Purge Unused Elements / Types / Families)
/// 2. Remove file CAD link
/// 3. Remove file Revit link
/// 4. Quản lý và Triệt tiêu các thông báo Warnings (Làm mờ nút Warnings)
/// 5. Remove các file CAD trong mục Imported Categories
/// Kèm tùy chọn Dọn dẹp tổng thể 1-Click.
/// </summary>
public sealed class ModelCleanupViewModel : ViewModelBase
{
    private readonly UIDocument _uidoc;
    private readonly Document _doc;

    // Cache các ElementId cần xử lý
    private List<ElementId> _cadLinkTypeIds = new();
    private List<ElementId> _cadLinkInstanceIds = new();
    private List<ElementId> _revitLinkTypeIds = new();
    private List<ElementId> _revitLinkInstanceIds = new();
    private List<ElementId> _importedCadInstanceIds = new();
    private List<ElementId> _importedSubcategoryIds = new();
    private int _warningCount;
    private int _purgeCandidateCount;

    private string _statusMessage = "Sẵn sàng";
    private bool _isBusy;
    private string _fileSizeText = "Chưa lưu";

    // Checkbox 1-Click
    private bool _includeCadLinks = true;
    private bool _includeRevitLinks = true;
    private bool _includeImportedCad = true;
    private bool _includeWarningsSuppress = true;
    private bool _includePurgeUnused = true;

    public ModelCleanupViewModel(UIDocument uidoc)
    {
        _uidoc = uidoc ?? throw new ArgumentNullException(nameof(uidoc));
        _doc = uidoc.Document;

        StudentName = StudentBranding.StudentName;
        Company = StudentBranding.Company;
        WindowTitle = "BIM TOOL · Dọn Dẹp File Bàn Giao (Model Delivery Cleanup Suite)";
        DocumentTitle = string.IsNullOrWhiteSpace(_doc.Title) ? "Mô hình hiện hành" : _doc.Title;

        ReadDocumentSize();

        // Khởi tạo Commands
        RefreshAllCommand = new RelayCommand(_ => ScanAll(), _ => !IsBusy);
        DeleteCadLinksCommand = new RelayCommand(_ => DeleteCadLinks(), _ => CanDeleteCadLinks);
        DeleteRevitLinksCommand = new RelayCommand(_ => DeleteRevitLinks(), _ => CanDeleteRevitLinks);
        DeleteImportedCadCommand = new RelayCommand(_ => DeleteImportedCad(), _ => CanDeleteImportedCad);
        ResolveAllWarningsCommand = new RelayCommand(_ => ResolveAllWarnings(), _ => CanResolveWarnings);
        ToggleWarningSuppressorCommand = new RelayCommand(_ => ToggleWarningSuppressor(), _ => !IsBusy);
        PurgeUnusedCommand = new RelayCommand(_ => PurgeUnused(), _ => CanPurgeUnused);
        OpenRevitPurgeCommand = new RelayCommand(_ => OpenRevitPurge(), _ => !IsBusy);
        ExecuteAllCleanupCommand = new RelayCommand(_ => ExecuteAllCleanup(), _ => CanExecuteAllCleanup);
        OpenDuplicateMarkAuditCommand = new RelayCommand(_ => OpenDuplicateMarkAudit(), _ => !IsBusy);
        ExportWarningsReportCommand = new RelayCommand(_ => ExportWarningsReport(), _ => !IsBusy);
        CloseCommand = new RelayCommand(_ => Close());

        // Quét số liệu ban đầu
        ScanAll();
    }

    // --- Thông tin chung ---
    public string StudentName { get; }
    public string Company { get; }
    public string WindowTitle { get; }
    public string DocumentTitle { get; }

    public string FileSizeText
    {
        get => _fileSizeText;
        set => SetProperty(ref _fileSizeText, value);
    }

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

    // --- 1. CAD Links ---
    public int CadLinkCount => _cadLinkTypeIds.Count + _cadLinkInstanceIds.Count;
    public string CadLinkSummary => CadLinkCount > 0
        ? $"{_cadLinkTypeIds.Count} link file ({_cadLinkInstanceIds.Count} instances)"
        : "0 link CAD (Sạch liên kết CAD)";
    public bool CanDeleteCadLinks => !IsBusy && CadLinkCount > 0;

    // --- 2. Revit Links ---
    public int RevitLinkCount => _revitLinkTypeIds.Count;
    public int RevitLinkInstanceCount => _revitLinkInstanceIds.Count;
    public string RevitLinkSummary => RevitLinkCount > 0
        ? $"{RevitLinkCount} file link ({RevitLinkInstanceCount} vị trí nạp)"
        : "0 link Revit (File độc lập)";
    public bool CanDeleteRevitLinks => !IsBusy && (RevitLinkCount > 0 || RevitLinkInstanceCount > 0);

    // --- 3. Imported CAD & Imported Categories ---
    public int ImportedCadInstanceCount => _importedCadInstanceIds.Count;
    public int ImportedSubcategoryCount => _importedSubcategoryIds.Count;
    public string ImportedCadSummary => (ImportedCadInstanceCount > 0 || ImportedSubcategoryCount > 0)
        ? $"{ImportedCadInstanceCount} bản vẽ import ({ImportedSubcategoryCount} subcategories rác)"
        : "0 bản vẽ (Sạch mục Imported Categories)";
    public bool CanDeleteImportedCad => !IsBusy && (ImportedCadInstanceCount > 0 || ImportedSubcategoryCount > 0);

    // --- 4. Warnings ---
    public int WarningCount
    {
        get => _warningCount;
        set
        {
            if (SetProperty(ref _warningCount, value))
            {
                OnPropertyChanged(nameof(WarningSummary));
            }
        }
    }
    public string WarningSummary => WarningCount > 0
        ? $"{WarningCount:N0} cảnh báo đang tồn đọng"
        : "0 cảnh báo (Mô hình sạch)";

    public bool IsWarningSuppressorActive => GlobalWarningSuppressorManager.IsEnabled;
    public string WarningSuppressorStatusText => IsWarningSuppressorActive
        ? "🟢 ĐANG BẬT (Tự động triệt tiêu popup cảnh báo phiền toái)"
        : "⚪ ĐANG TẮT (Revit sẽ hiện popup khi gặp cảnh báo)";

    // --- 5. Purge Unused Elements ---
    public int PurgeCandidateCount
    {
        get => _purgeCandidateCount;
        set
        {
            if (SetProperty(ref _purgeCandidateCount, value))
            {
                OnPropertyChanged(nameof(PurgeSummary));
                RaiseCanExecute();
            }
        }
    }
    public string PurgeSummary => PurgeCandidateCount > 0
        ? $"{PurgeCandidateCount:N0} đối tượng / Type không sử dụng"
        : "Sẵn sàng Purge nén file";
    public bool CanPurgeUnused => !IsBusy;

    // --- Checkboxes 1-Click ---
    public bool IncludeCadLinks
    {
        get => _includeCadLinks;
        set => SetProperty(ref _includeCadLinks, value);
    }
    public bool IncludeRevitLinks
    {
        get => _includeRevitLinks;
        set => SetProperty(ref _includeRevitLinks, value);
    }
    public bool IncludeImportedCad
    {
        get => _includeImportedCad;
        set => SetProperty(ref _includeImportedCad, value);
    }
    public bool IncludeWarningsSuppress
    {
        get => _includeWarningsSuppress;
        set => SetProperty(ref _includeWarningsSuppress, value);
    }
    public bool IncludePurgeUnused
    {
        get => _includePurgeUnused;
        set => SetProperty(ref _includePurgeUnused, value);
    }

    public bool CanExecuteAllCleanup => !IsBusy && (IncludeCadLinks || IncludeRevitLinks || IncludeImportedCad || IncludeWarningsSuppress || IncludePurgeUnused);

    public bool CanResolveWarnings => !IsBusy && WarningCount > 0;

    // Commands
    public RelayCommand RefreshAllCommand { get; }
    public RelayCommand DeleteCadLinksCommand { get; }
    public RelayCommand DeleteRevitLinksCommand { get; }
    public RelayCommand DeleteImportedCadCommand { get; }
    public RelayCommand ResolveAllWarningsCommand { get; }
    public RelayCommand ToggleWarningSuppressorCommand { get; }
    public RelayCommand PurgeUnusedCommand { get; }
    public RelayCommand OpenRevitPurgeCommand { get; }
    public RelayCommand ExecuteAllCleanupCommand { get; }
    public RelayCommand OpenDuplicateMarkAuditCommand { get; }
    public RelayCommand ExportWarningsReportCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public void Close()
    {
        RequestClose?.Invoke();
    }

    private void RaiseCanExecute()
    {
        RefreshAllCommand.RaiseCanExecuteChanged();
        DeleteCadLinksCommand.RaiseCanExecuteChanged();
        DeleteRevitLinksCommand.RaiseCanExecuteChanged();
        DeleteImportedCadCommand.RaiseCanExecuteChanged();
        ResolveAllWarningsCommand.RaiseCanExecuteChanged();
        ToggleWarningSuppressorCommand.RaiseCanExecuteChanged();
        OpenDuplicateMarkAuditCommand.RaiseCanExecuteChanged();
        ExportWarningsReportCommand.RaiseCanExecuteChanged();
        PurgeUnusedCommand.RaiseCanExecuteChanged();
        OpenRevitPurgeCommand.RaiseCanExecuteChanged();
        ExecuteAllCleanupCommand.RaiseCanExecuteChanged();

        OnPropertyChanged(nameof(CanDeleteCadLinks));
        OnPropertyChanged(nameof(CanDeleteRevitLinks));
        OnPropertyChanged(nameof(CanDeleteImportedCad));
        OnPropertyChanged(nameof(CanResolveWarnings));
        OnPropertyChanged(nameof(CanPurgeUnused));
        OnPropertyChanged(nameof(CanExecuteAllCleanup));
        OnPropertyChanged(nameof(IsWarningSuppressorActive));
        OnPropertyChanged(nameof(WarningSuppressorStatusText));
    }

    private void ReadDocumentSize()
    {
        try
        {
            if (!string.IsNullOrEmpty(_doc.PathName) && File.Exists(_doc.PathName))
            {
                var fi = new FileInfo(_doc.PathName);
                double mb = fi.Length / (1024.0 * 1024.0);
                FileSizeText = $"{mb:F1} MB";
            }
            else
            {
                FileSizeText = "Chưa lưu trên đĩa";
            }
        }
        catch
        {
            FileSizeText = "Không xác định";
        }
    }

    /// <summary>
    /// Quét toàn bộ 5 hạng mục cần dọn dẹp trong Document
    /// </summary>
    public void ScanAll()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Đang quét mô hình Revit để thống kê các hạng mục dọn dẹp...";

            // 1. Quét CAD Links
            _cadLinkTypeIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(CADLinkType))
                .ToElementIds()
                .ToList();

            _cadLinkInstanceIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .Where(inst => inst.IsLinked)
                .Select(inst => inst.Id)
                .ToList();

            // 2. Quét Revit Links
            _revitLinkTypeIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(RevitLinkType))
                .ToElementIds()
                .ToList();

            _revitLinkInstanceIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(RevitLinkInstance))
                .ToElementIds()
                .ToList();

            // 3. Quét CAD Import trực tiếp và Imported Categories Subcategories
            _importedCadInstanceIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .Where(inst => !inst.IsLinked)
                .Select(inst => inst.Id)
                .ToList();

            _importedSubcategoryIds.Clear();
            try
            {
                var importStylesCat = _doc.Settings.Categories.get_Item(BuiltInCategory.OST_ImportObjectStyles);
                if (importStylesCat?.SubCategories != null)
                {
                    foreach (Category sub in importStylesCat.SubCategories)
                    {
                        if (sub.Id != null && sub.Id != ElementId.InvalidElementId)
                        {
                            _importedSubcategoryIds.Add(sub.Id);
                        }
                    }
                }
            }
            catch { }

            // 4. Quét Warnings
            WarningCount = _doc.GetWarnings().Count;

            // 5. Thống kê nhanh ứng viên Purge Unused
            ScanPurgeCandidates();

            ReadDocumentSize();

            OnPropertyChanged(nameof(CadLinkCount));
            OnPropertyChanged(nameof(CadLinkSummary));
            OnPropertyChanged(nameof(RevitLinkCount));
            OnPropertyChanged(nameof(RevitLinkInstanceCount));
            OnPropertyChanged(nameof(RevitLinkSummary));
            OnPropertyChanged(nameof(ImportedCadInstanceCount));
            OnPropertyChanged(nameof(ImportedSubcategoryCount));
            OnPropertyChanged(nameof(ImportedCadSummary));
            OnPropertyChanged(nameof(IsWarningSuppressorActive));
            OnPropertyChanged(nameof(WarningSuppressorStatusText));

            StatusMessage = "Đã quét xong: Sẵn sàng thực hiện dọn dẹp.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi quét mô hình: " + ex.Message;
            Log.Write("ScanAll error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ScanPurgeCandidates()
    {
        try
        {
            var adviser = PerformanceAdviser.GetPerformanceAdviser();
            var allRules = adviser.GetAllRuleIds();
            var purgeRuleIds = allRules.Where(id =>
            {
                try
                {
                    var name = adviser.GetRuleName(id);
                    return name.IndexOf("Purge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           name.IndexOf("Unused", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                catch { return false; }
            }).ToList();

            if (purgeRuleIds.Count > 0)
            {
                var failures = adviser.ExecuteRules(_doc, purgeRuleIds);
                var candidateIds = new HashSet<ElementId>();
                foreach (var f in failures)
                {
                    foreach (var id in f.GetFailingElements())
                    {
                        candidateIds.Add(id);
                    }
                }
                PurgeCandidateCount = candidateIds.Count;
            }
            else
            {
                PurgeCandidateCount = 0;
            }
        }
        catch
        {
            PurgeCandidateCount = 0;
        }
    }

    /// <summary>
    /// Xóa toàn bộ file CAD Link
    /// </summary>
    public void DeleteCadLinks()
    {
        if (CadLinkCount == 0)
        {
            StatusMessage = "Không có link CAD nào để xóa.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang xóa các file CAD Link...";

        try
        {
            int deleted = 0;
            using (var t = new Transaction(_doc, "Xóa toàn bộ CAD Links"))
            {
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                opts.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(opts);

                t.Start();
                if (_cadLinkInstanceIds.Count > 0)
                {
                    var deletedInstances = _doc.Delete(_cadLinkInstanceIds);
                    deleted += deletedInstances.Count;
                }
                if (_cadLinkTypeIds.Count > 0)
                {
                    var deletedTypes = _doc.Delete(_cadLinkTypeIds);
                    deleted += deletedTypes.Count;
                }
                t.Commit();
            }

            Log.Write($"DeleteCadLinks: Đã xóa {deleted} đối tượng/loại CAD Link.");
            ScanAll();
            StatusMessage = $"Đã xóa sạch toàn bộ CAD Links thành công! (Nhấn Ctrl+Z để hoàn tác)";

            var dialog = new TaskDialog("BIM TOOL · Xóa CAD Links")
            {
                MainInstruction = $"Đã xóa sạch {deleted} liên kết / đối tượng CAD Links!",
                MainContent = $"Mô hình: {_doc.Title}\nToàn bộ liên kết file CAD ngoài đĩa đã được gỡ bỏ hoàn toàn khỏi mô hình.\n\nBạn có thể nhấn Ctrl+Z trong Revit để hoàn tác.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            dialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi xóa CAD Links: " + ex.Message;
            Log.Write("DeleteCadLinks error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Xóa toàn bộ file Revit Link
    /// </summary>
    public void DeleteRevitLinks()
    {
        if (RevitLinkCount == 0 && RevitLinkInstanceCount == 0)
        {
            StatusMessage = "Không có link Revit nào để xóa.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang gỡ bỏ các file Revit Links...";

        try
        {
            int deleted = 0;
            using (var t = new Transaction(_doc, "Xóa toàn bộ Revit Links"))
            {
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                opts.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(opts);

                t.Start();
                if (_revitLinkInstanceIds.Count > 0)
                {
                    var deletedInstances = _doc.Delete(_revitLinkInstanceIds);
                    deleted += deletedInstances.Count;
                }
                if (_revitLinkTypeIds.Count > 0)
                {
                    var deletedTypes = _doc.Delete(_revitLinkTypeIds);
                    deleted += deletedTypes.Count;
                }
                t.Commit();
            }

            Log.Write($"DeleteRevitLinks: Đã xóa {deleted} đối tượng Revit Link.");
            ScanAll();
            StatusMessage = $"Đã gỡ sạch toàn bộ {RevitLinkCount} file Revit Links thành công!";

            var dialog = new TaskDialog("BIM TOOL · Gỡ Revit Links")
            {
                MainInstruction = $"Đã gỡ bỏ toàn bộ {deleted} liên kết mô hình Revit!",
                MainContent = $"Mô hình: {_doc.Title}\nToàn bộ các liên kết Kiến trúc/Kết cấu/Cơ điện ngoài đã được gỡ bỏ, file trở thành mô hình độc lập (Detached).\n\nBạn có thể nhấn Ctrl+Z trong Revit để hoàn tác.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            dialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi gỡ Revit Links: " + ex.Message;
            Log.Write("DeleteRevitLinks error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Xóa các file CAD import trực tiếp và dọn sạch Imported Categories
    /// </summary>
    public void DeleteImportedCad()
    {
        if (ImportedCadInstanceCount == 0 && ImportedSubcategoryCount == 0)
        {
            StatusMessage = "Mô hình không có CAD import hoặc Subcategory rác.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang xóa CAD import và làm sạch bảng Imported Categories...";

        try
        {
            int deletedInstancesCount = 0;
            int deletedSubcatsCount = 0;

            using (var t = new Transaction(_doc, "Xóa CAD import và làm sạch Imported Categories"))
            {
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                opts.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(opts);

                t.Start();

                // 1. Xóa các instance CAD import
                if (_importedCadInstanceIds.Count > 0)
                {
                    var deletedList = _doc.Delete(_importedCadInstanceIds);
                    deletedInstancesCount = deletedList.Count;
                }

                // 2. Xóa các subcategories rác trong Imported Categories
                if (_importedSubcategoryIds.Count > 0)
                {
                    foreach (var subId in _importedSubcategoryIds)
                    {
                        try
                        {
                            _doc.Delete(subId);
                            deletedSubcatsCount++;
                        }
                        catch { }
                    }
                }

                t.Commit();
            }

            Log.Write($"DeleteImportedCad: Đã xóa {deletedInstancesCount} CAD instances và {deletedSubcatsCount} subcategories.");
            ScanAll();
            StatusMessage = $"Đã xóa {deletedInstancesCount} bản vẽ CAD import và dọn {deletedSubcatsCount} categories rác!";

            var cadDialog = new TaskDialog("BIM TOOL · Dọn CAD Import & Categories")
            {
                MainInstruction = $"Đã dọn sạch {deletedInstancesCount} bản vẽ CAD import và {deletedSubcatsCount} categories rác!",
                MainContent = $"Mô hình: {_doc.Title}\nDanh mục Imported Categories và Object Styles đã được làm sạch hoàn toàn.\n\nBạn có thể nhấn Ctrl+Z trong Revit để hoàn tác.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            cadDialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi dọn CAD import: " + ex.Message;
            Log.Write("DeleteImportedCad error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Triệt tiêu 100% cảnh báo để làm mờ icon Warnings trên tab Manage,
    /// BẢO TOÀN 100% CẤU KIỆN, ĐOẠN ỐNG VÀ HỆ THỐNG TRONG MÔ HÌNH.
    /// </summary>
    public void ResolveAllWarnings()
    {
        if (WarningCount == 0)
        {
            StatusMessage = "Mô hình đã có 0 cảnh báo. Nút Warnings trên tab Manage đã mờ.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang triệt tiêu cảnh báo để làm mờ biểu tượng Warnings (Bảo toàn 100% đối tượng)...";

        try
        {
            var sw = Stopwatch.StartNew();
            int initialCount = WarningCount;

            var (mepSystemsUpdated, marksFixed) = ResolveAllWarningsInternal();

            sw.Stop();
            ScanAll();

            int remaining = WarningCount;
            int cleared = initialCount - remaining;

            StatusMessage = remaining == 0
                ? "Đã đưa Warnings về 0! Biểu tượng Warnings trên tab Manage ĐÃ MỜ ĐI (Grayed out)."
                : $"Đã giảm {cleared:N0} cảnh báo! Còn lại {remaining:N0} cảnh báo.";

            var successDialog = new TaskDialog("BIM TOOL · Làm Mờ Biểu Tượng Warnings")
            {
                MainInstruction = remaining == 0
                    ? "Đã đưa Warnings về 0! Biểu tượng Warnings trên tab Manage ĐÃ MỜ ĐI."
                    : $"Đã triệt tiêu thành công {cleared:N0} cảnh báo trong mô hình!",
                MainContent = $"Mô hình: {_doc.Title}\n" +
                              $"Thời gian xử lý: {sw.Elapsed.TotalSeconds:F1} giây\n\n" +
                              $"KẾT QUẢ THỰC HIỆN:\n" +
                              $"• Cảnh báo ban đầu: {initialCount:N0}\n" +
                              $"• Cảnh báo đã triệt tiêu: {cleared:N0}\n" +
                              $"• Cảnh báo còn lại: {remaining:N0}\n" +
                              $"• Hệ thống MEP chuẩn hóa tính toán: {mepSystemsUpdated} System Types\n" +
                              $"• Mã Mark trùng lặp đã sửa phân biệt: {marksFixed} đối tượng\n\n" +
                              (remaining == 0 ? "👉 Biểu tượng Warnings (Manage > Inquiry > Warnings) hiện ĐÃ MỜ ĐI hoàn toàn!" : ""),
                ExpandedContent = "CAM KẾT AN TOÀN TUYỆT ĐỐI:\n" +
                                  "✔ BẢO TOÀN 100% ĐỐI TƯỢNG: Không xóa bất kỳ ống, phụ kiện, phễu thu sàn hay bơm nào.\n" +
                                  "✔ BẢO TOÀN 100% HỆ THỐNG: Tên hệ thống (RW 70, CW, Sanitary...), màu sắc và kết nối giữ nguyên vẹn.\n" +
                                  "✔ Bạn có thể nhấn Ctrl+Z trong Revit để hoàn tác toàn bộ bất cứ lúc nào.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            successDialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi triệt tiêu cảnh báo: " + ex.Message;
            Log.Write("ResolveAllWarnings error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private (int mepSystemsUpdated, int marksFixed) ResolveAllWarningsInternal()
    {
        int mepSystemsUpdated = 0;
        int marksFixed = 0;

        using (var t = new Transaction(_doc, "Triệt tiêu cảnh báo & Làm mờ icon Warnings"))
        {
            var opts = t.GetFailureHandlingOptions();
            opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
            opts.SetClearAfterRollback(true);
            t.SetFailureHandlingOptions(opts);

            t.Start();

            // 1. Chuyển chế độ tính toán của PipingSystemType về None
            // Bảo toàn 100% ống, phụ kiện, tên hệ thống và màu sắc
            var pipingSystemTypes = new FilteredElementCollector(_doc)
                .OfClass(typeof(PipingSystemType))
                .Cast<PipingSystemType>()
                .ToList();

            foreach (var pst in pipingSystemTypes)
            {
                try
                {
                    if (pst.CalculationLevel != SystemCalculationLevel.None)
                    {
                        pst.CalculationLevel = SystemCalculationLevel.None;
                        mepSystemsUpdated++;
                    }
                }
                catch { }
            }

            // 2. Chuyển chế độ tính toán của MechanicalSystemType về None
            var mechSystemTypes = new FilteredElementCollector(_doc)
                .OfClass(typeof(MechanicalSystemType))
                .Cast<MechanicalSystemType>()
                .ToList();

            foreach (var mst in mechSystemTypes)
            {
                try
                {
                    if (mst.CalculationLevel != SystemCalculationLevel.None)
                    {
                        mst.CalculationLevel = SystemCalculationLevel.None;
                        mepSystemsUpdated++;
                    }
                }
                catch { }
            }

            // 3. Chuẩn hóa mã Mark bị trùng lặp
            // Bảo toàn 100% đối tượng, chỉ phân biệt text Mark
            var warnings = _doc.GetWarnings();
            var duplicateMarkElements = new HashSet<ElementId>();

            foreach (var w in warnings)
            {
                var desc = w.GetDescriptionText();
                if (desc.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    desc.IndexOf("Mark", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foreach (var id in w.GetFailingElements())
                    {
                        duplicateMarkElements.Add(id);
                    }
                }
            }

            foreach (var id in duplicateMarkElements)
            {
                var el = _doc.GetElement(id);
                if (el == null) continue;

                var markParam = el.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (markParam != null && !markParam.IsReadOnly)
                {
                    string currentMark = markParam.AsString();
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

                    string newMark = !string.IsNullOrWhiteSpace(currentMark)
                        ? $"{prefix}-{currentMark}-{el.Id}"
                        : $"{prefix}-{el.Id}";

                    markParam.Set(newMark);
                    marksFixed++;
                }
            }

            // 4. Bật chế độ ẩn cảnh báo ngầm nếu chưa bật
            if (!GlobalWarningSuppressorManager.IsEnabled)
            {
                GlobalWarningSuppressorManager.Toggle(_uidoc.Application.Application);
            }

            _doc.Regenerate();
            t.Commit();
        }

        return (mepSystemsUpdated, marksFixed);
    }

    /// <summary>
    /// Bật/Tắt chế độ ẩn cảnh báo ngầm toàn cục (Warning Suppressor)
    /// </summary>
    public void ToggleWarningSuppressor()
    {
        try
        {
            bool newState = GlobalWarningSuppressorManager.Toggle(_uidoc.Application.Application);
            OnPropertyChanged(nameof(IsWarningSuppressorActive));
            OnPropertyChanged(nameof(WarningSuppressorStatusText));
            RaiseCanExecute();

            StatusMessage = newState
                ? "🟢 Đã BẬT Chế độ Ẩn Cảnh Báo Ngầm: Revit sẽ tự động triệt tiêu popup warning."
                : "⚪ Đã TẮT Chế độ Ẩn Cảnh Báo Ngầm: Revit quay về cơ chế mặc định.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi thay đổi trạng thái ẩn cảnh báo: " + ex.Message;
            Log.Write("ToggleWarningSuppressor error: " + ex);
        }
    }

    /// <summary>
    /// Mở bảng thống kê và kiểm tra chi tiết các đối tượng bị trùng thông số Mark
    /// </summary>
    public void OpenDuplicateMarkAudit()
    {
        try
        {
            var auditVm = new DuplicateMarkAuditViewModel(_uidoc);
            var window = new DuplicateMarkAuditWindow(auditVm, _uidoc.Application.MainWindowHandle);
            window.ShowDialog();
            ScanAll();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi mở bảng kiểm tra Mark: " + ex.Message;
            Log.Write("OpenDuplicateMarkAudit error: " + ex);
        }
    }

    /// <summary>
    /// Xuat toan bo bao cao kiem toan Warnings chi tiet ra file text va mo truc tiep
    /// </summary>
    public void ExportWarningsReport()
    {
        try
        {
            var warnings = _doc.GetWarnings();
            var totalCount = warnings.Count;

            var groupedWarnings = warnings
                .GroupBy(w => w.GetDescriptionText())
                .OrderByDescending(g => g.Count())
                .ToList();

            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine($"BAO CAO KIEM TOAN CANH BAO (WARNINGS AUDIT REPORT) - REVIT {_doc.Application.VersionNumber}");
            reportBuilder.AppendLine($"Mo hinh: {_doc.Title}");
            reportBuilder.AppendLine($"Duong dan: {_doc.PathName}");
            reportBuilder.AppendLine($"Thoi gian quet: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            reportBuilder.AppendLine($"Tong so canh bao (Warnings): {totalCount}");
            reportBuilder.AppendLine($"Tong so nhom loi: {groupedWarnings.Count}");
            reportBuilder.AppendLine($"Trang thai Bo loc An Canh Bao: {(GlobalWarningSuppressorManager.IsEnabled ? "DANG BAT" : "DANG TAT")}");
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine();

            reportBuilder.AppendLine("--- BANG TONG HOP THEO NHOM LOI ---");
            int rank = 1;
            foreach (var group in groupedWarnings)
            {
                reportBuilder.AppendLine($"{rank++}. [{group.Count()} vi tri] {group.Key}");
            }
            reportBuilder.AppendLine();

            reportBuilder.AppendLine("--- CHI TIET CAC DOI TUONG BI CANH BAO ---");
            int itemIndex = 1;
            foreach (var w in warnings)
            {
                var failingIds = w.GetFailingElements();
                var idStrings = failingIds.Select(id => id.ToString()).ToList();
                var idListText = idStrings.Count > 0 ? string.Join(", ", idStrings) : "Khong co ElementId cu the";

                reportBuilder.AppendLine($"[#{itemIndex++}] {w.GetDescriptionText()}");
                reportBuilder.AppendLine($"    ElementIds lien quan: {idListText}");
            }

            var reportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSCons", "Reports");
            Directory.CreateDirectory(reportDirectory);
            var reportFilePath = Path.Combine(reportDirectory, $"Warnings_Audit_{_doc.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(reportFilePath, reportBuilder.ToString(), Encoding.UTF8);

            Process.Start(new ProcessStartInfo(reportFilePath) { UseShellExecute = true });
            StatusMessage = $"Da xuat bao cao canh bao: {Path.GetFileName(reportFilePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Loi khi xuat bao cao Warnings: " + ex.Message;
            Log.Write("ExportWarningsReport error: " + ex);
        }
    }
    /// <summary>
    /// Purge Unused Elements tự động (Multi-pass)
    /// </summary>
    public void PurgeUnused()
    {
        IsBusy = true;
        StatusMessage = "Đang thực hiện Purge Unused Elements nén file...";

        try
        {
            var sw = Stopwatch.StartNew();
            int totalPurged = ExecutePurgeInternal();
            sw.Stop();

            ScanAll();
            StatusMessage = totalPurged > 0
                ? $"Đã Purge thành công {totalPurged:N0} đối tượng/Type không dùng trong {sw.Elapsed.TotalSeconds:F1}s!"
                : "Mô hình đã sạch, không còn phần tử không sử dụng nào cần Purge.";

            var dialog = new TaskDialog("BIM TOOL · Purge Unused Elements")
            {
                MainInstruction = totalPurged > 0
                    ? $"Đã Purge thành công {totalPurged:N0} đối tượng / kiểu loại không dùng!"
                    : "Mô hình đã sạch sẵn, không còn đối tượng dư thừa nào cần Purge.",
                MainContent = $"Mô hình: {_doc.Title}\nThời gian xử lý: {sw.Elapsed.TotalSeconds:F1} giây.\n\nBạn có thể nhấn Ctrl+Z trong Revit để hoàn tác.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            dialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi khi Purge: " + ex.Message;
            Log.Write("PurgeUnused error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private int ExecutePurgeInternal(int maxPasses = 3)
    {
        int totalPurged = 0;
        var adviser = PerformanceAdviser.GetPerformanceAdviser();
        var allRules = adviser.GetAllRuleIds();
        var purgeRuleIds = allRules.Where(id =>
        {
            try
            {
                var name = adviser.GetRuleName(id);
                return name.IndexOf("Purge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       name.IndexOf("Unused", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }).ToList();

        if (purgeRuleIds.Count == 0) return 0;

        for (int pass = 0; pass < maxPasses; pass++)
        {
            var failures = adviser.ExecuteRules(_doc, purgeRuleIds);
            var failingIds = new HashSet<ElementId>();
            foreach (var f in failures)
            {
                foreach (var id in f.GetFailingElements())
                {
                    failingIds.Add(id);
                }
            }

            if (failingIds.Count == 0) break;

            int passPurged = 0;
            using (var t = new Transaction(_doc, $"Purge Unused Pass {pass + 1}"))
            {
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                opts.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(opts);

                t.Start();
                foreach (var id in failingIds)
                {
                    try
                    {
                        _doc.Delete(id);
                        passPurged++;
                    }
                    catch { }
                }
                t.Commit();
            }

            totalPurged += passPurged;
            if (passPurged == 0) break;
        }

        return totalPurged;
    }

    /// <summary>
    /// Mở hộp thoại Purge Unused mặc định của Revit để kiểm tra trực quan
    /// </summary>
    public void OpenRevitPurge()
    {
        try
        {
            var purgeCmdId = RevitCommandId.LookupPostableCommandId(PostableCommand.PurgeUnused);
            if (purgeCmdId != null && _uidoc.Application.CanPostCommand(purgeCmdId))
            {
                Close();
                _uidoc.Application.PostCommand(purgeCmdId);
            }
            else
            {
                StatusMessage = "Lệnh Purge Unused hiện tại không khả dụng trong view này.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Không thể mở hộp thoại Purge của Revit: " + ex.Message;
        }
    }

    /// <summary>
    /// Gói dọn dẹp tổng thể 1-Click theo các tùy chọn đã tích
    /// </summary>
    public void ExecuteAllCleanup()
    {
        IsBusy = true;
        StatusMessage = "Đang thực hiện gói Dọn Dẹp Toàn Diện Để Gửi Đi...";

        try
        {
            var sw = Stopwatch.StartNew();
            var reportLines = new List<string>();

            // 1. Remove CAD Links
            if (IncludeCadLinks && CadLinkCount > 0)
            {
                using var t = new Transaction(_doc, "Gói Dọn Dẹp: Xóa CAD Links");
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                t.SetFailureHandlingOptions(opts);
                t.Start();
                if (_cadLinkInstanceIds.Count > 0) _doc.Delete(_cadLinkInstanceIds);
                if (_cadLinkTypeIds.Count > 0) _doc.Delete(_cadLinkTypeIds);
                t.Commit();
                reportLines.Add("✔ Đã xóa toàn bộ CAD Links");
            }

            // 2. Remove Revit Links
            if (IncludeRevitLinks && (RevitLinkCount > 0 || RevitLinkInstanceCount > 0))
            {
                using var t = new Transaction(_doc, "Gói Dọn Dẹp: Xóa Revit Links");
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                t.SetFailureHandlingOptions(opts);
                t.Start();
                if (_revitLinkInstanceIds.Count > 0) _doc.Delete(_revitLinkInstanceIds);
                if (_revitLinkTypeIds.Count > 0) _doc.Delete(_revitLinkTypeIds);
                t.Commit();
                reportLines.Add("✔ Đã gỡ bỏ toàn bộ Revit Links");
            }

            // 3. Remove Imported CAD & Categories
            if (IncludeImportedCad && (ImportedCadInstanceCount > 0 || ImportedSubcategoryCount > 0))
            {
                using var t = new Transaction(_doc, "Gói Dọn Dẹp: Xóa CAD Import & Categories");
                var opts = t.GetFailureHandlingOptions();
                opts.SetFailuresPreprocessor(new SilentWarningSuppressor());
                t.SetFailureHandlingOptions(opts);
                t.Start();
                if (_importedCadInstanceIds.Count > 0) _doc.Delete(_importedCadInstanceIds);
                foreach (var subId in _importedSubcategoryIds)
                {
                    try { _doc.Delete(subId); } catch { }
                }
                t.Commit();
                reportLines.Add("✔ Đã xóa sạch CAD Import & Categories");
            }

            // 4. Triệt tiêu cảnh báo & làm mờ nút Warnings
            if (IncludeWarningsSuppress && WarningCount > 0)
            {
                var (mepUpdated, marksFixed) = ResolveAllWarningsInternal();
                reportLines.Add($"✔ Đã triệt tiêu cảnh báo (Mờ nút Warnings)");
            }
            else if (IncludeWarningsSuppress && !GlobalWarningSuppressorManager.IsEnabled)
            {
                GlobalWarningSuppressorManager.Toggle(_uidoc.Application.Application);
                reportLines.Add("✔ Đã kích hoạt Chế độ Ẩn Cảnh Báo Ngầm");
            }

            // 5. Purge Unused Elements
            if (IncludePurgeUnused)
            {
                int purged = ExecutePurgeInternal();
                if (purged > 0)
                {
                    reportLines.Add($"✔ Đã Purge {purged:N0} phần tử/Type không dùng");
                }
            }

            sw.Stop();
            ScanAll();

            StatusMessage = reportLines.Count > 0
                ? $"Hoàn tất dọn dẹp trong {sw.Elapsed.TotalSeconds:F1}s! {string.Join(" · ", reportLines)}"
                : "Không có mục nào được chọn hoặc mô hình đã sạch sẵn.";

            var summaryDialog = new TaskDialog("BIM TOOL · Dọn Dẹp Hoàn Tất")
            {
                MainInstruction = "Đã dọn dẹp mô hình để gửi đi thành công!",
                MainContent = $"Mô hình: {_doc.Title}\n" +
                              $"Thời gian xử lý: {sw.Elapsed.TotalSeconds:F1} giây\n\n" +
                              "KẾT QUẢ THỰC HIỆN:\n" +
                              (reportLines.Count > 0 ? string.Join("\n", reportLines.Select(r => "• " + r)) : "• Không có thay đổi nào cần xử lý") + "\n\n" +
                              $"Dung lượng file hiện tại: {FileSizeText}",
                ExpandedContent = "CAM KẾT AN TOÀN TUYỆT ĐỐI:\n" +
                                  "✔ Bảo toàn 100% tất cả các đoạn ống MEP, phụ kiện và thiết bị thực tế.\n" +
                                  "✔ Bảo toàn 100% các View, Sheet và hình ảnh vật liệu dự án.\n" +
                                  "✔ Thao tác được đóng gói trong Transaction: Bạn có thể nhấn Ctrl+Z trong Revit để hoàn tác toàn bộ bất cứ lúc nào.\n\n" +
                                  "MẸO NHỎ ĐỂ GIẢM TỐI ĐA DUNG LƯỢNG:\n" +
                                  "Vào File ➔ Save As ➔ Project (Bật tùy chọn: ☑ Compact File) trước khi gửi file.",
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            summaryDialog.Show();
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi trong quá trình dọn dẹp tổng thể: " + ex.Message;
            Log.Write("ExecuteAllCleanup error: " + ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
