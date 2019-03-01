using Verse;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Text;
using Verse.AI;

namespace Replimat
{

    public class Building_ReplimatTerminal : Building_NutrientPasteDispenser
    {
        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public FoodPreferability MaxPreferability = FoodPreferability.MealLavish;

        public int ReplicatingTicks = 0;

        public static Pawn MealSearcher = null;

        public ThingDef chickendinner = ThingDef.Named("MealLavish");

        public override ThingDef DispensableDef
        {
            get
            {
                return chickendinner;
            }
        }

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);
            }
        }

        public override Thing FindFeedInAnyHopper()
        {
            return base.FindFeedInAnyHopper();
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(DispensableDef.BaseMass);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public bool HasEnoughFeedstockInHopperForIncident(float stockNeeded)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override Building AdjacentReachableHopper(Pawn reacher)
        {
            List<Building_ReplimatHopper> Hoppers = Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatHopper).Select(x => x as Building_ReplimatHopper).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.HasComputer).ToList();

            if (!Hoppers.NullOrEmpty())
            {
                foreach (var item in Hoppers)
                {
                    if (item != null && reacher.CanReach(item, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        return (Building_Storage)item;
                    }
                }
            }

            return null;
        }

        public override Thing TryDispenseFood()
        {
            if (!this.CanDispenseNow)
            {
                return null;
            }

            if (!HasEnoughFeedstockInHoppers())
            {
                Log.Error("Did not find enough food in hoppers while trying to dispense.");
                return null;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            ThingDef deffff = ThingDefOf.MealFine;

            if (MealSearcher != null)
            {               
                deffff = DefDatabase<ThingDef>.AllDefs.Where(x => x.ingestible != null && x.ingestible.IsMeal)
                    .OrderByDescending(x => x.ingestible.preferability)
                    //.OrderByDescending(x=>x.ingestible.CachedNutrition)
                    .FirstOrDefault(x => MealSearcher.foodRestriction.CurrentFoodRestriction.Allows(x));
            }
            MealSearcher = null;

            Thing dispensedMeal = ThingMaker.MakeThing(deffff, null);

            float dispensedMealMass = dispensedMeal.def.BaseMass;

            powerComp.PowerNet.TryConsumeFeedstock(ReplimatUtility.convertMassToFeedstockVolume(dispensedMealMass));
      
            return dispensedMeal;
        }

        public override void Draw()
        {
            base.Draw();

            if (ReplicatingTicks > 0)
            {
                float alpha;
                float quart = CollectDuration * 0.25f;
                if (ReplicatingTicks < quart)
                {
                    alpha = Mathf.InverseLerp(0, quart, ReplicatingTicks);
                }
                else if (ReplicatingTicks > quart * 3f)
                {
                    alpha = Mathf.InverseLerp(CollectDuration, quart * 3f, ReplicatingTicks);
                }
                else
                {
                    alpha = 1f;
                }

                Graphics.DrawMesh(GraphicsLoader.replimatTerminalGlow.MeshAt(base.Rotation), this.DrawPos + Altitudes.AltIncVect, Quaternion.identity,
                    FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatTerminalGlow.MatAt(base.Rotation, null), alpha), 0);
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
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (ParentHolder != null && !(ParentHolder is Map))
            {
                // If minified, don't show computer and feedstock check Inspector messages
            }
            else
            {
                if (!HasComputer)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotConnectedToComputer".Translate());
                }
                else if (!HasEnoughFeedstockInHoppers())
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotEnoughFeedstock".Translate());
                }
                else
                { }
            }

            return stringBuilder.ToString();
        }
    }
}