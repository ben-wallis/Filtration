using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Filtration.Annotations;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.UserControls
{
    public interface IEditableListBoxControl
    {
        RelayCommand AddItemCommand { get; }
        RelayCommand<string> DeleteItemCommand { get; }
        IEnumerable<string> AutoCompleteItemsSource { get; set; }
        ICollection<string> ItemsSource { get; set; }
        string AddItemText { get; set; }
    }

    public partial class EditableListBoxControl : INotifyPropertyChanged, IEditableListBoxControl
    {
        private string _addItemText;

        public EditableListBoxControl()
        {
            InitializeComponent();

            // ReSharper disable once PossibleNullReferenceException
            (Content as FrameworkElement).DataContext = this;
            AddItemCommand = new RelayCommand(OnAddItemCommand, () => !string.IsNullOrEmpty(AddItemText));
            DeleteItemCommand = new RelayCommand<string>(OnDeleteItemCommand);
        }

        public RelayCommand AddItemCommand { get; }
        public RelayCommand<string> DeleteItemCommand { get; }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof (ICollection<string>),
            typeof (EditableListBoxControl),
            new FrameworkPropertyMetadata(OnItemsSourcePropertyChanged)
        );

        public static readonly DependencyProperty AutoCompleteItemsSourceProperty = DependencyProperty.Register(
            "AutoCompleteItemsSource",
            typeof (IEnumerable<string>),
            typeof (EditableListBoxControl),
            new FrameworkPropertyMetadata()
        );

        public IEnumerable<string> AutoCompleteItemsSource
        {
            get { return (IEnumerable<string>) GetValue(AutoCompleteItemsSourceProperty); }
            set { SetValue(AutoCompleteItemsSourceProperty, value); }
        }

        public ICollection<string> ItemsSource
        {
            get { return (ICollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string AddItemText
        {
            get { return _addItemText; }
            set
            {
                _addItemText = value;
                OnPropertyChanged();
                AddItemCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnAddItemCommand()
        {
            if (string.IsNullOrEmpty(AddItemText)) return;

            ItemsSource.Add(AddItemText);
            AddItemText = string.Empty;
        }

        private void OnDeleteItemCommand(string itemToDelete)
        {
            if (!string.IsNullOrEmpty(itemToDelete))
            {
                ItemsSource.Remove(itemToDelete);
            }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var control = source as EditableListBoxControl;

            control?.OnPropertyChanged(nameof(ItemsSource));
        }

        private void AutoCompleteBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddItemCommand.Execute(null);
            }
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
