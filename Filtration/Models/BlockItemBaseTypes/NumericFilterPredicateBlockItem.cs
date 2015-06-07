using System;
using Filtration.Enums;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class NumericFilterPredicateBlockItem : BlockItemBase
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
    }
}
