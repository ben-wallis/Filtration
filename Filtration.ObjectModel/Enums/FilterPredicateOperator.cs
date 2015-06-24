using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
    public enum FilterPredicateOperator
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
