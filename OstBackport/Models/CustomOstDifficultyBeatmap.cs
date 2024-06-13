using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OstBackport.Models
{
    internal class CustomOstDifficultyBeatmap : IDifficultyBeatmap
    {
        public IBeatmapLevel level => _parentLevel;
        public IDifficultyBeatmapSet parentDifficultyBeatmapSet => _parentDifficultyBeatmapSet;
        public BeatmapDifficulty difficulty => _difficulty;
        public int difficultyRank => _difficultyRank;
        public float noteJumpMovementSpeed => _noteJumpMovementSpeed;
        public float noteJumpStartBeatOffset => _noteJumpStartBeatOffset;
        public string BeatmapFilename => _beatmapFilename;

        public async Task<IReadonlyBeatmapData> GetBeatmapDataAsync(EnvironmentInfoSO environmentInfo, PlayerSpecificSettings playerSpecificSettings)
        {
            return await BeatmapData.GetBeatmapDataAsync(difficulty, level.beatsPerMinute, _parentLevel.environmentInfo.serializedName == environmentInfo.serializedName, environmentInfo, playerSpecificSettings);
        }

        public async Task<IBeatmapDataBasicInfo> GetBeatmapDataBasicInfoAsync()
        {
            return await BeatmapData.GetBeatmapDataBasicInfoAsync();
        }

        public virtual void SetParents(IBeatmapLevel parentLevel, IDifficultyBeatmapSet beatmapSet)
        {
            _parentLevel = parentLevel;
            _parentDifficultyBeatmapSet = beatmapSet;
        }

        internal CustomOstBeatmapData BeatmapData;
        private IBeatmapLevel _parentLevel;
        private IDifficultyBeatmapSet _parentDifficultyBeatmapSet;


        [JsonProperty] [JsonConverter(typeof(StringEnumConverter))]
        private BeatmapDifficulty _difficulty;
        [JsonProperty]
        private int _difficultyRank;
        [JsonProperty]
        private float _noteJumpMovementSpeed;
        [JsonProperty]
        private float _noteJumpStartBeatOffset;
        [JsonProperty]
        private string _beatmapFilename;
    }
}
