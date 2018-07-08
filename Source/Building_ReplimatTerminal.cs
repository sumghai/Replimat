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

    public class Building_ReplimatTerminal : Building_NutrientPasteDispenser, IStoreSettingsParent
    {
        public static int CollectDuration = GenTicks.SecondsToTicks(2f);

        public StorageSettings MealFilter;

        public FoodPreferability MaxPreferability = FoodPreferability.MealLavish;

        public ThingDef SelectedFood = ThingDefOf.MealNutrientPaste;

        public int ReplicatingTicks = 0;

        public override ThingDef DispensableDef => SelectedFood;

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputerDef).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);
            }
        }

        public bool StorageTabVisible => false;

        public StorageSettings GetStoreSettings()
        {
            return this.MealFilter;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return this.def.building.fixedStorageSettings;
        }

        public override void PostMake()
        {
            base.PostMake();
            this.MealFilter = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
                this.MealFilter.CopyFrom(this.def.building.defaultStorageSettings);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.MealFilter = new StorageSettings();
            if (this.def.building.defaultStorageSettings != null)
            {
                this.MealFilter.CopyFrom(this.def.building.defaultStorageSettings);
            }
            ChooseMeal();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look<StorageSettings>(ref MealFilter, "MealFilter", this);
            Scribe_Defs.Look<ThingDef>(ref SelectedFood, "SelectedFood");
        }

        public void ChooseMeal()
        {
            SelectedFood = def.building.fixedStorageSettings.filter.AllowedThingDefs.Where(x => x.ingestible.preferability == MaxPreferability).RandomElement();
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

            Thing dispensedMeal = ThingMaker.MakeThing(DispensableDef, null);
          
            // ADD REPLICATED MATTER INGREDIENT
           // CompIngredients compIngredients = dispensedMeal.TryGetComp<CompIngredients>();
             //   compIngredients.RegisterIngredient(list[i]);
            

            float dispensedMealMass = dispensedMeal.def.BaseMass;
            //DEBUG
            //Log.Message("Replimat: " + dispensedMeal.ToString() + " has mass of " + dispensedMealMass.ToString() + "kg (" + ReplimatUtility.convertMassToFeedstockVolume(dispensedMealMass) + "L feedstock required)");
            powerComp.PowerNet.TryConsumeFeedstock(ReplimatUtility.convertMassToFeedstockVolume(dispensedMealMass));

            ChooseMeal();

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

            if (!HasComputer)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Requires connection to Replimat Computer");
            }
            else if (!HasEnoughFeedstockInHoppers())
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Insufficient Feedstock");
            }
            else
            { }

            return stringBuilder.ToString();
        }
    }
}