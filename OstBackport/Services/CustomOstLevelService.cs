using System.Threading;
using OstBackport.Models;
using System.Threading.Tasks;
using Zenject;
using static BeatmapLevelLoader;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
            CustomOstPreviewBeatmapLevelSO levelSo = ScriptableObject.CreateInstance<CustomOstPreviewBeatmapLevelSO>();
            JsonUtility.FromJsonOverwrite(json, levelSo);

            List<CustomOstDifficultyBeatmapSet> maps = JObject.Parse(json)["_difficultyBeatmapSets"].Value<JArray>().ToObject<List<CustomOstDifficultyBeatmapSet>>();

            levelSo._levelID = levelSo.songName.Replace(" ", "").Replace("-", "");
            levelSo._previewDifficultyBeatmapSets = new PreviewDifficultyBeatmapSet[1];
            levelSo._previewDifficultyBeatmapSets[0] = new PreviewDifficultyBeatmapSet(maps[0].beatmapCharacteristic, maps[0].beatmaps.Select(map => map.difficulty).ToArray());
            levelSo._environmentInfo = SongCore.Loader._customLevelLoader._defaultEnvironmentInfo;
            levelSo._allDirectionsEnvironmentInfo = SongCore.Loader._customLevelLoader._defaultAllDirectionsEnvironmentInfo;
            levelSo.InitCustomOstPreviewLevel(songFile, coverFile, infoFile);

            _levelsModel._loadedPreviewBeatmapLevels[levelSo._levelID] = levelSo;

            _log.Notice($"Loaded custom ost preview \"{levelSo.songName}\"");
        }

        public async Task<LoadBeatmapLevelResult> LoadCustomOstBeatmapLevelAsync(CustomOstPreviewBeatmapLevelSO previewLevel, CancellationToken cancellationToken)
        {
            _log.Notice($"Loading custom ost map \"{previewLevel.songName}\"");

            string infoFile = previewLevel.InfoDatPath;
            string songDirectory = Path.GetDirectoryName(infoFile) ?? "";

            string json = File.ReadAllText(infoFile);

            List<CustomOstDifficultyBeatmapSet> maps = JObject.Parse(json)["_difficultyBeatmapSets"].Value<JArray>().ToObject<List<CustomOstDifficultyBeatmapSet>>();

            foreach (CustomOstDifficultyBeatmap beatmap in maps[0].beatmaps)
            {
                CustomOstBeatmapData customBeatmapData = new CustomOstBeatmapData(Path.Combine(songDirectory, beatmap.BeatmapFilename));
                await customBeatmapData.GetBeatmapDataBasicInfoAsync();
                beatmap.BeatmapData = customBeatmapData;
                cancellationToken.ThrowIfCancellationRequested();
            }

            AudioClip song = await SongCore.Loader._customLevelLoader._audioClipAsyncLoader.LoadPreview(previewLevel);
            CustomOstBeatmapLevel level = new CustomOstBeatmapLevel(previewLevel, song, maps);
            level.InitData();
            return new LoadBeatmapLevelResult(false, level);
        }
    }
}
