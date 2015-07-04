using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Filtration.Annotations;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.UserControls
{
    public partial class BlockItemControl : INotifyPropertyChanged
    {
        public BlockItemControl()
        {
            InitializeComponent();
            // ReSharper disable once PossibleNullReferenceException
            (Content as FrameworkElement).DataContext = this;
        }

        public static readonly DependencyProperty BlockItemProperty = DependencyProperty.Register(
            "BlockItem",
            typeof(IItemFilterBlockItem),
            typeof(BlockItemControl),
            new FrameworkPropertyMetadata());
        
        public static readonly DependencyProperty RemoveItemCommandProperty = DependencyProperty.Register(
            "RemoveItemCommand",
            typeof(RelayCommand<IItemFilterBlockItem>),
            typeof(BlockItemControl),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty RemoveEnabledProperty = DependencyProperty.Register(
            "RemoveEnabled",
            typeof(Visibility),
            typeof(BlockItemControl),
            new FrameworkPropertyMetadata());

        public IItemFilterBlockItem BlockItem
        {
            get
            {
                return (IItemFilterBlockItem)GetValue(BlockItemProperty);
            }
            set
            {
                SetValue(BlockItemProperty, value);
                OnPropertyChanged();
            }
        }

        public RelayCommand<IItemFilterBlockItem> RemoveItemCommand
        {
            get
            {
                return (RelayCommand<IItemFilterBlockItem>)GetValue(RemoveItemCommandProperty);
            }
            set
            {
                SetValue(RemoveItemCommandProperty, value);
            }
        }
        public Visibility RemoveEnabled
        {
            get
            {
                return (Visibility)GetValue(RemoveEnabledProperty);
            }
            set
            {
                SetValue(RemoveEnabledProperty, value);
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
