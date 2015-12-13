using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using Filtration.Common.ViewModels;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.Services;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using Xceed.Wpf.Toolkit;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModel
    {
        void Initialise(ItemFilterBlock itemFilterBlock, ItemFilterScriptViewModel parentScriptViewModel);
        bool IsDirty { get; set; }
        bool IsExpanded { get; set; }
        ItemFilterBlock Block { get; }
        bool BlockEnabled { get; set; }
        void RefreshBlockPreview();
    }

    internal class ItemFilterBlockViewModel : FiltrationViewModelBase, IItemFilterBlockViewModel
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();
        private ItemFilterScriptViewModel _parentScriptViewModel;

        private bool _displaySettingsPopupOpen;
        private bool _isExpanded;
        private bool _audioVisualBlockItemsGridVisible;

        public ItemFilterBlockViewModel(IStaticDataService staticDataService, IReplaceColorsViewModel replaceColorsViewModel)
        {
            _staticDataService = staticDataService;
            _replaceColorsViewModel = replaceColorsViewModel;

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand);
            CopyBlockStyleCommand = new RelayCommand(OnCopyBlockStyleCommand);
            PasteBlockStyleCommand = new RelayCommand(OnPasteBlockStyleCommand);
            AddBlockCommand = new RelayCommand(OnAddBlockCommand);
            AddSectionCommand = new RelayCommand(OnAddSectionCommand);
            DeleteBlockCommand = new RelayCommand(OnDeleteBlockCommand);
            MoveBlockUpCommand = new RelayCommand(OnMoveBlockUpCommand);
            MoveBlockDownCommand = new RelayCommand(OnMoveBlockDownCommand);
            MoveBlockToTopCommand = new RelayCommand(OnMoveBlockToTopCommand);
            MoveBlockToBottomCommand = new RelayCommand(OnMoveBlockToBottomCommand);
            ReplaceColorsCommand = new RelayCommand(OnReplaceColorsCommand);
            AddFilterBlockItemCommand = new RelayCommand<Type>(OnAddFilterBlockItemCommand);
            ToggleBlockActionCommand = new RelayCommand(OnToggleBlockActionCommand);
            AddAudioVisualBlockItemCommand = new RelayCommand<Type>(OnAddAudioVisualBlockItemCommand);
            RemoveFilterBlockItemCommand = new RelayCommand<IItemFilterBlockItem>(OnRemoveFilterBlockItemCommand);
            SwitchBlockItemsViewCommand = new RelayCommand(OnSwitchBlockItemsViewCommand);
            PlaySoundCommand = new RelayCommand(OnPlaySoundCommand, () => HasSound);
        }

        public void Initialise(ItemFilterBlock itemFilterBlock, ItemFilterScriptViewModel parentScriptViewModel)
        {
            if (itemFilterBlock == null || parentScriptViewModel == null)
            {
                throw new ArgumentNullException(nameof(itemFilterBlock));
            }

            _parentScriptViewModel = parentScriptViewModel;

            Block = itemFilterBlock;
            itemFilterBlock.BlockItems.CollectionChanged += OnBlockItemsCollectionChanged;

            foreach (var blockItem in itemFilterBlock.BlockItems.OfType<IAudioVisualBlockItem>())
            {
                blockItem.PropertyChanged += OnAudioVisualBlockItemChanged;
            }
        }

        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand PasteBlockCommand { get; private set; }
        public RelayCommand CopyBlockStyleCommand { get; private set; }
        public RelayCommand PasteBlockStyleCommand { get; private set; }
        public RelayCommand AddBlockCommand { get; private set; }
        public RelayCommand AddSectionCommand { get; private set; }
        public RelayCommand DeleteBlockCommand { get; private set; }
        public RelayCommand MoveBlockUpCommand { get; private set; }
        public RelayCommand MoveBlockDownCommand { get; private set; }
        public RelayCommand MoveBlockToTopCommand { get; private set; }
        public RelayCommand MoveBlockToBottomCommand { get; private set; }
        public RelayCommand ToggleBlockActionCommand { get; private set; }
        public RelayCommand ReplaceColorsCommand { get; private set; }
        public RelayCommand<Type> AddFilterBlockItemCommand { get; private set; }
        public RelayCommand<Type> AddAudioVisualBlockItemCommand { get; private set; }
        public RelayCommand<IItemFilterBlockItem> RemoveFilterBlockItemCommand { get; private set; }
        public RelayCommand PlaySoundCommand { get; private set; }
        public RelayCommand SwitchBlockItemsViewCommand { get; private set; }

        public ItemFilterBlock Block { get; private set; }

        public bool IsDirty { get; set; }

        public bool IsExpanded
        {
            get { return _isExpanded; }
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
            typeof (BaseTypeBlockItem)
        };

        public List<Type> AudioVisualBlockItemTypesAvailable => new List<Type>
        {
            typeof (TextColorBlockItem),
            typeof (BackgroundColorBlockItem),
            typeof (BorderColorBlockItem),
            typeof (FontSizeBlockItem),
            typeof (SoundBlockItem)
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

        public bool HasTextColor => Block.HasBlockItemOfType<TextColorBlockItem>();

        public Color DisplayTextColor => HasTextColor
            ? BlockItems.OfType<TextColorBlockItem>().First().Color
            : new Color {A = 255, R = 200, G = 200, B = 200};

        public bool HasBackgroundColor => Block.HasBlockItemOfType<BackgroundColorBlockItem>();

        public Color DisplayBackgroundColor => HasBackgroundColor
            ? BlockItems.OfType<BackgroundColorBlockItem>().First().Color
            : new Color { A = 255, R = 0, G = 0, B = 0 };

        public bool HasBorderColor => Block.HasBlockItemOfType<BorderColorBlockItem>();

        public Color DisplayBorderColor => HasBorderColor
            ? BlockItems.OfType<BorderColorBlockItem>().First().Color
            : new Color { A = 255, R = 0, G = 0, B = 0 };

        public bool HasFontSize => Block.HasBlockItemOfType<FontSizeBlockItem>();

        public double DisplayFontSize
        {
            // Dividing by 1.8 roughly scales in-game font sizes down to WPF sizes
            get
            {
                var fontSize = HasFontSize ? BlockItems.OfType<FontSizeBlockItem>().First().Value / 1.8 : 19;
                
                return fontSize;
            }
        }

        public bool HasSound => Block.HasBlockItemOfType<SoundBlockItem>();


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
        
            BlockItems.Add(newBlockItem);
            IsDirty = true;
        }

        private void OnRemoveFilterBlockItemCommand(IItemFilterBlockItem blockItem)
        {
            BlockItems.Remove(blockItem);

            if (blockItem is IAudioVisualBlockItem)
            {
                blockItem.PropertyChanged -= OnAudioVisualBlockItemChanged;
                OnAudioVisualBlockItemChanged(this, EventArgs.Empty);
            }

            IsDirty = true;
        }

        private void OnAddAudioVisualBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (IItemFilterBlockItem) Activator.CreateInstance(blockItemType);

            newBlockItem.PropertyChanged += OnAudioVisualBlockItemChanged;
            BlockItems.Add(newBlockItem);
            OnAudioVisualBlockItemChanged(this, EventArgs.Empty);
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
            _parentScriptViewModel.AddSection(this);
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

        private void OnPlaySoundCommand()
        {
            var soundUri = "Resources/AlertSounds/AlertSound" + BlockItems.OfType<SoundBlockItem>().First().Value + ".wav";
            _mediaPlayer.Open(new Uri(soundUri, UriKind.Relative));
            _mediaPlayer.Play();
        }

        private void OnAudioVisualBlockItemChanged(object sender, EventArgs e)
        {
            RefreshBlockPreview();
        }

        public void RefreshBlockPreview()
        {
            RaisePropertyChanged("DisplayTextColor");
            RaisePropertyChanged("DisplayBackgroundColor");
            RaisePropertyChanged("DisplayBorderColor");
            RaisePropertyChanged("DisplayFontSize");
            RaisePropertyChanged("HasSound");
        }

        private void OnBlockItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("RegularBlockItems");
            RaisePropertyChanged("SummaryBlockItems");
            RaisePropertyChanged("AudioVisualBlockItems");
            RaisePropertyChanged("HasAudioVisualBlockItems");
        }
    }
}
