using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ViewModels;
using Filtration.ViewModels.ToolPanes;

namespace Filtration.WindsorInstallers
{
    public class ViewModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IMainWindowViewModel>()
                    .ImplementedBy<MainWindowViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IAvalonDockWorkspaceViewModel>()
                    .ImplementedBy<AvalonDockWorkspaceViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IItemFilterBlockViewModel>()
                    .ImplementedBy<ItemFilterBlockViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IItemFilterScriptViewModel>()
                    .ImplementedBy<ItemFilterScriptViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IReplaceColorsViewModel>()
                    .ImplementedBy<ReplaceColorsViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IStartPageViewModel>()
                    .ImplementedBy<StartPageViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ISectionBrowserViewModel>()
                    .ImplementedBy<SectionBrowserViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IBlockGroupBrowserViewModel>()
                    .ImplementedBy<BlockGroupBrowserViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IBlockOutputPreviewViewModel>()
                    .ImplementedBy<BlockOutputPreviewViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<ISettingsPageViewModel>()
                    .ImplementedBy<SettingsPageViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IUpdateAvailableViewModel>()
                    .ImplementedBy<UpdateAvailableViewModel>()
                    .LifeStyle.Transient);

            container.Register(
                Component.For<IItemFilterBlockViewModelFactory>().AsFactory());

            container.Register(
                Component.For<IItemFilterScriptViewModelFactory>().AsFactory());
        }
    }
}
