using System.ComponentModel;

namespace Filtration.Enums
{
    internal enum FilterPredicateOperator
    {
        [Description("=")]
        Equal,
        [Description("!=")]
        NotEqual,
        [Description("<")]
        LessThan,
        [Description("<=")]
        LessThanOrEqual,
        [Description(">")]
        GreaterThan,
        [Description(">=")]
        GreaterThanOrEqual
    }
}
