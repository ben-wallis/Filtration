using System;
using System.Collections.Generic;

namespace Filtration.ViewModels
{
    internal interface ISectionBrowserViewModel
    {
    }

    internal class SectionBrowserViewModel : ToolViewModel, ISectionBrowserViewModel
    {
        private IMainWindowViewModel _mainWindowViewModel;
        private IEnumerable<IItemFilterBlockViewModel> _sectionBlockViewModels;
        private IItemFilterBlockViewModel _selectedSectionBlockViewModel;

        public SectionBrowserViewModel() : base("Section Browser")
        {

            ContentId = ToolContentId;
        }

        public void Initialise(IMainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
        }

        public const string ToolContentId = "SectionBrowserTool";

        public IEnumerable<IItemFilterBlockViewModel> SectionBlockViewModels
        {
            get { return _sectionBlockViewModels; }
            private set
            {
                _sectionBlockViewModels = value;
                RaisePropertyChanged();
            }
        }

        public IItemFilterBlockViewModel SelectedSectionBlockViewModel
        {
            get { return _selectedSectionBlockViewModel; }
            set
            {
                _selectedSectionBlockViewModel = value;
                RaisePropertyChanged();
            }
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            SectionBlockViewModels = _mainWindowViewModel.ActiveDocument != null ? _mainWindowViewModel.ActiveDocument.ItemFilterSectionViewModels : null;
        }
    }
}
