using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class ColorBlockItem : ILootFilterBlockItem, IAudioVisualBlockItem
    {
        private Color _color;

        protected ColorBlockItem()
        {
        }
        
        protected ColorBlockItem(Color color)
        {
            Color = color;
        }

        public abstract string PrefixText { get; }
        public abstract int MaximumAllowed { get; }

        public abstract string DisplayHeading { get; }

        public string SummaryText
        {
            get { return string.Empty; }
        }

        public Color SummaryBackgroundColor { get { return Colors.Transparent; } }
        public Color SummaryTextColor { get { return Colors.Transparent; } }
        public abstract int SortOrder { get; }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
