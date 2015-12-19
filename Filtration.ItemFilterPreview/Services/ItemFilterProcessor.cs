using System.Linq;
using Filtration.ItemFilterPreview.Model;
using Filtration.ObjectModel;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ItemFilterPreview.Services
{
    public class ItemFilterProcessor
    {
        private IItemFilterScript _itemFilterScript;

        public ItemFilterProcessor()
        {
            
        }

        public void LoadItemFilterScript(IItemFilterScript itemFilterScript)
        {
            _itemFilterScript = itemFilterScript;
        }

        public void ItemIsVisible(IItem item)
        {
            foreach (var block in _itemFilterScript.ItemFilterBlocks)
            {
                
            }
        }

        
    }
}
