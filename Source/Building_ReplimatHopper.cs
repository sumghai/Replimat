using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Replimat
{
    class Building_ReplimatHopper : Building_Storage
    {
        public float MaxPerTransfer = 10f;

        public CompPowerTrader powerComp;

        public CompStateDependentPowerUse stateDependentPowerComp;

        public int DematerializingTicks = 0;

        public static int DematerializeDuration = GenTicks.SecondsToTicks(2f);

        public int dematerializingCycleInt;

        private Sustainer wickSustainer;

        public List<Building_ReplimatFeedTank> GetTanks => ReplimatUtility.GetTanks(powerComp.PowerNet);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
        }

        public void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            wickSustainer = def.building.soundDispense.TrySpawnSustainer(info);
        }

        public override void Draw()
        {
            base.Draw();

            if (powerComp.PowerOn)
            {
                Vector3 replimatHopperScreenGlowDrawPos = DrawPos;
                replimatHopperScreenGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

                Graphics.DrawMesh(GraphicsLoader.replimatHopperScreenGlow.MeshAt(Rotation), replimatHopperScreenGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatHopperScreenGlow.MatAt(Rotation, null), 1), 0);
            }

            float alpha;
            float quart = DematerializeDuration * 0.25f;
            if (DematerializingTicks < quart)
            {
                alpha = Mathf.InverseLerp(0, quart, DematerializingTicks);
            }
            else if (DematerializingTicks > quart * 3f)
            {
                alpha = Mathf.InverseLerp(DematerializeDuration, quart * 3f, DematerializingTicks);
            }
            else
            {
                alpha = 1f;
            }

            Vector3 replimatHopperGlowDrawPos = DrawPos;
            replimatHopperGlowDrawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor() + 0.03f;

            Graphics.DrawMesh(GraphicsLoader.replimatHopperGlow[dematerializingCycleInt].MeshAt(Rotation), replimatHopperGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatHopperGlow[dematerializingCycleInt].MatAt(Rotation, null), alpha), 0);
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;

            if (this.IsHashIntervalTick(15)) {

                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);
                Thing food = list.FirstOrDefault(x => settings.AllowedToAccept(x));

                if (food != null)
                {
                    List<Building_ReplimatFeedTank> tanks = GetTanks;

                    float stockvol = ReplimatUtility.ConvertMassToFeedstockVolume(food.def.BaseMass);
                    float FreeSpace = tanks.Sum(x => x.AmountCanAccept);

                    if (this.IsHashIntervalTick(60) && FreeSpace >= stockvol)
                    {
                        DematerializingTicks = GenTicks.SecondsToTicks(2f);
                    }

                    if (this.IsHashIntervalTick(5) && FreeSpace >= stockvol)
                    {
                        float buffy = stockvol;

                        food.stackCount--;

                        if (food.stackCount == 0)
                        {
                            food.Destroy();
                        }

                        foreach (var tank in tanks.InRandomOrder())
                        {
                            if (buffy > 0f)
                            {
                                float sent = Mathf.Min(buffy, tank.AmountCanAccept);
                                buffy -= sent;
                                tank.AddFeedstock(sent);
                            }
                        }
                    }
                }

            }

            if (DematerializingTicks > 0)
            {
                DematerializingTicks--;
                powerComp.PowerOutput = -Math.Max(stateDependentPowerComp.ActiveModePowerConsumption, powerComp.Props.basePowerConsumption);

                if (wickSustainer == null)
                {
                    StartWickSustainer();
                }
                else if (wickSustainer.Ended)
                {
                    StartWickSustainer();
                }
                else
                {
                    wickSustainer.Maintain();
                }

                if (this.IsHashIntervalTick(5))
                {
                    dematerializingCycleInt++;
                    if (dematerializingCycleInt > 2)
                    {
                        dematerializingCycleInt = 0;
                    }
                }

            }
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return Position;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if ((ParentHolder == null || ParentHolder is Map) && !ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(Translator.Translate("NotConnectedToComputer"));
            }
            return stringBuilder.ToString();
        }
    }
}