using System;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

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

        public override string OutputText => PrefixText + " " + FilterPredicate.PredicateOperator.GetAttributeDescription() +
                                             " " + FilterPredicate.PredicateOperand;

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
            IsDirty = true;
            OnPropertyChanged(nameof(FilterPredicate));
            OnPropertyChanged(nameof(SummaryText));
        }
    }
}
