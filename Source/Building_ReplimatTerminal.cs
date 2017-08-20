using Verse;
using Verse.Sound;
using RimWorld;

namespace Replimat
{
    public class Building_ReplimatTerminal : Building
    {
        public CompPowerTrader powerComp;

        public static int CollectDuration = 50;

        public bool CanDispenseNow
        {
            get
            {
                return this.powerComp.PowerOn; // TODO - Add condition to check if there is enough replicator feedstock
            }
        }

        public virtual ThingDef DispensableDef
        {
            get
            {
                return ThingDefOf.MealFine;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public virtual Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }
            
            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            Thing thing2 = ThingMaker.MakeThing(ThingDefOf.MealFine, null);
            return thing2;
        }
    }
}

