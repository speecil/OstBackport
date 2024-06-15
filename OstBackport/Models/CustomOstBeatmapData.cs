using System.IO;
using System.Threading.Tasks;
using BeatmapSaveDataVersion3;

namespace OstBackport.Models
{
    internal class CustomOstBeatmapData
    {
        public string JsonDataFilePath { get; }
        private IBeatmapDataBasicInfo _beatmapDataBasicInfo;

        public async Task<IBeatmapDataBasicInfo> GetBeatmapDataBasicInfoAsync()
        {
            if (_beatmapDataBasicInfo != null) return _beatmapDataBasicInfo;
            BeatmapSaveData beatmapSaveData = await LoadBeatmapSaveDataAsync();
            await Task.Run(() => _beatmapDataBasicInfo = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(beatmapSaveData));
            return _beatmapDataBasicInfo;
        }

        public async Task<IReadonlyBeatmapData> GetBeatmapDataAsync(BeatmapDifficulty beatmapDifficulty, float beatsPerMinute, bool loadingForDesignatedEnvironment, EnvironmentInfoSO environmentInfo, PlayerSpecificSettings playerSpecificSettings)
        {
            BeatmapSaveData beatmapSaveData = await LoadBeatmapSaveDataAsync();
            IReadonlyBeatmapData readonlyBeatmapData = null;
            await Task.Run(() => readonlyBeatmapData = BeatmapDataLoader.GetBeatmapDataFromSaveData(beatmapSaveData, beatmapDifficulty, beatsPerMinute, loadingForDesignatedEnvironment, environmentInfo, playerSpecificSettings));
            return readonlyBeatmapData;
        }

        public async Task<BeatmapSaveData> LoadBeatmapSaveDataAsync()
        {
            string jsonData = File.ReadAllText(JsonDataFilePath);
            BeatmapSaveData beatmapSaveData = null;
            await Task.Run(() => beatmapSaveData = BeatmapSaveData.DeserializeFromJSONString(jsonData));
            return beatmapSaveData;
        }

        public CustomOstBeatmapData(string filePath) => JsonDataFilePath = filePath;
        
    }
}
