using System.ComponentModel;
using System.Windows.Media;

namespace Filtration.Models
{
    internal interface IItemFilterBlockItem : INotifyPropertyChanged
    {
        string PrefixText { get; }
        int MaximumAllowed { get; }
        string DisplayHeading { get; }
        string SummaryText { get; }
        Color SummaryBackgroundColor { get; }
        Color SummaryTextColor { get; }
        int SortOrder { get; }
    }
}
