using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Filtration.Enums;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class SocketGroupBlockItemBase : BlockItemBase
    {
        protected SocketGroupBlockItemBase()
        {
            SocketColorGroups = new ObservableCollection<List<SocketColor>>();
            SocketColorGroups.CollectionChanged += OnSocketColorGroupsCollectionChanged;
        }
        public ObservableCollection<List<SocketColor>> SocketColorGroups { get; private set; }

        private void OnSocketColorGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("SocketColorGroups");
        }
    }
}
