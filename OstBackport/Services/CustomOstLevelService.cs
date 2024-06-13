using System.Threading;
using OstBackport.Models;
using System.Threading.Tasks;
using Zenject;
using static BeatmapLevelLoader;
using System.IO;
using System.Linq;
using System;
using SiraUtil.Logging;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace OstBackport.Services
{
    internal class CustomOstLevelService
    {
        [Inject] private readonly BeatmapLevelsModel _levelsModel;
        [Inject] private readonly SiraLog _log;

        public void LoadCustomOstPreviewBeatmapLevel(string songDirectory)
        {
            string[] files = Directory.GetFiles(songDirectory);

            string infoFile = files.FirstOrDefault(fileName => fileName.Contains("Info")) ?? "";
            string songFile = files.FirstOrDefault(fileName => fileName.Contains(".ogg") || fileName.Contains(".wav")) ?? "";
            string coverFile = files.FirstOrDefault(fileName => fileName.Contains(".png") || fileName.Contains(".jpg")) ?? "";

            string json = File.ReadAllText(infoFile);
            CustomOstPreviewBeatmapLevel level = ScriptableObject.CreateInstance<CustomOstPreviewBeatmapLevel>(); // helps with dynamic cover/audio loading
            JsonUtility.FromJsonOverwrite(json, level);
            level._levelID = level.songName.Replace(" ", "").Replace("-", "");
            level.InitCustomOstPreviewLevel(songFile, coverFile, infoFile);
            _levelsModel._loadedPreviewBeatmapLevels[level._levelID] = level;
        }

        public async Task<LoadBeatmapLevelResult> LoadCustomOstBeatmapLevelAsync(CustomOstPreviewBeatmapLevel previewLevel, CancellationToken cancellationToken)
        {
            _log.Info("now here we can load our custom ost level proper dynamically innit");

            string infoFile = previewLevel.InfoDatPath;
            string songFile = previewLevel.songAudioClipPath;
            string coverFile = previewLevel.CoverImagePath;
            string songDirectory = Path.GetDirectoryName(infoFile) ?? "";

            string json = File.ReadAllText(infoFile);
            CustomOstBeatmapLevel level = ScriptableObject.CreateInstance<CustomOstBeatmapLevel>(); // helps with dynamic cover/audio loading
            JsonUtility.FromJsonOverwrite(json, level);
            level._levelID = level.songName.Replace(" ", "").Replace("-", "");
            level._difficultyBeatmapSets[0]._beatmapCharacteristic = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName("Standard");

            Array.Resize(ref level._difficultyBeatmapSets, 1);

            level._environmentInfo = SongCore.Loader._customLevelLoader._defaultEnvironmentInfo;
            level._allDirectionsEnvironmentInfo = SongCore.Loader._customLevelLoader._defaultAllDirectionsEnvironmentInfo;
            level.InitData();
            level._beatmapLevelData = new CustomOstBeatmapLevelData(songFile, level._difficultyBeatmapSets); // helps get AudioClip from cache
            level.InitCustomOstLevel(songFile, coverFile);

            JObject infoObj = JObject.Parse(json);
            JArray difficultyBeatmaps = infoObj["_difficultyBeatmapSets"][0]["_difficultyBeatmaps"].Value<JArray>();
            foreach (JToken beatmap in difficultyBeatmaps)
            {
                int diff = ((beatmap["_difficultyRank"].Value<int>() + 1) / 2) - 1;
                string fileName = beatmap["_beatmapFilename"].Value<string>();
                BeatmapLevelSO.DifficultyBeatmap map = level._difficultyBeatmapSets[0]._difficultyBeatmaps[diff];
                CustomOstBeatmapData customBeatmapData = ScriptableObject.CreateInstance<CustomOstBeatmapData>();
                customBeatmapData.JsonDataFilePath = Path.Combine(songDirectory, fileName);
                await customBeatmapData.GetBeatmapDataBasicInfoAsync();
                map._difficulty = (BeatmapDifficulty)diff;
                map._beatmapData = customBeatmapData;
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new LoadBeatmapLevelResult(false, level); // placeholder
        }
    }
}
