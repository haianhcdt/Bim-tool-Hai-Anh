using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DSCons.Revit.Starter.ViewModels;

/// <summary>Read-only profile presented by the branded About Me WPF window.</summary>
public sealed class AboutMeViewModel
{
    public AboutMeViewModel()
    {
        StudentName = StudentBranding.StudentName;
        Company = StudentBranding.Company;
        RoleLine = StudentBranding.Role + " · Revit API Programming";
        Initials = BuildInitials(StudentName);
        ContactDetails = new ReadOnlyCollection<AboutMeDetail>(new List<AboutMeDetail>
        {
            new AboutMeDetail("Điện thoại", StudentBranding.Phone),
            new AboutMeDetail("Công ty", Company),
            new AboutMeDetail("Địa chỉ", StudentBranding.Address),
            new AboutMeDetail("Email", ReadOptionalBrandingValue("Email", "Chưa cập nhật"))
        });
        SpecialtyTags = new ReadOnlyCollection<string>(new[] { "Revit MEP", "BIM Automation", "Revit API" });
    }

    public string StudentName { get; }

    public string Company { get; }

    public string RoleLine { get; }

    public string Initials { get; }

    public string ProfileDescription => "Hồ sơ nhận diện cho bộ công cụ Revit được cá nhân hóa. Bạn có thể phát triển công cụ và quy trình theo nhu cầu công việc của mình.";

    public string CompanySubtitle => "BIM · Revit API · MEP Automation";

    public string FooterText => Company + " | Revit API Programming Kit";

    public string WindowTitle => StudentName + " · Revit MEP Tools";

    public ReadOnlyCollection<AboutMeDetail> ContactDetails { get; }

    public ReadOnlyCollection<string> SpecialtyTags { get; }

    private static string BuildInitials(string name)
    {
        var parts = (name ?? string.Empty)
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0) return "MEP";
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }

    private static string ReadOptionalBrandingValue(string fieldName, string fallback)
    {
        var field = typeof(StudentBranding).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        var value = field?.GetValue(null) as string;
        return string.IsNullOrWhiteSpace(value) ? fallback : value!;
    }
}

/// <summary>A compact label/value row for a read-only WPF window.</summary>
public sealed class AboutMeDetail
{
    public AboutMeDetail(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }

    public string Value { get; }
}
