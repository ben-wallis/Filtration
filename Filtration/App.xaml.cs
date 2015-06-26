using System.Linq;
using System.Windows;
using AutoMapper;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.Properties;
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
            
            Mapper.AssertConfigurationIsValid();

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}
