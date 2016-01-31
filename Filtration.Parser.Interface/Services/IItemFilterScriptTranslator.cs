using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterScriptTranslator
    {
        ItemFilterScript TranslateStringToItemFilterScript(string inputString);
        string TranslateItemFilterScriptToString(ItemFilterScript script);
    }
}