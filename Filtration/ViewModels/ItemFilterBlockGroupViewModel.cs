using System;
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
            SourceBlockGroup.BlockGroupStatusChanged += OnSourceBlockGroupStatusChanged;
            IsShowChecked = itemFilterBlockGroup.IsShowChecked;
            IsEnableChecked = itemFilterBlockGroup.IsEnableChecked;

            ChildGroups = new ObservableCollection<ItemFilterBlockGroupViewModel>();
            foreach (var childGroup in itemFilterBlockGroup.ChildGroups.Where(c => showAdvanced || !c.Advanced))
            {
                ChildGroups.Add(new ItemFilterBlockGroupViewModel(childGroup, showAdvanced, this));
            }

            VisibleChildGroups = new ObservableCollection<ItemFilterBlockGroupViewModel>();
            foreach (var childGroup in ChildGroups.Where(item => !item.IsHidden))
            {
                VisibleChildGroups.Add(childGroup);
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
        public ObservableCollection<ItemFilterBlockGroupViewModel> VisibleChildGroups { get; internal set; }
        public bool Advanced { get; internal set; }
        public bool IsHidden
        {
            get => SourceBlockGroup.IsLeafNode;
        }
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
                        SourceBlockGroup.IsShowChecked = value;
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
                        SourceBlockGroup.IsEnableChecked = value;
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

        public void SetIsExpandedForAll(bool isExpanded)
        {
            IsExpanded = isExpanded;
            foreach(var child in VisibleChildGroups)
            {
                child.SetIsExpandedForAll(isExpanded);
            }
        }

        public void RecalculateCheckState()
        {
            _isShowChecked = DetermineCheckState(true);
            _isEnableChecked = DetermineCheckState(false);
            RaisePropertyChanged(nameof(IsShowChecked));
            RaisePropertyChanged(nameof(IsEnableChecked));
        }

        private void UpdateCheckState(bool isShowCheck)
        {
            // update all children:
            if (ChildGroups.Count != 0)
            {
                UpdateChildrenCheckState(isShowCheck);
            }

            // inform parent about the change
            if (ParentGroup != null)
            {
                var parentValue = isShowCheck ? ParentGroup.IsShowChecked : ParentGroup.IsEnableChecked;
                var ownValue = isShowCheck ? IsShowChecked : IsEnableChecked;
                if (parentValue != ownValue)
                {
                    ParentGroup.RecalculateCheckState();
                }
            }
        }

        private void UpdateChildrenCheckState(bool isShowCheck)
        {
            // Update children only when state is not null which means update is either from children
            // (all children must have same value to be not null) or from user
            if (isShowCheck && IsShowChecked != null)
            {
                foreach (var childGroup in ChildGroups.Where(c => IsShowChecked != null))
                {
                    childGroup.IsShowChecked = IsShowChecked;
                }
            }
            else if (IsEnableChecked != null)
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

        private void OnSourceBlockGroupStatusChanged(object sender, EventArgs e)
        {
            // We assume that source block group status is only changed by either view model
            // or related ItemFilterBlock if leaf node
            if(SourceBlockGroup.IsShowChecked != IsShowChecked)
            {
                IsShowChecked = SourceBlockGroup.IsShowChecked;
            }
            if (SourceBlockGroup.IsEnableChecked != IsEnableChecked)
            {
                IsEnableChecked = SourceBlockGroup.IsEnableChecked;
            }
        }

        public void ClearStatusChangeSubscriptions()
        {
            if (SourceBlockGroup != null)
            {
                SourceBlockGroup.BlockGroupStatusChanged -= OnSourceBlockGroupStatusChanged;
            }

            foreach (var child in ChildGroups)
            {
                child.ClearStatusChangeSubscriptions();
            }
        }
    }
}
