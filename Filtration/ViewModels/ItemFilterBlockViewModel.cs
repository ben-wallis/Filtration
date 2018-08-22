using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.Services;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using Xceed.Wpf.Toolkit;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModel : IItemFilterBlockViewModelBase
    {
        bool IsExpanded { get; set; }
        IItemFilterBlock Block { get; }
        bool BlockEnabled { get; set; }
        string BlockDescription { get; set; }
        void RefreshBlockPreview();
    }

    internal class ItemFilterBlockViewModel : ItemFilterBlockViewModelBase, IItemFilterBlockViewModel
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private bool _displaySettingsPopupOpen;
        private bool _isExpanded;
        private bool _audioVisualBlockItemsGridVisible;

        public ItemFilterBlockViewModel(IStaticDataService staticDataService, IReplaceColorsViewModel replaceColorsViewModel)
        {
            _staticDataService = staticDataService;
            _replaceColorsViewModel = replaceColorsViewModel;

            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand);
            AddFilterBlockItemCommand = new RelayCommand<Type>(OnAddFilterBlockItemCommand);
            ToggleBlockActionCommand = new RelayCommand(OnToggleBlockActionCommand);
            RemoveFilterBlockItemCommand = new RelayCommand<IItemFilterBlockItem>(OnRemoveFilterBlockItemCommand);
            SwitchBlockItemsViewCommand = new RelayCommand(OnSwitchBlockItemsViewCommand);
            PlaySoundCommand = new RelayCommand(OnPlaySoundCommand, () => HasSound);
            PlayPositionalSoundCommand = new RelayCommand(OnPlayPositionalSoundCommand, () => HasPositionalSound);
        }

        public override void Initialise(IItemFilterBlockBase itemFilterBlockBase, IItemFilterScriptViewModel parentScriptViewModel)
        {
            var itemFilterBlock = itemFilterBlockBase as IItemFilterBlock;
            if (itemFilterBlock == null || parentScriptViewModel == null)
            {
                throw new ArgumentNullException(nameof(itemFilterBlock));
            }

            _parentScriptViewModel = parentScriptViewModel;

            Block = itemFilterBlock;

            itemFilterBlock.BlockItems.CollectionChanged += OnBlockItemsCollectionChanged;

            foreach (var blockItem in itemFilterBlock.BlockItems)
            {
                blockItem.PropertyChanged += OnBlockItemChanged;
            }


            base.Initialise(itemFilterBlock, parentScriptViewModel);
        }

        public RelayCommand CopyBlockStyleCommand { get; }
        public RelayCommand PasteBlockStyleCommand { get; }
        public RelayCommand ToggleBlockActionCommand { get; }
        public RelayCommand ReplaceColorsCommand { get; }
        public RelayCommand<Type> AddFilterBlockItemCommand { get; }
        public RelayCommand<IItemFilterBlockItem> RemoveFilterBlockItemCommand { get; }
        public RelayCommand PlaySoundCommand { get; }
        public RelayCommand PlayPositionalSoundCommand { get; }
        public RelayCommand SwitchBlockItemsViewCommand { get; }

        public IItemFilterBlock Block { get; private set; }


        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<IItemFilterBlockItem> BlockItems => Block.BlockItems;

        public IEnumerable<IItemFilterBlockItem> SummaryBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<IItemFilterBlockItem> RegularBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<IItemFilterBlockItem> AudioVisualBlockItems
        {
            get { return Block.BlockItems.Where(b => b is IAudioVisualBlockItem); }
        }

        public bool AdvancedBlockGroup => Block.BlockGroup != null && Block.BlockGroup.Advanced;

        public bool AudioVisualBlockItemsGridVisible
        {
            get { return _audioVisualBlockItemsGridVisible; }
            set
            {
                _audioVisualBlockItemsGridVisible = value;
                RaisePropertyChanged();
                if (value && IsExpanded == false)
                {
                    IsExpanded = true;
                }
            }
        }

        public bool DisplaySettingsPopupOpen
        {
            get { return _displaySettingsPopupOpen; }
            set
            {
                _displaySettingsPopupOpen = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> AutoCompleteItemClasses => _staticDataService.ItemClasses;

        public IEnumerable<string> AutoCompleteItemBaseTypes => _staticDataService.ItemBaseTypes;

        public List<Type> BlockItemTypesAvailable => new List<Type>
        {
            typeof (ItemLevelBlockItem),
            typeof (DropLevelBlockItem),
            typeof (QualityBlockItem),
            typeof (RarityBlockItem),
            typeof (SocketsBlockItem),
            typeof (LinkedSocketsBlockItem),
            typeof (WidthBlockItem),
            typeof (HeightBlockItem),
            typeof (SocketGroupBlockItem),
            typeof (ClassBlockItem),
            typeof (BaseTypeBlockItem),
            typeof (IdentifiedBlockItem),
            typeof (CorruptedBlockItem),
            typeof (ElderItemBlockItem),
            typeof (ShaperItemBlockItem),
            typeof (ShapedMapBlockItem),
            typeof (ElderMapBlockItem),
            typeof (GemLevelBlockItem),
            typeof (StackSizeBlockItem),
            typeof (HasExplicitModBlockItem)
        };

        public List<Type> AudioVisualBlockItemTypesAvailable => new List<Type>
        {
            typeof (TextColorBlockItem),
            typeof (BackgroundColorBlockItem),
            typeof (BorderColorBlockItem),
            typeof (FontSizeBlockItem),
            typeof (SoundBlockItem),
            typeof (PositionalSoundBlockItem),
            typeof (DisableDropSoundBlockItem),
            typeof (IconBlockItem),
            typeof (BeamBlockItem)
        };

        public bool BlockEnabled
        {
            get { return Block.Enabled; }
            set
            {
                if (Block.Enabled != value)
                {
                    Block.Enabled = value;
                    IsDirty = true;
                    RaisePropertyChanged();
                }
            }
        }

        public string BlockDescription
        {
            get
            {
                return Block.Description;
            }
            set
            {
                if (Block.Description != value)
                {
                    Block.Description = value;
                    IsDirty = true;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<ColorItem> AvailableColors => PathOfExileColors.DefaultColors;

        public Color DisplayTextColor => Block.DisplayTextColor;
        public Color DisplayBackgroundColor => Block.DisplayBackgroundColor;
        public Color DisplayBorderColor => Block.DisplayBorderColor;
        public double DisplayFontSize => Block.DisplayFontSize/1.8;
        public string DisplayIcon => Block.DisplayIcon;
        public Color DisplayBeamColor => Block.DisplayBeamColor;

        public bool HasSound => Block.HasBlockItemOfType<SoundBlockItem>();
        public bool HasPositionalSound => Block.HasBlockItemOfType<PositionalSoundBlockItem>();

        public bool HasAudioVisualBlockItems => AudioVisualBlockItems.Any();

        private void OnSwitchBlockItemsViewCommand()
        {
            AudioVisualBlockItemsGridVisible = !AudioVisualBlockItemsGridVisible;
        }

        private void OnToggleBlockActionCommand()
        {
            var actionBlock = Block.BlockItems.OfType<ActionBlockItem>().First();
            actionBlock?.ToggleAction();
        }

        private void OnAddFilterBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (IItemFilterBlockItem) Activator.CreateInstance(blockItemType);

            newBlockItem.PropertyChanged += OnBlockItemChanged;
            BlockItems.Add(newBlockItem);
            OnBlockItemChanged(this, EventArgs.Empty);
            IsDirty = true;
        }

        private void OnRemoveFilterBlockItemCommand(IItemFilterBlockItem blockItem)
        {
            BlockItems.Remove(blockItem);
            blockItem.PropertyChanged -= OnBlockItemChanged;

            if (blockItem is IAudioVisualBlockItem)
            {
                OnBlockItemChanged(this, EventArgs.Empty);
            }

            IsDirty = true;
        }

        private void OnCopyBlockCommand()
        {
            _parentScriptViewModel.CopyBlock(this);
        }

        private void OnPasteBlockCommand()
        {
            _parentScriptViewModel.PasteBlock(this);
        }

        private void OnCopyBlockStyleCommand()
        {
            _parentScriptViewModel.CopyBlockStyle(this);
        }

        private void OnPasteBlockStyleCommand()
        {
            _parentScriptViewModel.PasteBlockStyle(this);
        }

        private void OnAddBlockCommand()
        {
            _parentScriptViewModel.AddBlock(this);
        }

        private void OnAddSectionCommand()
        {
            _parentScriptViewModel.AddCommentBlock(this);
        }

        private void OnDeleteBlockCommand()
        {
            _parentScriptViewModel.DeleteBlock(this);
        }

        private void OnMoveBlockUpCommand()
        {
            _parentScriptViewModel.MoveBlockUp(this);
        }

        private void OnMoveBlockDownCommand()
        {
            _parentScriptViewModel.MoveBlockDown(this);
        }

        private void OnMoveBlockToTopCommand()
        {
            _parentScriptViewModel.MoveBlockToTop(this);
        }

        private void OnMoveBlockToBottomCommand()
        {
            _parentScriptViewModel.MoveBlockToBottom(this);
        }

        private void OnReplaceColorsCommand()
        {
            _replaceColorsViewModel.Initialise(_parentScriptViewModel.Script, Block);
            var replaceColorsWindow = new ReplaceColorsWindow { DataContext = _replaceColorsViewModel };
            replaceColorsWindow.ShowDialog();
        }
        
        private bool AddBlockItemAllowed(Type type)
        {
            var blockItem = (IItemFilterBlockItem)Activator.CreateInstance(type);
            var blockCount = BlockItems.Count(b => b.GetType() == type);
            return blockCount < blockItem.MaximumAllowed;
        }

        private string ComputeFilePartFromNumber(string identifier)
        {
            if (Int32.TryParse(identifier, out int x))
            {
                if (x <= 9)
                {
                    return "AlertSound_0" + x + ".wav";
                }
                else
                {
                    return "AlertSound_" + x + ".wav";
                }
            }

            return "";
        }

        private string ComputeFilePartFromID(string identifier)
        {
            string filePart;
            switch (identifier) {
                case "ShGeneral":
                    filePart = "SH22General.wav";
                    break;
                case "ShBlessed":
                    filePart = "SH22Blessed.wav";
                    break;
                case "ShChaos":
                    filePart = "SH22Chaos.wav";
                    break;
                case "ShDivine":
                    filePart = "SH22Divine.wav";
                    break;
                case "ShExalted":
                    filePart = "SH22Exalted.wav";
                    break;
                case "ShMirror":
                    filePart = "SH22Mirror.wav";
                    break;
                case "ShAlchemy":
                    filePart = "SH22Alchemy.wav";
                    break;
                case "ShFusing":
                    filePart = "SH22Fusing.wav";
                    break;
                case "ShRegal":
                    filePart = "SH22Regal.wav";
                    break;
                case "ShVaal":
                    filePart = "SH22Vaal.wav";
                    break;
                default:
                    filePart = ComputeFilePartFromNumber(identifier);
                    break;
            }

            return filePart;
        }

        private void OnPlaySoundCommand()
        {
            var identifier = BlockItems.OfType<SoundBlockItem>().First().Value;
            var prefix = "Resources/AlertSounds/";
            var filePart = ComputeFilePartFromID(identifier);

            if (filePart == "")
            {
                return;
            }
            else
            {
                _mediaPlayer.Open(new Uri(prefix + filePart, UriKind.Relative));
                _mediaPlayer.Play();
            }
        }

        private void OnPlayPositionalSoundCommand()
        {
            var identifier = BlockItems.OfType<PositionalSoundBlockItem>().First().Value;
            var prefix = "Resources/AlertSounds/";
            var filePart = ComputeFilePartFromID(identifier);

            if (filePart == "")
            {
                return;
            }
            else
            {
                _mediaPlayer.Open(new Uri(prefix + filePart, UriKind.Relative));
                _mediaPlayer.Play();
            }
        }

        private void OnBlockItemChanged(object sender, EventArgs e)
        {
            var itemFilterBlockItem = sender as IItemFilterBlockItem;
            if ( itemFilterBlockItem != null && itemFilterBlockItem.IsDirty)
            {
                IsDirty = true;
            }
            Block.IsEdited = true;
            //if (sender is IAudioVisualBlockItem)
            //{
            RefreshBlockPreview();
            //}
        }

        public void RefreshBlockPreview()
        {
            RaisePropertyChanged(nameof(DisplayTextColor));
            RaisePropertyChanged(nameof(DisplayBackgroundColor));
            RaisePropertyChanged(nameof(DisplayBorderColor));
            RaisePropertyChanged(nameof(DisplayFontSize));
            RaisePropertyChanged(nameof(DisplayIcon));
            RaisePropertyChanged(nameof(DisplayBeamColor));
            RaisePropertyChanged(nameof(HasSound));
        }

        private void OnBlockItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(RegularBlockItems));
            RaisePropertyChanged(nameof(SummaryBlockItems));
            RaisePropertyChanged(nameof(AudioVisualBlockItems));
            RaisePropertyChanged(nameof(HasAudioVisualBlockItems));
        }
    }
}
