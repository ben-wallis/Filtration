using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Filtration.Annotations;

namespace Filtration.Models
{
    internal class ItemFilterBlockGroup : INotifyPropertyChanged
    {
        private bool? _isChecked;
        private bool _reentrancyCheck;

        public ItemFilterBlockGroup(string groupName, ItemFilterBlockGroup parent, bool advanced = false)
        {
            GroupName = groupName;
            ParentGroup = parent;
            Advanced = advanced;
            ChildGroups = new ObservableCollection<ItemFilterBlockGroup>();
        }
        
        public event EventHandler BlockGroupStatusChanged;
        public string GroupName { get; private set; }
        public ItemFilterBlockGroup ParentGroup { get; private set; }
        public ObservableCollection<ItemFilterBlockGroup> ChildGroups { get; private set; }

        public override string ToString()
        {
            var currentBlockGroup = this;

            var outputString = GroupName;

            // TODO: This is retarded, fix this.
            if (currentBlockGroup.ParentGroup != null)
            {
                while (currentBlockGroup.ParentGroup.ParentGroup != null)
                {
                    outputString = currentBlockGroup.ParentGroup.GroupName + " - " + outputString;
                    currentBlockGroup = currentBlockGroup.ParentGroup;
                }
            }

            return outputString;
        }

        public bool Advanced { get; private set; }

        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    if (_reentrancyCheck)
                    {
                        return;
                    }
                    _reentrancyCheck = true;
                    _isChecked = value;
                    UpdateCheckState();
                    OnPropertyChanged();

                    // Raise an event to let blocks that have this block group assigned that
                    // they might need to change their Action due to the block group status changing.
                    if (BlockGroupStatusChanged != null)
                    {
                        BlockGroupStatusChanged.Invoke(null, null);
                    }
                    _reentrancyCheck = false;
                }
            }
        }

        private void UpdateCheckState()
        {
            // update all children:
            if (ChildGroups.Count != 0)
            {
                UpdateChildrenCheckState();
            }

            // update parent item
            if (ParentGroup != null)
            {
                var parentIsChecked = ParentGroup.DetermineCheckState();
                ParentGroup.IsChecked = parentIsChecked;
            }
        }

        private void UpdateChildrenCheckState()
        {
            foreach (var childGroup in ChildGroups.Where(c => IsChecked != null && !c.Advanced))
            {
                childGroup.IsChecked = IsChecked;
            }
        }

        private bool? DetermineCheckState()
        {
            var allChildrenChecked = ChildGroups.Count(x => x.IsChecked == true) == ChildGroups.Count;
            if (allChildrenChecked)
            {
                return true;
            }

            var allChildrenUnchecked = ChildGroups.Count(x => x.IsChecked == false) == ChildGroups.Count;
            if (allChildrenUnchecked)
            {
                return false;
            }

            return null;
        }

        public ItemFilterBlockGroup Search(Func<ItemFilterBlockGroup, bool> predicate)
        {
            // if node is a leaf
            if (ChildGroups == null || ChildGroups.Count == 0)
            {
                return predicate(this) ? this : null;
            }

            var results = ChildGroups
                .Select(i => i.Search(predicate))
                .Where(i => i != null).ToList();

            if (results.Any())
            {
                var result = (ItemFilterBlockGroup)MemberwiseClone();
                result.ChildGroups = new ObservableCollection<ItemFilterBlockGroup>(results);
                return result;
            }
            return null;
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
