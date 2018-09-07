using System;
using Filtration.Common.ViewModels;
using Filtration.Properties;

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

            switch (Name)
            {
                case "Section Browser":
                    IsVisible = Settings.Default.ShowSectionBrowser;
                    break;
                case "Block Group Browser":
                    IsVisible = Settings.Default.ShowBlockGroupBrowser;
                    break;
                case "Block Output Preview":
                    IsVisible = Settings.Default.ShowBlockOutputPreview;
                    break;
            }
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
                    switch(Name)
                    {
                        case "Section Browser":
                            Settings.Default.ShowSectionBrowser = value;
                            break;
                        case "Block Group Browser":
                            Settings.Default.ShowBlockGroupBrowser = value;
                            break;
                        case "Block Output Preview":
                            Settings.Default.ShowBlockOutputPreview = value;
                            break;
                    }
                    RaisePropertyChanged();
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
