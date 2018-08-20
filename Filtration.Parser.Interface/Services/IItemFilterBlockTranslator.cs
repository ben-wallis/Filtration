using System.Collections.ObjectModel;
using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterBlockTranslator
    {
        IItemFilterBlock TranslateStringToItemFilterBlock(string inputString, IItemFilterScript parentItemFilterScript, string originalString = "", bool initialiseBlockGroupHierarchyBuilder = false);
        IItemFilterCommentBlock TranslateStringToItemFilterCommentBlock(string inputString, IItemFilterScript parentItemFilterScript, string originalString = "");

        string TranslateItemFilterBlockToString(IItemFilterBlock block);
        void ReplaceAudioVisualBlockItemsFromString(ObservableCollection<IItemFilterBlockItem> blockItems, string inputString);
        string TranslateItemFilterCommentBlockToString(IItemFilterCommentBlock itemFilterCommentBlock);
        string TranslateItemFilterBlockBaseToString(IItemFilterBlockBase itemFilterBlockBase);
    }
}