using System.Windows.Media;

namespace Filtration.ObjectModel
{
    public class ReplaceColorsParameterSet
    {
        public Color OldTextColor { get; set; }
        public Color NewTextColor { get; set; }
        public Color OldBackgroundColor { get; set; }
        public Color NewBackgroundColor { get; set; }
        public Color OldBorderColor { get; set; }
        public Color NewBorderColor { get; set; }
        public bool ReplaceTextColor { get; set; }
        public bool ReplaceBackgroundColor { get; set; }
        public bool ReplaceBorderColor { get; set; }
    }
}
