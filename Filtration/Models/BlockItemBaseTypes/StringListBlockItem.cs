using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class StringListBlockItem : ILootFilterBlockItem
    {
        protected StringListBlockItem()
        {
            Items = new ObservableCollection<string>();
            Items.CollectionChanged += OnItemsCollectionChanged;
        }

        public abstract string PrefixText { get; }
        public abstract int MaximumAllowed { get; }
        public abstract string DisplayHeading { get; }

        public abstract string SummaryText { get; }
        public abstract Color SummaryBackgroundColor { get; }
        public abstract Color SummaryTextColor { get; }
        public abstract int SortOrder { get; }
        public ObservableCollection<string> Items { get; protected set; }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Items");
            OnPropertyChanged("SummaryText");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
