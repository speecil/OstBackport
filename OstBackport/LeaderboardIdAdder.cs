using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace OstBackport
{
    internal class LeaderboardIdAdder : IInitializable
    {
        private static bool _firstStartup = true;

        private readonly Dictionary<string, string> _ost7LeaderboardIds = new Dictionary<string, string>()
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

        private readonly Dictionary<string, string> _ost6LeaderboardIds = new Dictionary<string, string>()
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

        public void Initialize()
        {
            if (!_firstStartup) return;

            LeaderboardIdsModelSO ids = UnityEngine.Resources.FindObjectsOfTypeAll<MainSystemInit>().FirstOrDefault()?._steamLeaderboardIdsModel;

            foreach (KeyValuePair<string, string> kvp in _ost7LeaderboardIds)
            {
                ids?._leaderboardIds.Add(new LeaderboardIdsModelSO.LeaderboardIdData(kvp.Key, kvp.Value));
            }
            foreach (KeyValuePair<string, string> kvp in _ost6LeaderboardIds)
            {
                ids?._leaderboardIds.Add(new LeaderboardIdsModelSO.LeaderboardIdData(kvp.Key, kvp.Value));
            }
            ids?.RebuildMap();
            _firstStartup = false;
        }
    }
}
