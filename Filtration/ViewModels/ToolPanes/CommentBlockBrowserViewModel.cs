using System;
using System.Collections.Generic;
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

        public CommentBlockBrowserViewModel() : base("Section Browser")
        {
            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/add_section_icon.png");
            icon.EndInit();
            IconSource = icon;
            
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
                RaisePropertyChanged();
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
    }
}
