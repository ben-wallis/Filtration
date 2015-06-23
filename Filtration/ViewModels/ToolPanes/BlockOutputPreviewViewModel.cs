using System;
using System.Windows.Media.Imaging;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface IBlockOutputPreviewViewModel : IToolViewModel
    {
        bool IsVisible { get; set; }
    }

    internal class BlockOutputPreviewViewModel : ToolViewModel, IBlockOutputPreviewViewModel
    {
        public BlockOutputPreviewViewModel() : base("Block Output Preview")
        {
            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/block_output_preview_icon.png");
            icon.EndInit();
            IconSource = icon;

           IsVisible = false;
        }


        public const string ToolContentId = "BlockOutputPreviewTool";
    }
}
