using HarmonyLib;
using System.Linq;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.AmbientTemperature), MethodType.Getter)]
    public static class Harmony_Thing_AmbientTemperature
    {
        static void Postfix(Thing __instance, ref float __result)
        {
            if (__instance.Spawned && __instance.def.IsNutritionGivingIngestible)
            {
                var hop = __instance.Map.thingGrid.ThingsListAtFast(__instance.Position).OfType<Building_ReplimatHopper>().FirstOrDefault(x => x.powerComp.PowerOn);
                if (hop != null)
                {
                    __result = hop.FreezerTemp;
                }
            }
        }
    }
}
