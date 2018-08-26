using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface ICommentBlockBrowserViewModel : IToolViewModel
    {
        void ClearDown();
        bool IsVisible { get; set; }
    }

    internal class CommentBlockBrowserViewModel : ToolViewModel, ICommentBlockBrowserViewModel
    {
        private IEnumerable<IItemFilterCommentBlockViewModel> _itemFilterCommentBlockViewModels;
        private IItemFilterCommentBlockViewModel _selectedItemFilterCommentBlockViewModel;
        private CollectionView _commentBlocksView;
        private string _searchText;

        public CommentBlockBrowserViewModel() : base("Section Browser")
        {
            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/add_section_icon.png");
            icon.EndInit();
            IconSource = icon;
            _searchText = "";
            _commentBlocksView = (CollectionView)CollectionViewSource.GetDefaultView(new List<IItemFilterCommentBlockViewModel>());

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                switch (message.Notification)
                {
                    case "SectionsChanged":
                    {
                        OnActiveDocumentChanged(this, EventArgs.Empty);
                        break;
                    }
                }
            });
        }

        public const string ToolContentId = "SectionBrowserTool";

        public IEnumerable<IItemFilterCommentBlockViewModel> ItemFilterCommentBlockViewModels
        {
            get => _itemFilterCommentBlockViewModels;
            private set
            {
                _itemFilterCommentBlockViewModels = value;

                _commentBlocksView = _itemFilterCommentBlockViewModels != null ?
                    (CollectionView)CollectionViewSource.GetDefaultView(_itemFilterCommentBlockViewModels)
                    : (CollectionView)CollectionViewSource.GetDefaultView(new List<IItemFilterCommentBlockViewModel>());

                _commentBlocksView.Filter = SearchFilter;
                _commentBlocksView.Refresh();
                RaisePropertyChanged("CommentBlocksView");
            }
        }

        public CollectionView CommentBlocksView
        {
            get => _commentBlocksView;
            private set
            {
                _commentBlocksView = value;
            }
        }

        public IItemFilterCommentBlockViewModel SelectedItemFilterCommentBlockViewModel
        {
            get => _selectedItemFilterCommentBlockViewModel;
            set
            {
                _selectedItemFilterCommentBlockViewModel = value;
                if (AvalonDockWorkspaceViewModel.ActiveDocument.IsScript)
                {
                    AvalonDockWorkspaceViewModel.ActiveScriptViewModel.CommentBlockBrowserBrowserSelectedBlockViewModel = value;
                }
                RaisePropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _commentBlocksView.Refresh();
                RaisePropertyChanged();
                RaisePropertyChanged("CommentBlocksView");
            }
        }

        protected override void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (AvalonDockWorkspaceViewModel.ActiveScriptViewModel != null && AvalonDockWorkspaceViewModel.ActiveDocument.IsScript)
            {
                ItemFilterCommentBlockViewModels = AvalonDockWorkspaceViewModel.ActiveScriptViewModel.ItemFilterCommentBlockViewModels;
            }
            else
            {
               ClearDown();
            }
        }

        public void ClearDown()
        {
            ItemFilterCommentBlockViewModels = null;
            SelectedItemFilterCommentBlockViewModel = null;
        }

        private bool SearchFilter(object obj)
        {
            if (string.IsNullOrEmpty(_searchText))
                return true;

            var block = obj as IItemFilterCommentBlockViewModel;
            var searchWords = Regex.Split(_searchText, @"\s+");
            foreach(var word in searchWords)
            {
                if (string.IsNullOrEmpty(word))
                    continue;

                if (block.Comment.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
