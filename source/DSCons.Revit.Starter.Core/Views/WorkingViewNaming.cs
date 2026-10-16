namespace DSCons.Revit.Starter.Core.Views;

public static class WorkingViewNaming
{
    public const string DefaultPrefix = "3D_Working_";
    public const string PlanPrefix = "FL_Working_";
    public const string SectionPrefix = "SEC_Working_";
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

    public static string BuildWorking3DViewName(string? userName = null)
    {
        return BuildWorkingViewName(userName);
    }

    public static string BuildWorkingPlanName(string levelName, string? userName = null)
    {
        var owner = string.IsNullOrWhiteSpace(userName) ? DefaultOwner : userName!.Trim();
        var sanitizedOwner = Validation.NameRules.Sanitize(owner);
        if (string.IsNullOrWhiteSpace(sanitizedOwner))
        {
            sanitizedOwner = DefaultOwner;
        }

        var sanitizedLevel = Validation.NameRules.Sanitize(levelName);
        if (string.IsNullOrWhiteSpace(sanitizedLevel))
        {
            sanitizedLevel = "Level";
        }

        return $"{PlanPrefix}{sanitizedOwner.Replace(" ", "_")}_{sanitizedLevel.Replace(" ", "_")}";
    }

    public static string BuildWorkingSectionName(string suffix, string? userName = null)
    {
        var owner = string.IsNullOrWhiteSpace(userName) ? DefaultOwner : userName!.Trim();
        var sanitizedOwner = Validation.NameRules.Sanitize(owner);
        if (string.IsNullOrWhiteSpace(sanitizedOwner))
        {
            sanitizedOwner = DefaultOwner;
        }

        var sanitizedSuffix = Validation.NameRules.Sanitize(suffix);
        if (string.IsNullOrWhiteSpace(sanitizedSuffix))
        {
            sanitizedSuffix = "1";
        }

        return $"{SectionPrefix}{sanitizedOwner.Replace(" ", "_")}_{sanitizedSuffix.Replace(" ", "_")}";
    }
}