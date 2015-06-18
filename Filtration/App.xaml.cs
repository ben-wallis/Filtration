using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using AutoMapper;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.Models;
using Filtration.Utilities;
using Filtration.ViewModels;
using Filtration.Views;

namespace Filtration
{
    public partial class App
    {
        private IWindsorContainer _container;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _container = new WindsorContainer();

            var propInjector = _container.Kernel.ComponentModelBuilder
                .Contributors
                .OfType<PropertiesDependenciesModelInspector>()
                .Single();

            _container.Kernel.ComponentModelBuilder.RemoveContributor(propInjector);

            _container.Install(FromAssembly.This());


            // TODO: Find out how to parameterise this to map differently depending if ShowAdvanced is true or false.
            //Mapper.CreateMap<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>()
            //    .ForMember(dest => dest.IsChecked,
            //        opts => opts.MapFrom(from => from.IsChecked))
            //    .ForMember(dest => dest.SourceBlockGroup,
            //        opts => opts.MapFrom(from => from));

            //Mapper.CreateMap<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>()
            //    .ForMember(dest => dest.IsChecked,
            //        opts => opts.MapFrom(from => from.IsChecked))
            //    .ForMember(dest => dest.ChildGroups,
            //        opts => opts.ResolveUsing<ChildGroupsResolver>())
            //    .ForMember(dest => dest.SourceBlockGroup,
            //        opts => opts.MapFrom(from => from));

            //.ForMember(dest => dest.ChildGroups,
            //    opts => opts.Condition(src => src.Advanced == false))

            //opts => opts.ResolveUsing<ItemFilterBlockGroupChildGroupsResolver>())

            //opts => opts.MapFrom(from => from.ChildGroups))

            //Mapper.AssertConfigurationIsValid();

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();
        }


        //private class ChildGroupsResolver : ValueResolver<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>
        //{
        //    protected override ItemFilterBlockGroupViewModel ResolveCore(ItemFilterBlockGroup source)
        //    {
        //        return !source.Advanced ? Mapper.Map<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>(source) : null;
        //    }
        //}

        //private class ChildGroupsResolver :
        //    ValueResolver<List<ItemFilterBlockGroup>, ObservableCollection<ItemFilterBlockGroupViewModel>>
        //{
        //    protected override ObservableCollection<ItemFilterBlockGroupViewModel> ResolveCore(List<ItemFilterBlockGroup> source)
        //    {
        //        var result = new ObservableCollection<ItemFilterBlockGroupViewModel>();
                
        //        if (source != null && source.Count > 0)
        //        {
                    
        //            var filteredSource = source.Where(g => g.Advanced == false);
        //            foreach (var blockGroup in filteredSource)
        //            {
        //                result.Add(Mapper.Map<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>(blockGroup));
        //            }
        //        }

        //        return result;
        //    }
        //}

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            base.OnExit(e);
        }
    }
}
