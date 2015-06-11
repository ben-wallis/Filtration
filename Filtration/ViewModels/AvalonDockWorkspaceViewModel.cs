using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels
{
    internal interface IAvalonDockWorkspaceViewModel
    {
        event EventHandler ActiveDocumentChanged;
        IDocument ActiveDocument { get; set; }
        IItemFilterScriptViewModel ActiveScriptViewModel { get; }
        void AddDocument(IDocument document);
        void CloseDocument(IDocument document);
        void SwitchActiveDocument(IDocument document);
    }

    internal class AvalonDockWorkspaceViewModel : FiltrationViewModelBase, IAvalonDockWorkspaceViewModel
    {
        private readonly ISectionBrowserViewModel _sectionBrowserViewModel;
        private readonly IBlockGroupBrowserViewModel _blockGroupBrowserViewModel;

        private IDocument _activeDocument;
        private IItemFilterScriptViewModel _activeScriptViewModel;
        private readonly ObservableCollection<IDocument> _openDocuments;

        public AvalonDockWorkspaceViewModel(ISectionBrowserViewModel sectionBrowserViewModel,
                                            IBlockGroupBrowserViewModel blockGroupBrowserViewModel,
                                            IStartPageViewModel startPageViewModel)
        {
            _sectionBrowserViewModel = sectionBrowserViewModel;
            _blockGroupBrowserViewModel = blockGroupBrowserViewModel;

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

        private List<IToolViewModel> _tools;

        public IEnumerable<IToolViewModel> Tools
        {
            get
            {
                if (_tools == null)
                {
                    _tools = new List<IToolViewModel> { _sectionBrowserViewModel, _blockGroupBrowserViewModel };
                }

                return _tools;
            }
        }

        public void AddDocument(IDocument document)
        {
            if (document.IsScript)
            {
                _activeScriptViewModel = (IItemFilterScriptViewModel)document;
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

            if (document.IsScript)
            {
                _sectionBrowserViewModel.ClearDown();
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
