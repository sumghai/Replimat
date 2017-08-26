using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class JobDriver_IngestReplimat : JobDriver
    {
        public const TargetIndex IngestibleSourceInd = TargetIndex.A;

        private const TargetIndex TableCellInd = TargetIndex.B;

        private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        private Thing IngestibleSource
        {
            get
            {
                return base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        private float ChewDurationMultiplier
        {
            get
            {
                Thing ingestibleSource = this.IngestibleSource;
                if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
                {
                    return 1f;
                }
                return 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            }
        }

        public override string GetReport()
        {
            return base.CurJob.def.reportString.Replace("TargetA", ThingDefOf.MealFine.label);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil chew = Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            foreach (Toil toil in this.PrepareToIngestToils(chew))
            {
                yield return toil;
            }
            yield return chew;
            yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
            yield return Toils_Jump.JumpIf(chew, () => this.CurJob.GetTarget(TargetIndex.A).Thing is Corpse && this.pawn.needs.food.CurLevelPercentage < 0.9f);
        }

        private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
        {
            return this.PrepareToIngestToils_Dispenser();
        }

        private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
            yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
        }
    }
}
