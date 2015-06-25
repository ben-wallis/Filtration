using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.Translators;

namespace Filtration.WindsorInstallers
{
    public class TranslatorsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IItemFilterScriptTranslator>()
                    .ImplementedBy<ItemFilterScriptTranslator>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IItemFilterBlockTranslator>()
                    .ImplementedBy<ItemFilterBlockTranslator>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IBlockGroupHierarchyBuilder>()
                    .ImplementedBy<BlockGroupHierarchyBuilder>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IThemeComponentListBuilder>()
                    .ImplementedBy<ThemeComponentListBuilder>()
                    .LifeStyle.Singleton);
        }
    }
}
