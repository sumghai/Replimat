using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class Building_ReplimatSewageRecycler : Building
    {
        public CompPowerTrader powerComp;

        public CompStateDependentPowerUse stateDependentPowerComp;

        public ThingComp pipeComp;

        public List<Building_ReplimatFeedTank> GetTanks => ReplimatUtility.GetTanks(powerComp.PowerNet);

        public bool running;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
            pipeComp = GetComps<DubsBadHygiene.CompPipe>().FirstOrDefault();
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            powerComp.PowerOutput = running ? -Math.Max(stateDependentPowerComp.ActiveModePowerConsumption, powerComp.Props.basePowerConsumption) : -powerComp.Props.basePowerConsumption;

            if (this.IsHashIntervalTick(15))
            {               
                float sewageAvailable = ModCompatibility.DbhGetAvailableSewage((DubsBadHygiene.CompPipe)pipeComp);

                float repFeedstockTanksFreeSpace = 0;

                // If we have at least 1 litre of sewage available
                if (sewageAvailable > 1)
                {                    
                    List<Building_ReplimatFeedTank> repFeedstockTanks = GetTanks;
                    repFeedstockTanksFreeSpace = repFeedstockTanks.Sum(x => x.AmountCanAccept);

                    List<DubsBadHygiene.CompWaterStorage> dbhWaterStorage = ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.WaterTowers;

                    // If we have at least 150 mL of volume available in the feedstock tanks
                    if (repFeedstockTanksFreeSpace > 0.15)
                    {
                        // Withdraw a total of 1 litre of sewage from all sewage tanks
                        ModCompatibility.DbhConsumeSewage((DubsBadHygiene.CompPipe)pipeComp, 1);

                        // Add 150 mL of feedstock to feedstock tanks
                        // TODO - do proper density/volume conversions
                        foreach (var tank in repFeedstockTanks.InRandomOrder())
                        {
                            float sent = Mathf.Min(0.15f, tank.AmountCanAccept);
                            tank.AddFeedstock(sent);
                        }

                        // Add 850 mL of water to all water tanks
                        if (!dbhWaterStorage.NullOrEmpty())
                        {
                            ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.PushWater(0.85f);
                        }
                    }
                }

                running = sewageAvailable > 1 && repFeedstockTanksFreeSpace > 0.15;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if ((ParentHolder == null || ParentHolder is Map) && !ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("NotConnectedToComputer".Translate());
            }

            if (((DubsBadHygiene.CompPipe)pipeComp).pipeNet.Sewers.NullOrEmpty())
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Nosewage".Translate());
            }

            else
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(running ? "SewageRecyclerRunning".Translate() : "SewageRecyclerIdle".Translate());
            }

            return stringBuilder.ToString();
        }
    }
}
