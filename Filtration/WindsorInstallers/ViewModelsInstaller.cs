using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ViewModels;

namespace Filtration.WindsorInstallers
{
    public class ViewModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IMainWindowViewModel>()
                    .ImplementedBy<MainWindowViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<ILootFilterBlockViewModel>()
                    .ImplementedBy<LootFilterBlockViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<ILootFilterScriptViewModel>()
                    .ImplementedBy<LootFilterScriptViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IReplaceColorsViewModel>()
                    .ImplementedBy<ReplaceColorsViewModel>()
                    .LifeStyle.Singleton);

            container.AddFacility<TypedFactoryFacility>();
            container.Register(
                Component.For<ILootFilterBlockViewModelFactory>().AsFactory());

            container.Register(
                Component.For<ILootFilterScriptViewModelFactory>().AsFactory());
        }
    }
}
