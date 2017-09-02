using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class WorkGiver_Warden_FeedReplimat : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!base.ShouldTakeCareOfPrisoner(pawn, t))
            {
                return null;
            }
            Pawn pawn2 = (Pawn)t;
            if (!WardenFeedUtility.ShouldBeFed(pawn2))
            {
                return null;
            }
            if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
            {
                return null;
            }
            Thing t2;
            ThingDef def;
            if (!ReplimatUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out t2, out def, false, true, false, false, false))
            {
                JobFailReason.Is("NoFood".Translate());
                return null;
            }
            return new Job(ReplimatDef.feedPatientReplimatDef)
            {
                targetA = t2,
                targetB = pawn2,
                count = ReplimatUtility.WillIngestStackCountOf(pawn2, def)
            };
        }
    }
}