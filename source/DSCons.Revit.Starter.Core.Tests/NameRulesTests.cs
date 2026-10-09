using DSCons.Revit.Starter.Core.Validation; using Xunit;
namespace DSCons.Revit.Starter.Core.Tests;
public sealed class NameRulesTests
{
    [Fact] public void Sanitize_RemovesInvalidRevitNameCharacters(){Assert.Equal("Ống Chiller",NameRules.Sanitize("  Ống: Chiller?  "));}
    [Fact] public void Sanitize_ReturnsEmptyForWhitespace(){Assert.Equal(string.Empty,NameRules.Sanitize("   "));}
}
