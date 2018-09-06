using System.Collections.Generic;
using Filtration.ObjectModel;

namespace Filtration.Parser.Interface.Services
{
    public interface IBlockGroupHierarchyBuilder
    {
        void Initialise(ItemFilterBlockGroup rootBlockGroup);
        void Cleanup();
        ItemFilterBlockGroup IntegrateStringListIntoBlockGroupHierarchy(IEnumerable<string> groupStrings, bool show, bool enabled);
    }
}