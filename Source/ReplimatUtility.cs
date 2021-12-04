using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replimat
{

    [StaticConstructorOnStartup]
    public static class ReplimatUtility
    {
        private const float nutrientFeedStockDensity = 1.07f; // 1.07 kg/L (equivalent to Abbott's Ensure Nutritional Shake)

        public static float ConvertMassToFeedstockVolume(float mass) => mass / nutrientFeedStockDensity;

        public static float ConvertFeedstockVolumeToMass(float volume) => volume * nutrientFeedStockDensity;

        public static CompProperties_ReplimatRestrictions replimatRestrictions = ReplimatDef.ReplimatComputer.GetCompProperties<CompProperties_ReplimatRestrictions>();

        public static bool CanFindComputer(Building building)
        {
            var computer = building.Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == building.PowerComp.PowerNet && x.Working);
            return computer;
        }

        public static List<Building_ReplimatFeedTank> GetTanks(this PowerNet net)
        {
            var tanks = net.Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatFeedTank).OfType<Building_ReplimatFeedTank>().Where(x => x.PowerComp.PowerNet == net && CanFindComputer(x)).ToList();
            return tanks;
        }

        public static bool RepMatWillEat(Pawn p, ThingDef food, Pawn getter = null)
        {
            if (!p.IsPrisoner)
            {
                return p.WillEat(food, getter);
            }

            if (!p.RaceProps.CanEverEat(food))
            {
                return false;
            }
            if (p.foodRestriction != null)
            {
                if (!p.foodRestriction.Configurable)
                {
                    return true;
                }
                if (p.foodRestriction.pawn.InMentalState)
                {
                    return true;
                }
                var res = p.foodRestriction.CurrentFoodRestriction;

                if (res != null && !res.Allows(food) && (food.IsWithinCategory(ThingCategoryDefOf.Foods) || food.IsWithinCategory(ThingCategoryDefOf.Corpses)))
                {
                    return false;
                }
            }
            return true;
        }

        public static ThingDef PickMeal(Pawn eater, Pawn getter)
        {
            if (getter == null)
            {
                getter = eater;
            }
            // Default to null
            ThingDef SelectedMeal = null;

            if (eater != null)
            {
                // Compile list of allowed meals for current pawn, limited to at least 40% nutrition with preferability above awful
                // This eliminates stuff like chocolate, nutrient paste meals and corpses
                // Joy-based consumption will require more patches, and is outside the scope of this mod
                List<ThingDef> allowedMeals = ThingCategoryDefOf.Foods.DescendantThingDefs.Where(x => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0.4f && x.ingestible.preferability > FoodPreferability.MealAwful && RepMatWillEat(eater, x, getter)).ToList();

                // Manually remove Packaged Survival Meals, as pawns should only be getting "fresh" food to meet their immediate food needs
                // (Survival Meals are reserved for caravans, as per custom gizmo)
                allowedMeals.Remove(ThingDefOf.MealSurvivalPack);

                // Remove meals from a blacklist (stored in the Replimat Computer)
                allowedMeals.RemoveAll((ThingDef d) => replimatRestrictions.disallowedMeals.Contains(d));

                if (allowedMeals.NullOrEmpty())
                {
                    return null;
                }

                if (ReplimatMod.Settings.PrioritizeFoodQuality)
                {
                    var maxpref = allowedMeals.Max(x => x.ingestible.preferability);
                    SelectedMeal = allowedMeals.Where(x => x.ingestible.preferability == maxpref).RandomElement();
                }
                else
                {
                    // If set to random then attempt to replicate any meal with preferability above awful
                    if (allowedMeals.Any(x => x.ingestible.preferability > FoodPreferability.MealAwful))
                    {
                        SelectedMeal = allowedMeals.Where(x => x.ingestible.preferability > FoodPreferability.MealAwful).RandomElement();
                    }
                    else
                    {
                        SelectedMeal = allowedMeals.RandomElement();
                    }

                }
            }

            return SelectedMeal;
        }

        public static void GenerateIngredients(Thing meal, Ideo ideo)
        {            
            CompIngredients compIngredients = meal.TryGetComp<CompIngredients>();

            if (compIngredients != null) 
            {
                // Stage 1: Generate random ingredients according to recipe

                // 1.1: Find recipe
                Func<RecipeDef, bool> validator = delegate (RecipeDef r)
                {                    
                    bool directMatch = r.ProducedThingDef == meal.def;

                    // Add compatibility for VCE Soups and Stews, whose original recipes only makes uncooked versions
                    bool indirectMatch = (r.ProducedThingDef != null) ? (r.ProducedThingDef.ToString().Replace("Uncooked", "Cooked") == meal.def.ToString()) : false;

                    return directMatch || indirectMatch;
                };

                RecipeDef mealRecipe = DefDatabase<RecipeDef>.AllDefsListForReading.First(validator);

                if (mealRecipe != null)
                {
                    // 1.2: Generate ingredients from recipe
                    List<string> ingredientCategoryOptions = new List<string>();

                    List<ThingDef> ingredientThingDefs = new List<ThingDef>();
                    
                    // 1.3: Find ingredient categories and/or fixed thingDefs
                    foreach (IngredientCount currIngredientCount in mealRecipe.ingredients)
                    {
                        if (currIngredientCount.filter.categories != null)
                        {
                            // Limit to 3 instances of an ingredient category
                            int ingredientCatInstances = Math.Min((int)Math.Ceiling(currIngredientCount.count / 0.5f), 3);

                            for (int i = 0; i < ingredientCatInstances; i++)
                            {
                                // Max limit of one condiment from Vanilla Cooking Expanded
                                if (currIngredientCount.filter.categories.Contains("VCE_Condiments") && ingredientCategoryOptions.Contains("VCE_Condiments"))
                                {
                                    continue;
                                } else
                                {
                                    ingredientCategoryOptions.Add(currIngredientCount.filter.categories.RandomElement());
                                }
                            }
                        }
                        if (currIngredientCount.filter.thingDefs != null)
                        {
                            ingredientThingDefs.AddRange(currIngredientCount.filter.thingDefs);
                        }
                    }

                    // 1.4: Generate random ingredient thingDefs based on categories, and add them to the existing list of fixed thingDefs
                    // (Ignoring disallowed ingredients as well as humanlike and insect meats by default)
                    foreach (string currentIngredientCatOption in ingredientCategoryOptions)
                    {
                        ThingDef ingredient = new ThingDef();
                        
                        switch (currentIngredientCatOption)
                        {
                            case "FoodRaw":
                                ingredient = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => d.IsNutritionGivingIngestible && d.ingestible.HumanEdible && (d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw) || d.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw) || d.thingCategories.Contains(ThingCategoryDef.Named("AnimalProductRaw")) || d.thingCategories.Contains(ThingCategoryDefOf.EggsUnfertilized)) && !replimatRestrictions.disallowedIngredients.Contains(d) && FoodUtility.GetMeatSourceCategory(d) != MeatSourceCategory.Humanlike && FoodUtility.GetMeatSourceCategory(d) != MeatSourceCategory.Insect).RandomElement();
                                break;
                            case "VCE_Condiments":
                                ingredient = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDef.Named(currentIngredientCatOption)) && !replimatRestrictions.disallowedIngredients.Contains(d)).RandomElement();
                                break;
                            default:
                                ingredient = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => d.IsNutritionGivingIngestible && d.ingestible.HumanEdible && d.thingCategories.Contains(ThingCategoryDef.Named(currentIngredientCatOption)) && !replimatRestrictions.disallowedIngredients.Contains(d) && FoodUtility.GetMeatSourceCategory(d) != MeatSourceCategory.Humanlike && FoodUtility.GetMeatSourceCategory(d) != MeatSourceCategory.Insect).RandomElement();
                                break;
                        }

                        // Avoid duplicates
                        if (!ingredientThingDefs.Contains(ingredient))
                        {
                            ingredientThingDefs.Add(ingredient);
                        }
                    }

                    // Stage 2: Ideo replacements

                    // 2.1 Human cannibalism for meals containing meat
                    if (ideo?.HasHumanMeatEatingRequiredPrecept() == true)
                    {
                        List<ThingDef> existingMeats = ingredientThingDefs.FindAll((ThingDef d) => d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw));

                        // Replace existing meats with a single instance of human meat
                        if (existingMeats.Count > 0)
                        {
                            ingredientThingDefs = ingredientThingDefs.Except(existingMeats).ToList();

                            ingredientThingDefs.Add(ThingDefOf.Meat_Human);
                        }
                    }

                    // 2.2 Insect meat loved for meals containing meat
                    if (ideo?.HasPrecept(ReplimatDef.InsectMeatEating_Loved) == true)
                    {
                        List<ThingDef> existingMeats = ingredientThingDefs.FindAll((ThingDef d) => d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw));

                        // Replace existing meats with a single instance of insect meat
                        if (existingMeats.Count > 0)
                        {
                            ingredientThingDefs = ingredientThingDefs.Except(existingMeats).ToList();

                            ingredientThingDefs.Add(ReplimatDef.Meat_Megaspider);
                        }
                    }

                    // 2.3 Fungus preferred for meals containing raw plant food
                    if (ideo?.HasPrecept(ReplimatDef.FungusEating_Preferred) == true)
                    {
                        List<ThingDef> existingPlantFoodRaws = ingredientThingDefs.FindAll((ThingDef d) => d.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw) || d.ingestible.foodType == FoodTypeFlags.VegetableOrFruit);
                        // todo - fix

                        // Replace existing raw plant food with a single instance of fungus
                        if (existingPlantFoodRaws.Count > 0)
                        {
                            ingredientThingDefs = ingredientThingDefs.Except(existingPlantFoodRaws).ToList();

                            ingredientThingDefs.Add(ReplimatDef.RawFungus);
                        }
                    }

                    // 2.4 Fungus despised for meals containing raw plant food
                    if (ideo?.HasPrecept(ReplimatDef.FungusEating_Despised) == true)
                    {
                        ingredientThingDefs.Remove(ReplimatDef.RawFungus);
                    }

                    // Stage 3: Assign final ingredients to meal
                    compIngredients.ingredients.AddRange(ingredientThingDefs);
                }
            }
        }

        public static bool TryConsumeFeedstock(this PowerNet net, float feedstockNeeded)
        {

            if (feedstockNeeded <= 0f)
            {
                return false;
            }

            List<Building_ReplimatFeedTank> feedstockTanks = net.GetTanks();

            if (!feedstockTanks.NullOrEmpty())
            {
                float totalAvailableFeedstock = feedstockTanks.Sum(x => x.storedFeedstock);

                feedstockTanks.Shuffle();

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    float feedstockLeftToConsume = feedstockNeeded;

                    foreach (var currentTank in feedstockTanks)
                    {
                        if (feedstockLeftToConsume > 0)
                        {
                            float num = Math.Min(feedstockLeftToConsume, currentTank.StoredFeedstock);
                            currentTank.DrawFeedstock(num);

                            feedstockLeftToConsume -= num;

                            if (feedstockLeftToConsume <= 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
