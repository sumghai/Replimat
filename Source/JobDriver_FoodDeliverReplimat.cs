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
            if (base.CurJob.GetTarget(TargetIndex.A).Thing is Replimat.Building_ReplimatTerminal)
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
            JobDriver_FoodDeliverReplimat.< MakeNewToils > c__Iterator4B < MakeNewToils > c__Iterator4B = new JobDriver_FoodDeliver.< MakeNewToils > c__Iterator4B();

            < MakeNewToils > c__Iterator4B.<> f__this = this;
            JobDriver_FoodDeliverReplimat.< MakeNewToils > c__Iterator4B expr_0E = < MakeNewToils > c__Iterator4B;
            expr_0E.$PC = -2;
            return expr_0E;
        }
    }
}
