namespace DSCons.Revit.Starter.Core.Models;

public class ViewTemplateRuleResult
{
    public string ViewName { get; set; } = string.Empty;
    public bool IsMatched { get; set; }
    public string Scope { get; set; } = "UNKNOWN"; // WORKING, EXPORT, UNKNOWN
    public string Discipline { get; set; } = string.Empty;
    public string SuggestedTemplate { get; set; } = string.Empty;
    public string UserNumber { get; set; } = string.Empty;
    public string TargetFilterName { get; set; } = string.Empty;
}
