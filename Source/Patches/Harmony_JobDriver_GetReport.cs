using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.GetReport))]
    public static class Harmony_JobDriver_GetReport
    {
        public static void Postfix(ref string __result, Pawn ___pawn)
        {
            Job job = ___pawn.CurJob;
            
            if (job.def.reportString == JobDefOf.BottleFeedBaby.reportString && job.targetB.Thing is Building_ReplimatTerminal)
            {
                __result = job.def.reportString.Replace("TargetA", job.targetA.Thing.LabelShort).Replace("TargetB", "ReplicatedFoodPrefix".Translate() + ThingDefOf.BabyFood.label);
            }
        }
    }
}
