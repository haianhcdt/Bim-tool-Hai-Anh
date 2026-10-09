namespace DSCons.Revit.Starter.Core.Views;

public static class WorkingViewNaming
{
    public const string DefaultPrefix = "3D_Working_";
    public const string DefaultOwner = "HaiAnh";

    public static string BuildWorkingViewName(string? userName = null)
    {
        var owner = string.IsNullOrWhiteSpace(userName) ? DefaultOwner : userName!.Trim();
        var sanitized = Validation.NameRules.Sanitize(owner);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = DefaultOwner;
        }
        return $"{DefaultPrefix}{sanitized.Replace(" ", "_")}";
    }
}
