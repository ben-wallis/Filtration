using System.IO.Compression;
using System.Linq;
using System.Windows;
using AutoMapper;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.ObjectModel;
using Filtration.Properties;
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

            _container.AddFacility<TypedFactoryFacility>();
            _container.Install(FromAssembly.InThisApplication());
            
            Mapper.Configuration.ConstructServicesUsing(_container.Resolve);

            Mapper.CreateMap<ItemFilterBlockGroup, ItemFilterBlockGroupViewModel>()
                .ForMember(destination => destination.ChildGroups, options => options.ResolveUsing(
                    delegate(ResolutionResult resolutionResult)
                    {
                        var context = resolutionResult.Context;
                        var showAdvanced = (bool) context.Options.Items["showAdvanced"];
                        var group = (ItemFilterBlockGroup) context.SourceValue;
                        if (showAdvanced)
                            return group.ChildGroups;
                        else
                            return group.ChildGroups.Where(c => c.Advanced == false);
                    }))
                .ForMember(dest => dest.SourceBlockGroup,
                        opts => opts.MapFrom(from => from))
                .ForMember(dest => dest.IsExpanded, 
                        opts => opts.UseValue(false));
            
            Mapper.AssertConfigurationIsValid();

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();
        }

        public void TestTest()
        {
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}
