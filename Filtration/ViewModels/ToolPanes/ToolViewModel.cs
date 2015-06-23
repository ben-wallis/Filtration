using System;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface IToolViewModel
    {
        void Initialise(IAvalonDockWorkspaceViewModel avalonDockWorkspaceViewModel);
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
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        protected IAvalonDockWorkspaceViewModel AvalonDockWorkspaceViewModel{ get; private set; }

        protected virtual void OnActiveDocumentChanged(object sender, EventArgs e)
        {
        }

        public virtual void Initialise(IAvalonDockWorkspaceViewModel avalonDockWorkSpaceViewModel)
        {
            AvalonDockWorkspaceViewModel = avalonDockWorkSpaceViewModel;
            avalonDockWorkSpaceViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
        }
    }
}
