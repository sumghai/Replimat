using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat
{
    // Remove Replimat Hoppers from the cache when they are despawned
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DeSpawn))]
    public static class Harmony_ThingWithComps_DeSpawn
    {
		static public void Prefix(ThingWithComps __instance)
		{
            if (ReplimatMod.repHopperCache.TryGetValue(__instance, out CompPowerTrader comp))
            {
                comp.powerOnInt = false;
                ReplimatUtility.UpdateRepHopperGrid(comp);
                ReplimatMod.repHopperCache.Remove(__instance);
            }
        }
	}
}
