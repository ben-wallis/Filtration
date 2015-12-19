using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
    public enum FilterPredicateOperator
    {
        [Description("=")]
        Equal,
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
