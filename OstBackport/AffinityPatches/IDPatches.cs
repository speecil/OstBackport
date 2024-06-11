using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static LeaderboardIdsModelSO;

namespace OstBackport.AffinityPatches
{
    internal class IDPatches : IAffinity
    {
        [Inject] private readonly MapSaving.MapSaving _mapSaving;
        [Inject] private readonly SiraLog _log;

        public enum OST
        {
            OST6,
            OST7
        }

        private readonly List<BeatmapLevelSO> Ost7LevelSos = new List<BeatmapLevelSO>();
        private readonly List<BeatmapLevelSO> Ost6LevelSos = new List<BeatmapLevelSO>();

        private readonly Dictionary<string, string> Ost7LeaderboardIds = new Dictionary<string, string>()
        {
            {"DamageEasy", "4h4l2b69ml4e955fh88ai94f"},
            {"DamageNormal", "0ec4238273l9i6faj6kbf5ne"},
            {"DamageHard", "kb11m9fl304g65ehc1ib5c52"},
            {"DamageExpert", "9fddj2aa712lek6e5bej6313"},
            {"DamageExpertPlus", "4m9gdg7a6l9eh8hgai4dd4ki"},

            {"LustreEasy", "j42edai9ej356nig57cjln4d"},
            {"LustreNormal", "9ne6h3n585b65eci5e2dnh8f"},
            {"LustreHard", "10em46n92kbldilmh5c907bj"},
            {"LustreExpert", "ce9j0ammg2d5ikn4jke8la54"},
            {"LustreExpertPlus", "nd6gdfjd6jmmg2ja9l28c96b"},

            {"TheMasterEasy", "7li66gkib5mh418e52ghk2me"},
            {"TheMasterNormal", "gc81na32j2gmca454498j51k"},
            {"TheMasterHard", "eb46956n8j4ie1nd4gb8g2gf"},
            {"TheMasterExpert", "md7hg3ehalkecafebdikdmf5"},
            {"TheMasterExpertPlus", "565nh5i4861l3ca4m10bmgea"},

            {"UntamedEasy", "g8ia6698hg6bmg6hln6ag8eh"},
            {"UntamedNormal", "2md39k2elk6778k0bhc4gfi6"},
            {"UntamedHard", "1574dgbmcci63dhn919m8f3l"},
            {"UntamedExpert", "b388c0f0bd7g40k53mlc925a"},
            {"UntamedExpertPlus", "7gj8465mce22kbi6i0j8gh17"},

            {"WorldWideWebEasy", "93g2egg908c34c9hik38jhff"},
            {"WorldWideWebNormal", "3276iml9mje72dn2gjmf741c"},
            {"WorldWideWebHard", "i0cb5kkj22bachlm19gcjla0"},
            {"WorldWideWebExpert", "58873f5e8l00b58k48a7kifi"},
            {"WorldWideWebExpertPlus", "5hklb4bcb223aihg7e31k93e"},
        };

        private readonly Dictionary<string, string> Ost6LeaderboardIds = new Dictionary<string, string>()
        {
            {"HeavyWeightEasy", "2eilkhkd1bma22b5c4n1mjgn"},
            {"HeavyWeightNormal", "ebj8nn5f0043bfml0le9akc1"},
            {"HeavyWeightHard", "9f5ea6e6d88nc5l6ij89b76g"},
            {"HeavyWeightExpert", "6g5416a1geelkgm7jhe3aij4"},
            {"HeavyWeightExpertPlus", "a7gk3ghh7gh1ii6j9dcm77ja"},

            {"LiftOffEasy", "129fdja987ej0e38058j5lba"},
            {"LiftOffNormal", "2gme7c9gkclkb31hh277nbm0"},
            {"LiftOffHard", "hb02iflcbn5flldiic4i4ejc"},
            {"LiftOffExpert", "k9di1ikn4blg3260kc899fcg"},
            {"LiftOffExpertPlus", "nd77ee850k6d5hjg04jmgebn"},

            {"PowerOfTheSaberBladeEasy", "599ge8hdkl1g4m45bd607k7b"},
            {"PowerOfTheSaberBladeNormal", "9kdeb6c9m5ea38c56hjhmj4l"},
            {"PowerOfTheSaberBladeHard", "jangk1j632c7f1aki59bgii7"},
            {"PowerOfTheSaberBladeExpert", "biagm47jnb5b9m3d8ggl0762"},
            {"PowerOfTheSaberBladeExpertPlus", "k1693i1f0kbf502ei7dkh2k4"},

            {"TempoKatanaEasy", "5jin262ml018j2k7533f4l30"},
            {"TempoKatanaNormal", "h0mb95j4f9c75j16b8a28558"},
            {"TempoKatanaHard", "d5cl880c0chd6477d9kn40bj"},
            {"TempoKatanaExpert", "ckcja24m1669i9m5gi4ih0de"},
            {"TempoKatanaExpertPlus", "612b5a87n5mldiceid974cbj"},

            {"CathedralEasy", "elc073dl8nbc8a3ac4ja84j5"},
            {"CathedralNormal", "a0d6b1lg4nc4feiim29fg71f"},
            {"CathedralHard", "eb44075ehn0d9053n1db3845"},
            {"CathedralExpert", "5jehldi2a6hafg1c140nd9jd"},
            {"CathedralExpertPlus", "36b6bk127ajhg737njk7kb8l"},
        };

        [AffinityPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.SetupBeatmapLevelPacks))]
        [AffinityPostfix]
        public void LevelFilteringNavigationController_SetupBeatmapLevelPacks(LevelFilteringNavigationController __instance)
        {
            // ost 7

            _log.Info("Creating OST 7");
            BeatmapLevelPackSO OST7BeatmapLevelPackSO = ScriptableObject.CreateInstance<BeatmapLevelPackSO>();
            OST7BeatmapLevelPackSO._packName = "Original Soundtrack Vol. 7";
            OST7BeatmapLevelPackSO._packID = "OSTVol7";
            OST7BeatmapLevelPackSO._shortPackName = "OST7";
            Sprite OST7Sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OstBackport.Images.OST7.png");
            OST7BeatmapLevelPackSO._coverImage = OST7Sprite;
            OST7BeatmapLevelPackSO._smallCoverImage = OST7Sprite;
            BeatmapLevelCollectionSO OST7BeatmapLevelCollectionSO = ScriptableObject.CreateInstance<BeatmapLevelCollectionSO>();
            BeatmapLevelSO[] OST7Levels = Ost7LevelSos.ToArray();
            OST7BeatmapLevelCollectionSO._beatmapLevels = OST7Levels;
            OST7BeatmapLevelPackSO._beatmapLevelCollection = OST7BeatmapLevelCollectionSO;

            List<IBeatmapLevelPack> OST7BeatmapUpdated = __instance._ostBeatmapLevelPacks.ToList();
            OST7BeatmapUpdated.Insert(OST7BeatmapUpdated.Count - 2, OST7BeatmapLevelPackSO);
            __instance._ostBeatmapLevelPacks = OST7BeatmapUpdated.ToArray();

            foreach (BeatmapLevelSO level in Ost7LevelSos)
            {
                __instance._beatmapLevelsModel._loadedBeatmapLevels.PutToCache(level._levelID, level);
            }


            IBeatmapLevelPack[] allOfficialBeatmapLevelPacks = OST7BeatmapUpdated.Concat(__instance._musicPacksBeatmapLevelPacks).ToArray();
            __instance._allOfficialBeatmapLevelPacks = allOfficialBeatmapLevelPacks;

            // ost 6

            _log.Info("Creating OST 6");
            BeatmapLevelPackSO OST6BeatmapLevelPackSO = ScriptableObject.CreateInstance<BeatmapLevelPackSO>();
            OST6BeatmapLevelPackSO._packName = "Original Soundtrack Vol. 6";
            OST6BeatmapLevelPackSO._packID = "OSTVol6";
            OST6BeatmapLevelPackSO._shortPackName = "OST6";
            Sprite spriteOST6 = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OstBackport.Images.OST6.png");
            OST6BeatmapLevelPackSO._coverImage = spriteOST6;
            OST6BeatmapLevelPackSO._smallCoverImage = spriteOST6;
            BeatmapLevelCollectionSO OST6BeatmapCollectionSO = ScriptableObject.CreateInstance<BeatmapLevelCollectionSO>();
            BeatmapLevelSO[] levelsOST6 = Ost6LevelSos.ToArray();
            OST6BeatmapCollectionSO._beatmapLevels = levelsOST6;
            OST6BeatmapLevelPackSO._beatmapLevelCollection = OST6BeatmapCollectionSO;

            List<IBeatmapLevelPack> OST6BeatmapUpdated = __instance._ostBeatmapLevelPacks.ToList();
            OST6BeatmapUpdated.Insert(OST6BeatmapUpdated.Count - 3, OST6BeatmapLevelPackSO);
            __instance._ostBeatmapLevelPacks = OST6BeatmapUpdated.ToArray();

            foreach (BeatmapLevelSO level in Ost6LevelSos)
            {
                __instance._beatmapLevelsModel._loadedBeatmapLevels.PutToCache(level._levelID, level);
            }

            IBeatmapLevelPack[] thingiesOST6 = OST6BeatmapUpdated.Concat(__instance._musicPacksBeatmapLevelPacks).ToArray();

            __instance._allOfficialBeatmapLevelPacks = thingiesOST6;


            // always owned content
            AlwaysOwnedContentContainerSO container = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault()._alwaysOwnedContentContainer;
            container.alwaysOwnedPacksIds.Add(OST7BeatmapLevelPackSO.packID);
            container.alwaysOwnedPacksIds.Add(OST6BeatmapLevelPackSO.packID);

            AlwaysOwnedContentSO alwaysOwnedContent = container._alwaysOwnedContent;
            List<BeatmapLevelPackSO> beatmapLevelPackSOs = alwaysOwnedContent._alwaysOwnedPacks.ToList();
            beatmapLevelPackSOs.Add(OST7BeatmapLevelPackSO);
            beatmapLevelPackSOs.Add(OST6BeatmapLevelPackSO);
            alwaysOwnedContent._alwaysOwnedPacks = beatmapLevelPackSOs.ToArray();


            container.InitAlwaysOwnedItems();

            //var view = __instance._annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._gridView;

            //__instance._selectLevelCategoryViewController.didSelectLevelCategoryEvent += (one, two) =>
            //{
            //    if (two != SelectLevelCategoryViewController.LevelCategory.MusicPacks) return;
            //    AdjustGridView(view);
            //};
            //__instance.didActivateEvent += (one, two, three) =>
            //{
            //    if (__instance.selectedLevelCategory != SelectLevelCategoryViewController.LevelCategory.MusicPacks) return;
            //    AdjustGridView(view);
            //};
        }

        private void AdjustGridView(GridView view)
        {
            view.transform.position = new Vector3(0.8f, view.transform.position.y, view.transform.position.z);
            RectTransform viewport = (RectTransform)view.transform.GetChild(1);
            RectTransform content = (RectTransform)viewport.transform.GetChild(0);
            viewport.sizeDelta = new Vector2(33, viewport.sizeDelta.y);
            content.offsetMax = new Vector2(-20.33f, content.offsetMax.y);
            view.ReloadData();
        }

        private void HandleLevelDifficulty(BeatmapLevelSO.DifficultyBeatmap map, string directory)
        {
            switch (map._difficultyRank)
            {
                case 1:
                    map._beatmapData._jsonData = File.ReadAllText(directory + "/EasyStandard.dat");
                    map._difficulty = BeatmapDifficulty.Easy;
                    break;
                case 3:
                    map._beatmapData._jsonData = File.ReadAllText(directory + "/NormalStandard.dat");
                    map._difficulty = BeatmapDifficulty.Normal;
                    break;
                case 5:
                    map._beatmapData._jsonData = File.ReadAllText(directory + "/HardStandard.dat");
                    map._difficulty = BeatmapDifficulty.Hard;
                    break;
                case 7:
                    map._beatmapData._jsonData = File.ReadAllText(directory + "/ExpertStandard.dat");
                    map._difficulty = BeatmapDifficulty.Expert;
                    break;
                case 9:
                    map._beatmapData._jsonData = File.ReadAllText(directory + "/ExpertPlusStandard.dat");
                    map._difficulty = BeatmapDifficulty.ExpertPlus;
                    break;
            }
        }

        private async Task CreateOstSong(string songDirectory, OST ost)
        {
            string[] files = Directory.GetFiles(songDirectory);

            string infoFile = files.FirstOrDefault(fileName => fileName.Contains("Info")) ?? "";
            string songFile = files.FirstOrDefault(fileName => fileName.Contains(".ogg")) ?? "";
            string coverFile = files.FirstOrDefault(fileName => fileName.Contains(".png")) ?? "";

            string json = File.ReadAllText(infoFile);
            BeatmapLevelSO level = ScriptableObject.CreateInstance<BeatmapLevelSO>();
            JsonUtility.FromJsonOverwrite(json, level);
            level._levelID = level.songName.Replace(" ", "").Replace("-", "");
            level._audioClip = await SongCore.Loader._customLevelLoader._audioClipAsyncLoader._mediaAsyncLoader.LoadAudioClipFromFilePathAsync(songFile);
            level._coverImage = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(File.ReadAllBytes(coverFile));
            level._difficultyBeatmapSets[0]._beatmapCharacteristic = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName("Standard");
            level._environmentInfo = SongCore.Loader._customLevelLoader._defaultEnvironmentInfo;
            level._allDirectionsEnvironmentInfo = SongCore.Loader._customLevelLoader._defaultAllDirectionsEnvironmentInfo;

            AudioClipAsyncLoader audioLoader = SongCore.Loader._customLevelLoader._audioClipAsyncLoader;
            audioLoader._cache.Insert(audioLoader.GetCacheKey(level._audioClip), Task.FromResult(level._audioClip));

            level.InitData();
            foreach (BeatmapLevelSO.DifficultyBeatmap map in level._difficultyBeatmapSets[0]._difficultyBeatmaps)
            {
                map._beatmapData = ScriptableObject.CreateInstance<BeatmapDataSO>();
                HandleLevelDifficulty(map, songDirectory);
            }
            switch (ost)
            {
                case OST.OST6:
                    Ost6LevelSos.Add(level);
                    break;
                case OST.OST7:
                    Ost7LevelSos.Add(level);
                    break;
            }
        }

        private void AddOstLeaderboards()
        {
            LeaderboardIdsModelSO ids = Resources.FindObjectsOfTypeAll<MainSystemInit>().FirstOrDefault()?._steamLeaderboardIdsModel;
            foreach (KeyValuePair<string, string> kvp in Ost7LeaderboardIds)
            {
                ids?._leaderboardIds.Add(new LeaderboardIdData(kvp.Key, kvp.Value));
            }
            foreach (KeyValuePair<string, string> id in Ost6LeaderboardIds)
            {
                ids?._leaderboardIds.Add(new LeaderboardIdData(id.Key, id.Value));
            }
            ids?.RebuildMap();
        }


        [AffinityPatch(typeof(MainMenuViewController), nameof(MainMenuViewController.DidActivate))]
        [AffinityPostfix]
        public async void MainSettings(bool firstActivation)
        {
            if (!firstActivation) return;
            await WaitUntil(() => _mapSaving.GetIsReady());
            AddOstLeaderboards();
            string ost7Path = "./UserData/OstBackport/OST7";
            string ost6Path = "./UserData/OstBackport/OST6";
            if (!Directory.Exists(ost7Path) || !Directory.Exists(ost6Path)) return;
            string[] ost7dir = Directory.GetDirectories(ost7Path);
            foreach (string directory in ost7dir)
            {
                await CreateOstSong(directory, OST.OST7);
            }
            _log.Notice("Created OST 7");
            string[] ost6dir = Directory.GetDirectories(ost6Path);
            foreach (string directory in ost6dir)
            {
                await CreateOstSong(directory, OST.OST6);
            }
            _log.Notice("Created OST 6");
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
