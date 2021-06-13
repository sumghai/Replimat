using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFinalIngestibleDef))]
    public static class Harmony_FoodUtility_GetFinalIngestibleDef
    {
        static bool Prefix(ref Thing foodSource, ref ThingDef __result)
        {
            if (foodSource is Building_ReplimatTerminal && ReplimatMod.BestFoodSourceOnMap)
            {
                __result = ReplimatUtility.PickMeal(ReplimatMod.eater, ReplimatMod.getter);
                return false;
            }

            return true;
        }
    }
}
