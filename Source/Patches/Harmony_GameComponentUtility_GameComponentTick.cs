using HarmonyLib;
using Verse;

namespace Replimat
{
	// Update the Replimat Hopper grid periodically for power state changes
	[HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick))]
	public static class Harmony_GameComponentUtility_GameComponentTick
    {
		static int tick;

		static void Postfix()
		{
			if (++tick == 600) // 10 seconds
			{
				tick = 0;
				foreach (var repHopper in ReplimatMod.repHopperCache)
				{
					if (repHopper.Key.Map == null)
					{
						ReplimatMod.repHopperCache.Remove(repHopper.Key);
						break;
					}

					ReplimatUtility.UpdateRepHopperGrid(repHopper.Value);
				}
			}
		}
	}
}
