using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ObjectModel.Commands;

namespace Filtration.ObjectModel.WindsorInstallers
{
    public class CommandsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {

            container.Register(Component
                .For<ICommandManager>()
                .Forward<ICommandManagerInternal>()
                .ImplementedBy<CommandManager>()
                .LifestyleTransient());

        }
    }
}
