using System.ComponentModel;
using System.Windows.Media;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel
{
    public interface IItemFilterBlockItem : INotifyPropertyChanged
    {
        string PrefixText { get; }
        string OutputText { get; }
        string DisplayHeading { get; }
        string SummaryText { get; }
        Color SummaryBackgroundColor { get; }
        Color SummaryTextColor { get; }
        int MaximumAllowed { get; }
        BlockItemOrdering SortOrder { get; }
        bool IsDirty { get; }
        string Comment { get; set; }
    }
}
