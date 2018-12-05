using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IReplaceColorsViewModel
    {
        void Initialise(IItemFilterScript itemFilterScript, IItemFilterBlock initialiseFromBlock);
        void Initialise(IItemFilterScript itemFilterScript);
    }

    internal class ReplaceColorsViewModel : ViewModelBase, IReplaceColorsViewModel
    {
        private IItemFilterScript _itemFilterScript;
        private ReplaceColorsParameterSet _replaceColorsParameterSet;

        public ReplaceColorsViewModel()
        {
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand);
        }

        public RelayCommand ReplaceColorsCommand { get; private set; }

        public void Initialise(IItemFilterScript itemFilterScript, IItemFilterBlock initialiseFromBlock)
        {
            _replaceColorsParameterSet = new ReplaceColorsParameterSet();

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof (TextColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceTextColor = true;
                var existingBlockColor = ((TextColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof (TextColorBlockItem))).Color;
                _replaceColorsParameterSet.OldTextColor = existingBlockColor;
                _replaceColorsParameterSet.NewTextColor = existingBlockColor;
            }

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof(BackgroundColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceBackgroundColor = true;
                var existingBlockColor = ((BackgroundColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof(BackgroundColorBlockItem))).Color;
                _replaceColorsParameterSet.OldBackgroundColor = existingBlockColor;
                _replaceColorsParameterSet.NewBackgroundColor = existingBlockColor;
            }

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof(BorderColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceBorderColor = true;
                var existingBlockColor = ((BorderColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof(BorderColorBlockItem))).Color;
                _replaceColorsParameterSet.OldBorderColor = existingBlockColor;
                _replaceColorsParameterSet.NewBorderColor = existingBlockColor;
            }

            _itemFilterScript = itemFilterScript;
        }

        public Color NewTextColor
        {
            get => _replaceColorsParameterSet.NewTextColor;
            set
            {
                _replaceColorsParameterSet.NewTextColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayTextColor");
            }
        }

        public Color DisplayTextColor => _replaceColorsParameterSet.ReplaceTextColor
            ? _replaceColorsParameterSet.NewTextColor
            : new Color {A = 240, R = 255, G = 255, B = 255};

        public bool ReplaceTextColor
        {
            get => _replaceColorsParameterSet.ReplaceTextColor;
            set
            {
                _replaceColorsParameterSet.ReplaceTextColor = value;
                RaisePropertyChanged("DisplayTextColor");
            }
        }

        public Color NewBackgroundColor
        {
            get => _replaceColorsParameterSet.NewBackgroundColor;
            set
            {
                _replaceColorsParameterSet.NewBackgroundColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayBackgroundColor");
            }
        }

        public Color DisplayBackgroundColor => _replaceColorsParameterSet.ReplaceBackgroundColor
            ? _replaceColorsParameterSet.NewBackgroundColor
            : new Color { A = 240, R = 0, G = 0, B = 0 };

        public bool ReplaceBackgroundColor
        {
            get => _replaceColorsParameterSet.ReplaceBackgroundColor;
            set
            {
                _replaceColorsParameterSet.ReplaceBackgroundColor = value;
                RaisePropertyChanged("DisplayBackgroundColor");
            }
        }

        public Color NewBorderColor
        {
            get => _replaceColorsParameterSet.NewBorderColor;
            set
            {
                _replaceColorsParameterSet.NewBorderColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayBorderColor");
            }
        }

        public Color DisplayBorderColor => _replaceColorsParameterSet.ReplaceBorderColor
            ? _replaceColorsParameterSet.NewBorderColor
            : new Color { A = 240, R = 0, G = 0, B = 0 };

        public bool ReplaceBorderColor
        {
            get => _replaceColorsParameterSet.ReplaceBorderColor;
            set
            {
                _replaceColorsParameterSet.ReplaceBorderColor = value;
                RaisePropertyChanged("DisplayBorderColor");
            }
        }

        public ReplaceColorsParameterSet ReplaceColorsParameterSet => _replaceColorsParameterSet;

        public void Initialise(IItemFilterScript itemFilterScript)
        {
            _replaceColorsParameterSet = new ReplaceColorsParameterSet();
            _itemFilterScript = itemFilterScript;
        }

        private void OnReplaceColorsCommand()
        {
            _itemFilterScript.ReplaceColors(_replaceColorsParameterSet);
        }
        
    }
}
