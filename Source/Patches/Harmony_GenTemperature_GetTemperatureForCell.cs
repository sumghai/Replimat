using HarmonyLib;
using Verse;

namespace Replimat.Patches
{
    // Change the temperature of cells occupied by Replimat Hoppers (as needed)
	[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.GetTemperatureForCell))]
    public static class Harmony_GenTemperature_GetTemperatureForCell
    {
		static public bool Prefix(Map map, IntVec3 c, ref float __result)
		{
			if (map?.info != null && ReplimatMod.repHopperGrid.TryGetValue(map, out bool[] grid))
			{
				int index = c.z * map.info.sizeInt.x + c.x;
				if (index > -1 && index < grid.Length && grid[index])
				{
					__result = -2f;
					return false;
				}
			}
			return true;
		}
	}
}
