﻿using System.Runtime.CompilerServices;
using Filtration.Annotations;
using GalaSoft.MvvmLight;

namespace Filtration.ViewModels
{
    internal class FiltrationViewModelBase : ViewModelBase
    {
        /// This gives us the ReSharper option to transform an autoproperty into a property with change notification
        /// Also leverages .net 4.5 callermembername attribute
        [NotifyPropertyChangedInvocator]
        protected override void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            base.RaisePropertyChanged(property);
        }
    }
}
