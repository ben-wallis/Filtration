using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using Filtration.ThemeEditor.ViewModels;
using Filtration.ViewModels.ToolPanes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Filtration.Properties;

namespace Filtration.ViewModels
{
    internal interface IAvalonDockWorkspaceViewModel
    {
        event EventHandler ActiveDocumentChanged;
        IDocument ActiveDocument { get; set; }
        ReadOnlyObservableCollection<IDocument> OpenDocuments { get; }
        IItemFilterScriptViewModel ActiveScriptViewModel { get; }
        IThemeEditorViewModel ActiveThemeViewModel { get; }
        ICommentBlockBrowserViewModel CommentBlockBrowserViewModel { get; }
        IBlockGroupBrowserViewModel BlockGroupBrowserViewModel { get; }
        IBlockOutputPreviewViewModel BlockOutputPreviewViewModel { get; }
        void AddDocument(IDocument document);
        void CloseDocument(IDocument document);
        void SwitchActiveDocument(IDocument document);
        IThemeEditorViewModel OpenMasterThemeForScript(IItemFilterScriptViewModel scriptViewModel);
    }

    internal class AvalonDockWorkspaceViewModel : ViewModelBase, IAvalonDockWorkspaceViewModel
    {
        private readonly ICommentBlockBrowserViewModel _commentBlockBrowserViewModel;
        private readonly IBlockGroupBrowserViewModel _blockGroupBrowserViewModel;
        private readonly IBlockOutputPreviewViewModel _blockOutputPreviewViewModel;

        private IDocument _activeDocument;
        private IItemFilterScriptViewModel _activeScriptViewModel;
        private IThemeEditorViewModel _activeThemeViewModel;
        private readonly ObservableCollection<IDocument> _openDocuments;

        public AvalonDockWorkspaceViewModel(ICommentBlockBrowserViewModel commentBlockBrowserViewModel,
            IBlockGroupBrowserViewModel blockGroupBrowserViewModel,
            IStartPageViewModel startPageViewModel,
            IBlockOutputPreviewViewModel blockOutputPreviewViewModel)
        {
            _commentBlockBrowserViewModel = commentBlockBrowserViewModel;
            _blockGroupBrowserViewModel = blockGroupBrowserViewModel;
            _blockOutputPreviewViewModel = blockOutputPreviewViewModel;

            _commentBlockBrowserViewModel.Initialise(this);
            _blockGroupBrowserViewModel.Initialise(this);
            _blockOutputPreviewViewModel.Initialise(this);

            _openDocuments = new ObservableCollection<IDocument> {startPageViewModel};
            OpenDocuments = new ReadOnlyObservableCollection<IDocument>(_openDocuments);
            ActiveDocument = startPageViewModel;
        }

        public event EventHandler ActiveDocumentChanged;

        public ReadOnlyObservableCollection<IDocument> OpenDocuments { get; }

        public IDocument ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (value == _activeDocument) return;

                _activeDocument = value;
                RaisePropertyChanged();

                if (value.IsScript)
                {
                    _activeScriptViewModel = (IItemFilterScriptViewModel) value;
                }
                else if (value.IsTheme)
                {
                    _activeThemeViewModel = (IThemeEditorViewModel) value;
                }
                else
                {
                    _activeScriptViewModel = null;
                    _activeThemeViewModel = null;
                }

                ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);

                Messenger.Default.Send(new NotificationMessage("ActiveDocumentChanged"));
            }
        }

        public IItemFilterScriptViewModel ActiveScriptViewModel => _activeScriptViewModel;
        public IThemeEditorViewModel ActiveThemeViewModel => _activeThemeViewModel;
        public IBlockGroupBrowserViewModel BlockGroupBrowserViewModel => _blockGroupBrowserViewModel;
        public IBlockOutputPreviewViewModel BlockOutputPreviewViewModel => _blockOutputPreviewViewModel;
        public ICommentBlockBrowserViewModel CommentBlockBrowserViewModel => _commentBlockBrowserViewModel;

        private List<IToolViewModel> _tools;

        public IEnumerable<IToolViewModel> Tools => _tools ?? (_tools = new List<IToolViewModel>
        {
            _commentBlockBrowserViewModel,
            _blockGroupBrowserViewModel,
            _blockOutputPreviewViewModel
        });

        public void AddDocument(IDocument document)
        {
            if (document.IsScript)
            {
                _activeScriptViewModel = (IItemFilterScriptViewModel) document;
            }
            else if (document.IsTheme)
            {
                _activeThemeViewModel = (IThemeEditorViewModel) document;
            }

            Application.Current.Dispatcher.Invoke(() => _openDocuments.Add(document));
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
                _commentBlockBrowserViewModel.ClearDown();
                _blockGroupBrowserViewModel.ClearDown();
                _blockOutputPreviewViewModel.ClearDown();
            }

            _openDocuments.Remove(document);
            if (_activeDocument == document)
            {
                _activeDocument = null;
            }


            // TODO: Replace _activeScriptViewModel and _activeThemeViewModel with a better solution.

            if (document.IsScript && _activeScriptViewModel == (IItemFilterScriptViewModel) document)
            {
                _activeScriptViewModel = null;
            }

            if (document.IsTheme && _activeThemeViewModel == (IThemeEditorViewModel)document)
            {
                _activeThemeViewModel = null;
            }
        }

        public void SwitchActiveDocument(IDocument document)
        {
            if (!OpenDocuments.Contains(document))
            {
                throw new ArgumentException("SwitchActiveDocument called with non-existant document");
            }
            
            ActiveDocument = document;
        }

        public IThemeEditorViewModel OpenMasterThemeForScript(IItemFilterScriptViewModel scriptViewModel)
        {
            var existingMasterThemeViewModelCount =
                OpenDocuments.OfType<IThemeEditorViewModel>()
                    .Count(c => c.IsMasterThemeForScript == scriptViewModel.Script);
            if (existingMasterThemeViewModelCount > 0)
            {
                return OpenDocuments.OfType<IThemeEditorViewModel>()
                    .First(c => c.IsMasterThemeForScript == scriptViewModel.Script);
            }

            return null;
        }
    }
}
