using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ItemFilterPreview.Services;

namespace Filtration.ItemFilterPreview.WindsorInstallers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IBlockItemMatcher>()
                    .ImplementedBy<BlockItemMatcher>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IItemFilterProcessor>()
                    .ImplementedBy<ItemFilterProcessor>()
                    .LifeStyle.Singleton);
        }
    }
}
