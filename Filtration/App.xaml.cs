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
using Filtration.Services;
using Filtration.ThemeEditor.ViewModels;
using Filtration.ViewModels;
using Filtration.Views;
using NLog;

namespace Filtration
{
    public partial class App
    {
        private IWindsorContainer _container;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            _container = new WindsorContainer();

            // Disable property injection
            var propInjector = _container.Kernel.ComponentModelBuilder
                .Contributors
                .OfType<PropertiesDependenciesModelInspector>()
                .Single();

            _container.Kernel.ComponentModelBuilder.RemoveContributor(propInjector);

            _container.AddFacility<TypedFactoryFacility>();
            _container.Install(FromAssembly.InThisApplication());
            _container.Install(FromAssembly.Named("Filtration.Parser")); // Not directly referenced so manually call its installers

            Mapper.Initialize(cfg =>
            {
                cfg.ConstructServicesUsing(_container.Resolve);
                cfg.CreateMap<Theme, IThemeEditorViewModel>().ConstructUsingServiceLocator();
                cfg.CreateMap<ThemeComponent, ThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<ColorThemeComponent, ColorThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<IntegerThemeComponent, IntegerThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<StrIntThemeComponent, StrIntThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<StringThemeComponent, StringThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<IconThemeComponent, IconThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<EffectColorThemeComponent, EffectColorThemeComponentViewModel>().ReverseMap();
                cfg.CreateMap<IThemeEditorViewModel, Theme>();
            });

            Mapper.AssertConfigurationIsValid();

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();

            var updateCheckService = _container.Resolve<IUpdateCheckService>();
            updateCheckService.CheckForUpdates();
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception);
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
