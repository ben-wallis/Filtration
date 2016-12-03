using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class StringListBlockItem : BlockItemBase
    {
        protected StringListBlockItem()
        {
            Items = new ObservableCollection<string>();
            Items.CollectionChanged += OnItemsCollectionChanged;
        }

        public override string OutputText
        {
            get
            {
                var enumerable = Items as IList<string> ?? Items.ToList();
                if (enumerable.Count > 0)
                {
                    return PrefixText + " " +
                           $"\"{string.Join("\" \"", enumerable.ToArray())}\"";
                }

                return string.Empty;
            }
        }

        public ObservableCollection<string> Items { get; protected set; }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsDirty = true;
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SummaryText));
        }
    }
}
