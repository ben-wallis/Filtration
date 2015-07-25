using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AutoMapper;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.ObjectModel;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Properties;
using Filtration.ThemeEditor.ViewModels;
using Filtration.ViewModels;
using Filtration.Views;
using NLog;

namespace Filtration
{
    public partial class App
    {
        private IWindsorContainer _container;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;

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

            Mapper.CreateMap<Theme, IThemeEditorViewModel>().ConstructUsingServiceLocator();
            Mapper.CreateMap<ThemeComponent, ThemeComponentViewModel>().ReverseMap();
            Mapper.CreateMap<IThemeEditorViewModel, Theme>();

            Mapper.AssertConfigurationIsValid();

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();
        }

        public void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception);
            var exception = e.Exception.Message + Environment.NewLine + e.Exception.StackTrace;
            var innerException = e.Exception.InnerException != null
                ? e.Exception.InnerException.Message + Environment.NewLine +
                  e.Exception.InnerException.StackTrace
                : string.Empty;

            MessageBox.Show(
                exception + Environment.NewLine + innerException,
                "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}
