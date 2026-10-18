using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class WorkingSectionViewModel : ViewModelBase
{
    private readonly UIDocument _uiDoc;
    private readonly Document _doc;
    private readonly ViewPlan _activePlanView;

    private string _selectedUser = "User1";
    private bool _createVer = true;
    private bool _createHoz = true;
    private string _statusMessage = "Sẵn sàng tạo mặt cắt làm việc.";
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
                OnPropertyChanged(nameof(TargetVerName));
                OnPropertyChanged(nameof(TargetHozName));
            }
        }
    }

    public string TargetVerName => $"{SelectedUser}_Ver";
    public string TargetHozName => $"{SelectedUser}_Hoz";

    public bool CreateVer
    {
        get => _createVer;
        set
        {
            if (SetProperty(ref _createVer, value))
            {
                OnPropertyChanged(nameof(CanExecute));
                ExecuteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CreateHoz
    {
        get => _createHoz;
        set
        {
            if (SetProperty(ref _createHoz, value))
            {
                OnPropertyChanged(nameof(CanExecute));
                ExecuteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanExecute => (CreateVer || CreateHoz) && !IsBusy;

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

    public string ActiveViewName => _activePlanView.Name;

    public RelayCommand ExecuteCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action? RequestClose;

    public WorkingSectionViewModel(UIDocument uiDoc, ViewPlan activePlanView)
    {
        _uiDoc = uiDoc ?? throw new ArgumentNullException(nameof(uiDoc));
        _doc = uiDoc.Document;
        _activePlanView = activePlanView ?? throw new ArgumentNullException(nameof(activePlanView));

        ExecuteCommand = new RelayCommand(_ => ExecuteCreateSections(), _ => CanExecute);
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
    }

    private void ExecuteCreateSections()
    {
        if (!CanExecute) return;

        IsBusy = true;
        StatusMessage = "Đang tính toán tâm công trình và tạo mặt cắt...";

        try
        {
            // 1. Tìm ViewFamilyType cho Section
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

            // 2. Tính toán cao độ kéo qua tất cả các Level
            var levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            double zMin;
            double zMax;
            if (levels.Any())
            {
                double lowestElev = levels.First().Elevation;
                double highestElev = levels.Last().Elevation;
                // Đáy cách sàn thấp nhất -1000 mm, Đỉnh cách sàn cao nhất +3000 mm
                zMin = lowestElev - (1000.0 / 304.8);
                zMax = highestElev + (3000.0 / 304.8);
            }
            else
            {
                zMin = -10.0;
                zMax = 100.0;
            }

            double sectionHeight = zMax - zMin;

            // 3. Quét toàn bộ lưới trục (Grid) để xác định tâm công trình và Grid dài nhất
            var allGrids = new FilteredElementCollector(_doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            double maxGridLength = 0;
            double minGridX = double.MaxValue;
            double maxGridX = double.MinValue;
            double minGridY = double.MaxValue;
            double maxGridY = double.MinValue;

            foreach (var grid in allGrids)
            {
                if (grid.Curve != null)
                {
                    double len = grid.Curve.Length;
                    if (len > maxGridLength)
                    {
                        maxGridLength = len;
                    }

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

            // Dài hơn Grid dài nhất hoặc bao trọn hệ Grid + 3000 mm (khoảng 10 feet)
            double extraLength = 3000.0 / 304.8;

            if (allGrids.Any() && minGridX < maxGridX && minGridY < maxGridY)
            {
                // Căn giữa công trình (Tâm của hệ lưới trục Grid)
                centerWorld = new XYZ((minGridX + maxGridX) * 0.5, (minGridY + maxGridY) * 0.5, zMin);

                double spanX = maxGridX - minGridX;
                double spanY = maxGridY - minGridY;

                // Chiều dài luôn dài hơn Grid dài nhất và phủ trọn bề rộng hệ Grid công trình
                lengthHoz = Math.Max(spanX, maxGridLength) + extraLength;
                lengthVer = Math.Max(spanY, maxGridLength) + extraLength;
            }
            else
            {
                // Fallback: Nếu mô hình không có Grid thì căn theo CropBox
                BoundingBoxXYZ cropBox = _activePlanView.CropBox;
                if (cropBox != null)
                {
                    XYZ centerLocal = (cropBox.Min + cropBox.Max) * 0.5;
                    centerWorld = cropBox.Transform.OfPoint(centerLocal);
                    double cropWidth = Math.Abs(cropBox.Max.X - cropBox.Min.X);
                    double cropHeight = Math.Abs(cropBox.Max.Y - cropBox.Min.Y);
                    lengthHoz = cropWidth + extraLength;
                    lengthVer = cropHeight + extraLength;
                }
                else
                {
                    centerWorld = new XYZ(0, 0, zMin);
                    lengthHoz = 150.0;
                    lengthVer = 150.0;
                }
            }

            // 4. Quét lưới trục để né trùng Grid (Grid Avoidance)
            double safeTolerance = 300.0 / 304.8; // 300 mm
            double shiftStep = 500.0 / 304.8;     // 500 mm né Grid

            XYZ centerHoz = new XYZ(centerWorld.X, centerWorld.Y, zMin);
            XYZ centerVer = new XYZ(centerWorld.X, centerWorld.Y, zMin);

            foreach (var grid in allGrids)
            {
                if (grid.Curve is Line line)
                {
                    XYZ p0 = line.GetEndPoint(0);
                    XYZ p1 = line.GetEndPoint(1);

                    // Grid dọc (X xấp xỉ không đổi)
                    if (Math.Abs(p0.X - p1.X) < 0.05)
                    {
                        double gridX = (p0.X + p1.X) * 0.5;
                        if (Math.Abs(centerVer.X - gridX) <= safeTolerance)
                        {
                            centerVer = new XYZ(centerVer.X + shiftStep, centerVer.Y, centerVer.Z);
                        }
                    }

                    // Grid ngang (Y xấp xỉ không đổi)
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

            double farClipOffset = 1000.0 / 304.8; // Đúng 1000 mm

            var results = new List<string>();

            using (var trans = new Transaction(_doc, "Tạo Mặt Cắt MEP Làm Việc"))
            {
                trans.Start();

                // A. Tạo mặt cắt Dọc (Ver)
                if (CreateVer)
                {
                    string verName = TargetVerName;
                    var boxVer = CreateSectionBox(centerVer, lengthVer, sectionHeight, farClipOffset, isVertical: true);
                    CreateOrUpdateSection(verName, sectionVft.Id, boxVer, farClipOffset, results);
                }

                // B. Tạo mặt cắt Ngang (Hoz)
                if (CreateHoz)
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
                + "\n\n• Chiều sâu nhìn (Far Clip): 1000 mm"
                + "\n• Chiều dài: Dài hơn Grid dài nhất (+3000 mm)"
                + "\n• Chiều cao: Kéo qua toàn bộ các tầng trong dự án";

            TaskDialog.Show("Tạo Mặt Cắt MEP", dialogMsg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khi tạo mặt cắt: {ex.Message}";
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
            // Mặt cắt Dọc (Ver): đường cắt dọc trục Y (Nam -> Bắc), nhìn sang phía Đông (+X)
            transform.BasisX = new XYZ(0, 1, 0);
            transform.BasisY = new XYZ(0, 0, 1);
            transform.BasisZ = transform.BasisX.CrossProduct(transform.BasisY); // (1, 0, 0)
        }
        else
        {
            // Mặt cắt Ngang (Hoz): đường cắt ngang trục X (Tây -> Đông), nhìn về phía Nam (-Y)
            transform.BasisX = new XYZ(1, 0, 0);
            transform.BasisY = new XYZ(0, 0, 1);
            transform.BasisZ = transform.BasisX.CrossProduct(transform.BasisY); // (0, -1, 0)
        }

        box.Transform = transform;
        box.Min = new XYZ(-length * 0.5, 0, 0);
        box.Max = new XYZ(length * 0.5, height, farClipOffset);

        return box;
    }

    private void CreateOrUpdateSection(string viewName, ElementId vftId, BoundingBoxXYZ box, double farClipOffset, List<string> results)
    {
        // Kiểm tra xem đã tồn tại mặt cắt có tên này chưa
        var existingSection = new FilteredElementCollector(_doc)
            .OfClass(typeof(ViewSection))
            .Cast<ViewSection>()
            .FirstOrDefault(v => string.Equals(v.Name, viewName, StringComparison.OrdinalIgnoreCase));

        if (existingSection != null)
        {
            // Cập nhật lại BoundingBox và FarClipOffset
            existingSection.CropBox = box;
            SetFarClipOffset(existingSection, farClipOffset);
            results.Add($"Cập nhật '{viewName}'");
        }
        else
        {
            // Tạo mới ViewSection
            var newSection = ViewSection.CreateSection(_doc, vftId, box);
            newSection.Name = viewName;
            SetFarClipOffset(newSection, farClipOffset);
            results.Add($"Tạo mới '{viewName}'");
        }
    }

    private static void SetFarClipOffset(ViewSection section, double offsetFeet)
    {
        // Gán tham số VIEWER_BOUND_OFFSET_FAR
        var param = section.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
        if (param != null && !param.IsReadOnly)
        {
            param.Set(offsetFeet);
        }

        // Bật chế độ Clip with line
        var clipActiveParam = section.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
        if (clipActiveParam != null && !clipActiveParam.IsReadOnly)
        {
            clipActiveParam.Set(1);
        }
    }
}