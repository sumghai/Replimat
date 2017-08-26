using Verse;
using Verse.Sound;
using RimWorld;

namespace Replimat
{
    public class Building_ReplimatTerminal : Building
    {
        public CompPowerTrader powerComp;

        public ThingDef SelectedMeal = ThingDefOf.MealSimple;

        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public int ReplicatingTicks = 0;

        public bool CanDispenseNow
        {
            get
            {
                return this.powerComp.PowerOn; // TODO - Add condition to check if there is enough replicator feedstock
            }
        }

        public ThingDef DispensableDef
        {
            get
            {
                return SelectedMeal;
            }
        }

        public override void Tick()
        {
            base.Tick();

            powerComp.PowerOutput = -125f;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -1500f;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public void Replicate()
        {
            if (!this.CanDispenseNow)
            {
                return;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        public Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }

            Thing thing2 = ThingMaker.MakeThing(DispensableDef, null);
            return thing2;
        }

    }
}

