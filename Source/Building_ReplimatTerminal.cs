using Verse;
using Verse.Sound;
using RimWorld;

namespace Replimat
{
    public class Building_ReplimatTerminal : Building_NutrientPasteDispenser
    {
        public CompPowerTrader powerComp;

        public ThingDef SelectedMeal = ThingDefOf.MealFine;

        public static int CollectDuration = 50;

        public new bool CanDispenseNow
        {
            get
            {
                return this.powerComp.PowerOn; // TODO - Add condition to check if there is enough replicator feedstock
            }
        }

        public override ThingDef DispensableDef
        {
            get
            {
                return SelectedMeal;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public override Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }

            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            Thing thing2 = ThingMaker.MakeThing(DispensableDef, null);
            return thing2;
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            return true;
        }
    }
}

