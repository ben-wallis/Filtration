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
                Component.For<IItemFilterScriptDirectoryService>()
                         .ImplementedBy<ItemFilterScriptDirectoryService>()
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
                Component.For<IUpdateService>()
                    .ImplementedBy<UpdateService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IClipboardService>()
                    .ImplementedBy<ClipboardService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IBootstrapper>()
                    .ImplementedBy<Bootstrapper>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ISettingsService>()
                    .ImplementedBy<SettingsService>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IDialogService>()
                    .ImplementedBy<DialogService>()
                    .LifeStyle.Singleton);
        }
    }
}
