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
        private EventHandler _themeComponentUpdatedEventHandler;
        private readonly object _eventLock = new object();
        
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

        // By implementing a custom event accessor here we can keep the UsageCount up to date.
        public event EventHandler ThemeComponentUpdated
        {
            add
            {
                lock (_eventLock)
                {
                    _themeComponentUpdatedEventHandler += value;
                    OnPropertyChanged("UsageCount");
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    // ReSharper disable once DelegateSubtraction
                    _themeComponentUpdatedEventHandler -= value;
                    OnPropertyChanged("UsageCount");
                }
            }
        }

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
                if (_themeComponentUpdatedEventHandler != null)
                {
                    _themeComponentUpdatedEventHandler.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int UsageCount
        {
            get
            {
                if (_themeComponentUpdatedEventHandler == null)
                {
                    return 0;
                }

                return _themeComponentUpdatedEventHandler.GetInvocationList().Length;
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
