using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ThemeEditor.Services;

namespace Filtration.ThemeEditor.WindsorInstallers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IThemePersistenceService>()
                    .ImplementedBy<ThemePersistenceService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IThemeService>()
                    .ImplementedBy<ThemeService>()
                    .LifeStyle.Singleton);
        }
    }
}
