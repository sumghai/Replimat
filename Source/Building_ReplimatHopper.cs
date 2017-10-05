using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Replimat
{
    class Building_ReplimatHopper : Building_Storage
    {
        private List<IntVec3> cachedOccupiedCells;

        public CompPowerTrader powerComp;

        public float freezerTemp
        {
            get
            {
                return (this.powerComp != null && this.powerComp.PowerOn) ? -2f : base.Position.GetTemperature(base.Map);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.cachedOccupiedCells = this.AllSlotCells().ToList<IntVec3>();
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return this.Position;
        }
    }
}
