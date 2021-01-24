using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class WorkGiver_LoadReplimatCorpseRecycler : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ReplimatDef.ReplimatCorpseRecycler);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_ReplimatCorpseRecycler building_ReplimatCorpseRecycler) || !building_ReplimatCorpseRecycler.powerComp.PowerOn)
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (!building_ReplimatCorpseRecycler.Empty)
            {
                JobFailReason.Is("CorpseRecyclerNotEmpty".Translate());
                return false;
            }
            if (FindHumanlikeCorpse(pawn, building_ReplimatCorpseRecycler) == null)
            {
                JobFailReason.Is("CorpseRecyclerNeedValidCorpse".Translate());
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }
            if (t.IsBrokenDown())
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_ReplimatCorpseRecycler corpseRecycler = (Building_ReplimatCorpseRecycler)t;
            Thing t2 = FindHumanlikeCorpse(pawn, corpseRecycler);
            return JobMaker.MakeJob(ReplimatDef.LoadReplimatCorpseRecycler, t, t2);
        }

        private Corpse FindHumanlikeCorpse(Pawn pawn, Building_ReplimatCorpseRecycler corpseRecycler)
        {
            // Only allow corpses that are:
            // - Humanlike
            // - Have organic flesh
            // - Is NOT dessicated
            // - Is NOT forbidden
            // - Is allowed by the current corpse recycler's storage settings
            // - Can be reserved by current pawn
            Predicate<Thing> validator = delegate (Thing t)
            {
                Corpse corpse = t as Corpse;
                return corpse.InnerPawn.RaceProps.Humanlike && corpse.InnerPawn.RaceProps.IsFlesh && (corpse.GetRotStage() != RotStage.Dessicated) && !corpse.IsForbidden(pawn) && corpseRecycler.allowedCorpseFilterSettings.AllowedToAccept(corpse) && pawn.CanReserve(corpse);
            };

            return (Corpse)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }
    }
}
