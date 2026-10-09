using System; using System.Linq;
namespace DSCons.Revit.Starter.Core.Validation;
public static class NameRules
{
    private static readonly char[] InvalidCharacters={'\\',':','{','}','[',']','|',';','<','>','?','`','~'};
    public static string Sanitize(string? value){if(string.IsNullOrWhiteSpace(value))return string.Empty;var cleaned=new string(value!.Trim().Where(c=>!InvalidCharacters.Contains(c)).ToArray());return string.Join(" ",cleaned.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries));}
}
