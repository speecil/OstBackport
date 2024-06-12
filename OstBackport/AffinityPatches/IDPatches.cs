using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatmapSaveDataVersion3;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenject;
using static LeaderboardIdsModelSO;

namespace OstBackport.AffinityPatches
{
    internal class IdPatches : IAffinity
    {
        [Inject] private readonly MapSaving.MapSaving _mapSaving;
        [Inject] private readonly SiraLog _log;

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

        internal class CustomOstBeatmapLevelData : BeatmapLevelData, IBeatmapLevelData
        {
            private readonly string _audioClipPath;
            private readonly WeakReference<AudioClip> _song = new WeakReference<AudioClip>(null);
            public new AudioClip audioClip
            {
                get
                {
                    bool exists = _song.TryGetTarget(out AudioClip target);
                    if (exists) return target;
                    var loader = SongCore.Loader._customLevelLoader._audioClipAsyncLoader;
                    loader._cache.TryGet(loader.GetCacheKey(_audioClipPath), out Task<AudioClip> result);
                    _song.SetTarget(result.Result);
                    return result.Result;
                }
            }

            public CustomOstBeatmapLevelData(string audioClipPath, IReadOnlyList<IDifficultyBeatmapSet> difficultyBeatmapSets) : base(null, difficultyBeatmapSets)
            {
                _audioClipPath = audioClipPath;
            }
        }

        internal class CustomOstBeatmapLevel : BeatmapLevelSO, IBeatmapLevel, IFilePathSongAudioClipProvider, IFilePathSongPreviewAudioClipProvider
        {
            public ISpriteAsyncLoader SpriteAsyncLoader { get; private set; }

            public new float songDuration { get; private set; }

            public string songAudioClipPath { get; private set; }

            public string songPreviewAudioClipPath => songAudioClipPath;

            public string CoverImagePath { get; private set; }

            public override async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
            {
                return await SpriteAsyncLoader.LoadSpriteAsync(CoverImagePath, cancellationToken);
            }

            public void InitCustomOstLevel(string songAudioPath, string coverPath)
            {
                CoverImagePath = coverPath;
                SpriteAsyncLoader = SongCore.Loader._customLevelLoader._cachedMediaAsyncLoader;
                songAudioClipPath = songAudioPath;
                songDuration = SongCore.Loader.GetLengthFromOgg(songAudioPath);
            }
        }

        private readonly List<BeatmapLevelSO> _ost7LevelSos = new List<BeatmapLevelSO>();
        private readonly List<BeatmapLevelSO> _ost6LevelSos = new List<BeatmapLevelSO>();

        public static void AddLevelPack(string coverImage, List<BeatmapLevelSO> levelSos, int packNum, LevelFilteringNavigationController controller)
        {

            var levelPack = ScriptableObject.CreateInstance<BeatmapLevelPackSO>();
            levelPack._packName = "Original Soundtrack Vol. " + packNum;
            levelPack._packID = "OSTVol" + packNum;
            levelPack._shortPackName = "OST" + packNum;
            var sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly(coverImage);
            levelPack._coverImage = sprite;
            levelPack._smallCoverImage = sprite;
            BeatmapLevelCollectionSO man = ScriptableObject.CreateInstance<BeatmapLevelCollectionSO>();
            BeatmapLevelSO[] levels = levelSos.ToArray();
            man._beatmapLevels = levels;
            levelPack._beatmapLevelCollection = man;

            List<IBeatmapLevelPack> updated = controller._ostBeatmapLevelPacks.ToList();
            updated.Insert(updated.Count - 2, levelPack);
            controller._ostBeatmapLevelPacks = updated.ToArray();

            var ownedContainer = controller._beatmapLevelsModel._additionalContentModel._alwaysOwnedContentContainer;
            ownedContainer.alwaysOwnedPacksIds.Add(levelPack.packID);
            var beatmapLevelPackSOs = ownedContainer._alwaysOwnedContent._alwaysOwnedPacks.ToList();
            beatmapLevelPackSOs.Add(levelPack);
            ownedContainer._alwaysOwnedContent._alwaysOwnedPacks = beatmapLevelPackSOs.ToArray();
        }

        [AffinityPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.SetupBeatmapLevelPacks))]
        [AffinityPostfix]
        public void LevelFilteringNavigationController_SetupBeatmapLevelPacks(LevelFilteringNavigationController __instance)
        {
            AddLevelPack("OstBackport.Images.OST6.png", _ost6LevelSos, 6, __instance);
            AddLevelPack("OstBackport.Images.OST7.png", _ost7LevelSos, 7, __instance);

            foreach (var level in _ost7LevelSos) __instance._beatmapLevelsModel._loadedPreviewBeatmapLevels[level.levelID] = level;
            foreach (var level in _ost6LevelSos) __instance._beatmapLevelsModel._loadedPreviewBeatmapLevels[level.levelID] = level;

            IBeatmapLevelPack[] allPacks = __instance._ostBeatmapLevelPacks.Concat(__instance._musicPacksBeatmapLevelPacks).ToArray();

            __instance._allOfficialBeatmapLevelPacks = allPacks;

            __instance._beatmapLevelsModel._additionalContentModel._alwaysOwnedContentContainer.InitAlwaysOwnedItems();


            // i hate my life
            var view = __instance._annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._gridView;

            __instance._selectLevelCategoryViewController.didSelectLevelCategoryEvent += (one, two) =>
            {
                bool adjust = two == SelectLevelCategoryViewController.LevelCategory.MusicPacks;
                AdjustGridView(view, adjust);
            };
            __instance.didActivateEvent += (one, two, three) =>
            {
                bool adjust = __instance.selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.MusicPacks;
                AdjustGridView(view, adjust);
            };
        }

        private static void AdjustGridView(GridView view, bool adjust)
        {
            //float[] one = { 15.5f, 33, -20.33f };
            //float[] two = { 0, -3, -1.5f };
            //float[] three = adjust ? one : two;

            //RectTransform viewport = (RectTransform)view.transform.GetChild(1);
            //RectTransform content = (RectTransform)viewport.transform.GetChild(0);

            //viewport.localPosition = new Vector2(three[0], viewport.localPosition.y);
            //viewport.sizeDelta = new Vector2(three[1], viewport.sizeDelta.y);
            //content.offsetMax = new Vector2(three[2], content.offsetMax.y);
            //view.ReloadData();

            //float[] one = { 33, 5.5f, -25, -10 };
            //float[] two = { -3, 0, 0, 0 };
            //float[] three = adjust ? one : two;

            //RectTransform viewport = (RectTransform)view.transform.GetChild(1);
            //RectTransform content = (RectTransform)viewport.transform.GetChild(0);
            //viewport.sizeDelta = new Vector2(three[0], viewport.sizeDelta.y);

            //viewport.localPosition = new Vector2(three[1], viewport.localPosition.y);

            //content.sizeDelta = new Vector2(three[2], content.sizeDelta.y);
            ////content.localPosition = new Vector2(three[3], content.localPosition.y);

            //view.ReloadData();
        }

        private BeatmapLevelSO CreateOstSong(string songDirectory)
        {
            var files = Directory.GetFiles(songDirectory);

            string infoFile = files.FirstOrDefault(fileName => fileName.Contains("Info")) ?? "";
            string songFile = files.FirstOrDefault(fileName => fileName.Contains(".ogg") || fileName.Contains(".wav")) ?? "";
            string coverFile = files.FirstOrDefault(fileName => fileName.Contains(".png") || fileName.Contains(".jpg")) ?? "";

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

            var infoObj = JObject.Parse(json);
            JArray difficultyBeatmaps = infoObj["_difficultyBeatmapSets"][0]["_difficultyBeatmaps"].Value<JArray>();
            foreach (var beatmap in difficultyBeatmaps)
            {
                int diff = (beatmap["_difficultyRank"].Value<int>() + 1) / 2 - 1;
                string fileName = beatmap["_beatmapFilename"].Value<string>();
                var map = level._difficultyBeatmapSets[0]._difficultyBeatmaps[diff];
                var customBeatmapData = ScriptableObject.CreateInstance<CustomOstBeatmapData>();
                customBeatmapData.JsonDataFilePath = Path.Combine(songDirectory, fileName);
                map._difficulty = (BeatmapDifficulty)diff;
                map._beatmapData = customBeatmapData;
            }
            return level;
        }

        [AffinityPatch(typeof(MainMenuViewController), nameof(MainMenuViewController.DidActivate))]
        [AffinityPostfix]
        public async void MainSettings(bool firstActivation, MainMenuViewController __instance)
        {
            if (!firstActivation) return;
            _mapSaving.MapSavingCallback += (one, two) => _log.Info($"Maps saved {one} / {two}");

            __instance._soloButton.gameObject.SetActive(false);
            __instance._partyButton.gameObject.SetActive(false);
            __instance._campaignButton.gameObject.SetActive(false);
            __instance._multiplayerButton.gameObject.SetActive(false);

            await WaitUntil(() => _mapSaving.GetIsReady());
            string ost7Path = "./UserData/OstBackport/OST7";
            string ost6Path = "./UserData/OstBackport/OST6";
            if (!Directory.Exists(ost7Path) || !Directory.Exists(ost6Path)) return;
            string[] ost7dir = Directory.GetDirectories(ost7Path);
            foreach (string directory in ost7dir)
            {
                _ost7LevelSos.Add(CreateOstSong(directory));
            }
            _log.Notice("Created OST 7");
            string[] ost6dir = Directory.GetDirectories(ost6Path);
            foreach (string directory in ost6dir)
            {
                _ost6LevelSos.Add(CreateOstSong(directory));
            }
            _log.Notice("Created OST 6");
            __instance._soloButton.gameObject.SetActive(true);
            __instance._partyButton.gameObject.SetActive(true);
            __instance._campaignButton.gameObject.SetActive(true);
            __instance._multiplayerButton.gameObject.SetActive(true);
        }

        public async Task WaitUntil(Func<bool> condition)
        {
            while (!condition())
            {
                await Task.Delay(100);
            }
            _log.Info("Condition met");
        }
    }
}
