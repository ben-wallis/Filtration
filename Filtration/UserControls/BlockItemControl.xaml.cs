using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Filtration.Annotations;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.ThemeEditor;
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

            SetBlockValueCommand = new RelayCommand(OnSetBlockValueCommmand);
        }

        public RelayCommand SetBlockValueCommand { get; private set; }

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

        public List<string> SoundsAvailable => new List<string> {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16",
            "ShGeneral", "ShBlessed", "ShChaos", "ShDivine", "ShExalted", "ShMirror", "ShAlchemy",
            "ShFusing", "ShRegal", "ShVaal"
        };

        private void OnSetBlockValueCommmand()
        {
            var blockItemWithTheme = BlockItem as IBlockItemWithTheme;
            if (blockItemWithTheme?.ThemeComponent == null) return;

            var componentType = ((IBlockItemWithTheme)BlockItem).ThemeComponent.ComponentType;
            switch(componentType)
            {
                case ThemeComponentType.BackgroundColor:
                case ThemeComponentType.BorderColor:
                case ThemeComponentType.TextColor:
                    var colorBlockItem = BlockItem as ColorBlockItem;
                    colorBlockItem.Color = ((ColorThemeComponent)colorBlockItem.ThemeComponent).Color;
                    break;
                case ThemeComponentType.FontSize:
                    var integerBlockItem = BlockItem as IntegerBlockItem;
                    integerBlockItem.Value = ((IntegerThemeComponent)integerBlockItem.ThemeComponent).Value;
                    break;
                case ThemeComponentType.AlertSound:
                    var strIntBlockItem = BlockItem as StrIntBlockItem;
                    strIntBlockItem.Value = ((StrIntThemeComponent)strIntBlockItem.ThemeComponent).Value;
                    strIntBlockItem.SecondValue = ((StrIntThemeComponent)strIntBlockItem.ThemeComponent).SecondValue;
                    break;
                case ThemeComponentType.CustomSound:
                    var stringBlockItem = BlockItem as StringBlockItem;
                    stringBlockItem.Value = ((StringThemeComponent)stringBlockItem.ThemeComponent).Value;
                    break;
                case ThemeComponentType.Icon:
                    var iconBlockItem = BlockItem as IconBlockItem;
                    iconBlockItem.Size = ((IconThemeComponent)iconBlockItem.ThemeComponent).IconSize;
                    iconBlockItem.Color = ((IconThemeComponent)iconBlockItem.ThemeComponent).IconColor;
                    iconBlockItem.Shape = ((IconThemeComponent)iconBlockItem.ThemeComponent).IconShape;
                    break;
                case ThemeComponentType.Effect:
                    var effectColorBlockItem = BlockItem as EffectColorBlockItem;
                    effectColorBlockItem.Color = ((EffectColorThemeComponent)effectColorBlockItem.ThemeComponent).EffectColor;
                    effectColorBlockItem.Temporary = ((EffectColorThemeComponent)effectColorBlockItem.ThemeComponent).Temporary;
                    break;
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
