using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BeatmapSaveDataVersion3;
using UnityEngine;

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
            await RunTaskAndLogException(() => _beatmapDataBasicInfo = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(beatmapSaveData));
            return _beatmapDataBasicInfo;
        }

        public async Task<IReadonlyBeatmapData> GetBeatmapDataAsync(BeatmapDifficulty beatmapDifficulty, float beatsPerMinute, bool loadingForDesignatedEnvironment, EnvironmentInfoSO environmentInfo, PlayerSpecificSettings playerSpecificSettings)
        {
            BeatmapSaveData beatmapSaveData = await LoadBeatmapSaveDataAsync();
            IReadonlyBeatmapData readonlyBeatmapData = null;
            await RunTaskAndLogException(() => readonlyBeatmapData = BeatmapDataLoader.GetBeatmapDataFromSaveData(beatmapSaveData, beatmapDifficulty, beatsPerMinute, loadingForDesignatedEnvironment, environmentInfo, playerSpecificSettings));
            return readonlyBeatmapData;
        }

        public async Task<BeatmapSaveData> LoadBeatmapSaveDataAsync()
        {
            BeatmapSaveData beatmapSaveData = null;
            await RunTaskAndLogException(() => beatmapSaveData = BeatmapSaveData.DeserializeFromJSONString(File.ReadAllText(JsonDataFilePath)));
            return beatmapSaveData;
        }

        public async Task RunTaskAndLogException(Action action) => await Task.Run(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        });

        public CustomOstBeatmapData(string filePath)
        {
            JsonDataFilePath = filePath;
        }
    }
}
