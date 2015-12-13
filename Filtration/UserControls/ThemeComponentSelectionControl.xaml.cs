using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Filtration.Annotations;
using Filtration.ObjectModel.ThemeEditor;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.UserControls
{
    public partial class ThemeComponentSelectionControl : INotifyPropertyChanged
    {
        public ThemeComponentSelectionControl()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;
            AddThemeComponentCommand = new RelayCommand(OnAddThemeComponentCommand);
            RemoveThemeComponentCommand = new RelayCommand(OnRemoveThemeComponentCommand);
        }

        public static readonly DependencyProperty ThemeComponentProperty = DependencyProperty.Register(
           "ThemeComponent",
           typeof(ThemeComponent),
           typeof(ThemeComponentSelectionControl),
           new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty AvailableThemeComponentsProperty = DependencyProperty.Register(
           "AvailableThemeComponents",
           typeof(List<ThemeComponent>),
           typeof(ThemeComponentSelectionControl),
           new FrameworkPropertyMetadata());

        private bool _showThemeComponentComboBox;

        public RelayCommand AddThemeComponentCommand { get; private set; }
        public RelayCommand RemoveThemeComponentCommand { get; private set; }

        public ThemeComponent ThemeComponent
        {
            get
            {
                return (ThemeComponent)GetValue(ThemeComponentProperty);
            }
            set
            {
                SetValue(ThemeComponentProperty, value);
            }
        }

        public List<ThemeComponent> AvailableThemeComponents
        {
            get
            {
                return (List<ThemeComponent>)GetValue(AvailableThemeComponentsProperty);
            }
            set
            {
                SetValue(AvailableThemeComponentsProperty, value);
                OnPropertyChanged();
            }
        }

        public bool ShowThemeComponentComboBox
        {
            get { return _showThemeComponentComboBox || HasThemeComponent; }
            set
            {
                _showThemeComponentComboBox = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasThemeComponent));
            }
        }

        public bool HasThemeComponent => ThemeComponent != null;

        private void OnAddThemeComponentCommand()
        {
            ShowThemeComponentComboBox = true;
        }

        private void OnRemoveThemeComponentCommand()
        {
            ThemeComponent = null;
            ShowThemeComponentComboBox = false;
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
