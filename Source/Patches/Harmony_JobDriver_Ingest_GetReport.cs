using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Replimat
{
    [HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.GetReport))]
    public static class Harmony_JobDriver_Ingest_GetReport
    {
        static void Postfix(JobDriver_Ingest __instance, ref string __result)
        {
            if (__instance.usingNutrientPasteDispenser)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "ReplicatedMeal".Translate());
                }
                else
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", __instance.job.GetTarget(TargetIndex.A).Thing.Label);
                }
            }
        }
    }
}
