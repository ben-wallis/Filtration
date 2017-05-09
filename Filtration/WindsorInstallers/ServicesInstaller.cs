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
                Component.For<IItemFilterPersistenceService>()
                    .ImplementedBy<ItemFilterPersistenceService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IStaticDataService>()
                    .ImplementedBy<StaticDataService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IHTTPService>()
                    .ImplementedBy<HTTPService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IUpdateCheckService>()
                    .ImplementedBy<UpdateCheckService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IClipboardService>()
                    .ImplementedBy<ClipboardService>()
                    .LifeStyle.Singleton);
        }
    }
}
