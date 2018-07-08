using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Replimat
{
    class Building_ReplimatHopper : Building_Storage
    {
        public float MaxPerTransfer = 10f;

        public CompPowerTrader powerComp;

        public int DematerializingTicks = 0;

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.FeedTankDef).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.HasComputer).ToList();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputerDef).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

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

        public override void Tick()
        {
            base.Tick();


            if (this.IsHashIntervalTick(60))
            {
                List<Building_ReplimatFeedTank> tanks = GetTanks;
                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);

                Thing food = list.FirstOrDefault(x => x.def.IsNutritionGivingIngestible);

                powerComp.PowerOutput = -175f;

                if (food == null)
                {
                    return;
                }

                powerComp.PowerOutput = -1000f;

                MaxPerTransfer = FoodUtility.StackCountForNutrition(food.def.ingestible.CachedNutrition, 1f); ;

                float remainingVolumeAvailableInTanks = tanks.Sum(x => x.AmountCanAccept);

                float totalStackLiquidVolume = food.stackCount * ReplimatUtility.convertMassToFeedstockVolume(food.def.BaseMass);

                float maxStackLiquidVolume = MaxPerTransfer * ReplimatUtility.convertMassToFeedstockVolume(food.def.BaseMass);

                float transferBufferLiquidVolume = Mathf.Min(totalStackLiquidVolume, remainingVolumeAvailableInTanks, maxStackLiquidVolume);

                food.stackCount = food.stackCount - (int) (transferBufferLiquidVolume / ReplimatUtility.convertMassToFeedstockVolume(food.def.BaseMass));

                if (food.stackCount == 0)
                {
                    food.Destroy();
                }

                foreach (var tank in tanks.InRandomOrder())
                {
                    if (transferBufferLiquidVolume > 0f)
                    {
                        float buffer = Mathf.Min(transferBufferLiquidVolume, tank.AmountCanAccept);
                        transferBufferLiquidVolume -= buffer;
                        tank.AddFeedstock(buffer);
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
