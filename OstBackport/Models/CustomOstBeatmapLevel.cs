using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OstBackport.Models
{
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
}
