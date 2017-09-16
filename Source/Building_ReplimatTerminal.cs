using Verse;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Text;

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

        public bool hasEnoughFeedstock;

        public void CheckFeedstockAvailability(float feedstockNeeded, out bool hasReplimatTanks, out bool hasEnoughFeedstock)
        {
            List<Building_ReplimatFeedTank> feedstockTanks = Map.listerThings.ThingsOfDef(ThingDef.Named("ReplimatFeedTank")).OfType<Building_ReplimatFeedTank>().Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet).ToList();

            float totalAvailableFeedstock = 0f;

            if (feedstockTanks.Count() > 0)
            {
                hasReplimatTanks = true;

                foreach (var currentTank in feedstockTanks)
                {
                    totalAvailableFeedstock += currentTank.GetComp<CompFeedstockTank>().StoredFeedstock;
                }

                Log.Message("Replimat: " + totalAvailableFeedstock.ToString() + " feedstock available across " + feedstockTanks.Count().ToString() + " tanks");

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    hasEnoughFeedstock = true;
                }
                else {
                    hasEnoughFeedstock = false;
                }
            }
            else
            {
                hasReplimatTanks = false;
                hasEnoughFeedstock = false;
                Log.Message("Replimat: No feedstock tanks found!");
            }
        }

        public void ConsumeFeedstock(float feedstockNeeded)
        {
            List<Building_ReplimatFeedTank> feedstockTanks = Map.listerThings.ThingsOfDef(ThingDef.Named("ReplimatFeedTank")).OfType<Building_ReplimatFeedTank>().Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet).ToList();

            float totalAvailableFeedstock = 0f;

            if (feedstockTanks.Count() > 0)
            {
                feedstockTanks.Shuffle();

                foreach (var currentTank in feedstockTanks)
                {
                    totalAvailableFeedstock += currentTank.GetComp<CompFeedstockTank>().StoredFeedstock;
                }

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    float feedstockLeftToConsume = feedstockNeeded;

                    foreach (var currentTank in feedstockTanks)
                    {
                        if (feedstockLeftToConsume <= 0f)
                        {
                            break;
                        } else {
                            float num = Math.Min(feedstockLeftToConsume, currentTank.GetComp<CompFeedstockTank>().StoredFeedstock);

                            currentTank.GetComp<CompFeedstockTank>().DrawFeedstock(num);

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

        public bool CanDispenseNow
        {
            get
            {
                CheckFeedstockAvailability(1f, out hasReplimatTanks, out hasEnoughFeedstock);
                return this.powerComp.PowerOn && hasReplimatTanks && hasEnoughFeedstock;
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
            CheckFeedstockAvailability(1f, out hasReplimatTanks, out hasEnoughFeedstock);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (!this.hasReplimatTanks)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Requires connection to Replimat Feedstock Tank");
            }

            if (this.hasReplimatTanks && !this.hasEnoughFeedstock)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Insufficient Feedstock");
            }
            return stringBuilder.ToString();
        }

        public void Replicate()
        {
            if (!this.CanDispenseNow)
            {
                return;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            CheckFeedstockAvailability(1f, out hasReplimatTanks, out hasEnoughFeedstock);
            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        public Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }

            Thing dispensedMeal = ThingMaker.MakeThing(DispensableDef, null);
            float dispensedMealMass = dispensedMeal.def.BaseMass;
            Log.Message("Replimat: " + dispensedMeal.ToString() + " has mass of " + dispensedMealMass.ToString() + "kg (" + ReplimatUtility.convertMassToFeedstockVolume(dispensedMealMass) + "L feedstock required)");
            ConsumeFeedstock(ReplimatUtility.convertMassToFeedstockVolume(dispensedMealMass));
            return dispensedMeal;
        }

    }
}

