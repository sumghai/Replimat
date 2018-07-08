using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Replimat
{
    class Building_ReplimatAnimalFeeder : Building_Storage
    {
        public CompPowerTrader powerComp;

        public int ReplicatingTicks = 0;

        public float volumePerKibble = ReplimatUtility.convertMassToFeedstockVolume(ThingDefOf.Kibble.BaseMass);

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputerDef).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return Position;
        }

        public bool HasEnoughFeedstockInHopperForIncident(float stockNeeded)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            string copyStr = "CommandCopyZoneSettingsLabel".Translate();
            string pasteStr = "CommandPasteZoneSettingsLabel".Translate();
            foreach (Gizmo g in base.GetGizmos())
            {
                if (g is Command_Action act && (act.defaultLabel == copyStr || act.defaultLabel == pasteStr))
                {
                    continue;
                }
                yield return g;
            }
        }

        public bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(1f);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override void Tick()
        {
            base.Tick();

            if (this.IsHashIntervalTick(60))
            {
                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);
                Thing foodInFeeder = list.FirstOrDefault(x => x.def.IsNutritionGivingIngestible);

                if (foodInFeeder == null)
                {
                    Log.Message(this.ThingID.ToString() + " is empty");

                    int maxKib = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerKibble);
                    maxKib = Mathf.Min(maxKib, 75);

                    if (maxKib > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerKibble * maxKib))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);

                        Thing t = ThingMaker.MakeThing(ThingDefOf.Kibble, null);
                        t.stackCount = maxKib;
                        GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Direct);
                    }

                }
                else if (foodInFeeder.def == ThingDefOf.Kibble && foodInFeeder.stackCount < 20)
                {
                    Log.Message(this.ThingID.ToString() + " currently has " + foodInFeeder.stackCount.ToString() + " units of " + foodInFeeder.def.label.ToString());

                    int refill = Mathf.Min(foodInFeeder.def.stackLimit - foodInFeeder.stackCount, 75);
                    int maxKib = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerKibble);
                    maxKib = Mathf.Min(maxKib, refill);

                    if (maxKib > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerKibble * maxKib))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);

                        foodInFeeder.stackCount += maxKib;
                    }
                }
            }

            powerComp.PowerOutput = -125f;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -1500f;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (!HasComputer)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Requires connection to Replimat Computer");
            }
            else if (!HasEnoughFeedstockInHoppers())
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Insufficient Feedstock");
            }
            else
            { }

            return stringBuilder.ToString();
        }
    }
}
