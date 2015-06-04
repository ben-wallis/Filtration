using System.Collections.ObjectModel;
using System.ComponentModel;
using Filtration.Models;

namespace Filtration.ViewModels
{
    internal interface ILootFilterScriptViewModel
    {
        ObservableCollection<ILootFilterBlockViewModel> LootFilterBlockViewModels { get; }
        LootFilterScript Script { get; }
        bool IsDirty { get; }
        string Description { get; set; }
        string Filename { get; }
        string DisplayName { get; }
        string Filepath { get; }
        void Initialise(LootFilterScript lootFilterScript);
        void RemoveDirtyFlag();
        event PropertyChangedEventHandler PropertyChanged;
    }
}