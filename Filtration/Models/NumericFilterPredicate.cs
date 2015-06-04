using System.ComponentModel;
using System.Runtime.CompilerServices;
using Filtration.Annotations;
using Filtration.Enums;
using Filtration.Extensions;

namespace Filtration.Models
{
    internal class NumericFilterPredicate : INotifyPropertyChanged
    {
        private FilterPredicateOperator _predicateOperator;
        private int _predicateOperand;

        public NumericFilterPredicate(FilterPredicateOperator predicateOperator, int predicateOperand)
        {
            PredicateOperator = predicateOperator;
            PredicateOperand = predicateOperand;
        }

        public NumericFilterPredicate()
        {
            
        }

        public FilterPredicateOperator PredicateOperator
        {
            get { return _predicateOperator; }
            set
            {
                _predicateOperator = value; 
                OnPropertyChanged();
            }
        }

        public int PredicateOperand
        {
            get { return _predicateOperand; }
            set
            {
                _predicateOperand = value; 
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return PredicateOperator.GetAttributeDescription() + " " + PredicateOperand;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
