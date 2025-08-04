using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Replimat
{
    // Pawn attending parties or gatherings that want to eat should prioritize (working) Replimat Terminals
    [HarmonyPatch(typeof(JobGiver_EatInGatheringArea), nameof(JobGiver_EatInGatheringArea.TryGiveJob))]
    public static class Harmony_JobGiver_EatInGatheringArea_TryGiveJob_CheckReplimatsFirst
    {
        static bool Prefix(Pawn pawn, ref Job __result)
        {
            // Duplicating some vanilla checks
            PawnDuty duty = pawn.mindState.duty;
            if (duty == null || pawn.needs?.food == null || (double)pawn.needs.food.CurLevelPercentage > 0.9)
            {
                __result = null;
                return false;
            }
            else 
            {
                // Get region for gathering
                IntVec3 gatheringFocusCell = duty.focus.Cell;
                Region gatheringRegion = gatheringFocusCell.GetRegion(pawn.Map);

                // Get list of Replimat Terminals within region
                static bool IsReplimatTerminal(Thing t) => t is Building_ReplimatTerminal;
                List<Thing> terminals = [.. gatheringRegion.ListerThings.GetAllThings(IsReplimatTerminal)];

                // Find the first usable Terminal, issue a custom job, and skip executing the rest of the original method
                foreach (Thing currTerminal in terminals)
                {
                    ReplimatMod.BestFoodSourceOnMap = true;
                    ReplimatMod.getter = pawn;
                    ReplimatMod.eater = pawn;
                    ReplimatMod.allowDispenserFull = true;
                    ReplimatMod.allowForbidden = false;
                    ReplimatMod.allowSociallyImproper = true;

                    if (ReplimatUtility.CanUseTerminal((Building_ReplimatTerminal)currTerminal))
                    {
                        Job replimatEatJob = JobMaker.MakeJob(JobDefOf.Ingest, currTerminal);
                        replimatEatJob.count = 1;
                        __result = replimatEatJob;
                        ReplimatMod.BestFoodSourceOnMap = false;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
