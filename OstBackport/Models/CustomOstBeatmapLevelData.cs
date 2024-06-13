using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OstBackport.Models
{
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
                AudioClipAsyncLoader loader = SongCore.Loader._customLevelLoader._audioClipAsyncLoader;
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
}
