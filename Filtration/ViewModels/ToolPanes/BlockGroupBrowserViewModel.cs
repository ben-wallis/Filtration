using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Filtration.Utilities;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels.ToolPanes
{
    internal interface IBlockGroupBrowserViewModel : IToolViewModel
    {
        void ClearDown();
        bool IsVisible { get; set; }
    }

    internal class BlockGroupBrowserViewModel : ToolViewModel, IBlockGroupBrowserViewModel
    {
        private readonly IBlockGroupMapper _blockGroupMapper;
        private ObservableCollection<ItemFilterBlockGroupViewModel> _blockGroupViewModelViewModels;
        private ItemFilterBlockGroupViewModel _selectedBlockGroupViewModel;

        public BlockGroupBrowserViewModel(IBlockGroupMapper blockGroupMapper) : base("Block Group Browser")
        {
            _blockGroupMapper = blockGroupMapper;
            FilterToSelectedBlockGroupCommand = new RelayCommand(OnFilterToSelectedBlockGroupCommand, () => SelectedBlockGroupViewModel != null);

            ContentId = ToolContentId;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/Filtration;component/Resources/Icons/block_group_browser_icon.png");
            icon.EndInit();
            IconSource = icon;
            Messenger.Default.Register<NotificationMessage<bool>>(this, message =>
            {
                switch (message.Notification)
                {
                    case "ShowAdvancedToggled":
                    {
                        OnShowAdvancedToggled(message.Content);
                        break;
                    }
                }
            });

        }

        public const string ToolContentId = "BlockGroupBrowserTool";
        
        protected override void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (AvalonDockWorkspaceViewModel.ActiveScriptViewModel != null && AvalonDockWorkspaceViewModel.ActiveDocument.IsScript)
            {
                BlockGroupViewModels = RebuildBlockGroupViewModels(AvalonDockWorkspaceViewModel.ActiveScriptViewModel.ShowAdvanced);
            }
            else
            {
                ClearDown();
            }
        }

        public ItemFilterBlockGroupViewModel SelectedBlockGroupViewModel
        {
            get { return _selectedBlockGroupViewModel; }
            set
            {
                _selectedBlockGroupViewModel = value;
                RaisePropertyChanged();
                FilterToSelectedBlockGroupCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand FilterToSelectedBlockGroupCommand { get; private set; }
        
        public ObservableCollection<ItemFilterBlockGroupViewModel> BlockGroupViewModels
        {
            get { return _blockGroupViewModelViewModels; }
            private set
            {
                _blockGroupViewModelViewModels = value;
                RaisePropertyChanged();
            }
        }

        public void ClearDown()
        {
            BlockGroupViewModels = null;
            SelectedBlockGroupViewModel = null;
        }

        private void OnShowAdvancedToggled(bool showAdvanced)
        {
            BlockGroupViewModels = RebuildBlockGroupViewModels(showAdvanced);
        }

        private ObservableCollection<ItemFilterBlockGroupViewModel> RebuildBlockGroupViewModels(bool showAdvanced)
        {
            return
                _blockGroupMapper.MapBlockGroupsToViewModels(
                    AvalonDockWorkspaceViewModel.ActiveScriptViewModel.Script.ItemFilterBlockGroups, showAdvanced);
        }

        private void OnFilterToSelectedBlockGroupCommand()
        {
            AvalonDockWorkspaceViewModel.ActiveScriptViewModel.BlockFilterPredicate =
                b => b.Block.HasBlockGroupInParentHierarchy(SelectedBlockGroupViewModel.SourceBlockGroup, b.Block.BlockGroup);
        }
    }
}
