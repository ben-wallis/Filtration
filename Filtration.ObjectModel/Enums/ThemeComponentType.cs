using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
    public enum ThemeComponentType
    {
        [Description("Text")]
        TextColor,
        [Description("Background")]
        BackgroundColor,
        [Description("Border")]
        BorderColor,
        [Description("Font Size")]
        FontSize,
        [Description("Alert Sound")]
        AlertSound
    }
}
