using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.Services;
using Filtration.Views;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using static System.Int32;

namespace Filtration.ViewModels
{
    internal interface IItemFilterBlockViewModel : IItemFilterBlockViewModelBase
    {
        bool IsExpanded { get; set; }
        IItemFilterBlock Block { get; }
        bool BlockEnabled { get; set; }
        string BlockDescription { get; set; }
        RelayCommand CopyBlockStyleCommand { get; }
        RelayCommand PasteBlockStyleCommand { get; }
        RelayCommand ToggleBlockActionCommand { get; }
        RelayCommand ReplaceColorsCommand { get; }
        RelayCommand<Type> AddFilterBlockItemCommand { get; }
        RelayCommand<IItemFilterBlockItem> RemoveFilterBlockItemCommand { get; }
        RelayCommand PlaySoundCommand { get; }
        RelayCommand PlayPositionalSoundCommand { get; }
        RelayCommand SwitchBlockItemsViewCommand { get; }
        RelayCommand CustomSoundFileDialogCommand { get; }
        RelayCommand PlayCustomSoundCommand { get; }
        RelayCommand AddBlockGroupCommand { get; }
        RelayCommand DeleteBlockGroupCommand { get; }
        ObservableCollection<ItemFilterBlockGroup> BlockGroups { get; }
        ObservableCollection<string> BlockGroupSuggestions { get; }
        string BlockGroupSearch { get; set; }
        ObservableCollection<IItemFilterBlockItem> BlockItems { get; }
        IEnumerable<IItemFilterBlockItem> SummaryBlockItems { get; }
        IEnumerable<IItemFilterBlockItem> RegularBlockItems { get; }
        IEnumerable<IItemFilterBlockItem> AudioVisualBlockItems { get; }
        bool AdvancedBlockGroup { get; }
        bool AudioVisualBlockItemsGridVisible { get; set; }
        bool DisplaySettingsPopupOpen { get; set; }
        IEnumerable<string> AutoCompleteItemClasses { get; }
        IEnumerable<string> AutoCompleteItemBaseTypes { get; }
        IEnumerable<string> AutoCompleteProphecies { get; }
        IEnumerable<string> AutocompleteItemMods { get; }
        List<Type> BlockItemTypesAvailable { get; }
        List<Type> AudioVisualBlockItemTypesAvailable { get; }
        Color DisplayTextColor { get; }
        Color DisplayBackgroundColor { get; }
        Color DisplayBorderColor { get; }
        double DisplayFontSize { get; }
        int DisplayIconSize { get; }
        int DisplayIconColor { get; }
        int DisplayIconShape { get; }
        Color DisplayEffectColor { get; }
        bool HasSound { get; }
        bool HasPositionalSound { get; }
        bool HasCustomSound { get; }
        bool HasDisabledDefaultSound { get; }
        bool HasAudioVisualBlockItems { get; }
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
            PlayCustomSoundCommand = new RelayCommand(OnPlayCustomSoundCommand, () => HasCustomSound);
            CustomSoundFileDialogCommand = new RelayCommand(OnCustomSoundFileDialog);
            AddBlockGroupCommand = new RelayCommand(OnAddBlockGroupCommand);
            DeleteBlockGroupCommand = new RelayCommand(OnDeleteBlockGroupCommand, () => BlockGroups.Count > 0);
        }

        public override void Initialise(IItemFilterBlockBase itemFilterBlockBase, IItemFilterScriptViewModel parentScriptViewModel)
        {
            if (!(itemFilterBlockBase is IItemFilterBlock itemFilterBlock) || parentScriptViewModel == null)
            {
                throw new ArgumentNullException(nameof(itemFilterBlock));
            }

            BlockGroups = new ObservableCollection<ItemFilterBlockGroup>();

            _parentScriptViewModel = parentScriptViewModel;

            Block = itemFilterBlock;
            Block.EnabledStatusChanged += OnBlockEnabledStatusChanged;

            itemFilterBlock.BlockItems.CollectionChanged += OnBlockItemsCollectionChanged;

            foreach (var blockItem in itemFilterBlock.BlockItems)
            {
                blockItem.PropertyChanged += OnBlockItemChanged;
            }
            base.Initialise(itemFilterBlock, parentScriptViewModel);

            UpdateBlockGroups();
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
        public RelayCommand CustomSoundFileDialogCommand { get; }
        public RelayCommand PlayCustomSoundCommand { get; }
        public RelayCommand AddBlockGroupCommand { get; }
        public RelayCommand DeleteBlockGroupCommand { get; }

        public IItemFilterBlock Block { get; private set; }

        public ObservableCollection<ItemFilterBlockGroup> BlockGroups { get; internal set; }

        public ObservableCollection<string> BlockGroupSuggestions { get; internal set; }

        public string BlockGroupSearch { get; set; }

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

        public bool AdvancedBlockGroup => Block.BlockGroup?.ParentGroup != null && Block.BlockGroup.ParentGroup.Advanced;

        public bool AudioVisualBlockItemsGridVisible
        {
            get => _audioVisualBlockItemsGridVisible;
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
            get => _displaySettingsPopupOpen;
            set
            {
                _displaySettingsPopupOpen = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> AutoCompleteItemClasses => _staticDataService.ItemClasses;

        public IEnumerable<string> AutoCompleteItemBaseTypes => _staticDataService.ItemBaseTypes;

        public IEnumerable<string> AutoCompleteProphecies => _staticDataService.Prophecies;

        public IEnumerable<string> AutocompleteItemMods => _staticDataService.ItemMods;

        public IEnumerable<string> AutocompleteEnchantments => _staticDataService.Enchantments;

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
            typeof (ProphecyBlockItem),
            typeof (IdentifiedBlockItem),
            typeof (CorruptedBlockItem),
            typeof (ElderItemBlockItem),
            typeof (ShaperItemBlockItem),
            typeof (SynthesisedItemBlockItem),
            typeof (FracturedItemBlockItem),
            typeof (AnyEnchantmentBlockItem),
            typeof (MapTierBlockItem),
            typeof (ShapedMapBlockItem),
            typeof (ElderMapBlockItem),
            typeof (BlightedMapBlockItem),
            typeof (GemLevelBlockItem),
            typeof (StackSizeBlockItem),
            typeof (HasExplicitModBlockItem),
            typeof (HasEnchantmentBlockItem),
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
            typeof (MapIconBlockItem),
            typeof (PlayEffectBlockItem),
            typeof (CustomSoundBlockItem)
        };

        public bool BlockEnabled
        {
            get => Block.Enabled;
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
            get => Block.Description;
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

        public Color DisplayTextColor => Block.DisplayTextColor;
        public Color DisplayBackgroundColor => Block.DisplayBackgroundColor;
        public Color DisplayBorderColor => Block.DisplayBorderColor;
        public double DisplayFontSize => Block.DisplayFontSize/1.8;
        public int DisplayIconSize => Block.DisplayIconSize;
        public int DisplayIconColor => Block.DisplayIconColor;
        public int DisplayIconShape => Block.DisplayIconShape;
        public Color DisplayEffectColor => Block.DisplayEffectColor;

        public bool HasSound => Block.HasBlockItemOfType<SoundBlockItem>();
        public bool HasPositionalSound => Block.HasBlockItemOfType<PositionalSoundBlockItem>();
        public bool HasCustomSound => Block.HasBlockItemOfType<CustomSoundBlockItem>();
        public bool HasDisabledDefaultSound => Block.HasBlockItemOfType<DisableDropSoundBlockItem>();

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

            if(newBlockItem is CustomSoundBlockItem customSoundBlockItem && _parentScriptViewModel.CustomSoundsAvailable.Count > 0)
            {
                customSoundBlockItem.Value = _parentScriptViewModel.CustomSoundsAvailable[0];
            }

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
            foreach (var blockItem in Block.BlockItems.OfType<IAudioVisualBlockItem>())
            {
                blockItem.PropertyChanged += OnBlockItemChanged;
            }
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
            if (TryParse(identifier, out int x))
            {
                if (x <= 9)
                {
                    return "AlertSound_0" + x + ".mp3";
                }

                return "AlertSound_" + x + ".mp3";
            }

            return "";
        }

        private string ComputeFilePartFromID(string identifier)
        {
            string filePart;
            switch (identifier) {
                case "ShGeneral":
                    filePart = "SH22General.mp3";
                    break;
                case "ShBlessed":
                    filePart = "SH22Blessed.mp3";
                    break;
                case "ShChaos":
                    filePart = "SH22Chaos.mp3";
                    break;
                case "ShDivine":
                    filePart = "SH22Divine.mp3";
                    break;
                case "ShExalted":
                    filePart = "SH22Exalted.mp3";
                    break;
                case "ShMirror":
                    filePart = "SH22Mirror.mp3";
                    break;
                case "ShAlchemy":
                    filePart = "SH22Alchemy.mp3";
                    break;
                case "ShFusing":
                    filePart = "SH22Fusing.mp3";
                    break;
                case "ShRegal":
                    filePart = "SH22Regal.mp3";
                    break;
                case "ShVaal":
                    filePart = "SH22Vaal.mp3";
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

            _mediaPlayer.Open(new Uri(prefix + filePart, UriKind.Relative));
            _mediaPlayer.Play();
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

            _mediaPlayer.Open(new Uri(prefix + filePart, UriKind.Relative));
            _mediaPlayer.Play();
        }

        private void OnBlockEnabledStatusChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(BlockEnabled));
        }

        private void OnBlockItemChanged(object sender, EventArgs e)
        {
            if (sender is IItemFilterBlockItem itemFilterBlockItem && itemFilterBlockItem.IsDirty)
            {
                IsDirty = true;
            }

            if (sender is CustomSoundBlockItem customSoundBlockItem)
            {
                if (!string.IsNullOrWhiteSpace(customSoundBlockItem.Value) && _parentScriptViewModel.CustomSoundsAvailable.IndexOf(customSoundBlockItem.Value) < 0)
                {
                    _parentScriptViewModel.CustomSoundsAvailable.Add(customSoundBlockItem.Value);
                }
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
            RaisePropertyChanged(nameof(DisplayIconSize));
            RaisePropertyChanged(nameof(DisplayIconColor));
            RaisePropertyChanged(nameof(DisplayIconShape));
            RaisePropertyChanged(nameof(DisplayEffectColor));
            RaisePropertyChanged(nameof(HasSound));
            RaisePropertyChanged(nameof(HasPositionalSound));
            RaisePropertyChanged(nameof(HasCustomSound));
            RaisePropertyChanged(nameof(HasDisabledDefaultSound));
        }

        private void OnBlockItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(RegularBlockItems));
            RaisePropertyChanged(nameof(SummaryBlockItems));
            RaisePropertyChanged(nameof(AudioVisualBlockItems));
            RaisePropertyChanged(nameof(HasAudioVisualBlockItems));
            RaisePropertyChanged(nameof(HasDisabledDefaultSound));
        }

        private void OnCustomSoundFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog {DefaultExt = ".mp3"};
            var poePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Path of Exile\";
            fileDialog.InitialDirectory = poePath;

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = fileDialog.FileName;
                if(fileName.StartsWith(poePath))
                {
                    fileName = fileName.Replace(poePath, "");
                }

                var customSoundBlockItem = BlockItems.First(b => b.GetType() == typeof(CustomSoundBlockItem)) as CustomSoundBlockItem;

                if (_parentScriptViewModel.CustomSoundsAvailable.IndexOf(fileName) < 0)
                {
                    _parentScriptViewModel.CustomSoundsAvailable.Add(fileName);
                }
                customSoundBlockItem.Value = fileName;
            }
        }

        private void OnPlayCustomSoundCommand()
        {
            var poePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Path of Exile\";
            var identifier = BlockItems.OfType<CustomSoundBlockItem>().First().Value;

            if(!Path.IsPathRooted(identifier))
            {
                identifier = poePath + identifier;
            }

            try
            {
                _mediaPlayer.Open(new Uri(identifier, UriKind.Absolute));
                _mediaPlayer.Play();
            }
            catch
            {
                MessageBox.Show("Couldn't play the file. Please be sure it is a valid audio file.");
            }
        }

        private void OnAddBlockGroupCommand()
        {
            var baseBlock = Block as ItemFilterBlock;
            if (baseBlock == null)
                return;

            if (!string.IsNullOrWhiteSpace(BlockGroupSearch))
            {
                var blockToAdd = _parentScriptViewModel.Script.ItemFilterBlockGroups.First();
                if(BlockGroups.Count > 0)
                {
                    blockToAdd = BlockGroups.Last();
                }

                var newGroup = new ItemFilterBlockGroup(BlockGroupSearch, null, AdvancedBlockGroup, false);
                if (baseBlock.BlockGroup == null)
                {
                    baseBlock.BlockGroup = new ItemFilterBlockGroup("", null, false, true)
                    {
                        IsShowChecked = baseBlock.Action == BlockAction.Show,
                        IsEnableChecked = BlockEnabled
                    };
                }
                newGroup.AddOrJoinBlockGroup(baseBlock.BlockGroup);
                blockToAdd.AddOrJoinBlockGroup(newGroup);

                Block.IsEdited = true;
                _parentScriptViewModel.SetDirtyFlag();
                _parentScriptViewModel.Script.ItemFilterScriptSettings.BlockGroupsEnabled = true;

                Messenger.Default.Send(new NotificationMessage<bool>(_parentScriptViewModel.ShowAdvanced, "BlockGroupsChanged"));
                UpdateBlockGroups();
            }

            BlockGroupSearch = "";
            RaisePropertyChanged(nameof(BlockGroupSearch));
        }

        private void OnDeleteBlockGroupCommand()
        {
            if(BlockGroups.Count > 0)
            {
                Block.BlockGroup.DetachSelf(false);
                BlockGroups.RemoveAt(BlockGroups.Count - 1);

                var blockToAdd = _parentScriptViewModel.Script.ItemFilterBlockGroups.First();
                if (BlockGroups.Count > 0)
                {
                    blockToAdd = BlockGroups.Last();
                }
                blockToAdd.AddOrJoinBlockGroup(Block.BlockGroup);

                Block.IsEdited = true;
                _parentScriptViewModel.SetDirtyFlag();

                Messenger.Default.Send(new NotificationMessage<bool>(_parentScriptViewModel.ShowAdvanced, "BlockGroupsChanged"));
                UpdateBlockGroups();

                if (BlockGroups.Count <= 0)
                {
                    _parentScriptViewModel.Script.ItemFilterScriptSettings.BlockGroupsEnabled = false;
                }
            }
        }

        private void UpdateBlockGroups()
        {
            if (!(Block is ItemFilterBlock baseBlock))
                return;

            var currentGroup = baseBlock.BlockGroup;
            var groupList = new List<ItemFilterBlockGroup>();
            while (currentGroup != null)
            {
                groupList.Add(currentGroup);
                currentGroup = currentGroup.ParentGroup;
            }

            var topGroup = _parentScriptViewModel.Script.ItemFilterBlockGroups.First();
            if (groupList.Count > 1)
            {
                groupList.Reverse();
                groupList.RemoveAt(0);
                groupList.RemoveAt(groupList.Count - 1);

                if(groupList.Count > 0)
                {
                    topGroup = groupList.Last();
                }
            }

            BlockGroups = new ObservableCollection<ItemFilterBlockGroup>(groupList);
            BlockGroupSuggestions = new ObservableCollection<string>();

            foreach(var child in topGroup.ChildGroups)
            {
                if(!child.IsLeafNode)
                {
                    BlockGroupSuggestions.Add(child.GroupName);
                }
            }

            RaisePropertyChanged(nameof(BlockGroups));
            RaisePropertyChanged(nameof(BlockGroupSuggestions));
        }
    }
}
