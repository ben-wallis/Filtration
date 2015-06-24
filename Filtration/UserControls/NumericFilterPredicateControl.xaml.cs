using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Filtration.Annotations;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;

namespace Filtration.UserControls
{
    internal partial class NumericFilterPredicateControl : INotifyPropertyChanged
    {
        public NumericFilterPredicateControl()
        {
            InitializeComponent();
            // ReSharper disable once PossibleNullReferenceException
            (Content as FrameworkElement).DataContext = this;
        }

        public static readonly DependencyProperty NumericFilterPredicateProperty = DependencyProperty.Register(
            "NumericFilterPredicate",
            typeof (NumericFilterPredicate),
            typeof (NumericFilterPredicateControl),
            new FrameworkPropertyMetadata(OnNumericFilterPredicatePropertyChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof (string),
            typeof (NumericFilterPredicateControl),
            new FrameworkPropertyMetadata {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum",
            typeof (int),
            typeof (NumericFilterPredicateControl),
            new FrameworkPropertyMetadata {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum",
            typeof (int),
            typeof (NumericFilterPredicateControl),
            new FrameworkPropertyMetadata {BindsTwoWayByDefault = true});

        public NumericFilterPredicate NumericFilterPredicate
        {
            get
            {
                return (NumericFilterPredicate)GetValue(NumericFilterPredicateProperty);
            }
            set
            {
                SetValue(NumericFilterPredicateProperty, value);
                OnPropertyChanged();
                OnPropertyChanged("FilterPredicateOperator");
                OnPropertyChanged("FilterPredicateOperand");
            }
        }

        public FilterPredicateOperator FilterPredicateOperator
        {
            get { return NumericFilterPredicate.PredicateOperator; }
            set
            {
                NumericFilterPredicate.PredicateOperator = value;
                OnPropertyChanged();
            }
        }

        public int FilterPredicateOperand
        {
            get { return NumericFilterPredicate.PredicateOperand; }
            set
            {
                NumericFilterPredicate.PredicateOperand = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public int Minimum
        {
            get
            {
                return (int)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
            }
        }

        public int Maximum
        {
            get
            {
                return (int)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        private static void OnNumericFilterPredicatePropertyChanged(DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var control = source as NumericFilterPredicateControl;
            if (control == null) return;

            control.OnPropertyChanged("FilterPredicateOperator");
            control.OnPropertyChanged("FilterPredicateOperand");
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
