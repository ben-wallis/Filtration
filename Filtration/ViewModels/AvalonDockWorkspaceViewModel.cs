using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Filtration.ViewModels.ToolPanes;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels
{
    internal interface IAvalonDockWorkspaceViewModel
    {
        event EventHandler ActiveDocumentChanged;
        IDocument ActiveDocument { get; set; }
        IItemFilterScriptViewModel ActiveScriptViewModel { get; }
        ISectionBrowserViewModel SectionBrowserViewModel { get; }
        IBlockGroupBrowserViewModel BlockGroupBrowserViewModel { get; }
        IBlockOutputPreviewViewModel BlockOutputPreviewViewModel { get; }
        void AddDocument(IDocument document);
        void CloseDocument(IDocument document);
        void SwitchActiveDocument(IDocument document);
    }

    internal class AvalonDockWorkspaceViewModel : FiltrationViewModelBase, IAvalonDockWorkspaceViewModel
    {
        private readonly ISectionBrowserViewModel _sectionBrowserViewModel;
        private readonly IBlockGroupBrowserViewModel _blockGroupBrowserViewModel;
        private readonly IBlockOutputPreviewViewModel _blockOutputPreviewViewModel;

        private IDocument _activeDocument;
        private IItemFilterScriptViewModel _activeScriptViewModel;
        private readonly ObservableCollection<IDocument> _openDocuments;

        public AvalonDockWorkspaceViewModel(ISectionBrowserViewModel sectionBrowserViewModel,
                                            IBlockGroupBrowserViewModel blockGroupBrowserViewModel,
                                            IStartPageViewModel startPageViewModel,
                                            IBlockOutputPreviewViewModel blockOutputPreviewViewModel)
        {
            _sectionBrowserViewModel = sectionBrowserViewModel;
            _blockGroupBrowserViewModel = blockGroupBrowserViewModel;
            _blockOutputPreviewViewModel = blockOutputPreviewViewModel;

            _sectionBrowserViewModel.Initialise(this);
            _blockGroupBrowserViewModel.Initialise(this);

            _openDocuments = new ObservableCollection<IDocument> {startPageViewModel};
            ActiveDocument = startPageViewModel;
        }

        public event EventHandler ActiveDocumentChanged;

        public ObservableCollection<IDocument> OpenDocuments
        {
            get { return _openDocuments; }
        }

        public IDocument ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                _activeDocument = value;
                RaisePropertyChanged();

                if (value.IsScript)
                {
                    _activeScriptViewModel = (IItemFilterScriptViewModel) value;
                }
                else
                {
                    _activeScriptViewModel = null;
                }

                if (ActiveDocumentChanged != null)
                {
                    ActiveDocumentChanged(this, EventArgs.Empty);
                }

                Messenger.Default.Send(new NotificationMessage("ActiveDocumentChanged"));
            }
        }

        public IItemFilterScriptViewModel ActiveScriptViewModel
        {
            get { return _activeScriptViewModel; }
        }

        public IBlockGroupBrowserViewModel BlockGroupBrowserViewModel
        {
            get { return _blockGroupBrowserViewModel; }
        }

        public IBlockOutputPreviewViewModel BlockOutputPreviewViewModel
        {
            get { return _blockOutputPreviewViewModel; }
        }

        public ISectionBrowserViewModel SectionBrowserViewModel
        {
            get { return _sectionBrowserViewModel; }
        }

        private List<IToolViewModel> _tools;

        public IEnumerable<IToolViewModel> Tools
        {
            get
            {
                return _tools ?? (_tools = new List<IToolViewModel>
                {
                    _sectionBrowserViewModel,
                    _blockGroupBrowserViewModel,
                    _blockOutputPreviewViewModel
                });
            }
        }

        public void AddDocument(IDocument document)
        {
            if (document.IsScript)
            {
                _activeScriptViewModel = (IItemFilterScriptViewModel) document;
            }

            OpenDocuments.Add(document);
            ActiveDocument = document;
        }

        public void CloseDocument(IDocument document)
        {
            if (!OpenDocuments.Contains(document))
            {
                throw new ArgumentException("CloseDocument called with non-existant document");
            }

            if (document.IsScript && ActiveDocument == document)
            {
                _sectionBrowserViewModel.ClearDown();
                _blockGroupBrowserViewModel.ClearDown();
            }

            OpenDocuments.Remove(document);
        }

        public void SwitchActiveDocument(IDocument document)
        {
            if (!OpenDocuments.Contains(document))
            {
                throw new ArgumentException("SwitchActiveDocument called with non-existant document");
            }
            
            ActiveDocument = document;
        }
    }
}
