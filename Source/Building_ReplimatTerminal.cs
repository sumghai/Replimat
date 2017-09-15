using Verse;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Replimat
{
    public class Building_ReplimatTerminal : Building
    {
        public CompPowerTrader powerComp;

        public static List<ThingDef> allMeals = ThingCategoryDefOf.FoodMeals.DescendantThingDefs.ToList();

        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public int ReplicatingTicks = 0;

        static System.Random rnd = new System.Random();

        public bool hasReplimatTanks;

        public bool ConsumeFeedstock(float feedstockNeeded, bool consumeForReal)
        {
            List<Building_ReplimatFeedTank> feedstockTanks = powerComp.PowerNet.powerComps.Where(x => x.parent is Building_ReplimatFeedTank).Select(x => x.parent as Building_ReplimatFeedTank).ToList();

            float totalAvailableFeedstock = 0f;

            if (feedstockTanks.Count() > 0)
            {
                hasReplimatTanks = true;
                
                feedstockTanks.Shuffle();

                foreach (var currentTank in feedstockTanks)
                {
                    totalAvailableFeedstock += currentTank.GetComp<CompFeedstockTank>().StoredFeedstock;
                }

                Log.Message("Replimat - " + totalAvailableFeedstock.ToString() + " feedstock available across " + feedstockTanks.Count().ToString() + " tanks");

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    float feedstockLeftToConsume = feedstockNeeded;

                    foreach (var currentTank in feedstockTanks)
                    {
                        float num = Math.Min(feedstockLeftToConsume, currentTank.GetComp<CompFeedstockTank>().StoredFeedstock);

                        if (consumeForReal == true)
                        {
                            currentTank.GetComp<CompFeedstockTank>().DrawFeedstock(num);
                        }

                        feedstockLeftToConsume -= num;
                    }

                    if (feedstockLeftToConsume <= 0f)
                    {
                        return true;
                    }
                }
            }
            else
            {
                hasReplimatTanks = false;
            }

            return false;
        }

        public bool CanDispenseNow
        {
            get
            {
                return this.powerComp.PowerOn && ConsumeFeedstock(1,false);
            }
        }

        public ThingDef DispensableDef
        {
            get
            {
                int r = rnd.Next(allMeals.Count);
                return ThingDef.Named(allMeals[r].defName);
            }
        }

        public override void Tick()
        {
            base.Tick();

            powerComp.PowerOutput = -125f;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -1500f;
                Graphics.DrawMesh(GraphicsLoader.replimatTerminalGlow.MeshAt(base.Rotation), this.DrawPos + Altitudes.AltIncVect, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatTerminalGlow.MatAt(base.Rotation, null), 1), 0);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public void Replicate()
        {
            if (!this.CanDispenseNow)
            {
                return;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            ConsumeFeedstock(1, true);
            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        public Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }

            Thing dispensedMeal = ThingMaker.MakeThing(DispensableDef, null);

            return dispensedMeal;
        }

    }
}

