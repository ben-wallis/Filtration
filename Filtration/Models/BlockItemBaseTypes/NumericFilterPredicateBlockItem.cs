using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;
using Filtration.Enums;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal abstract class NumericFilterPredicateBlockItem : ILootFilterBlockItem
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

        public abstract string PrefixText { get; }
        public abstract int MaximumAllowed { get; }
        public abstract string DisplayHeading { get; }
        public abstract string SummaryText { get; }
        public abstract Color SummaryBackgroundColor { get; }
        public abstract Color SummaryTextColor { get; }
        public abstract int SortOrder { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
