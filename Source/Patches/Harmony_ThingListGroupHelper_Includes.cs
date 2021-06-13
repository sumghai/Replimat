using HarmonyLib;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class Harmony_ThingListGroupHelper_Includes
    {
        static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
        {
            if (group == ThingRequestGroup.FoodSource || group == ThingRequestGroup.FoodSourceNotPlantOrTree)
            {
                if (def.thingClass == typeof(Building_ReplimatTerminal))
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}
