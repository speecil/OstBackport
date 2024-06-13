using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OstBackport.Services;
using UnityEngine;
using Zenject;

namespace OstBackport.AffinityPatches
{
    internal class IdPatches : IAffinity
    {
        [Inject] private readonly MapSaving.MapSaving _mapSaving;
        [Inject] private readonly SiraLog _log;
        [Inject] private readonly CustomOstLevelService _customOstLevelService;

        

        public static void AddLevelPack(string coverImage, List<string> levelIds, int packNum, LevelFilteringNavigationController controller)
        {
            PreviewBeatmapLevelPackSO levelPack = ScriptableObject.CreateInstance<PreviewBeatmapLevelPackSO>();
            levelPack._packName = "Original Soundtrack Vol. " + packNum;
            levelPack._packID = "OSTVol" + packNum;
            levelPack._shortPackName = "OST" + packNum;
            Sprite sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly(coverImage);
            levelPack._coverImage = sprite;
            levelPack._smallCoverImage = sprite;
            PreviewBeatmapLevelCollectionSO man = ScriptableObject.CreateInstance<PreviewBeatmapLevelCollectionSO>();
            PreviewBeatmapLevelSO[] levels = levelIds.Select(id => controller._beatmapLevelsModel._loadedPreviewBeatmapLevels[id] as PreviewBeatmapLevelSO).ToArray();
            man._beatmapLevels = levels;
            levelPack._previewBeatmapLevelCollection = man;

            List<IBeatmapLevelPack> updated = controller._ostBeatmapLevelPacks.ToList();
            updated.Insert(updated.Count - 2, levelPack);
            controller._ostBeatmapLevelPacks = updated.ToArray();

            AlwaysOwnedContentContainerSO ownedContent = controller._beatmapLevelsModel._additionalContentModel._alwaysOwnedContentContainer;
            ownedContent._alwaysOwnedPacksIds.Add(levelPack._packID);
            levelIds.ForEach(id => ownedContent._alwaysOwnedBeatmapLevelIds.Add(id));
        }

        [AffinityPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.SetupBeatmapLevelPacks))]
        [AffinityPostfix]
        public void LevelFilteringNavigationController_SetupBeatmapLevelPacks(LevelFilteringNavigationController __instance)
        {
            AddLevelPack("OstBackport.Images.OST6.png", _mapSaving.OST6IDs, 6, __instance);
            AddLevelPack("OstBackport.Images.OST7.png", _mapSaving.OST7IDs, 7, __instance);

            IBeatmapLevelPack[] allPacks = __instance._ostBeatmapLevelPacks.Concat(__instance._musicPacksBeatmapLevelPacks).ToArray();
            __instance._allOfficialBeatmapLevelPacks = allPacks;

            // i hate my life
            GridView view = __instance._annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._gridView;

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
                //_ost7LevelSos.Add(CreateOstSong(directory));
                _customOstLevelService.LoadCustomOstPreviewBeatmapLevel(directory);
            }
            _log.Notice("Created OST 7");
            string[] ost6dir = Directory.GetDirectories(ost6Path);
            foreach (string directory in ost6dir)
            {
                //_ost6LevelSos.Add(CreateOstSong(directory));
                _customOstLevelService.LoadCustomOstPreviewBeatmapLevel(directory);
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
