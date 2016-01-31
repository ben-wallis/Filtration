using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ItemFilterPreview.ViewModels;

namespace Filtration.ItemFilterPreview.WindsorInstallers
{
    public class ViewModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IMainWindowViewModel>()
                    .ImplementedBy<MainWindowViewModel>()
                    .LifeStyle.Singleton);
        }
    }
}
