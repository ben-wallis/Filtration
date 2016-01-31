using System;
using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
    [Serializable]
    public enum SocketColor
    {
        Yellow,
        [Description("R")]
        Red,
        [Description("G")]
        Green,
        [Description("B")]
        Blue,
        [Description("W")]
        White
    }
}
