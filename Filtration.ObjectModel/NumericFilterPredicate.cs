using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Filtration.ObjectModel.Annotations;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ObjectModel
{
    public class NumericFilterPredicate : INotifyPropertyChanged
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

        public bool CompareUsing(int target)
        {
            switch (PredicateOperator)
            {
                case FilterPredicateOperator.Equal:
                    {
                        return target == PredicateOperand;
                    }
                case FilterPredicateOperator.GreaterThan:
                    {
                        return target > PredicateOperand;
                    }
                case FilterPredicateOperator.GreaterThanOrEqual:
                    {
                        return target >= PredicateOperand;
                    }
                case FilterPredicateOperator.LessThan:
                    {
                        return target < PredicateOperand;
                    }
                case FilterPredicateOperator.LessThanOrEqual:
                    {
                        return target <= PredicateOperand;
                    }
                default:
                    {
                        return false;
                    }
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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
