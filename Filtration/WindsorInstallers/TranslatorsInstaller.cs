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
                Component.For<ILootFilterScriptTranslator>()
                    .ImplementedBy<LootFilterScriptTranslator>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ILootFilterBlockTranslator>()
                    .ImplementedBy<LootFilterBlockTranslator>()
                    .LifeStyle.Singleton);
        }
    }
}
