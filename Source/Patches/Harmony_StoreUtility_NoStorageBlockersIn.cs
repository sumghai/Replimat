using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat.Patches
{
    public static class Harmony_StoreUtility_NoStorageBlockersIn
    {
        [HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.NoStorageBlockersIn))]
        static void Postfix(ref bool __result, IntVec3 c, Map map, Thing thing)
        {
            if (__result)
            {
                if (c.GetSlotGroup(map)?.parent?.GetType().ToString() == "Replimat.Building_ReplimatHopper")
                {
                    // Apply the minimum Hopper refilling threshold only to Replimat Hoppers
                    __result &= !map.thingGrid.ThingsListAt(c).Any(t => t.def.EverStorable(false) && t.stackCount >= thing.def.stackLimit * Settings.HopperRefillThresholdPercent);
                }
                else
                {
                    // Ignore the threshold for all other storage buildings and stockpiles
                    // This ensures compatibility with other mods that control stack refilling thresholds on a more general level (e.g. Hauling Hysteresis, Satisfied Storage)
                    __result = __result;
                }
            }
        }
    }
}
