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

        protected Building_ReplimatCorpseRecycler CorpseRecycler => (Building_ReplimatCorpseRecycler)job.GetTarget(TargetIndex.A).Thing;

        protected Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.B).Thing;

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
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
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
            Toil reserveCorpse = Toils_Reserve.Reserve(TargetIndex.B);
            yield return reserveCorpse;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A);
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
