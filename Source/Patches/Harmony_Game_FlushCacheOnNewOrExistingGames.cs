using HarmonyLib;
using Verse;

namespace Replimat
{
    // Flush the Replimat Hopper caches when loading new or existing games
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static class Harmony_Game_FlushCacheOnNewOrExistingGames
    {
        static void Prefix()
        {
            ReplimatMod.repHopperCache.Clear();
            ReplimatMod.repHopperGrid.Clear();
        }
    }
}
