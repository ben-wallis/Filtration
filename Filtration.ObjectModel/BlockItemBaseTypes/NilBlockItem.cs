namespace Filtration.ObjectModel.BlockItemBaseTypes
{
    public abstract class NilBlockItem : BlockItemBase
    {
        protected NilBlockItem()
        {

        }

        public override string OutputText => PrefixText;
        public override string SummaryText => DisplayHeading;
        public override int MaximumAllowed => 1;

        public abstract string Description { get; }
    }
}
