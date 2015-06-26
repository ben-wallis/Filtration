using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ThemeEditor.Providers;

namespace Filtration.ThemeEditor.WindsorInstallers
{
    public class ProvidersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
              Component.For<IThemeProvider>()
                  .ImplementedBy<ThemeProvider>()
                  .LifeStyle.Singleton);
        }
    }
}
