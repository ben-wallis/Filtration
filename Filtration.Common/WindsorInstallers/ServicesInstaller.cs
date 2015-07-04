using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.Common.Services;

namespace Filtration.Common.WindsorInstallers
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
                Component.For<IMessageBoxService>()
                    .ImplementedBy<MessageBoxService>()
                    .LifeStyle.Singleton);
        }
    }
}
