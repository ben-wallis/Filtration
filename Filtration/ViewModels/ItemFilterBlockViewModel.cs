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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Xceed.Wpf.Toolkit;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModel : IItemFilterBlockViewModelBase
    {
        void Initialise(IItemFilterBlock itemFilterBlock, ItemFilterScriptViewModel parentScriptViewModel);
        bool IsExpanded { get; set; }
        IItemFilterBlock Block { get; }
        bool BlockEnabled { get; set; }
        string BlockDescription { get; set; }
        void RefreshBlockPreview();
    }

    internal interface IItemFilterBlockViewModelBase
    {
        IItemFilterBlockBase BaseBlock { get; }
        bool IsDirty { get; set; }
        event EventHandler BlockBecameDirty;
    }

    internal abstract class ItemFilterBlockViewModelBase : ViewModelBase, IItemFilterBlockViewModelBase
    {
        private bool _isDirty;

        protected void Initialise(IItemFilterBlockBase itemfilterBlock)
        {
            BaseBlock = itemfilterBlock;
        }


        public event EventHandler BlockBecameDirty;

        public IItemFilterBlockBase BaseBlock { get; protected set; }
        
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (value != _isDirty)
                {
                    _isDirty = value;
                    RaisePropertyChanged();
                    BlockBecameDirty?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    internal interface IItemFilterCommentBlockViewModel : IItemFilterBlockViewModelBase
    {
        void Initialise(IItemFilterCommentBlock itemFilterCommentBlock);
        IItemFilterCommentBlock ItemFilterCommentBlock { get; }
        string Comment { get; }
    }

    internal class ItemFilterCommentBlockViewModel : ItemFilterBlockViewModelBase, IItemFilterCommentBlockViewModel
    {
        public ItemFilterCommentBlockViewModel()
        {
        }

        public void Initialise(IItemFilterCommentBlock itemFilterCommentBlock)
        {
            ItemFilterCommentBlock = itemFilterCommentBlock;
            BaseBlock = itemFilterCommentBlock;
        }

        public IItemFilterCommentBlock ItemFilterCommentBlock { get; private set; }

        public string Comment => ItemFilterCommentBlock.Comment;
    }

    internal class ItemFilterBlockViewModel : ItemFilterBlockViewModelBase, IItemFilterBlockViewModel
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
            RemoveFilterBlockItemCommand = new RelayCommand<IItemFilterBlockItem>(OnRemoveFilterBlockItemCommand);
            SwitchBlockItemsViewCommand = new RelayCommand(OnSwitchBlockItemsViewCommand);
            PlaySoundCommand = new RelayCommand(OnPlaySoundCommand, () => HasSound);
        }

        public void Initialise(IItemFilterBlock itemFilterBlock, ItemFilterScriptViewModel parentScriptViewModel)
        {
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


            base.Initialise(itemFilterBlock);
        }

        public RelayCommand CopyBlockCommand { get; }
        public RelayCommand PasteBlockCommand { get; }
        public RelayCommand CopyBlockStyleCommand { get; }
        public RelayCommand PasteBlockStyleCommand { get; }
        public RelayCommand AddBlockCommand { get; }
        public RelayCommand AddSectionCommand { get; }
        public RelayCommand DeleteBlockCommand { get; }
        public RelayCommand MoveBlockUpCommand { get; }
        public RelayCommand MoveBlockDownCommand { get; }
        public RelayCommand MoveBlockToTopCommand { get; }
        public RelayCommand MoveBlockToBottomCommand { get; }
        public RelayCommand ToggleBlockActionCommand { get; }
        public RelayCommand ReplaceColorsCommand { get; }
        public RelayCommand<Type> AddFilterBlockItemCommand { get; }
        public RelayCommand<IItemFilterBlockItem> RemoveFilterBlockItemCommand { get; }
        public RelayCommand PlaySoundCommand { get; }
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
            typeof (CorruptedBlockItem)
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

        public Color DisplayTextColor => Block.DisplayTextColor;
        public Color DisplayBackgroundColor => Block.DisplayBackgroundColor;
        public Color DisplayBorderColor => Block.DisplayBorderColor;
        public double DisplayFontSize => Block.DisplayFontSize/1.8;
        
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

        private void OnBlockItemChanged(object sender, EventArgs e)
        {
            var itemFilterBlockItem = sender as IItemFilterBlockItem;
            if ( itemFilterBlockItem != null && itemFilterBlockItem.IsDirty)
            {
                IsDirty = true;
            }

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
