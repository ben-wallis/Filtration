using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Filtration.Models;
using Filtration.Models.BlockItemTypes;
using Filtration.Services;
using Filtration.Translators;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal class LootFilterBlockViewModel : FiltrationViewModelBase, ILootFilterBlockViewModel
    {
        private readonly ILootFilterBlockTranslator _translator;
        private readonly IStaticDataService _staticDataService;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private bool _displaySettingsPopupOpen;
        
        public LootFilterBlockViewModel(ILootFilterBlockTranslator translator, IStaticDataService staticDataService)
        {
            _translator = translator;
            _staticDataService = staticDataService;
            CopyBlockCommand = new RelayCommand(OnCopyConditionCommand);

            AddFilterBlockItemCommand = new RelayCommand<Type>(OnAddFilterBlockItemCommand);
            AddAudioVisualBlockItemCommand = new RelayCommand<Type>(OnAddAudioVisualBlockItemCommand);
            RemoveFilterBlockItemCommand = new RelayCommand<ILootFilterBlockItem>(OnRemoveFilterBlockItemCommand);
            RemoveAudioVisualBlockItemCommand = new RelayCommand<ILootFilterBlockItem>(OnRemoveAudioVisualBlockItemCommand);
            PlaySoundCommand = new RelayCommand(OnPlaySoundCommand, () => HasSound);
        }

        public void Initialise(LootFilterBlock lootFilterBlock)
        {
            if (lootFilterBlock == null)
            {
                throw new ArgumentNullException("lootFilterBlock");
            }

            Block = lootFilterBlock;
            lootFilterBlock.BlockItems.CollectionChanged += OnBlockItemsCollectionChanged;

            foreach (var blockItem in lootFilterBlock.BlockItems.OfType<IAudioVisualBlockItem>())
            {
                blockItem.PropertyChanged += OnAudioVisualBlockItemChanged;
            }
        }

        public RelayCommand CopyBlockCommand { get; private set; }
        public RelayCommand<Type> AddFilterBlockItemCommand { get; private set; }
        public RelayCommand<Type> AddAudioVisualBlockItemCommand { get; private set; }
        public RelayCommand<ILootFilterBlockItem> RemoveFilterBlockItemCommand { get; private set; }
        public RelayCommand<ILootFilterBlockItem> RemoveAudioVisualBlockItemCommand { get; private set; }
        public RelayCommand PlaySoundCommand { get; private set; }

        public LootFilterBlock Block { get; private set; }
        public bool IsDirty { get; set; }

        public ObservableCollection<ILootFilterBlockItem> FilterBlockItems
        {
            get { return Block.BlockItems; }
        }

        public IEnumerable<ILootFilterBlockItem> SummaryBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<ILootFilterBlockItem> AudioVisualBlockItems
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
                Block.Description = value;
                RaisePropertyChanged();
            }
        }
        
        public bool HasTextColor
        {
            get { return FilterBlockItems.Count(b => b is TextColorBlockItem) > 0; }
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
            get { return FilterBlockItems.Count(b => b is BackgroundColorBlockItem) > 0; }
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
            get { return FilterBlockItems.Count(b => b is BorderColorBlockItem) > 0; }
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
            get { return FilterBlockItems.Count(b => b is FontSizeBlockItem) > 0; }
        }

        public bool HasSound
        {
            get { return FilterBlockItems.Count(b => b is SoundBlockItem) > 0; }
        }

        private void OnCopyConditionCommand()
        {
            Clipboard.SetText(_translator.TranslateLootFilterBlockToString(Block));
        }

        private void OnAddFilterBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (ILootFilterBlockItem) Activator.CreateInstance(blockItemType);
        
            FilterBlockItems.Add(newBlockItem);
            IsDirty = true;
        }

        private void OnRemoveFilterBlockItemCommand(ILootFilterBlockItem blockItem)
        {
            FilterBlockItems.Remove(blockItem);
            IsDirty = true;
        }

        private void OnAddAudioVisualBlockItemCommand(Type blockItemType)
        {
            if (!AddBlockItemAllowed(blockItemType)) return;
            var newBlockItem = (ILootFilterBlockItem) Activator.CreateInstance(blockItemType);

            newBlockItem.PropertyChanged += OnAudioVisualBlockItemChanged;
            FilterBlockItems.Add(newBlockItem);
            OnAudioVisualBlockItemChanged(null, null);
            IsDirty = true;
        }

        private void OnRemoveAudioVisualBlockItemCommand(ILootFilterBlockItem blockItem)
        {
            blockItem.PropertyChanged -= OnAudioVisualBlockItemChanged;
            FilterBlockItems.Remove(blockItem);
            OnAudioVisualBlockItemChanged(null, null);
            IsDirty = true;
        }

        private bool AddBlockItemAllowed(Type type)
        {
            var blockItem = (ILootFilterBlockItem)Activator.CreateInstance(type);
            var blockCount = FilterBlockItems.Count(b => b.GetType() == type);
            return blockCount < blockItem.MaximumAllowed;
        }

        private void OnPlaySoundCommand()
        {
            var soundUri = "Resources/AlertSound" + FilterBlockItems.OfType<SoundBlockItem>().First().Value + ".wav";
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
