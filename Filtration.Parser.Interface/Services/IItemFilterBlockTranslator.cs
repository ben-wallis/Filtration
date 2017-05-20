using System.Collections.ObjectModel;
using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterBlockTranslator
    {
        IItemFilterBlock TranslateStringToItemFilterBlock(string inputString, IItemFilterScriptSettings itemFilterScriptSettings);
        string TranslateItemFilterBlockToString(IItemFilterBlock block);
        void ReplaceAudioVisualBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString);
        IItemFilterCommentBlock TranslateStringToItemFilterCommentBlock(string inputString);
    }
}