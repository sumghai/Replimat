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
        public static Toil TakeMealFromReplimat(TargetIndex ind, Pawn eater)
        {            
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                (toil.actor.jobs.curJob.GetTarget(ind).Thing as Building_ReplimatTerminal).Replicate();
            };
            toil.AddFinishAction(delegate
            {
                Thing thing = (toil.actor.jobs.curJob.GetTarget(ind).Thing as Building_ReplimatTerminal).TryDispenseFood();
                if (thing == null)
                {
                    toil.actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                toil.actor.carryTracker.TryStartCarry(thing);
                toil.actor.Map.reservationManager.Release(toil.actor.jobs.curJob.GetTarget(ind), toil.actor);
                toil.actor.CurJob.SetTarget(ind, toil.actor.carryTracker.CarriedThing);
            });
            toil.FailOnCannotTouch(ind, PathEndMode.Touch);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Building_ReplimatTerminal.CollectDuration;
            return toil;
        }


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
                return 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            }
        }

        public override string GetReport()
        {
            //  if (IngestibleSource is Building_ReplimatTerminal Replimat)
            //  {
            //      return base.CurJob.def.reportString.Replace("TargetA", Replimat.SelectedFood.label);
            //   }

            Thing thing = this.pawn.CurJob.targetA.Thing;
            if (thing != null && thing.def.ingestible != null && !thing.def.ingestible.ingestReportString.NullOrEmpty())
            {
                return string.Format(thing.def.ingestible.ingestReportString, this.pawn.CurJob.targetA.Thing.LabelShort);
            }
            return base.GetReport();
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil chew = Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return TakeMealFromReplimat(TargetIndex.A, this.pawn);
            yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);

            yield return chew;
            yield return new Toil
            {
                initAction = delegate
                    {
                        if (pawn.story.traits.HasTrait(ReplimatDef.SensitiveTaster))
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ReplimatDef.AteReplicatedFood, null);
                        }
                    }
            };
            yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
            yield return Toils_Jump.JumpIf(chew, () => CurJob.GetTarget(TargetIndex.A).Thing is Corpse && pawn.needs.food.CurLevelPercentage < 0.9f);
        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
        {
            IntVec3 cell = base.CurJob.GetTarget(TargetIndex.B).Cell;
            return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, this.pawn);
        }
    }
}
