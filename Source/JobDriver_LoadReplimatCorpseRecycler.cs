using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class JobDriver_LoadReplimatCorpseRecycler : JobDriver
    {
        private const TargetIndex CorpseRecyclerInd = TargetIndex.A;

        private const TargetIndex CorpseInd = TargetIndex.B;

        private const int Duration = 200;

        protected Building_ReplimatCorpseRecycler CorpseRecycler => (Building_ReplimatCorpseRecycler)job.GetTarget(CorpseRecyclerInd).Thing;

        protected Corpse Corpse => (Corpse)job.GetTarget(CorpseInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(CorpseRecycler, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Corpse, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CorpseRecyclerInd);
            this.FailOnBurningImmobile(CorpseRecyclerInd);
            this.FailOn(delegate 
            {
                // Fail if corpse recycler has no power, or its current storage settings does not allow the chosen corpse
                return !CorpseRecycler.powerComp.PowerOn || !CorpseRecycler.allowedCorpseFilterSettings.AllowedToAccept(Corpse);
            });
            AddEndCondition(() => (CorpseRecycler.Empty) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = 1;
            });
            Toil reserveCorpse = Toils_Reserve.Reserve(CorpseInd);
            yield return reserveCorpse;
            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(CorpseInd).FailOnSomeonePhysicallyInteracting(CorpseInd);
            yield return Toils_Haul.StartCarryThing(CorpseInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(CorpseInd);
            yield return Toils_Goto.GotoThing(CorpseRecyclerInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(CorpseInd).FailOnDestroyedNullOrForbidden(CorpseRecyclerInd).FailOnCannotTouch(CorpseRecyclerInd, PathEndMode.Touch).WithProgressBarToilDelay(CorpseRecyclerInd);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                CorpseRecycler.LoadCorpse(Corpse);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }
    }
}
