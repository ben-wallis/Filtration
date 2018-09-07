using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
	/// <summary>
	/// Each of the sizes supported by the MinimapIcon block rule.
	/// </summary>
	/// <remarks>
	/// The ordering here should match the ordering of the sizes within the source image.
	/// </remarks>
	public enum IconSize
    {
        [Description("Largest")]
        Largest,
        [Description("Medium")]
        Medium,
        [Description("Small")]
        Small
    }
}
