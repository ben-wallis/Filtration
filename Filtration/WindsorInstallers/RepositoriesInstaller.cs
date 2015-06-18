using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.Repositories;

namespace Filtration.WindsorInstallers
{
    public class RepositoriesInstaller :IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IItemFilterScriptRepository>()
                    .ImplementedBy<ItemFilterScriptRepository>()
                    .LifeStyle.Singleton);
        }
    }
}
