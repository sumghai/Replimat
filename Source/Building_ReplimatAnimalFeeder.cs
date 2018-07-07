﻿using System;
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

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.FeedTankDef).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.HasComputer).ToList();

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
            float totalAvailableFeedstock = GetTanks.Sum(x => x.storedFeedstock);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public void ConsumeFeedstock(float feedstockNeeded)
        {
            List<Building_ReplimatFeedTank> feedstockTanks = GetTanks;

            float totalAvailableFeedstock = feedstockTanks.Sum(x => x.storedFeedstock);

            if (feedstockTanks.Count() > 0)
            {
                feedstockTanks.Shuffle();

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    float feedstockLeftToConsume = feedstockNeeded;

                    foreach (var currentTank in feedstockTanks)
                    {
                        if (feedstockLeftToConsume <= 0f)
                        {
                            break;
                        }
                        else
                        {
                            float num = Math.Min(feedstockLeftToConsume, currentTank.StoredFeedstock);

                            currentTank.DrawFeedstock(num);

                            feedstockLeftToConsume -= num;
                        }
                    }
                }

            }
            else
            {
                Log.Error("Replimat: Tried to draw feedstock from non-existent tanks!");
            }
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
            float totalAvailableFeedstock = GetTanks.Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(1f);
            return totalAvailableFeedstock >= stockNeeded;
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
