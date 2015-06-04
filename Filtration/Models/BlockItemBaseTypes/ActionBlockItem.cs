using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Filtration.Annotations;
using Filtration.Enums;

namespace Filtration.Models.BlockItemBaseTypes
{
    internal class ActionBlockItem : ILootFilterBlockItem
    {
        private BlockAction _action;

        public ActionBlockItem(BlockAction action)
        {
            Action = action;
        }

        public BlockAction Action
        {
            get { return _action; }
            set
            {
                _action = value;
                OnPropertyChanged();
                OnPropertyChanged("SummaryText");
                OnPropertyChanged("SummaryBackgroundColor");
                OnPropertyChanged("SummaryTextColor");
            }
        }

        public string PrefixText
        {
            get { return string.Empty; }
        }

        public int MaximumAllowed
        {
            get { return 1; }
        }

        public string DisplayHeading
        {
            get
            {
                return "Action";
            }
        }

        public string SummaryText
        {
            get
            {
                return Action == BlockAction.Show ? "Show" : "Hide";
            }
        }

        public Color SummaryBackgroundColor
        {
            get
            {
                return Action == BlockAction.Show ? Colors.LimeGreen : Colors.OrangeRed;
            }
        }

        public Color SummaryTextColor
        {
            get
            {
                return Action == BlockAction.Show ? Colors.Black : Colors.White;
            }
        }

        public int SortOrder { get { return 0; } }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
