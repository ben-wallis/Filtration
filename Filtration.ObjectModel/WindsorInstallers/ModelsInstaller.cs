using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ObjectModel.Factories;

namespace Filtration.ObjectModel.WindsorInstallers
{
    public class ModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<IItemFilterScript>()
                .ImplementedBy<ItemFilterScript>()
                .LifestyleTransient());

            container.Register(Component
                .For<IItemFilterScriptFactory>()
                .AsFactory());
        }
    }
}
