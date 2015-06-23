using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface ISectionBrowserViewModel : IToolViewModel
    {
        void ClearDown();
        bool IsVisible { get; set; }
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
                if (AvalonDockWorkspaceViewModel.ActiveDocument.IsScript)
                {
                    AvalonDockWorkspaceViewModel.ActiveScriptViewModel.SectionBrowserSelectedBlockViewModel = value;
                }
                RaisePropertyChanged();
            }
        }

        protected override void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (AvalonDockWorkspaceViewModel.ActiveScriptViewModel != null && AvalonDockWorkspaceViewModel.ActiveDocument.IsScript)
            {
                SectionBlockViewModels = AvalonDockWorkspaceViewModel.ActiveScriptViewModel.ItemFilterSectionViewModels;
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
