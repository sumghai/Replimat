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

    public class Building_ReplimatTerminal : Building_NutrientPasteDispenser
    {
        public CompStateDependentPowerUse stateDependentPowerComp;

        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public FoodPreferability MaxPreferability = FoodPreferability.MealLavish;

        public int ReplicatingTicks = 0;

        public ThingDef CurrentSurvivalMealDef = ThingDefOf.MealSurvivalPack; // Default for batch production is packaged survival meal

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
        }

        // Leave this as a stub
        public override ThingDef DispensableDef
        {
            get
            {
                return ThingDef.Named("MealLavish");
            }
        }

        public bool HasStockFor(ThingDef def)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.ConvertMassToFeedstockVolume(def.BaseMass);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            // Use a default amount for all meals
            float stockNeeded = ReplimatUtility.ConvertMassToFeedstockVolume(DispensableDef.BaseMass);
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
                return null;
            }

            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            Thing dispensedMeal = ThingMaker.MakeThing(meal, null);

            ReplimatUtility.GenerateIngredients(dispensedMeal, eater);

            float dispensedMealMass = dispensedMeal.def.BaseMass;

            powerComp.PowerNet.TryConsumeFeedstock(ReplimatUtility.ConvertMassToFeedstockVolume(dispensedMealMass));

            return dispensedMeal;
        }

        public override void Draw()
        {
            base.Draw();

            if (powerComp.PowerOn && (Rotation == Rot4.North))
            {
                Vector3 replimatTerminalScreenGlowDrawPos = DrawPos;
                replimatTerminalScreenGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

                Graphics.DrawMesh(GraphicsLoader.replimatTerminalScreenGlow.MeshAt(Rotation), replimatTerminalScreenGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatTerminalScreenGlow.MatAt(Rotation, null), 1), 0);
            }

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

                Vector3 replimatTerminalGlowDrawPos = DrawPos;
                replimatTerminalGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

                Graphics.DrawMesh(GraphicsLoader.replimatTerminalGlow.MeshAt(Rotation), replimatTerminalGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatTerminalGlow.MatAt(Rotation, null), alpha), 0);
            }
        }

        public override void Tick()
        {
            base.Tick();

            powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -Math.Max(stateDependentPowerComp.ActiveModePowerConsumption, powerComp.Props.basePowerConsumption);
            }
        }

        public void TryBatchMakingSurvivalMeals()
        {          
            // Determine the maximum number of survival meals that can be replicated, based on available feedstock
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float totalAvailableFeedstockMass = ReplimatUtility.ConvertFeedstockVolumeToMass(totalAvailableFeedstock);
            int maxPossibleSurvivalMeals = (int)Math.Floor(totalAvailableFeedstockMass / CurrentSurvivalMealDef.BaseMass);

            float survivalMealCapVolumeOfFeedstockRequired = ReplimatUtility.ConvertMassToFeedstockVolume(maxPossibleSurvivalMeals * CurrentSurvivalMealDef.BaseMass);

            if (!CanDispenseNow)
            {
                Messages.Message("MessageCannotBatchMakeSurvivalMeals".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!HasEnoughFeedstockInHopperForIncident(survivalMealCapVolumeOfFeedstockRequired))
            {
                return;
            }

            Dialog_BatchMakeSurvivalMeals window = new Dialog_BatchMakeSurvivalMeals(totalAvailableFeedstock, CurrentSurvivalMealDef, delegate (int x, ThingDef thingDef)
            {
                ConfirmAction(x, thingDef, ReplimatUtility.ConvertMassToFeedstockVolume(CurrentSurvivalMealDef.BaseMass));
                CurrentSurvivalMealDef = thingDef;
            }, 1);
            Find.WindowStack.Add(window);
        }

        [MP]
        public void ConfirmAction(int x, ThingDef survivalMealDef, float volumeOfFeedstockRequired)
        {
            ReplicatingTicks = GenTicks.SecondsToTicks(2f);
            def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            powerComp.PowerNet.TryConsumeFeedstock(x * volumeOfFeedstockRequired);
            Thing t = ThingMaker.MakeThing(survivalMealDef, null);
            t.stackCount = x;
            GenPlace.TryPlaceThing(t, InteractionCell, Map, ThingPlaceMode.Near);
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref CurrentSurvivalMealDef, "currentSurvivalMealDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // If no saved survival meal type is defined, or the one defined no longer exists, reset the default type
                if (CurrentSurvivalMealDef == null || !ReplimatUtility.GetSurvivalMealChoices().Contains(CurrentSurvivalMealDef))
                {
                    CurrentSurvivalMealDef = ThingDefOf.MealSurvivalPack;
                }
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
                if (!ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotConnectedToComputer".Translate());
                }
            }

            return stringBuilder.ToString();
        }
    }
}