using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat.Patches
{
    // Skip adding food poisoning hediff for replicated food
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
    public static class Harmony_FoodUtility_AddFoodPoisoningHediff
    {
        public static bool Prefix(Thing ingestible)
        {
            return !ReplimatUtility.IsReplicatedFood(ingestible);
        }
    }
}