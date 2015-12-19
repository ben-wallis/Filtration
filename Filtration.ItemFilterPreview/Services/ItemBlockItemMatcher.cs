using System;
using System.Linq;
using Filtration.ItemFilterPreview.Model;
using Filtration.ObjectModel.BlockItemBaseTypes;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;

namespace Filtration.ItemFilterPreview.Services
{
    internal interface IItemBlockItemMatcher
    {
        bool BaseTypeBlockItemMatch(BaseTypeBlockItem baseTypeBlockItem, IItem item);
        bool ClassBlockItemMatch(ClassBlockItem classBlockItem, IItem item);
        bool DropLevelBlockItemMatch(DropLevelBlockItem dropLevelBlockItem, IItem item);
    }

    internal class ItemBlockItemMatcher : IItemBlockItemMatcher
    {
        public bool BaseTypeBlockItemMatch(BaseTypeBlockItem baseTypeBlockItem, IItem item)
        {
            return baseTypeBlockItem.Items.Any(b => item.BaseType.StartsWith(b));
        }

        public bool ClassBlockItemMatch(ClassBlockItem classBlockItem, IItem item)
        {
            return classBlockItem.Items.Any(c => item.ItemClass.StartsWith(c));
        }

        public bool DropLevelBlockItemMatch(DropLevelBlockItem dropLevelBlockItem, IItem item)
        {
            return NumericFilterPredicateBlockItemMatch(dropLevelBlockItem, item.DropLevel);
        }

        private bool NumericFilterPredicateBlockItemMatch<T>(T numericFilterPredicateBlockItem, int matchValue) where T : NumericFilterPredicateBlockItem
        {
            switch (numericFilterPredicateBlockItem.FilterPredicate.PredicateOperator)
            {
                case FilterPredicateOperator.Equal:
                {
                    return matchValue == numericFilterPredicateBlockItem.FilterPredicate.PredicateOperand;
                }
                case FilterPredicateOperator.GreaterThan:
                {
                    return matchValue > numericFilterPredicateBlockItem.FilterPredicate.PredicateOperand;
                }
                case FilterPredicateOperator.GreaterThanOrEqual:
                {
                    return matchValue >= numericFilterPredicateBlockItem.FilterPredicate.PredicateOperand;
                }
                case FilterPredicateOperator.LessThan:
                {
                    return matchValue < numericFilterPredicateBlockItem.FilterPredicate.PredicateOperand;
                }
                case FilterPredicateOperator.LessThanOrEqual:
                {
                    return matchValue <= numericFilterPredicateBlockItem.FilterPredicate.PredicateOperand;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}
