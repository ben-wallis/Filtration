using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.Parser.Interface.Services;
using Filtration.Parser.Services;

namespace Filtration.Parser
{
    public class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<IBlockGroupHierarchyBuilder>()
                .ImplementedBy<BlockGroupHierarchyBuilder>()
                .LifestyleSingleton());

            container.Register(Component
                .For<IItemFilterBlockTranslator>()
                .ImplementedBy<ItemFilterBlockTranslator>()
                .LifestyleSingleton());

            container.Register(Component
                .For<IItemFilterScriptTranslator>()
                .ImplementedBy<ItemFilterScriptTranslator>()
                .LifestyleSingleton());
        }
    }
}
