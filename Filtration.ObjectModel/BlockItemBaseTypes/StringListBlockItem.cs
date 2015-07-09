using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Filtration.ObjectModel.LootExplosionStudio;

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
                           string.Format("\"{0}\"",
                               string.Join("\" \"", enumerable.ToArray()));
                }

                return string.Empty;
            }
        }

        public ObservableCollection<string> Items { get; protected set; }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Items");
            OnPropertyChanged("SummaryText");
        }

        public abstract string GetLootItemProperty(LootItem lootItem);

        public virtual bool MatchesLootItem(LootItem lootItem)
        {
            return Items.Any(i => i == GetLootItemProperty(lootItem));
        }
    }
}
