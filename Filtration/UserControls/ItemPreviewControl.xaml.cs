using System.Windows;
using System.Windows.Media;

namespace Filtration.UserControls
{
    public partial class ItemPreviewControl
    {
        public ItemPreviewControl()
        {
            InitializeComponent();
            // ReSharper disable once PossibleNullReferenceException
            (Content as FrameworkElement).DataContext = this;
        }

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor",
            typeof (Color),
            typeof(ItemPreviewControl),
            new FrameworkPropertyMetadata()
        );

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            "BackgroundColor",
            typeof(Color),
            typeof(ItemPreviewControl),
            new FrameworkPropertyMetadata()
        );

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
            "BorderColor",
            typeof(Color),
            typeof(ItemPreviewControl),
            new FrameworkPropertyMetadata()
        );

        public static readonly DependencyProperty BlockFontSizeProperty = DependencyProperty.Register(
            "BlockFontSize",
            typeof(double),
            typeof(ItemPreviewControl),
            new FrameworkPropertyMetadata((double)19)
        );

        public Color TextColor
        {
            get { return (Color) GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public double BlockFontSize
        {
            get
            {
                return (double)GetValue(BlockFontSizeProperty);
            }
            set { SetValue(BlockFontSizeProperty, value); }
        }
    }
}
