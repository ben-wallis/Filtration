using System.ComponentModel;
using Filtration.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels
{
    internal interface ILootFilterBlockViewModel
    {
        void Initialise(LootFilterBlock lootFilterBlock);
        bool IsDirty { get; set; }
        LootFilterBlock Block { get; }
        RelayCommand CopyBlockCommand { get; }
        string BlockDescription { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}