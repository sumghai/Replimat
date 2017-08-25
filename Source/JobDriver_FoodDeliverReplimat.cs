using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class JobDriver_FoodDeliverReplimat : JobDriver
    {
        private const TargetIndex FoodSourceInd = TargetIndex.A;

        private const TargetIndex DelivereeInd = TargetIndex.B;

        private bool usingReplimatTerminal;

        private bool eatingFromInventory;

        private Pawn Deliveree
        {
            get
            {
                return (Pawn)base.CurJob.targetB.Thing;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.usingReplimatTerminal, "usingReplimatTerminal", false, false);
            Scribe_Values.Look<bool>(ref this.eatingFromInventory, "eatingFromInventory", false, false);
        }

        public override string GetReport()
        {
            if (base.CurJob.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal)
            {
                return base.CurJob.def.reportString.Replace("TargetA", ThingDefOf.MealFine.label).Replace("TargetB", ((Pawn)((Thing)base.CurJob.targetB)).LabelShort);
            }
            return base.GetReport();
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.usingReplimatTerminal = (base.TargetThingA is Building_ReplimatTerminal);
            this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA));
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            if (this.eatingFromInventory)
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
            }
            else if (this.usingReplimatTerminal)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
                yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
            }
            else
            {
                yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
                yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.Deliveree);
            }
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = this.pawn;
                Job curJob = actor.jobs.curJob;
                actor.pather.StartPath(curJob.targetC, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            toil.AddFailCondition(delegate
            {
                Pawn pawn = (Pawn)this.pawn.jobs.curJob.targetB.Thing;
                return !pawn.IsPrisonerOfColony || !pawn.guest.CanBeBroughtFood;
            });
            yield return toil;
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing;
                    this.pawn.carryTracker.TryDropCarriedThing(this.pawn.jobs.curJob.targetC.Cell, ThingPlaceMode.Direct, out thing, null);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
