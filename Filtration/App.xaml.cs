using System.Linq;
using System.Windows;
using AutoMapper;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.ObjectModel.ThemeEditor;
using Filtration.Properties;
using Filtration.Services;
using Filtration.ThemeEditor.ViewModels;

namespace Filtration
{
    public partial class App
    {
        private IWindsorContainer _container;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
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
                cfg.CreateMap<ThemeEditorViewModel, Theme>();
            });

            Mapper.AssertConfigurationIsValid();
            
            var bootstrapper = _container.Resolve<IBootstrapper>();
            await bootstrapper.GoAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}
