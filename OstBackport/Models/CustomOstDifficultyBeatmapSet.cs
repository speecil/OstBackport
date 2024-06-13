using System.Collections.Generic;
using Newtonsoft.Json;

namespace OstBackport.Models
{
    internal class CustomOstDifficultyBeatmapSet : IDifficultyBeatmapSet
    {

        public BeatmapCharacteristicSO beatmapCharacteristic => GetCharacteristicSo();

        public string BeatmapCharacteristicName => _beatmapCharacteristicName;

        public IReadOnlyList<IDifficultyBeatmap> difficultyBeatmaps => _difficultyBeatmaps;

        public CustomOstDifficultyBeatmap[] beatmaps => _difficultyBeatmaps;

        public virtual void SetParentLevel(IBeatmapLevel level)
        {
            foreach (CustomOstDifficultyBeatmap difficultyBeatmap in _difficultyBeatmaps) difficultyBeatmap.SetParents(level, this);
        }

        private BeatmapCharacteristicSO GetCharacteristicSo()
        {
            return SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(_beatmapCharacteristicName);
        }
        [JsonProperty]
        private string _beatmapCharacteristicName;
        [JsonProperty]
        private CustomOstDifficultyBeatmap[] _difficultyBeatmaps;
    }
}
