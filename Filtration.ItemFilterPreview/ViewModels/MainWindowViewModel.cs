using System.Collections.Generic;
using System.Collections.ObjectModel;
using Filtration.ItemFilterPreview.Properties;
using Filtration.ItemFilterPreview.Services;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using Filtration.Parser.Interface.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ItemFilterPreview.ViewModels
{
    internal interface IMainWindowViewModel
    {
    }

    internal class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly IItemFilterScriptTranslator _itemFilterScriptTranslator;
        private readonly IItemFilterProcessor _itemFilterProcessor;
        private IItemFilterScript _itemFilterScript;


        public MainWindowViewModel(IItemFilterScriptTranslator itemFilterScriptTranslator, IItemFilterProcessor itemFilterProcessor)
        {
            _itemFilterScriptTranslator = itemFilterScriptTranslator;
            _itemFilterProcessor = itemFilterProcessor;
            LoadScriptCommand = new RelayCommand(OnLoadScriptCommand);
            LoadAlternateScriptCommand = new RelayCommand(OnLoadAlternateScriptCommand);
            ProcessItemFilterCommand = new RelayCommand(OnProcessItemFilterCommand);
        }

        public RelayCommand LoadScriptCommand { get; private set; }
        public RelayCommand LoadAlternateScriptCommand { get; private set; }
        public RelayCommand ProcessItemFilterCommand { get; private set; }
        
        public IEnumerable<IFilteredItem> FilteredItems
        {
            get { return _filteredItems; }
            private set
            {
                _filteredItems = value;
                RaisePropertyChanged();
            }
        }

        private void OnLoadScriptCommand()
        {
            _itemFilterScript = _itemFilterScriptTranslator.TranslateStringToItemFilterScript(Resources.neversinkfilter);
        }

        private void OnLoadAlternateScriptCommand()
        {
            _itemFilterScript = _itemFilterScriptTranslator.TranslateStringToItemFilterScript(Resources.muldini);
        }

        private void OnProcessItemFilterCommand()
        {
            FilteredItems = _itemFilterProcessor.ProcessItemsAgainstItemFilterScript(_itemFilterScript, TestItems);
        }

        private readonly List<IItem> TestItems = new List<IItem>
        {
              new Item
                {
                    Description = "Full Plate",
                    BaseType = "Full Plate",
                    ItemClass = "Body Armors",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 66,
                    DropLevel = 28,
                    Height = 3,
                    Width = 2,
                    SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> { new Socket(SocketColor.Red) , new Socket(SocketColor.Red) , new Socket(SocketColor.Red) , new Socket(SocketColor.Red) , new Socket(SocketColor.Red) , new Socket(SocketColor.Red) }, true)}
                },
                new Item
                {
                    Description = "Scroll of Wisdom",
                    BaseType = "Scroll of Wisdom",
                    ItemClass = "Currency",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 75,
                    DropLevel = 1,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Unset Ring",
                    BaseType = "Unset Ring",
                    ItemClass = "Rings",
                    ItemRarity = ItemRarity.Rare,
                    ItemLevel = 53,
                    DropLevel = 45,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Incinerate",
                    BaseType = "Incinerate",
                    ItemClass = "Active Skill Gems",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 9,
                    DropLevel = 9,
                    Quality = 10,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Mirror of Kalandra",
                    BaseType = "Mirror of Kalandra",
                    ItemClass = "Currency",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 77,
                    DropLevel = 1,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "The Gemcutter",
                    BaseType = "The Gemcutter",
                    ItemClass = "Divination Card",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 1,
                    DropLevel = 72,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Thaumetic Sulphite",
                    BaseType = "Thaumetic Sulphite",
                    ItemClass = "Quest Items",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 32,
                    DropLevel = 1,
                    Height = 2,
                    Width = 2,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Fishing Rod",
                    BaseType = "Fishing Rod",
                    ItemClass = "Fishing Rods",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 1,
                    DropLevel = 1,
                    Height = 4,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Dry Peninsula Map",
                    BaseType = "Dry Peninsula Map",
                    ItemClass = "Maps",
                    ItemRarity = ItemRarity.Magic,
                    ItemLevel = 75,
                    DropLevel = 75,
                    Height = 1,
                    Width = 1,
                    SocketGroups = new List<SocketGroup>()
                },
                new Item
                {
                    Description = "Stone Hammer",
                    BaseType = "Stone Hammer",
                    ItemClass = "One Hand Maces",
                    ItemRarity = ItemRarity.Normal,
                    ItemLevel = 1,
                    DropLevel = 1,
                    Height = 3,
                    Width = 2,
                    SocketGroups = new List<SocketGroup>()
                }
        };

        private IEnumerable<IFilteredItem> _filteredItems;
    }
}
