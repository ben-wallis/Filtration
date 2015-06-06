using System.Linq;
using System.Windows.Media;
using Filtration.Models;
using Filtration.Models.BlockItemTypes;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface IReplaceColorsViewModel
    {
        void Initialise(LootFilterScript lootFilterScript, LootFilterBlock initialiseFromBlock);
        void Initialise(LootFilterScript lootFilterScript);
    }

    internal class ReplaceColorsViewModel : FiltrationViewModelBase, IReplaceColorsViewModel
    {
        private LootFilterScript _lootFilterScript;
        private ReplaceColorsParameterSet _replaceColorsParameterSet;

        public ReplaceColorsViewModel()
        {
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand);
        }

        public RelayCommand ReplaceColorsCommand { get; private set; }

        public void Initialise(LootFilterScript lootFilterScript, LootFilterBlock initialiseFromBlock)
        {
            _replaceColorsParameterSet = new ReplaceColorsParameterSet();

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof (TextColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceTextColor = true;
                _replaceColorsParameterSet.OldTextColor =
                    ((TextColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof (TextColorBlockItem))).Color;
            }

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof(BackgroundColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceBackgroundColor = true;
                _replaceColorsParameterSet.OldBackgroundColor =
                    ((BackgroundColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof(BackgroundColorBlockItem))).Color;
            }

            if (initialiseFromBlock.BlockItems.Count(b => b.GetType() == typeof(BorderColorBlockItem)) > 0)
            {
                _replaceColorsParameterSet.ReplaceBorderColor = true;
                _replaceColorsParameterSet.OldBorderColor =
                    ((BorderColorBlockItem)
                        initialiseFromBlock.BlockItems.First(b => b.GetType() == typeof(BorderColorBlockItem))).Color;
            }

            _lootFilterScript = lootFilterScript;
        }

        public Color NewTextColor
        {
            get { return _replaceColorsParameterSet.NewTextColor; }
            set
            {
                _replaceColorsParameterSet.NewTextColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayTextColor");
            }
        }

        public Color DisplayTextColor
        {
            get
            {
                return _replaceColorsParameterSet.ReplaceTextColor
                    ? _replaceColorsParameterSet.NewTextColor
                    : new Color {A = 255, R = 255, G = 255, B = 255};
            }
        }

        public bool ReplaceTextColor
        {
            get { return _replaceColorsParameterSet.ReplaceTextColor; }
            set
            {
                _replaceColorsParameterSet.ReplaceTextColor = value;
                RaisePropertyChanged("DisplayTextColor");
            }
        }

        public Color NewBackgroundColor
        {
            get { return _replaceColorsParameterSet.NewBackgroundColor; }
            set
            {
                _replaceColorsParameterSet.NewBackgroundColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayBackgroundColor");
            }
        }

        public Color DisplayBackgroundColor
        {
            get
            {
                return _replaceColorsParameterSet.ReplaceBackgroundColor
                    ? _replaceColorsParameterSet.NewBackgroundColor
                    : new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public bool ReplaceBackgroundColor
        {
            get { return _replaceColorsParameterSet.ReplaceBackgroundColor; }
            set
            {
                _replaceColorsParameterSet.ReplaceBackgroundColor = value;
                RaisePropertyChanged("DisplayBackgroundColor");
            }
        }

        public Color NewBorderColor
        {
            get { return _replaceColorsParameterSet.NewBorderColor; }
            set
            {
                _replaceColorsParameterSet.NewBorderColor = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayBorderColor");
            }
        }

        public Color DisplayBorderColor
        {
            get
            {
                return _replaceColorsParameterSet.ReplaceBorderColor
                    ? _replaceColorsParameterSet.NewBorderColor
                    : new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public bool ReplaceBorderColor
        {
            get { return _replaceColorsParameterSet.ReplaceBorderColor; }
            set
            {
                _replaceColorsParameterSet.ReplaceBorderColor = value;
                RaisePropertyChanged("DisplayBorderColor");
            }
        }

        public ReplaceColorsParameterSet ReplaceColorsParameterSet
        {
            get
            {
                return _replaceColorsParameterSet;
            }
        }

        public void Initialise(LootFilterScript lootFilterScript)
        {
            _replaceColorsParameterSet = new ReplaceColorsParameterSet();
            _lootFilterScript = lootFilterScript;
        }

        private void OnReplaceColorsCommand()
        {
            _lootFilterScript.ReplaceColors(_replaceColorsParameterSet);
        }
        
    }
}
