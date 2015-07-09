using System;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;
using Filtration.ObjectModel.LootExplosionStudio;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class NumericFilterPredicateBlockItem : BlockItemBase
    {
        private NumericFilterPredicate _filterPredicate;

        protected NumericFilterPredicateBlockItem()
        {
            FilterPredicate = new NumericFilterPredicate();
            FilterPredicate.PropertyChanged += OnFilterPredicateChanged;
        }

        protected NumericFilterPredicateBlockItem(FilterPredicateOperator predicateOperator, int predicateOperand)
        {
            FilterPredicate = new NumericFilterPredicate(predicateOperator, predicateOperand);
            FilterPredicate.PropertyChanged += OnFilterPredicateChanged;
        }

        public override string OutputText
        {
            get
            {
                return PrefixText + " " + FilterPredicate.PredicateOperator.GetAttributeDescription() +
                       " " + FilterPredicate.PredicateOperand;
            }
        }

        public abstract int Minimum { get; }
        public abstract int Maximum { get; }

        public NumericFilterPredicate FilterPredicate
        {
            get { return _filterPredicate; }
            protected set
            {
                _filterPredicate = value;
                OnPropertyChanged();
            }
        }

        private void OnFilterPredicateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("FilterPredicate");
            OnPropertyChanged("SummaryText");
        }



        public abstract int GetLootItemProperty(LootItem lootItem);

        public virtual bool MatchesLootItem(LootItem lootItem)
        {
            var lootItemProperty = GetLootItemProperty(lootItem);
            var predicateOperand = FilterPredicate.PredicateOperand;

            switch (FilterPredicate.PredicateOperator)
            {
                case FilterPredicateOperator.Equal:
                {
                    return lootItemProperty == predicateOperand;
                }
                case FilterPredicateOperator.GreaterThan:
                {
                    return lootItemProperty > predicateOperand;
                }
                case FilterPredicateOperator.GreaterThanOrEqual:
                {
                    return lootItemProperty >= predicateOperand;
                }
                case FilterPredicateOperator.LessThan:
                {
                    return lootItemProperty < predicateOperand;
                }
                case FilterPredicateOperator.LessThanOrEqual:
                {
                    return lootItemProperty <= predicateOperand;
                }
                case FilterPredicateOperator.NotEqual:
                {
                    return lootItemProperty != predicateOperand;
                }
                default:
                    return false;
            }
        }
    }
}
