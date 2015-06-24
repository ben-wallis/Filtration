using System.Windows.Media;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.Extensions;

namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public class ActionBlockItem : BlockItemBase
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

        public override string OutputText
        {
            get { return Action.GetAttributeDescription(); }
        }

        public override string PrefixText
        {
            get { return string.Empty; }
        }

        public override int MaximumAllowed
        {
            get { return 1; }
        }

        public override string DisplayHeading
        {
            get
            {
                return "Action";
            }
        }

        public override string SummaryText
        {
            get
            {
                return Action == BlockAction.Show ? "Show" : "Hide";
            }
        }

        public override Color SummaryBackgroundColor
        {
            get
            {
                return Action == BlockAction.Show ? Colors.LimeGreen : Colors.OrangeRed;
            }
        }

        public override Color SummaryTextColor
        {
            get
            {
                return Action == BlockAction.Show ? Colors.Black : Colors.White;
            }
        }

        public override int SortOrder { get { return 0; } }

        public void ToggleAction()
        {
            Action = Action == BlockAction.Show ? BlockAction.Hide : BlockAction.Show;
        }
    }
}
