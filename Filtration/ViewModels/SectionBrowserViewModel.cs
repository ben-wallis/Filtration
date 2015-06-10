using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Filtration.ViewModels
{
    internal interface ISectionBrowserViewModel : IToolViewModel
    {
        void ClearDown();
    }

    internal class SectionBrowserViewModel : ToolViewModel, ISectionBrowserViewModel
    {
        private IEnumerable<IItemFilterBlockViewModel> _sectionBlockViewModels;
        private IItemFilterBlockViewModel _selectedSectionBlockViewModel;

        public SectionBrowserViewModel() : base("Section Browser")
        {
            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/add_section_icon.png");
            icon.EndInit();
            IconSource = icon;
        }

        public const string ToolContentId = "SectionBrowserTool";

        public override void Initialise(IMainWindowViewModel mainWindowViewModel)
        {
            base.Initialise(mainWindowViewModel);
            MainWindowViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
        }
        
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
                if (MainWindowViewModel.ActiveDocument.IsScript)
                {
                    MainWindowViewModel.ActiveScriptViewModel.SectionBrowserSelectedBlockViewModel = value;
                }
                RaisePropertyChanged();
            }
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (MainWindowViewModel.ActiveScriptViewModel != null && MainWindowViewModel.ActiveDocument.IsScript)
            {
                SectionBlockViewModels = MainWindowViewModel.ActiveScriptViewModel.ItemFilterSectionViewModels;
            }
            else
            {
               ClearDown();
            }
        }

        public void ClearDown()
        {
            SectionBlockViewModels = null;
            SelectedSectionBlockViewModel = null;
        }
    }
}
