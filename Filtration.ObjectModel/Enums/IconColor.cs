using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
	/// <summary>
	/// Each of the colors supported by the MinimapIcon block rule.
	/// </summary>
	/// <remarks>
	/// The ordering here should match the ordering of the colors within the source image.
	/// </remarks>
	public enum IconColor
    {
		[Description("Blue")]
		Blue,
		[Description("Green")]
		Green,
		[Description("Brown")]
		Brown,
		[Description("Red")]
        Red,
        [Description("White")]
        White,
        [Description("Yellow")]
        Yellow
    }
}