using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;
using Filtration.Enums;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class SocketGroupBlockItemBase : ILootFilterBlockItem
    {
        protected SocketGroupBlockItemBase()
        {
            SocketColorGroups = new ObservableCollection<List<SocketColor>>();
            SocketColorGroups.CollectionChanged += OnSocketColorGroupsCollectionChanged;
        }

        public abstract string PrefixText { get; }
        public abstract int MaximumAllowed { get; }
        public abstract string DisplayHeading { get; }

        public abstract string SummaryText { get; }
        public abstract Color SummaryBackgroundColor { get; }
        public abstract Color SummaryTextColor { get; }
        public abstract int SortOrder { get; }
        public ObservableCollection<List<SocketColor>> SocketColorGroups { get; private set; }

        private void OnSocketColorGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("SocketColorGroups");
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
