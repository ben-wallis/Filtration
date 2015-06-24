using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
    public enum ItemRarity
    {
        [Description("Normal")]
        Normal,
        [Description("Magic")]
        Magic,
        [Description("Rare")]
        Rare,
        [Description("Unique")]
        Unique
    }
}
