using System.Collections.ObjectModel;
using System.Linq;
using Filtration.ObjectModel;

namespace Filtration.ViewModels
{
    internal class ItemFilterBlockGroupViewModel : FiltrationViewModelBase
    {
        private bool? _isChecked;
        private bool _reentrancyCheck;
        private bool _postMapComplete;
        private bool _isExpanded;

        public ItemFilterBlockGroupViewModel()
        {
            ChildGroups = new ObservableCollection<ItemFilterBlockGroupViewModel>();
        }

        public string GroupName { get; internal set; }
        public ItemFilterBlockGroupViewModel ParentGroup { get; internal set; }
        public ObservableCollection<ItemFilterBlockGroupViewModel> ChildGroups { get; internal set; }
        public bool Advanced { get; internal set; }
        public ItemFilterBlockGroup SourceBlockGroup { get; internal set; }

        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (!_postMapComplete)
                {
                    _isChecked = value;
                    _postMapComplete = true;
                }
                else
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
                        RaisePropertyChanged();
                        SourceBlockGroup.IsChecked = value ?? false;
                        _reentrancyCheck = false;
                    }
                }
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
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
            foreach (var childGroup in ChildGroups.Where(c => IsChecked != null))
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
    }
}
