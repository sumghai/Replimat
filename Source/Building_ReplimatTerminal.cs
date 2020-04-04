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

    public class Building_ReplimatTerminal : Building_NutrientPasteDispenser
    {
        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public FoodPreferability MaxPreferability = FoodPreferability.MealLavish;

        public int ReplicatingTicks = 0;

        // Leave this as a stub
        public override ThingDef DispensableDef
        {
            get
            {
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

        public bool HasStockFor(ThingDef def)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(def.BaseMass);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            // Use a default amount for all meals
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
            return null;
        }

        public Thing TryDispenseFood(Pawn eater, Pawn getter)
        {
            if (getter == null)
            {
                getter = eater;
            }
            if (!CanDispenseNow)
            {
                return null;
            }

            ThingDef meal = ReplimatUtility.PickMeal(eater, getter);
            if (meal == null)
            {
                return null;
            }

            if (!HasStockFor(meal))
            {
                //Log.Error("[Replimat] " + "Did not find enough foodstock in tanks while trying to replicate.");
                return null;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            Thing dispensedMeal = ThingMaker.MakeThing(meal, null);

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

        public void TryBatchMakingSurvivalMeals()
        {
            //Log.Message("[Replimat] " + "Requesting survival meals!");

            // Determine the maximum number of survival meals that can be replicated, based on available feedstock
            // (Cap this at 30 meals so that players don't accidentally use up all their feedstock on survival meals)
            ThingDef survivalMeal = ThingDefOf.MealSurvivalPack;
            int maxSurvivalMeals = 30;
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float totalAvailableFeedstockMass = ReplimatUtility.convertFeedstockVolumeToMass(totalAvailableFeedstock);
            int maxPossibleSurvivalMeals = (int)Math.Floor(totalAvailableFeedstockMass / survivalMeal.BaseMass);
            int survivalMealCap = (maxPossibleSurvivalMeals < maxSurvivalMeals) ? maxPossibleSurvivalMeals : maxSurvivalMeals;

            /*Log.Message("[Replimat] " + "Default max survival meals is " + maxSurvivalMeals + "\n"
                + "Total available feedstock of " + totalAvailableFeedstock + " can provide up to " + maxPossibleSurvivalMeals + " survival meals\n"
                + "Final cap on survival meals is " + survivalMealCap);*/

            float volumeOfFeedstockRequired = ReplimatUtility.convertMassToFeedstockVolume(survivalMealCap * survivalMeal.BaseMass);

            if (!CanDispenseNow)
            {
                Messages.Message("MessageCannotBatchMakeSurvivalMeals".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockRequired))
            {
                //Log.Error("[Replimat] " + "Not enough feedstock to make required survival meals.");
                return;
            }

            Func<int, string> textGetter;
            textGetter = ((int x) => "SetSurvivalMealBatchSize".Translate(x, survivalMealCap));

            Dialog_Slider window = new Dialog_Slider(textGetter, 1, survivalMealCap, delegate (int x)
            {
                ConfirmAction(x, volumeOfFeedstockRequired);
            }, 1);
            Find.WindowStack.Add(window);
        }

        [MP]
        public void ConfirmAction(int x, float volumeOfFeedstockRequired)
        {
            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            this.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockRequired);
            Thing t = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack, null);
            t.stackCount = x;
            GenPlace.TryPlaceThing(t, this.InteractionCell, base.Map, ThingPlaceMode.Near);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }

            yield return new Command_Action
            {
                defaultLabel = "BatchMakeSurvivalMeals".Translate(),
                defaultDesc = "BatchMakeSurvivalMeals_Desc".Translate(),
                action = delegate
                {
                    TryBatchMakingSurvivalMeals();
                },
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/BatchMakeSurvivalMeals", true)
            };
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
            }

            return stringBuilder.ToString();
        }
    }
}