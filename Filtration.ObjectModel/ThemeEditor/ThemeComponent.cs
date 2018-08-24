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
        protected EventHandler _themeComponentUpdatedEventHandler;
        private readonly object _eventLock = new object();

        public ThemeComponent()
        {
            
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
        public ThemeComponentType ComponentType{ get; set; }

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
            ThemeComponentDeleted?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
