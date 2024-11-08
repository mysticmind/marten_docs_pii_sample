using System.Linq.Expressions;
using Marten;
using Marten.Linq.Parsing;

namespace marten_docs_pii;

public class MaskingRules
{
    private readonly Dictionary<Type, List<KeyValuePair<string, object>>> _rules = new();
    private static volatile MaskingRules? _instance;
    private static readonly object _lock = new();
    private readonly Casing _casing;

    private MaskingRules(Casing casing)
    {
        _casing = casing;
    }

    public static void Initialize(StoreOptions opts)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("MaskingRules has already been initialized");
        }

        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = new MaskingRules(opts.Serializer().Casing);
            }
            else
            {
                throw new InvalidOperationException("MaskingRules has already been initialized");
            }
        }
    }

    public static MaskingRules Instance 
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    "Call UseMaskingRulesForProtectedInformation on StoreOptions before using it");
            }
            return _instance;
        }
    }

    public void AddMaskingRule<T>(
        Expression<Func<T, object>> propertySelector, 
        object maskValue) where T : class
    {
        var propertyPath = GetPropertyPath(propertySelector);
        
        if (!_rules.ContainsKey(typeof(T)))
        {
            _rules[typeof(T)] = [];
        }

        _rules[typeof(T)].Add(new KeyValuePair<string, object>(propertyPath, maskValue));
    }

    private string GetPropertyPath<T>(Expression<Func<T, object>> propertySelector)
    {
        var visitor = new MemberFinder();
        visitor.Visit(propertySelector);
        return string.Join(".", visitor.Members.Select(x => x.Name.FormatCase(_casing)));
    }

    public List<KeyValuePair<string, object>> GeneratePatches<T>()
    {
        return !_rules.TryGetValue(typeof(T), out var typeRules) 
            ? [] 
            : typeRules;
    }
}