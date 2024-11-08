using System.Linq.Expressions;
using Marten;
using Marten.Patching;

namespace marten_docs_pii;

public static class MaskingExtensions
{
    public static void UseMaskingRulesForProtectedInformation(this StoreOptions opts)
    {
        MaskingRules.Initialize(opts);
    }

    public static MartenRegistry.DocumentMappingExpression<T> AddMaskingRuleForProtectedInformation<T>(
        this MartenRegistry.DocumentMappingExpression<T> documentMappingExpression, 
        Expression<Func<T, object>> memberExpression, object maskValue) where T : class
    {
        MaskingRules.Instance.AddMaskingRule(memberExpression, maskValue);
        return documentMappingExpression;
    }

    public static void ApplyMaskForProtectedInformation<T>(this IPatchExpression<T> patcher)
    {
        var patches = MaskingRules.Instance.GeneratePatches<Person>();

        foreach (var patch in patches)
        {
            patcher = patcher.Set(patch.Key, patch.Value);
        }
    }
}