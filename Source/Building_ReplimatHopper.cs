using System.Collections.Generic;
using System.Linq;
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

            if (powerComp.PowerOn)
            {
                string screenGlowFxGraphicPath = null;

                if (Rotation == Rot4.North)
                {
                    screenGlowFxGraphicPath = "FX/replimatHopperScreenGlow_north";
                }
                if (Rotation == Rot4.East)
                {
                    screenGlowFxGraphicPath = "FX/replimatHopperScreenGlow_east";
                }
                if (Rotation == Rot4.South)
                {
                    screenGlowFxGraphicPath = "FX/replimatHopperScreenGlow_south";
                }
                if (Rotation == Rot4.West)
                {
                    screenGlowFxGraphicPath = "FX/replimatHopperScreenGlow_west";
                }

                Graphic screenGlow = GraphicDatabase.Get<Graphic_Single>(screenGlowFxGraphicPath, ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);
                Mesh screenGlowMesh = screenGlow.MeshAt(Rotation);
                Vector3 screenGlowDrawPos = DrawPos;
                screenGlowDrawPos.y = AltitudeLayer.ItemImportant.AltitudeFor() + 0.03f;

                Graphics.DrawMesh(screenGlowMesh, screenGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(screenGlow.MatAt(Rotation, null), 1), 0);
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
