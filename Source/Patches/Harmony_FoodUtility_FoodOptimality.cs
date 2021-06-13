using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class Harmony_FoodUtility_FoodOptimality
    {
        static bool Prefix(ref ThingDef foodDef, ref float __result)
        {
            if (foodDef == null)
            {
                __result = -9999999f;
                return false;
            }

            return true;
        }
    }
}
