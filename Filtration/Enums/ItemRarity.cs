using System.ComponentModel;

namespace Filtration.Enums
{
    internal enum ItemRarity
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
