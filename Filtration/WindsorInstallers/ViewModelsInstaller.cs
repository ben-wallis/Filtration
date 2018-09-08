using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Filtration.ViewModels;
using Filtration.ViewModels.Factories;
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
                Component.For<IItemFilterCommentBlockViewModel>()
                    .ImplementedBy<ItemFilterCommentBlockViewModel>()
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
                Component.For<ICommentBlockBrowserViewModel>()
                    .ImplementedBy<CommentBlockBrowserViewModel>()
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
                Component.For<IUpdateViewModel>()
                    .ImplementedBy<UpdateViewModel>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IItemFilterBlockViewModelFactory>().AsFactory());

            container.Register(
                Component.For<IItemFilterCommentBlockViewModelFactory>().AsFactory());

            container.Register(
                Component.For<IItemFilterScriptViewModelFactory>().AsFactory());
            
            container.Register(
                Component.For<IItemFilterBlockBaseViewModelFactory>()
                    .ImplementedBy<ItemFilterBlockBaseViewModelFactory>()
                    .LifeStyle.Singleton);
        }
    }
}
