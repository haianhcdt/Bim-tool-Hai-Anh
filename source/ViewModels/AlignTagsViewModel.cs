using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DSCons.Revit.Starter.Core.BatchTag;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.ViewModels;

public sealed class AlignTagsViewModel : ViewModelBase
{
    private bool _isHorizontal = true;
    private bool _isVertical = false;
    private bool _useFirstSelected = true;
    private bool _useAverage = false;
    private readonly List<TagPositionItem> _rawItems;

    public ObservableCollection<TagPositionItem> PreviewItems { get; }

    public bool IsHorizontal
    {
        get => _isHorizontal;
        set
        {
            if (SetProperty(ref _isHorizontal, value) && value)
            {
                SetProperty(ref _isVertical, false, nameof(IsVertical));
                Recalculate();
            }
        }
    }

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            if (SetProperty(ref _isVertical, value) && value)
            {
                SetProperty(ref _isHorizontal, false, nameof(IsHorizontal));
                Recalculate();
            }
        }
    }

    public bool UseFirstSelected
    {
        get => _useFirstSelected;
        set
        {
            if (SetProperty(ref _useFirstSelected, value) && value)
            {
                SetProperty(ref _useAverage, false, nameof(UseAverage));
                Recalculate();
            }
        }
    }

    public bool UseAverage
    {
        get => _useAverage;
        set
        {
            if (SetProperty(ref _useAverage, value) && value)
            {
                SetProperty(ref _useFirstSelected, false, nameof(UseFirstSelected));
                Recalculate();
            }
        }
    }

    public int TotalCount => PreviewItems.Count;

    public string SummaryText => $"Đã chọn {TotalCount} thẻ ghi chú sẵn sàng căn chỉnh.";

    public string PrimaryActionText => $"Căn chỉnh ({TotalCount} nhãn)";

    public bool DialogResult { get; private set; }

    public Action? RequestClose { get; set; }

    public ICommand AlignCommand { get; }
    public ICommand CancelCommand { get; }

    public AlignTagsViewModel(IEnumerable<TagPositionItem> items)
    {
        _rawItems = new List<TagPositionItem>(items ?? Array.Empty<TagPositionItem>());
        PreviewItems = new ObservableCollection<TagPositionItem>();

        AlignCommand = new RelayCommand(_ => OnAlign());
        CancelCommand = new RelayCommand(_ => OnCancel());

        Recalculate();
    }

    private void Recalculate()
    {
        var direction = IsHorizontal ? TagAlignmentDirection.Horizontal : TagAlignmentDirection.Vertical;
        var reference = UseFirstSelected ? TagAlignmentReference.FirstSelected : TagAlignmentReference.Average;

        var computed = TagAlignmentCalculator.CalculateAlignment(_rawItems, direction, reference, minSpacing: 2.5);

        PreviewItems.Clear();
        foreach (var item in computed)
        {
            PreviewItems.Add(item);
        }
    }

    private void OnAlign()
    {
        DialogResult = true;
        RequestClose?.Invoke();
    }

    private void OnCancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}