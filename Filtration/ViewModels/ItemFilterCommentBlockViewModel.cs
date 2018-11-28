using Filtration.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Text.RegularExpressions;

namespace Filtration.ViewModels
{
    internal interface IItemFilterCommentBlockViewModel : IItemFilterBlockViewModelBase
    {
        IItemFilterCommentBlock ItemFilterCommentBlock { get; }
        string Comment { get; }
        bool IsExpanded { get; set; }
        bool HasVisibleChild { get; }
        int ChildCount { get; set; }
    }

    internal class ItemFilterCommentBlockViewModel : ItemFilterBlockViewModelBase, IItemFilterCommentBlockViewModel
    {
        private bool _isExpanded;
        private int _childCount;
        private int _visibleChildCount;

        public ItemFilterCommentBlockViewModel()
        {
            _isExpanded = true;
            _childCount = 0;
            _visibleChildCount = 0;

            ToggleSectionCommand = new RelayCommand(OnToggleSectionCommand);
        }

        public override void Initialise(IItemFilterBlockBase itemFilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            _parentScriptViewModel = itemFilterScriptViewModel;
            ItemFilterCommentBlock = itemFilterBlock as IItemFilterCommentBlock;
            BaseBlock = ItemFilterCommentBlock;

            base.Initialise(itemFilterBlock, itemFilterScriptViewModel);
        }

        public RelayCommand ToggleSectionCommand { get; }

        public IItemFilterCommentBlock ItemFilterCommentBlock { get; private set; }

        public string Comment
        {
            get => ItemFilterCommentBlock.Comment;
            set
            {
                if (ItemFilterCommentBlock.Comment != value)
                {
                    ItemFilterCommentBlock.Comment = value;
                    IsDirty = true;
                    RaisePropertyChanged();
                    RaisePropertyChanged("Header");
                }
            }
        }

        public string Header
        {
            get
            {
                string[] commentLines = ItemFilterCommentBlock.Comment.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var titleOffset = 1;
                if (commentLines.Length > 1 && !Regex.Match(commentLines[0], "[a-zA-Z]+").Success)
                {
                    titleOffset = 3;
                    commentLines[0] = commentLines[1];
                }

                commentLines[0] = commentLines[0].TrimStart(' ');
                if (commentLines[0].Length > 80)
                {
                    commentLines[0] = commentLines[0].Substring(0, 80) + " (...)";
                }
                else if (commentLines.Length > titleOffset)
                {
                    commentLines[0] = commentLines[0] + " (...)";
                }

                return commentLines[0];
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
            }
        }

        public int ChildCount
        {
            get => _childCount;
            set
            {
                _childCount = value;
                RaisePropertyChanged();
            }
        }

        public int VisibleChildCount
        {
            get => _visibleChildCount;
            set
            {
                _visibleChildCount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasVisibleChild));
            }
        }

        public bool HasVisibleChild
        {
            get => (_visibleChildCount > 0);
        }

        private void OnToggleSectionCommand()
        {
            _parentScriptViewModel.ToggleSection(this);
        }
    }
}