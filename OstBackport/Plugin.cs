using IPA;
using OstBackport.AffinityPatches;
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
                container.BindInterfacesAndSelfTo<IdPatches>().AsSingle();

                container.BindInterfacesAndSelfTo<LeaderboardIdAdder>().AsSingle();
            });
            logger.Info("OstBackport initialized.");
        }
    }
}
