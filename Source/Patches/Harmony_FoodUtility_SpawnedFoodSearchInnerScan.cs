using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Replimat
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.SpawnedFoodSearchInnerScan))]
    public static class Harmony_FoodUtility_SpawnedFoodSearchInnerScan
    {
        static bool Prefix(ref Predicate<Thing> validator)
        {
            var malidator = validator;
            bool salivator(Thing x) => x is Building_ReplimatTerminal rep ? RepDel(rep) : malidator(x);
            validator = salivator;
            return true;
        }

        private static bool RepDel(Building_ReplimatTerminal t)
        {
            if (
                !ReplimatMod.allowDispenserFull
                || !(ReplimatMod.getter.RaceProps.ToolUser && ReplimatMod.getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                || t.Faction != ReplimatMod.getter.Faction && t.Faction != ReplimatMod.getter.HostFaction
                || !ReplimatMod.allowForbidden && t.IsForbidden(ReplimatMod.getter)
                || !t.powerComp.PowerOn
                || !t.InteractionCell.Standable(t.Map)
                || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, ReplimatMod.getter, ReplimatMod.eater, ReplimatMod.allowSociallyImproper)
                || ReplimatMod.getter.IsWildMan()
                || ReplimatUtility.PickMeal(ReplimatMod.eater, ReplimatMod.getter) == null
                || !t.HasStockFor(ReplimatUtility.PickMeal(ReplimatMod.eater, ReplimatMod.getter))
                || !ReplimatMod.getter.Map.reachability.CanReachNonLocal(ReplimatMod.getter.Position, new TargetInfo(t.InteractionCell, t.Map),
                    PathEndMode.OnCell, TraverseParms.For(ReplimatMod.getter, Danger.Some)))
            {
                return false;
            }

            return true;
        }
    }
}
