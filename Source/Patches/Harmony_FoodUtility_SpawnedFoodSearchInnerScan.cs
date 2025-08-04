using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Replimat
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.SpawnedFoodSearchInnerScan))]
    public static class Harmony_FoodUtility_SpawnedFoodSearchInnerScan
    {
        static bool Prefix(ref Predicate<Thing> validator)
        {
            var origValidator = validator;
            bool finalValidator(Thing x) => x is Building_ReplimatTerminal terminal ? ReplimatUtility.CanUseTerminal(terminal) : origValidator(x);
            validator = finalValidator;
            return true;
        }
    }
}
