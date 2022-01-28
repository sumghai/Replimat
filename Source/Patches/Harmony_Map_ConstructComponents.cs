using HarmonyLib;
using Verse;

namespace Replimat.Patches
{
    
    // Setup Replimat Hopper cache for each game map
    [HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
    public static class Harmony_Map_ConstructComponents
    {
        static public void Prefix(Map __instance)
        {
            if (!ReplimatMod.repHopperGrid.ContainsKey(__instance))
            {
                ReplimatMod.repHopperGrid.Add(__instance, new bool[__instance.info.NumCells]);
            }
        }
    }
}
