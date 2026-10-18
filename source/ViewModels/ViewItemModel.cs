using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;

namespace DSCons.Revit.Starter.ViewModels;

public class TemplateOption
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}

public class ViewItemModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _status = string.Empty;
    private TemplateOption? _selectedTemplate;

    public ElementId ViewId { get; set; } = ElementId.InvalidElementId;
    public string ViewName { get; set; } = string.Empty;
    public string ViewType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Discipline { get; set; } = string.Empty;
    public string UserNumber { get; set; } = string.Empty;
    public string CurrentTemplateName { get; set; } = "<None>";
    public ElementId CurrentTemplateId { get; set; } = ElementId.InvalidElementId;
    public string SuggestedTemplateName { get; set; } = string.Empty;
    public ElementId SuggestedTemplateId { get; set; } = ElementId.InvalidElementId;
    public string TargetFilterName { get; set; } = string.Empty;

    public TemplateOption? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (_selectedTemplate != value)
            {
                _selectedTemplate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasTargetTemplate));
                OnPropertyChanged(nameof(EffectiveTemplateId));
                OnPropertyChanged(nameof(EffectiveTemplateName));
            }
        }
    }

    public bool HasTargetTemplate => SelectedTemplate != null && SelectedTemplate.Id != ElementId.InvalidElementId;
    public ElementId EffectiveTemplateId => SelectedTemplate?.Id ?? ElementId.InvalidElementId;
    public string EffectiveTemplateName => SelectedTemplate?.Name ?? "-";

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
