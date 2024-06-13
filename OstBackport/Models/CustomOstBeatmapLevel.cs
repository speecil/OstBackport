using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OstBackport.Models
{
    internal class CustomOstBeatmapLevel : IBeatmapLevel, IFilePathSongAudioClipProvider, IFilePathSongPreviewAudioClipProvider
    {
        public string levelID { get; }
        public string songName { get; }
        public string songSubName { get; }
        public string songAuthorName { get; }
        public string levelAuthorName { get; }
        public float beatsPerMinute { get; }
        public float songTimeOffset { get; }
        public float shuffle { get; }
        public float shufflePeriod { get; }
        public float previewStartTime { get; }
        public float previewDuration { get; }
        public EnvironmentInfoSO environmentInfo { get; }
        public EnvironmentInfoSO allDirectionsEnvironmentInfo { get; }
        public ISpriteAsyncLoader SpriteAsyncLoader { get; }
        public IReadOnlyList<PreviewDifficultyBeatmapSet> previewDifficultyBeatmapSets { get; }
        public IBeatmapLevelData beatmapLevelData => CustomBeatmapLevelData;
        public float songDuration { get; }
        public string songAudioClipPath { get; }
        public string songPreviewAudioClipPath => songAudioClipPath;
        public string CoverImagePath { get; }
        public BeatmapLevelData CustomBeatmapLevelData { get; }

        private readonly CustomOstDifficultyBeatmapSet[] _difficultyBeatmapSets;

        public async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            return await SpriteAsyncLoader.LoadSpriteAsync(CoverImagePath, cancellationToken);
        }

        public void InitData()
        {
            foreach (var mapSet in _difficultyBeatmapSets) mapSet.SetParentLevel(this);
        }

        public CustomOstBeatmapLevel(CustomOstPreviewBeatmapLevelSO previewLevel, AudioClip song, CustomOstDifficultyBeatmapSet[] difficultyBeatmapSets)
        {
            levelID = previewLevel.levelID;
            songName = previewLevel.songName;
            songSubName = previewLevel.songSubName;
            songAuthorName = previewLevel.songAuthorName;
            levelAuthorName = previewLevel.levelAuthorName;
            beatsPerMinute = previewLevel.beatsPerMinute;
            songTimeOffset = previewLevel.songTimeOffset;
            shuffle = previewLevel.shuffle;
            shufflePeriod = previewLevel.shufflePeriod;
            previewStartTime = previewLevel.previewStartTime;
            previewDuration = previewLevel.previewDuration;
            environmentInfo = previewLevel.environmentInfo;
            allDirectionsEnvironmentInfo = previewLevel.allDirectionsEnvironmentInfo;
            SpriteAsyncLoader = SongCore.Loader._customLevelLoader._cachedMediaAsyncLoader;
            previewDifficultyBeatmapSets = previewLevel.previewDifficultyBeatmapSets;
            songDuration = previewLevel.songDuration;
            songAudioClipPath = previewLevel.songAudioClipPath;
            CoverImagePath = previewLevel.CoverImagePath;
            _difficultyBeatmapSets = difficultyBeatmapSets;
            CustomBeatmapLevelData = new BeatmapLevelData(song, difficultyBeatmapSets.ToList());
        }
    }
}
