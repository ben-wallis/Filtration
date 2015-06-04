using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.Services;

namespace Filtration.WindsorInstallers
{
    public class ServicesInstaller :IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IFileSystemService>()
                    .ImplementedBy<FileSystemService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ILootFilterPersistenceService>()
                    .ImplementedBy<LootFilterPersistenceService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IStaticDataService>()
                    .ImplementedBy<StaticDataService>()
                    .LifeStyle.Singleton);
        }
    }
}
