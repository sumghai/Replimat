using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), nameof(JobDriver_FoodFeedPatient.GetReport))]
    public static class Harmony_JobDriver_FoodFeedPatient_GetReport
    {
        static void Postfix(JobDriver_FoodFeedPatient __instance, ref string __result)
        {
            if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal && (Pawn)__instance.job.targetB.Thing != null)
            {
                __result = __instance.job.def.reportString.Replace("TargetA", "ReplicatedMeal".Translate()).Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
            }
        }
    }
}
