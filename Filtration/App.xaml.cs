using System;
using System.Linq;
using System.Windows;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.Windsor;
using Castle.Windsor.Installer;
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

            var mainWindow = _container.Resolve<IMainWindow>();
            mainWindow.Show();

            //String[] arguments = Environment.GetCommandLineArgs();

            //if (arguments.GetLength(0) <= 1) return;
            //if (!arguments[1].EndsWith(".filter")) return;

            //var filePathFormMainArgs = arguments[1];
            //mainWindow.OpenScriptFromCommandLineArgument(filePathFormMainArgs);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
            base.OnExit(e);
        }
    }
}
