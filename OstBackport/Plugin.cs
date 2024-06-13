using IPA;
using OstBackport.AffinityPatches;
using OstBackport.Models;
using OstBackport.Services;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace OstBackport
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.Install(Location.App, container =>
            {
                container.BindInterfacesAndSelfTo<MapSaving.MapSaving>().AsSingle();
                container.BindInterfacesAndSelfTo<LeaderboardIdAdder>().AsSingle();
            });

            zenjector.Install(Location.Menu, container =>
            {
                container.BindInterfacesAndSelfTo<IdPatches>().AsSingle();
                var model = container.Resolve<BeatmapLevelsModel>();
                var customLoader = new CustomOstBeatmapLevelLoader(model._beatmapLevelDataLoader, model._beatmapDataAssetFileModel);
                container.BindInstance(customLoader);
                model._beatmapLevelLoader = customLoader;
                container.Bind<CustomOstLevelService>().AsSingle();
                container.QueueForInject(customLoader);
            });
            logger.Info("OstBackport initialized.");
        }
    }
}
