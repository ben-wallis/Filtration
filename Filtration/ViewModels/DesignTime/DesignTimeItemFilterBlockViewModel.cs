using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Commands;
using Filtration.ObjectModel.Enums;
using GalaSoft.MvvmLight.CommandWpf;
using Xceed.Wpf.Toolkit;

namespace Filtration.ViewModels.DesignTime
{
    internal class FakeCommandManager : ICommandManagerInternal {
        public void ExecuteCommand(ICommand command)
        {
            throw new NotImplementedException();
        }

        public void Undo(int undoLevels = 1)
        {
            throw new NotImplementedException();
        }

        public void Redo(int redoLevels = 1)
        {
            throw new NotImplementedException();
        }

        public void SetScript(IItemFilterScriptInternal layout)
        {
        }
    }

    internal class DesignTimeItemFilterBlockViewModel : IItemFilterBlockViewModel
    {
        private ItemFilterBlock itemFilterBlock;

        public DesignTimeItemFilterBlockViewModel()
        {
            itemFilterBlock = new ItemFilterBlock(new ItemFilterScript(new FakeCommandManager()))
                                  {
                                      Action = BlockAction.Show,
                                      Enabled = true
                                  };

            itemFilterBlock.BlockItems.Add(new RarityBlockItem(FilterPredicateOperator.Equal, ItemRarity.Rare));
            itemFilterBlock.BlockItems.Add(new DropLevelBlockItem(FilterPredicateOperator.GreaterThan, 23));
            itemFilterBlock.BlockItems.Add(new BaseTypeBlockItem());
            itemFilterBlock.BlockItems.Add(new BaseTypeBlockItem());
            itemFilterBlock.BlockItems.Add(new BaseTypeBlockItem());
        }

        public void Initialise(IItemFilterBlockBase itemFilterBlock, IItemFilterScriptViewModel itemFilterScriptViewModel)
        {
            //throw new NotImplementedException();
        }

        public IItemFilterBlockBase BaseBlock { get; }
        public bool IsDirty { get; set; }
        public bool IsVisible { get; set; }
        public event EventHandler BlockBecameDirty;

        public bool IsExpanded
        {
            get => true;
            set { }
        }

        public IItemFilterBlock Block => itemFilterBlock;

        public bool BlockEnabled
        {
            get => true;
            set { }
        }

        public string BlockDescription { get; set; }
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
        public ObservableCollection<ItemFilterBlockGroup> BlockGroups { get; }
        public ObservableCollection<string> BlockGroupSuggestions { get; }
        public string BlockGroupSearch { get; set; }
        public ObservableCollection<IItemFilterBlockItem> BlockItems => Block.BlockItems;

        public IEnumerable<IItemFilterBlockItem> SummaryBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<IItemFilterBlockItem> RegularBlockItems
        {
            get { return Block.BlockItems.Where(b => !(b is IAudioVisualBlockItem)); }
        }

        public IEnumerable<IItemFilterBlockItem> AudioVisualBlockItems { get; }
        public bool AdvancedBlockGroup { get; }
        public bool AudioVisualBlockItemsGridVisible { get; set; }
        public bool DisplaySettingsPopupOpen { get; set; }
        public IEnumerable<string> AutoCompleteItemClasses { get; }
        public IEnumerable<string> AutoCompleteItemBaseTypes { get; }
        public IEnumerable<string> AutoCompleteProphecies { get; }
        public IEnumerable<string> AutocompleteItemMods { get; }
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
                                                         typeof (GemLevelBlockItem),
                                                         typeof (StackSizeBlockItem),
                                                         typeof (HasExplicitModBlockItem),
                                                         typeof (HasEnchantmentBlockItem)
                                                     };
        public List<Type> AudioVisualBlockItemTypesAvailable { get; }
        public Color DisplayTextColor => Colors.Red;
        public Color DisplayBackgroundColor => Colors.White;
        public Color DisplayBorderColor => Colors.GreenYellow;
        public double DisplayFontSize => 20;
        public int DisplayIconSize { get; }
        public int DisplayIconColor { get; }
        public int DisplayIconShape { get; }
        public Color DisplayEffectColor { get; }
        public bool HasSound { get; }
        public bool HasPositionalSound { get; }
        public bool HasCustomSound { get; }
        public bool HasDisabledDefaultSound { get; }
        public bool HasAudioVisualBlockItems { get; }
        public void RefreshBlockPreview()
        {
            throw new NotImplementedException();
        }
    }
}
