using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class StringListBlockItem : BlockItemBase
    {
        protected StringListBlockItem()
        {
            Items = new ObservableCollection<string>();
            Items.CollectionChanged += OnItemsCollectionChanged;
        }

        public ObservableCollection<string> Items { get; protected set; }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Items");
            OnPropertyChanged("SummaryText");
        }
    }
}
