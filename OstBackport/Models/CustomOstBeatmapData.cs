using System.IO;
using System.Threading.Tasks;
using BeatmapSaveDataVersion3;

namespace OstBackport.Models
{
    internal class CustomOstBeatmapData : BeatmapDataSO
    {
        public string JsonDataFilePath { get; set; }
        private IBeatmapDataBasicInfo _beatmapDataBasicInfo;

        public override async Task<IBeatmapDataBasicInfo> GetBeatmapDataBasicInfoAsync()
        {
            if (_beatmapDataBasicInfo != null) return _beatmapDataBasicInfo;
            if (string.IsNullOrEmpty(_jsonData)) _jsonData = File.ReadAllText(JsonDataFilePath);

            BeatmapSaveData beatmapSaveData = await LoadBeatmapSaveDataAsync();
            IBeatmapDataBasicInfo beatmapDataBasicInfo = null;
            await RunTaskAndLogException(() => beatmapDataBasicInfo = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(beatmapSaveData));
            _beatmapDataBasicInfo = beatmapDataBasicInfo;
            return _beatmapDataBasicInfo;
        }
    }
}
