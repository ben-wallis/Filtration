using System.Collections.ObjectModel;
using System.Linq;
using Filtration.Common.ViewModels;
using Filtration.ObjectModel;
using GalaSoft.MvvmLight;

namespace Filtration.ViewModels
{
    internal class ItemFilterBlockGroupViewModel : ViewModelBase
    {
        private bool? _isShowChecked;
        private bool? _isEnableChecked;
        private bool _showReentrancyCheck;
        private bool _enableReentrancyCheck;
        private bool _showPostMapComplete;
        private bool _enablePostMapComplete;
        private bool _isExpanded;

        public ItemFilterBlockGroupViewModel()
        {
            ChildGroups = new ObservableCollection<ItemFilterBlockGroupViewModel>();
        }

        public ItemFilterBlockGroupViewModel(ItemFilterBlockGroup itemFilterBlockGroup, bool showAdvanced, ItemFilterBlockGroupViewModel parent)
        {
            GroupName = itemFilterBlockGroup.GroupName;
            ParentGroup = parent;
            Advanced = itemFilterBlockGroup.Advanced;
            SourceBlockGroup = itemFilterBlockGroup;
            IsShowChecked = itemFilterBlockGroup.IsShowChecked;
            IsEnableChecked = itemFilterBlockGroup.IsEnableChecked;

            ChildGroups = new ObservableCollection<ItemFilterBlockGroupViewModel>();
            foreach (var childGroup in itemFilterBlockGroup.ChildGroups.Where(c => showAdvanced || !c.Advanced))
            {
                ChildGroups.Add(new ItemFilterBlockGroupViewModel(childGroup, showAdvanced, this));
            }

            if (ChildGroups.Any())
            {
                SetIsCheckedBasedOnChildGroups();
            }
        }

        private void SetIsCheckedBasedOnChildGroups()
        {
            if (ChildGroups.All(g => g.IsShowChecked == true))
            {
                IsShowChecked = true;
            }
            else if (ChildGroups.Any(g => g.IsShowChecked == true || g.IsShowChecked == null))
            {
                IsShowChecked = null;
            }
            else
            {
                IsShowChecked = false;
            }

            if (ChildGroups.All(g => g.IsEnableChecked == true))
            {
                IsEnableChecked = true;
            }
            else if (ChildGroups.Any(g => g.IsEnableChecked == true || g.IsEnableChecked == null))
            {
                IsEnableChecked = null;
            }
            else
            {
                IsEnableChecked = false;
            }
        }

        public string GroupName { get; internal set; }
        public ItemFilterBlockGroupViewModel ParentGroup { get; internal set; }
        public ObservableCollection<ItemFilterBlockGroupViewModel> ChildGroups { get; internal set; }
        public bool Advanced { get; internal set; }
        public ItemFilterBlockGroup SourceBlockGroup { get; internal set; }

        public bool? IsShowChecked
        {
            get
            {
                return _isShowChecked;
            }
            set
            {
                if (!_showPostMapComplete)
                {
                    _isShowChecked = value;
                    _showPostMapComplete = true;
                }
                else
                {
                    if (_isShowChecked != value)
                    {

                        if (_showReentrancyCheck)
                        {
                            return;
                        }
                        _showReentrancyCheck = true;
                        _isShowChecked = value;
                        UpdateCheckState(true);
                        RaisePropertyChanged();
                        SourceBlockGroup.IsShowChecked = value ?? false;
                        _showReentrancyCheck = false;
                    }
                }
            }
        }

        public bool? IsEnableChecked
        {
            get
            {
                return _isEnableChecked;
            }
            set
            {
                if (!_enablePostMapComplete)
                {
                    _isEnableChecked = value;
                    _enablePostMapComplete = true;
                }
                else
                {
                    if (_isEnableChecked != value)
                    {

                        if (_enableReentrancyCheck)
                        {
                            return;
                        }
                        _enableReentrancyCheck = true;
                        _isEnableChecked = value;
                        UpdateCheckState(false);
                        RaisePropertyChanged();
                        SourceBlockGroup.IsEnableChecked = value ?? false;
                        _enableReentrancyCheck = false;
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

        private void UpdateCheckState(bool isShowCheck)
        {
            // update all children:
            if (ChildGroups.Count != 0)
            {
                UpdateChildrenCheckState(isShowCheck);
            }

            // update parent item
            if (ParentGroup != null)
            {
                var parentIsChecked = ParentGroup.DetermineCheckState(isShowCheck);
                if(isShowCheck)
                {
                    ParentGroup.IsShowChecked = parentIsChecked;
                }
                else
                {
                    ParentGroup.IsEnableChecked = parentIsChecked;
                }
            }
        }

        private void UpdateChildrenCheckState(bool isShowCheck)
        {
            if(isShowCheck)
            {
                foreach (var childGroup in ChildGroups.Where(c => IsShowChecked != null))
                {
                    childGroup.IsShowChecked = IsShowChecked;
                }
            }
            else
            {
                foreach (var childGroup in ChildGroups.Where(c => IsEnableChecked != null))
                {
                    childGroup.IsEnableChecked = IsEnableChecked;
                }
            }
        }

        private bool? DetermineCheckState(bool isShowCheck)
        {
            var allChildrenChecked = (isShowCheck ? ChildGroups.Count(x => x.IsShowChecked == true) :
                ChildGroups.Count(x => x.IsEnableChecked == true)) == ChildGroups.Count;
            if (allChildrenChecked)
            {
                return true;
            }

            var allChildrenUnchecked = (isShowCheck ? ChildGroups.Count(x => x.IsShowChecked == false) :
                ChildGroups.Count(x => x.IsEnableChecked == false)) == ChildGroups.Count;
            if (allChildrenUnchecked)
            {
                return false;
            }

            return null;
        }
    }
}
