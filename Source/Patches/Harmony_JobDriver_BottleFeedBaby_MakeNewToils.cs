using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Replimat
{
    [HarmonyPatch(typeof(JobDriver_BottleFeedBaby), nameof(JobDriver_BottleFeedBaby.MakeNewToils))]
    public static class Harmony_JobDriver_BottleFeedBaby_MakeNewToils
    {
        public static bool Prefix(JobDriver_BottleFeedBaby __instance, ref IEnumerable<Toil> __result)
        {
            if (__instance.BabyFoodTarget.Thing is Building_ReplimatTerminal repTerminal && repTerminal.CanDispenseNow)
            {
                __result = MakeNewToils(__instance, repTerminal);
                return false;
            }
            return true;
        }

        public static IEnumerable<Toil> MakeNewToils(JobDriver_BottleFeedBaby __instance, Building_ReplimatTerminal repTerminal)
        {
            Pawn feeder = __instance.pawn;
            Pawn baby = __instance.Baby;

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOnDestroyedNullOrForbidden(TargetIndex.B);

            Toil toil = new();
            toil.initAction = delegate
            {
                Thing babyFoodSingle = ThingMaker.MakeThing(ThingDefOf.BabyFood, null);
                int babyFoodRequired = FoodUtility.WillIngestStackCountOf(baby, ThingDefOf.BabyFood, FoodUtility.NutritionForEater(baby, babyFoodSingle));
                Thing babyFoodDispensed = repTerminal.TryDispenseFood(baby, feeder, ThingDefOf.BabyFood, babyFoodRequired);

                feeder.carryTracker.TryStartCarry(babyFoodDispensed);
            };
            toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;
            yield return toil;
        }
    }
}