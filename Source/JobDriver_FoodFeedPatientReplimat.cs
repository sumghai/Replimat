using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class JobDriver_FoodFeedPatientReplimat : JobDriver
    {
        public const TargetIndex IngestibleSourceInd = TargetIndex.A;

        private const TargetIndex DelivereeInd = TargetIndex.B;

        private const float FeedDurationMultiplier = 1.5f;

        protected Thing Food
        {
            get
            {
                return base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn Deliveree
        {
            get
            {
                return (Pawn)base.CurJob.targetB.Thing;
            }
        }

        public override string GetReport()
        {
            if (base.CurJob.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal Replimat)
            {
                return base.CurJob.def.reportString.Replace("TargetA", Replimat.DispensableDef.label).Replace("TargetB", ((Pawn)((Thing)base.CurJob.targetB)).LabelShort);
            }
            return base.GetReport();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOn(() => !FoodUtility.ShouldBeFedBySomeone(this.Deliveree));
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);

            if (base.TargetThingA is Building_ReplimatTerminal)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
                yield return JobDriver_IngestReplimat.TakeMealFromReplimat(TargetIndex.A, this.pawn);
            }

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, TargetIndex.A, TargetIndex.None).FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Ingest.FinalizeIngest(this.Deliveree, TargetIndex.A);
        }
    }
}
