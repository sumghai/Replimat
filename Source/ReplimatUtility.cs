using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Replimat
{

    [StaticConstructorOnStartup]
    public static class ReplimatUtility
    {
        private static HashSet<Thing> filtered = new HashSet<Thing>();

        private const float nutrientFeedStockDensity = 1.07f; // 1.07 kg/L (equivalent to Abbott's Ensure Nutritional Shake)

        public static float convertMassToFeedstockVolume(float mass) => mass / nutrientFeedStockDensity;

        public static float convertFeedstockVolumeToMass(float volume) => volume * nutrientFeedStockDensity;

        public static List<Building_ReplimatFeedTank> GetTanks(this PowerNet net)
        {
            var tanks = net.Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatFeedTank).OfType<Building_ReplimatFeedTank>().Where(x => x.PowerComp.PowerNet == net && x.HasComputer).ToList();
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
                //if (p.foodRestriction.pawn.Faction != Faction.OfPlayer && (getter == null || getter.Faction != Faction.OfPlayer))
                //{
                //    return true;
                //}
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

                if (allowedMeals.NullOrEmpty())
                {
                    // Log.Warning("Null meal.");
                    return null;
                }

                if (ReplimatMod.Settings.PrioritizeFoodQuality)
                {
                    //       Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " will prioritize meal quality");
                    var maxpref = allowedMeals.Max(x => x.ingestible.preferability);
                    SelectedMeal = allowedMeals.Where(x => x.ingestible.preferability == maxpref).RandomElement();
                }
                else
                {
                    //      Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " can choose random meals regardless of quality");

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

                // Debug Messages
                //  Log.Message("[Replimat] Pawn " + eater.Name.ToString() + " is allowed the following meals: \n"
                //       + string.Join(", ", allowedMeals.Select(def => def.defName).ToArray()));
            }

            return SelectedMeal;
        }

        public static bool TryConsumeFeedstock(this PowerNet net, float feedstockNeeded)
        {

            if (feedstockNeeded <= 0f)
            {
                Log.Warning("[Replimat] " + "Tried to draw 0 feedstock!");
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

                    Log.Warning("[Replimat] " + "Tried but tanks ran out of feedstock, needed:" + feedstockNeeded);
                    return false;
                }
                else
                {
                    Log.Warning("[Replimat] " + " Tanks didn't have enough feedstock, needed:" + feedstockNeeded);
                    return false;
                }
            }

            Log.Warning("[Replimat] " + "Tried to draw feedstock from non-existent tanks!");
            return false;

        }
    }
}
