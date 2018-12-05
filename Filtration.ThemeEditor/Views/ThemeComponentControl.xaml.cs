using System.Windows.Input;

namespace Filtration.ThemeEditor.Views
{
    public partial class ThemeComponentControl
    {
        public ThemeComponentControl()
        {
            InitializeComponent();
        }

        private void ColorPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // For some reason if we don't mark OnMouseDown events as handled for this control
            // it ignores them and they end up getting picked up by the parent ListBoxItem instead,
            // resulting in the Advanced tab not being clickable.
            e.Handled = true;
        }
    }
}
