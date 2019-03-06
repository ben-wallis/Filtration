using System.Windows.Media;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ObjectModel.BlockItemTypes
{
    public sealed class AnyEnchantmentBlockItem : BooleanBlockItem
    {
        public AnyEnchantmentBlockItem()
        {
        }

        public AnyEnchantmentBlockItem(bool booleanValue) : base(booleanValue)
        {
        }

        public override string PrefixText => "AnyEnchantment";
        public override string DisplayHeading => "Any Enchantment";
        public override Color SummaryBackgroundColor => Colors.YellowGreen;
        public override Color SummaryTextColor => Colors.Black;
        public override BlockItemOrdering SortOrder => BlockItemOrdering.AnyEnchantment;
    }
}
