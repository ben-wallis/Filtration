using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IItemFilterScriptTranslator
    {
        IItemFilterScript TranslateStringToItemFilterScript(string inputString);
        IItemFilterScript TranslatePastedStringToItemFilterScript(string inputString, bool blockGroupsEnabled);
        string TranslateItemFilterScriptToString(IItemFilterScript script);
    }
}