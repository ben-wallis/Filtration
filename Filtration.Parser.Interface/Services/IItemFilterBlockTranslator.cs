using System.Collections.ObjectModel;
using Filtration.ObjectModel;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterBlockTranslator
    {
        IItemFilterBlock TranslateStringToItemFilterBlock(string inputString,
            ThemeComponentCollection masterComponentCollection);
        string TranslateItemFilterBlockToString(IItemFilterBlock block);
        void ReplaceColorBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString);
    }
}