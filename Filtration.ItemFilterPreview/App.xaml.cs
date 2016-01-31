using System.Windows;
using Castle.Facilities.TypedFactory;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Filtration.ItemFilterPreview.Views;

namespace Filtration.ItemFilterPreview
{
    public partial class App : Application
    {
        private IWindsorContainer _container;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _container = new WindsorContainer();

            _container.AddFacility<TypedFactoryFacility>();
            _container.Install(FromAssembly.InThisApplication());
            _container.Install(FromAssembly.Named("Filtration.Parser"));

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();
        }
    }
}
