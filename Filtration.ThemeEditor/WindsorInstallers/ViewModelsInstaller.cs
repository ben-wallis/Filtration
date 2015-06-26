using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ThemeEditor.ViewModels;

namespace Filtration.ThemeEditor.WindsorInstallers
{
    public class ViewModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IThemeViewModel>()
                    .ImplementedBy<ThemeViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IThemeComponentViewModel>()
                    .ImplementedBy<ThemeComponentViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IThemeViewModelFactory>().AsFactory());
        }
    }
}
