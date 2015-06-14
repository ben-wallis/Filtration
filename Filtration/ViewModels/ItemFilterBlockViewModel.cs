using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using Filtration.Models;
using Filtration.Models.BlockItemBaseTypes;
using Filtration.Models.BlockItemTypes;
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
        ItemFilterBlock Block { get; }
    }

    internal class ItemFilterBlockViewModel : FiltrationViewModelBase, IItemFilterBlockViewModel
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IReplaceColorsViewModel _replaceColorsViewModel;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();
        private ItemFilterScriptViewModel _parentScriptViewModel;

        private bool _displaySettingsPopupOpen;
        
        public ItemFilterBlockViewModel(IStaticDataService staticDataService, IReplaceColorsViewModel replaceColorsViewModel)
        {
            _staticDataService = staticDataService;
            _replaceColorsViewModel = replaceColorsViewModel;

            CopyBlockCommand = new RelayCommand(OnCopyBlockCommand);
            PasteBlockCommand = new RelayCommand(OnPasteBlockCommand);
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
            RemoveAudioVisualBlockItemCommand = new RelayCommand<IItemFilterBlockItem>(OnRemoveAudioVisualBlockItemCommand);
            PlaySoundCommand = new RelayCommand(OnPlaySoundCommand, () => HasSound);
        }

        public void Initialise(ItemFilterBlock itemFilterBlock, ItemFilterScriptViewModel parentScriptViewModel)
        {
            if (itemFilterBlock == null || parentScriptViewModel == null)
            {
                throw new ArgumentNullException("itemFilterBlock");
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
        public RelayCommand<IItemFilterBlockItem> RemoveAudioVisualBlockItemCommand { get; private set; }
        public RelayCommand PlaySoundCommand { get; private set; }

        public ItemFilterBlock Block { get; private set; }
        public bool IsDirty { get; set; }

        public ObservableCollection<IItemFilterBlockItem> FilterBlockItems
        {
            get { return Block.BlockItems; }
        }

        public IEnumerable<IItemFilterBlockItem> SummaryBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<IItemFilterBlockItem> AudioVisualBlockItems
        {
            get { return Block.BlockItems.Where(b => b is IAudioVisualBlockItem); }
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

        public IEnumerable<string> AutoCompleteItemClasses
        {
            get { return _staticDataService.ItemClasses; }
        }

        public IEnumerable<string> AutoCompleteItemBaseTypes
        {
            get { return _staticDataService.ItemBaseTypes; }
        } 

        public List<int> SoundsAvailable
        {
            get
            {
                return new List<int>
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9
                };
            }
        }

        public List<Type> BlockItemTypesAvailable
        {
            get
            {
                return new List<Type>
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
            }
        }

        public List<Type> AudioVisualBlockItemTypesAvailable
        {
            get
            {
                return new List<Type>
                {
                    typeof (TextColorBlockItem),
                    typeof (BackgroundColorBlockItem),
                    typeof (BorderColorBlockItem),
                    typeof (FontSizeBlockItem),
                    typeof (SoundBlockItem)
                };
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

        public ObservableCollection<ColorItem> AvailableColors
        {
            get
            {
                {
                    return PathOfExileColors.DefaultColors;
                }
            }
        }

        public bool HasTextColor
        {
            get { return Block.HasBlockItemOfType<TextColorBlockItem>(); }
        }

        public Color DisplayTextColor
        {
            get
            {
                return HasTextColor
                    ? FilterBlockItems.OfType<TextColorBlockItem>().First().Color
                    : new Color { A = 255, R = 255, G = 255, B = 255 };
            }
        }

        public bool HasBackgroundColor
        {
            get { return Block.HasBlockItemOfType<BackgroundColorBlockItem>(); }
        }

        public Color DisplayBackgroundColor
        {
            get
            {
                return HasBackgroundColor
                    ? FilterBlockItems.OfType<BackgroundColorBlockItem>().First().Color
                    : new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public bool HasBorderColor
        {
            get { return Block.HasBlockItemOfType<BorderColorBlockItem>(); }
        }

        public Color DisplayBorderColor
        {
            get
            {
                return HasBorderColor
                    ? FilterBlockItems.OfType<BorderColorBlockItem>().First().Color
                    : new Color { A = 255, R = 0, G = 0, B = 0 };
            }
        }

        public bool HasFontSize
        {
            get { return Block.HasBlockItemOfType<FontSizeBlockItem>(); }
        }

        public bool HasSound
        {
            get { return Block.HasBlockItemOfType<SoundBlockItem>(); }
        }

        private void OnToggleBlockActionCommand()
        {
            var actionBlock = Block.BlockItems.OfType<ActionBlockItem>().First();
            if (actionBlock != null)
            {
                actionBlock.ToggleAction();
            }
        }

        private void OnAddFilterBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (IItemFilterBlockItem) Activator.CreateInstance(blockItemType);
        
            FilterBlockItems.Add(newBlockItem);
            IsDirty = true;
        }

        private void OnRemoveFilterBlockItemCommand(IItemFilterBlockItem blockItem)
        {
            FilterBlockItems.Remove(blockItem);
            IsDirty = true;
        }

        private void OnAddAudioVisualBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (IItemFilterBlockItem) Activator.CreateInstance(blockItemType);

            newBlockItem.PropertyChanged += OnAudioVisualBlockItemChanged;
            FilterBlockItems.Add(newBlockItem);
            OnAudioVisualBlockItemChanged(null, null);
            IsDirty = true;
        }

        private void OnRemoveAudioVisualBlockItemCommand(IItemFilterBlockItem blockItem)
        {
            blockItem.PropertyChanged -= OnAudioVisualBlockItemChanged;
            FilterBlockItems.Remove(blockItem);
            OnAudioVisualBlockItemChanged(null, null);
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
            var blockCount = FilterBlockItems.Count(b => b.GetType() == type);
            return blockCount < blockItem.MaximumAllowed;
        }

        private void OnPlaySoundCommand()
        {
            var soundUri = "Resources/AlertSounds/AlertSound" + FilterBlockItems.OfType<SoundBlockItem>().First().Value + ".wav";
            _mediaPlayer.Open(new Uri(soundUri, UriKind.Relative));
            _mediaPlayer.Play();
        }

        private void OnAudioVisualBlockItemChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("DisplayTextColor");
            RaisePropertyChanged("DisplayBackgroundColor");
            RaisePropertyChanged("DisplayBorderColor");
            RaisePropertyChanged("HasSound");
        }

        private void OnBlockItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SummaryBlockItems");
            RaisePropertyChanged("AudioVisualBlockItems");
        }
    }
}
