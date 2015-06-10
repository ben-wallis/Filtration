namespace Filtration.ViewModels
{
    internal interface IToolViewModel
    {
        void Initialise(IMainWindowViewModel mainWindowViewModel);
    }

    class ToolViewModel : PaneViewModel, IToolViewModel
    {
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }

        public string Name { get; private set; }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected IMainWindowViewModel MainWindowViewModel { get; private set; }

        public virtual void Initialise(IMainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
    }
}
