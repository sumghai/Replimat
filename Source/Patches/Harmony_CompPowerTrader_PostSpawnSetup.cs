using HarmonyLib;
using RimWorld;

namespace Replimat
{
	// Register newly-spawned Replimat Hoppers to the cache
	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PostSpawnSetup))]
	public static class Harmony_CompPowerTrader_PostSpawnSetup
    {
		static public void Postfix(CompPowerTrader __instance)
		{
			if (__instance.parent.def == ReplimatDef.ReplimatHopper)
			{
				ReplimatMod.repHopperCache.Add(__instance.parent, __instance);
				ReplimatUtility.UpdateRepHopperGrid(__instance);
			}
		}
	}
}
