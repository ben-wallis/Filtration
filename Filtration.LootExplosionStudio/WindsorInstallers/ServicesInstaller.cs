using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.LootExplosionStudio.Services;

namespace Filtration.LootExplosionStudio.WindsorInstallers
{
    class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IItemFilterBlockFinderService>()
                    .ImplementedBy<ItemFilterBlockFinderService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ILootItemAppearanceService>()
                    .ImplementedBy<LootItemAppearanceService>()
                    .LifeStyle.Singleton);
            container.Register(
                Component.For<ILootItemCollectionItemFilterCombinerService>()
                    .ImplementedBy<LootItemCollectionItemFilterCombinerService>()
                    .LifeStyle.Singleton);
        }
    }
}
