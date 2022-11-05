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

        private readonly float rawSewageDensity = 0.72f; // 0.72 kg/L

        private readonly float sewageSludgeSolidsPct = 0.15f; // 15%

        private readonly float minSewageVolumeForProcessing = 0.5f; // 0.5 L

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

            if (this.IsHashIntervalTick(30))
            {               
                float sewageAvailable = ModCompatibility.DbhGetAvailableSewage((DubsBadHygiene.CompPipe)pipeComp);

                float repFeedstockTanksFreeSpace = 0;

                float recoveredSludgeMass = rawSewageDensity * sewageSludgeSolidsPct * minSewageVolumeForProcessing;
                float recoveredSludgeToFeedstockVol = ReplimatUtility.ConvertMassToFeedstockVolume(recoveredSludgeMass);

                // If we have at least a minimum amount of sewage available
                if (sewageAvailable > minSewageVolumeForProcessing)
                {                    
                    List<Building_ReplimatFeedTank> repFeedstockTanks = GetTanks;
                    repFeedstockTanksFreeSpace = repFeedstockTanks.Sum(x => x.AmountCanAccept);

                    List<DubsBadHygiene.CompWaterStorage> dbhWaterStorage = ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.WaterTowers;

                    // If we have enough free volume in the feedstock tanks
                    if (repFeedstockTanksFreeSpace > recoveredSludgeToFeedstockVol)
                    {                      
                        // Withdraw a set amount of sewage from all sewage tanks
                        ModCompatibility.DbhConsumeSewage((DubsBadHygiene.CompPipe)pipeComp, minSewageVolumeForProcessing);

                        // Add the (converted) sewage solids fraction to feedstock tanks
                        float buffer = recoveredSludgeToFeedstockVol;

                        foreach (var tank in repFeedstockTanks.InRandomOrder())
                        {
                            if (buffer > 0f)
                            {
                                float sent = Mathf.Min(buffer, tank.AmountCanAccept);
                                buffer -= sent;
                                tank.AddFeedstock(sent);
                            }
                        }

                        // Add the remaining liquid fraction to all water tanks
                        if (!dbhWaterStorage.NullOrEmpty())
                        {
                            ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.PushWater((1 - sewageSludgeSolidsPct) * minSewageVolumeForProcessing);
                        }
                    }
                }

                running = sewageAvailable > minSewageVolumeForProcessing && repFeedstockTanksFreeSpace > recoveredSludgeToFeedstockVol;
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
