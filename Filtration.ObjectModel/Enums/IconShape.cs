using System.ComponentModel;

namespace Filtration.ObjectModel.Enums
{
	/// <summary>
	/// Each of the shapes supported by the MinimapIcon block rule.
	/// </summary>
	/// <remarks>
	/// The ordering here should match the ordering of the shapes within the source image.
	/// </remarks>
	public enum IconShape
    {
        [Description("Circle")]
        Circle,
        [Description("Diamond")]
        Diamond,
        [Description("Hexagon")]
        Hexagon,
        [Description("Square")]
        Square,
        [Description("Star")]
        Star,
        [Description("Triangle")]
        Triangle
    }
}