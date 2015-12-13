using System.Threading.Tasks;
using Filtration.Common.ViewModels;
using Filtration.Interface;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace Filtration.ViewModels
{
    internal interface IStartPageViewModel : IDocument
    {
    }

    internal class StartPageViewModel : PaneViewModel, IStartPageViewModel
    {

        public StartPageViewModel()
        {
            Title = "Start Page";
            OpenScriptCommand = new RelayCommand(OnOpenScriptCommand);
            NewScriptCommand = new RelayCommand(OnNewScriptCommand);
        }
        
        public RelayCommand OpenScriptCommand { get; private set; }
        public RelayCommand NewScriptCommand { get; private set; }

        public bool IsScript => false;
        public bool IsTheme => false;

        public Task Close()
        {
            throw new System.NotImplementedException();
        }

        private static void OnOpenScriptCommand()
        {
            Messenger.Default.Send(new NotificationMessage("OpenScript"));
        }

        private static void OnNewScriptCommand()
        {
            Messenger.Default.Send(new NotificationMessage("NewScript"));
        }
    }
}
