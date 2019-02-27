using System.ComponentModel;

namespace Filtration.Enums
{
    internal enum AutosaveInterval
    {
        [Description("Never")]
        Never = -1,
        [Description("5 sec")]
        FiveSec = 5 * 1000,
        [Description("10 sec")]
        TenSec = 10 * 1000,
        [Description("30 sec")]
        ThirtySec = 30 * 1000,
        [Description("1 min")]
        OneMin = 60 * 1000,
        [Description("5 min")]
        FiveMin = 5 * 60 * 1000
    }
}
