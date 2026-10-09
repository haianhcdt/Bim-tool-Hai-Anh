#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit{}
    [System.AttributeUsage(System.AttributeTargets.All,Inherited=false)] internal sealed class RequiredMemberAttribute:System.Attribute{}
    [System.AttributeUsage(System.AttributeTargets.All,AllowMultiple=true,Inherited=false)] internal sealed class CompilerFeatureRequiredAttribute:System.Attribute{public CompilerFeatureRequiredAttribute(string featureName)=>FeatureName=featureName;public string FeatureName{get;}public bool IsOptional{get;set;}}
}
namespace System.Diagnostics.CodeAnalysis{[System.AttributeUsage(System.AttributeTargets.Constructor,Inherited=false)]internal sealed class SetsRequiredMembersAttribute:System.Attribute{}}
#endif
