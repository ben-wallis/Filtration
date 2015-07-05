using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.ObjectModel.Annotations;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.ThemeEditor
{
    [Serializable]
    public class ThemeComponent : INotifyPropertyChanged
    {
        private Color _color;

        public ThemeComponent()
        {
            
        }

        public ThemeComponent(ThemeComponentType componentType, string componentName, Color componentColor)
        {
            if (componentName == null || componentColor == null)
            {
                throw new ArgumentException("Null parameters not allowed in ThemeComponent constructor");
            }

            ComponentType = componentType;
            Color = componentColor;
            ComponentName = componentName;
        }

        public event EventHandler ThemeComponentUpdated;
        public event EventHandler ThemeComponentDeleted;

        public string ComponentName { get; set; }
        public ThemeComponentType ComponentType{ get; private set; }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
                if (ThemeComponentUpdated != null)
                {
                    ThemeComponentUpdated.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void TerminateComponent()
        {
            if (ThemeComponentDeleted != null)
            {
                ThemeComponentDeleted.Invoke(this, EventArgs.Empty);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
