using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Filtration.ObjectModel.Annotations;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Commands;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel
{
    public interface IItemFilterScript : INotifyPropertyChanged
    {
        ICommandManager CommandManager { get; }

        ObservableCollection<IItemFilterBlockBase> ItemFilterBlocks { get; }
        ObservableCollection<ItemFilterBlockGroup> ItemFilterBlockGroups { get; }
        ThemeComponentCollection ThemeComponents { get; set; }
        string FilePath { get; set; }
        string Description { get; set; }
        DateTime DateModified { get; set; }
        bool IsDirty { get; }

        IItemFilterScriptSettings ItemFilterScriptSettings { get; }

        List<string> Validate();
        void ReplaceColors(ReplaceColorsParameterSet replaceColorsParameterSet);
    }

    public interface IItemFilterScriptInternal : IItemFilterScript
    {
        void SetIsDirty(bool isDirty);
    }

    public class ItemFilterScript : IItemFilterScriptInternal
    {
        private bool _isDirty;
        private string _description;

        internal ItemFilterScript()
        {
            ItemFilterBlocks = new ObservableCollection<IItemFilterBlockBase>();
            ItemFilterBlockGroups = new ObservableCollection<ItemFilterBlockGroup>
            {
                new ItemFilterBlockGroup("Root", null)
            };
            ThemeComponents = new ThemeComponentCollection { IsMasterCollection = true };
            ItemFilterScriptSettings = new ItemFilterScriptSettings(ThemeComponents);
        }

        public ItemFilterScript(ICommandManagerInternal commandManager) : this()
        {
            CommandManager = commandManager;
            commandManager.SetScript(this);
        }

        public ICommandManager CommandManager { get; }

        public ObservableCollection<IItemFilterBlockBase> ItemFilterBlocks { get; }
        public ObservableCollection<ItemFilterBlockGroup> ItemFilterBlockGroups { get; }

        public ThemeComponentCollection ThemeComponents { get; set; } 

        public IItemFilterScriptSettings ItemFilterScriptSettings { get; }

        public string FilePath { get; set; }

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateModified { get; set; }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (value == _isDirty) return;
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public void SetIsDirty(bool isDirty)
        {
            IsDirty = isDirty;
        }

        public List<string> Validate()
        {
            var validationErrors = new List<string>();

            if (ItemFilterBlocks.Count == 0)
            {
                validationErrors.Add("A script must have at least one block");
            }

            return validationErrors;
        }
        
        public void ReplaceColors(ReplaceColorsParameterSet replaceColorsParameterSet)
        {
            foreach (var block in ItemFilterBlocks.OfType<ItemFilterBlock>().Where(b => BlockIsColorReplacementCandidate(replaceColorsParameterSet, b)))
            {
                if (replaceColorsParameterSet.ReplaceTextColor)
                {
                    var textColorBlockItem = block.BlockItems.OfType<TextColorBlockItem>().First();
                    textColorBlockItem.Color = replaceColorsParameterSet.NewTextColor;
                }
                if (replaceColorsParameterSet.ReplaceBackgroundColor)
                {
                    var backgroundColorBlockItem = block.BlockItems.OfType<BackgroundColorBlockItem>().First();
                    backgroundColorBlockItem.Color = replaceColorsParameterSet.NewBackgroundColor;
                }
                if (replaceColorsParameterSet.ReplaceBorderColor)
                {
                    var borderColorBlockItem = block.BlockItems.OfType<BorderColorBlockItem>().First();
                    borderColorBlockItem.Color = replaceColorsParameterSet.NewBorderColor;
                }
            }
        }

        private bool BlockIsColorReplacementCandidate(ReplaceColorsParameterSet replaceColorsParameterSet, IItemFilterBlock block)
        {
            var textColorItem = block.BlockItems.OfType<TextColorBlockItem>().FirstOrDefault();
            var backgroundColorItem = block.BlockItems.OfType<BackgroundColorBlockItem>().FirstOrDefault();
            var borderColorItem = block.BlockItems.OfType<BorderColorBlockItem>().FirstOrDefault();

            // If we don't have all of the things we want to replace, then we aren't a candidate for replacing those things.
            if ((textColorItem == null && replaceColorsParameterSet.ReplaceTextColor) ||
                (backgroundColorItem == null && replaceColorsParameterSet.ReplaceBackgroundColor) ||
                (borderColorItem == null && replaceColorsParameterSet.ReplaceBorderColor))
            {
                return false;
            }

            if ((replaceColorsParameterSet.ReplaceTextColor &&
                 textColorItem.Color != replaceColorsParameterSet.OldTextColor) ||
                (replaceColorsParameterSet.ReplaceBackgroundColor &&
                 backgroundColorItem.Color != replaceColorsParameterSet.OldBackgroundColor) ||
                (replaceColorsParameterSet.ReplaceBorderColor &&
                 borderColorItem.Color != replaceColorsParameterSet.OldBorderColor))
            {
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
