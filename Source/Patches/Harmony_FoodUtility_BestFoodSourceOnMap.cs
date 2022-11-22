using HarmonyLib;
using RimWorld;
using Verse;

namespace Replimat
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap_NewTemp))]
    public static class Harmony_FoodUtility_BestFoodSourceOnMap
    {
        static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull,
                ref bool allowForbidden, ref bool allowSociallyImproper)
        {
            ReplimatMod.BestFoodSourceOnMap = true;
            ReplimatMod.getter = getter;
            ReplimatMod.eater = eater;
            ReplimatMod.allowDispenserFull = allowDispenserFull;
            ReplimatMod.allowForbidden = allowForbidden;
            ReplimatMod.allowSociallyImproper = allowSociallyImproper;
        }

        static void Postfix()
        {
            ReplimatMod.BestFoodSourceOnMap = false;
        }
    }
}
