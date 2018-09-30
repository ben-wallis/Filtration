using System;
using System.Linq;
using System.Windows.Media.Imaging;
using Filtration.Parser.Interface.Services;
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

            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                if (message.Notification == "LastSelectedBlockChanged")
                    OnLastSelectedBlockChanged(this, EventArgs.Empty);
                else if (message.Notification == "ActiveDocumentChanged")
                    OnLastSelectedBlockChanged(this, EventArgs.Empty);
            });

        }
        
        public const string ToolContentId = "BlockOutputPreviewTool";

        public string PreviewText
        {
            get => _previewText;
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

        private void OnLastSelectedBlockChanged(object sender, EventArgs e)
        {
            if (AvalonDockWorkspaceViewModel.ActiveScriptViewModel?.SelectedBlockViewModels == null || 
                AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModels.Count == 0 ||
                AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModels.FirstOrDefault() == null)
            {
                PreviewText = string.Empty;
                return;
            }
            
            PreviewText = AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SelectedBlockViewModels
                .Select(s => _itemFilterBlockTranslator.TranslateItemFilterBlockBaseToString(s.BaseBlock))
                .Aggregate((prev, curr) => prev + Environment.NewLine + Environment.NewLine + curr);
        }
    }
}
