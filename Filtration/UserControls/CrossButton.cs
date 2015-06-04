using System.Windows;
using System.Windows.Controls;

namespace Filtration.UserControls
{
    public class CrossButton : Button
    {
        static CrossButton()
        {
            //  Set the style key, so that our control template is used.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossButton),
                   new FrameworkPropertyMetadata(typeof(CrossButton)));
        }
    }
}
