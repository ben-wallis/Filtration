using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;

namespace Filtration.ViewModels.DesignTime
{
    public class DesignTimeMainWindowViewModel : IMainWindowViewModel
    {
        public RelayCommand OpenScriptCommand { get; }
        public RelayCommand NewScriptCommand { get; }

        public int WindowWidth
        {
            get => 1200; set {} }

        public int WindowHeight
        {
            get => 800; set { }
        }

        public IUpdateViewModel UpdateViewModel => new DesignTimeUpdateViewModel();

        public Task<bool> CloseAllDocumentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task OpenDroppedFilesAsync(List<string> filenames)
        {
            throw new NotImplementedException();
        }
    }
}
