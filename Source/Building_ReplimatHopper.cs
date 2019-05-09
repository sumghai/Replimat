using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace Replimat
{
    class Building_ReplimatHopper : Building_Storage
    {
        public float MaxPerTransfer = 10f;

        public CompPowerTrader powerComp;

        public int DematerializingTicks = 0;

        public static int DematerializeDuration = GenTicks.SecondsToTicks(2f);

        public int dematerializingCycleInt;

        private Sustainer wickSustainer;

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatFeedTank).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.HasComputer).ToList();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

            }
        }

        public float freezerTemp
        {
            get
            {
                if (powerComp.PowerOn)
                {
                    return -2f;
                }
                return this.AmbientTemperature;
            }
        }

        public void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            this.wickSustainer = this.def.building.soundDispense.TrySpawnSustainer(info);
        }

        public override void Draw()
        {
            base.Draw();


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

            Graphics.DrawMesh(GraphicsLoader.replimatHopperGlow[dematerializingCycleInt].MeshAt(base.Rotation), this.DrawPos + new Vector3(0f, (int)AltitudeLayer.MoteOverhead
                * Altitudes.AltInc, 0f), Quaternion.identity,
                    FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatHopperGlow[dematerializingCycleInt].MatAt(base.Rotation, null), alpha), 0);
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            powerComp.PowerOutput = -125f;

            if (this.IsHashIntervalTick(60))
            {
                List<Building_ReplimatFeedTank> tanks = GetTanks;
                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);



                Thing food = list.FirstOrDefault(x => settings.AllowedToAccept(x));

                if (food == null)
                {
                    return;
                }

                float stockvol = ReplimatUtility.convertMassToFeedstockVolume(food.def.BaseMass);
                float FreeSpace = tanks.Sum(x => x.AmountCanAccept);

                if (FreeSpace >= stockvol)
                {
                    float buffy = stockvol;

                    food.stackCount = food.stackCount - 1;

                    if (food.stackCount == 0)
                    {
                        food.Destroy();
                    }

                    DematerializingTicks = GenTicks.SecondsToTicks(2f);

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

            if (DematerializingTicks > 0)
            {
                DematerializingTicks--;
                powerComp.PowerOutput = -1000f;

                if (this.wickSustainer == null)
                {
                    this.StartWickSustainer();
                }
                else if (this.wickSustainer.Ended)
                {
                    this.StartWickSustainer();
                }
                else
                {
                    this.wickSustainer.Maintain();
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
    }
}
