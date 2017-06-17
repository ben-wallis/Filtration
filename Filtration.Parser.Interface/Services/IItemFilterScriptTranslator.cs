using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterScriptTranslator
    {
        IItemFilterScript TranslateStringToItemFilterScript(string inputString);
        string TranslateItemFilterScriptToString(IItemFilterScript script);
    }
}