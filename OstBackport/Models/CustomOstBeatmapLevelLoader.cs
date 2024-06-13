using System.Threading;
using System.Threading.Tasks;
using OstBackport.Services;
using Zenject;

namespace OstBackport.Models
{
    internal class CustomOstBeatmapLevelLoader : BeatmapLevelLoader
    {
        [Inject] private readonly CustomOstLevelService _ostLevelService;

        public CustomOstBeatmapLevelLoader(BeatmapLevelDataLoaderSO beatmapLevelDataLoader, IBeatmapDataAssetFileModel beatmapDataAssetFileModel) : base(beatmapLevelDataLoader, beatmapDataAssetFileModel)
        {
        }

        public override async Task<LoadBeatmapLevelResult> LoadBeatmapLevelAsync(IPreviewBeatmapLevel previewLevel, CancellationToken cancellationToken)
        {
            CustomOstPreviewBeatmapLevelSO customOstPreviewBeatmapLevelSo = previewLevel as CustomOstPreviewBeatmapLevelSO;
            if (customOstPreviewBeatmapLevelSo == null) return await base.LoadBeatmapLevelAsync(previewLevel, cancellationToken);
            return await _ostLevelService.LoadCustomOstBeatmapLevelAsync(customOstPreviewBeatmapLevelSo, cancellationToken);
        }
    }
}
