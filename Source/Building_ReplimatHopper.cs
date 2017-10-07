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

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.FeedTankDef).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet).ToList();

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

        public override void Tick()
        {
            if (!this.IsHashIntervalTick(60))
            {
                return;
            }
            if (this.powerComp == null || !this.powerComp.PowerOn)
            {
                return;
            }

            List<Building_ReplimatFeedTank> feedstockTanks = GetTanks;

            if (feedstockTanks.Count > 0)
            {

                foreach (var currentTank in feedstockTanks)
                {
                    if (currentTank.AmountCanAccept > 0)
                    {
                        currentTank.AddFeedstock(0.01f);
                    }
                }
            }
        }
    }
}
