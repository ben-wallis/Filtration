using System.Windows.Media;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class ColorBlockItem : BlockItemBase, IAudioVisualBlockItem
    {
        private Color _color;

        protected ColorBlockItem()
        {
        }
        
        protected ColorBlockItem(Color color)
        {
            Color = color;
        }

        public override string SummaryText
        {
            get { return string.Empty; }
        }

        public override Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public override Color SummaryTextColor { get { return Colors.Transparent; } }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }
}
