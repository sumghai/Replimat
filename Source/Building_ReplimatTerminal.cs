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

        public bool HasStockFor(ThingDef def)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(def.BaseMass);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public ThingDef PickMeal(Pawn eater)
        {
            // Should we default to Fine Meals if the pawn cannot be identified or if a food restriction policy can't be found?
            ThingDef SelectedMeal = ThingDef.Named("MealFine");

            if (eater != null)
            {
                //wihtout IsMeal they try to eat stuff like chocolate and corpses, joy based consumption will need a lot more custom code
                List<ThingDef> allowedMeals = eater.foodRestriction.CurrentFoodRestriction.filter.AllowedThingDefs.Where(x => x.ingestible.IsMeal).ToList();

                //If a pawn's food restriction has other, better items available, then remove Nutrient Paste Meals from the available meal options
                if (allowedMeals.Count() > 1)
                {
                    allowedMeals.Remove(ThingDef.Named("MealNutrientPaste"));
                }

                if (ReplimatMod.Settings.PrioritizeFoodQuality)
                {
                    Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " will prioritize meal quality");
                    var maxpref = allowedMeals.Max(x => x.ingestible.preferability);
                    SelectedMeal = allowedMeals.Where(x => x.ingestible.preferability == maxpref).RandomElement();
                }
                else
                {
                    Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " can choose random meals regardless of quality");
                    SelectedMeal = allowedMeals.RandomElement();
                }

                // Debug Messages
                Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " is allowed the following meals: \n" 
                    + string.Join(", ", allowedMeals.Select(def => def.defName).ToArray()));
            }

            return SelectedMeal;
        }


        // leave this as a stub
        public override ThingDef DispensableDef
        {
            get
            {
                // Log.Warning("checked for def");
                return ThingDef.Named("MealLavish");
            }
        }

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == PowerComp.PowerNet && x.Working);
            }
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            // USE A DEFAULT AMOUNT FOR NOW FOR ALL MEALS
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
            // DONT DO THIS ELSE THE PAWN THINKS THEY CAN REFILL THE HOPPERS
            //List<Building_ReplimatHopper> Hoppers = Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatHopper).OfType<Building_ReplimatHopper>().Where(x => x.PowerComp.PowerNet == PowerComp.PowerNet && x.HasComputer).ToList();

            //if (!Hoppers.NullOrEmpty())
            //{
            //    foreach (var item in Hoppers)
            //    {
            //        if (item != null && reacher.CanReach(item, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            //        {
            //            return item;
            //        }
            //    }
            //}

            return null;
        }

        public Thing TryDispenseFood(Pawn eater)
        {
            if (!CanDispenseNow)
            {
                return null;
            }

            if (!HasEnoughFeedstockInHoppers())
            {
                Log.Error("Did not find enough food in hoppers while trying to dispense.");
                return null;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            Thing dispensedMeal = ThingMaker.MakeThing(PickMeal(eater), null);

            // STACK CALC, DONT BOTHER
          //  int num = FoodUtility.WillIngestStackCountOf(eater, dispensedMeal.def, dispensedMeal.GetStatValue(StatDefOf.Nutrition, true));
           // dispensedMeal.stackCount = num;

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
                //CANT DO ANYMORE
                //else if (!HasEnoughFeedstockInHoppers())
                //{
                //    stringBuilder.AppendLine();
                //    stringBuilder.Append("NotEnoughFeedstock".Translate());
                //}
                //else
                //{ }
            }

            return stringBuilder.ToString();
        }
    }
}