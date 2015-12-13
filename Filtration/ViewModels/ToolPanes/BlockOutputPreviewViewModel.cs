using System;
using System.Windows.Media.Imaging;
using Filtration.Translators;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface IBlockOutputPreviewViewModel : IToolViewModel
    {
        bool IsVisible { get; set; }
        void ClearDown();
    }

    internal class BlockOutputPreviewViewModel : ToolViewModel, IBlockOutputPreviewViewModel
    {
        private readonly IItemFilterBlockTranslator _itemFilterBlockTranslator;
        private string _previewText;

        public BlockOutputPreviewViewModel(IItemFilterBlockTranslator itemFilterBlockTranslator) : base("Block Output Preview")
        {
            _itemFilterBlockTranslator = itemFilterBlockTranslator;
            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/block_output_preview_icon.png");
            icon.EndInit();
            IconSource = icon;

            IsVisible = false;

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                switch (message.Notification)
                {
                    case "SelectedBlockChanged":
                    {
                        OnSelectedBlockChanged(this, EventArgs.Empty);
                        break;
                    }
                    case "ActiveDocumentChanged":
                    {
                        OnSelectedBlockChanged(this, EventArgs.Empty);
                        break;
                    }
                }
            });

        }
        
        public const string ToolContentId = "BlockOutputPreviewTool";

        public string PreviewText
        {
            get { return _previewText; }
            private set
            {
                _previewText = value;
                RaisePropertyChanged();
            }
        }

        public void ClearDown()
        {
            PreviewText = string.Empty;
        }

        private void OnSelectedBlockChanged(object sender, EventArgs e)
        {
            if (AvalonDockWorkspaceViewModel.ActiveScriptViewModel?.SelectedBlockViewModel == null)
            {
                PreviewText = string.Empty;
                return;
            }

            PreviewText =
                _itemFilterBlockTranslator.TranslateItemFilterBlockToString(
                    AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModel.Block);
        }
    }
}
