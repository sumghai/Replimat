using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Replimat
{
    public static class ReplimatUtility
    {
        private static HashSet<Thing> filtered = new HashSet<Thing>();

        private const float nutrientFeedStockDensity = 1.07f; // 1.07 kg/L (equivalent to Abbott's Ensure Nutritional Shake)

        public static float convertMassToFeedstockVolume(float mass)
        {
            return mass / nutrientFeedStockDensity;
        }

        public static float convertFeedstockVolumeToMass(float volume)
        {
            return volume * nutrientFeedStockDensity;
        }

        private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
        {
            {
                new CurvePoint(-100f, -600f),
                true
            },
            {
                new CurvePoint(-10f, -100f),
                true
            },
            {
                new CurvePoint(-5f, -70f),
                true
            },
            {
                new CurvePoint(-1f, -50f),
                true
            },
            {
                new CurvePoint(0f, 0f),
                true
            },
            {
                new CurvePoint(100f, 800f),
                true
            }
        };

        private static List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();

        public static bool TryFindBestFoodSourceFor(List<Thing> replimats, Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false)
        {
            bool allowDrug = !eater.IsTeetotaler();
            Thing thing = null;
            if (canUseInventory)
            {

                thing = FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.MealAwful, FoodPreferability.MealLavish, 0f, false);

                if (thing != null)
                {
                    if (getter.Faction != Faction.OfPlayer)
                    {
                        foodSource = thing;
                        foodDef = GetFinalIngestibleDef(foodSource);
                        return true;
                    }
                    CompRottable compRottable = thing.TryGetComp<CompRottable>();
                    if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        foodSource = thing;
                        foodDef = GetFinalIngestibleDef(foodSource);
                        return true;
                    }
                }
            }
            bool allowPlant = getter == eater;
            Thing thing2 = BestFoodSourceOnMap(replimats, getter, eater, desperate, FoodPreferability.MealLavish, allowPlant, allowDrug, allowCorpse, true, canRefillDispenser, allowForbidden, allowSociallyImproper);
            if (thing == null && thing2 == null)
            {
                if (canUseInventory)
                {
                    thing = FoodUtility.BestFoodInInventory(getter, null, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, allowDrug);
                    if (thing != null)
                    {
                        foodSource = thing;
                        foodDef = GetFinalIngestibleDef(foodSource);
                        return true;
                    }
                }

                foodSource = null;
                foodDef = null;
                return false;
            }
            if (thing == null && thing2 != null)
            {
                foodSource = thing2;
                foodDef = GetFinalIngestibleDef(foodSource);
                return true;
            }
            if (thing2 == null && thing != null)
            {
                foodSource = thing;
                foodDef = GetFinalIngestibleDef(foodSource);
                return true;
            }
            float num = FoodSourceOptimality(eater, thing2, (float)(getter.Position - thing2.Position).LengthManhattan, false);
            float num2 = FoodSourceOptimality(eater, thing, 0f, false);
            num2 -= 32f;
            if (num > num2)
            {
                foodSource = thing2;
                foodDef = GetFinalIngestibleDef(foodSource);
                return true;
            }
            foodSource = thing;
            foodDef = GetFinalIngestibleDef(foodSource);
            return true;
        }

        public static ThingDef GetFinalIngestibleDef(Thing foodSource)
        {
            if (foodSource is Building_ReplimatTerminal rep)
            {
                return rep.SelectedFood;
            }
            if (foodSource is Building_NutrientPasteDispenser disp)
            {
                return disp.DispensableDef;
            }

            Pawn pawn = foodSource as Pawn;
            if (pawn != null)
            {
                return pawn.RaceProps.corpseDef;
            }
            return foodSource.def;
        }

        public static Thing BestFoodSourceOnMap(List<Thing> replimats, Pawn getter, Pawn eater, bool desperate, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Error(string.Concat(new object[]
                {
                    getter,
                    " tried to find food to bring to ",
                    eater,
                    " but ",
                    getter,
                    " is incapable of Manipulation."
                }));
                return null;
            }
            FoodPreferability minPref;
            if (!eater.RaceProps.Humanlike)
            {
                minPref = FoodPreferability.NeverForNutrition;
            }
            else if (desperate)
            {
                minPref = FoodPreferability.DesperateOnly;
            }
            else
            {
                minPref = ((eater.needs.food.CurCategory <= HungerCategory.UrgentlyHungry) ? FoodPreferability.RawBad : FoodPreferability.MealAwful);
            }
            Predicate<Thing> foodValidator = delegate (Thing t)
            {
                if (!allowForbidden && t.IsForbidden(getter))
                {
                    return false;
                }

                if (t is Building_ReplimatTerminal replimat)
                {
                    
                    if (!getter.CanReserve(replimat, 1) || !replimat.CanDispenseNow || replimat.MaxPreferability < minPref || replimat.MaxPreferability > maxPref || !getterCanManipulate || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !t.InteractionCell.Standable(t.Map) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                    {
                        return false;
                    }
                }
                else if (t is Building_NutrientPasteDispenser building_NutrientPasteDispenser)
                {
                    if (!allowDispenserFull || ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref || ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref || !getterCanManipulate || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !t.InteractionCell.Standable(t.Map) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (t.def.ingestible.preferability < minPref)
                    {
                        return false;
                    }
                    if (t.def.ingestible.preferability > maxPref)
                    {
                        return false;
                    }
                    if (!t.IngestibleNow || !t.def.IsNutritionGivingIngestible || (!allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug) || (!desperate && t.IsNotFresh()) || t.IsDessicated() || !eater.RaceProps.WillAutomaticallyEat(t) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.AnimalAwareOf(t) || !getter.CanReserve(t, 1, -1, null, false))
                    {
                        return false;
                    }
                }

                return true;
            };
            ThingRequest thingRequest;
            if ((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != FoodTypeFlags.None && allowPlant)
            {
                thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            }
            else
            {
                thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
            }
            Thing thing = null;
            if (getter.RaceProps.Humanlike)
            {
                Predicate<Thing> validator = foodValidator;

                List<Thing> searchset = getter.Map.listerThings.ThingsMatching(thingRequest);
                searchset.AddRange(replimats);

                thing = SpawnedFoodSearchInnerScan(eater, getter.Position, searchset, PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
            }

            return thing;
        }

        private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
        {
            if (!allowSociallyImproper)
            {
                bool animalsCare = !getter.RaceProps.Animal;
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                {
                    return false;
                }
            }
            return true;
        }

        public static float FoodSourceOptimality(Pawn eater, Thing t, float dist, bool takingToInventory = false)
        {
            float num = 300f;
            num -= dist;
            ThingDef thingDef = t.def;
            if (t is Building_NutrientPasteDispenser disp)
            {
                thingDef = disp.DispensableDef;
            }
            if (t is Building_ReplimatTerminal rep)
            {
                thingDef = rep.SelectedFood;
            }
            FoodPreferability preferability = thingDef.ingestible.preferability;
            if (preferability != FoodPreferability.NeverForNutrition)
            {
                if (preferability == FoodPreferability.DesperateOnly)
                {
                    num -= 150f;
                }
                CompRottable compRottable = t.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    if (compRottable.Stage == RotStage.Dessicated)
                    {
                        return -9999999f;
                    }
                    if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        num += 12f;
                    }
                }
                if (eater.needs != null && eater.needs.mood != null)
                {
                    List<ThoughtDef> list = ThoughtsFromIngesting(eater, t);
                    for (int i = 0; i < list.Count; i++)
                    {
                        num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect);
                    }
                }
                if (thingDef.ingestible != null)
                {
                    num += thingDef.ingestible.optimalityOffset;
                }
                return num;
            }
            return -9999999f;
        }

        private static Thing SpawnedFoodSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null)
        {
            if (searchSet == null)
            {
                return null;
            }
            Pawn pawn = traverseParams.pawn ?? eater;
            int num = 0;
            int num2 = 0;
            Thing result = null;
            float num3 = -3.40282347E+38f;
            for (int i = 0; i < searchSet.Count; i++)
            {
                Thing thing = searchSet[i];
                num2++;
                float num4 = (float)(root - thing.Position).LengthManhattan;
                if (num4 <= maxDistance)
                {
                    float num5 = FoodSourceOptimality(eater, thing, num4, false);
                    if (num5 >= num3)
                    {
                        if (pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams))
                        {
                            if (thing.Spawned)
                            {
                                if (validator == null || validator(thing))
                                {
                                    result = thing;
                                    num3 = num5;
                                    num++;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<ThoughtDef> ThoughtsFromIngesting(Pawn ingester, Thing t)
        {
            ingestThoughts.Clear();
            if (ingester.needs == null || ingester.needs.mood == null)
            {
                return ingestThoughts;
            }
            ThingDef thingDef = t.def;
            if (t is Building_NutrientPasteDispenser disp)
            {
                thingDef = disp.DispensableDef;
            }
            if (t is Building_ReplimatTerminal rep)
            {
                thingDef = rep.SelectedFood;
            }
            if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && thingDef.ingestible.tasteThought != null)
            {
                ingestThoughts.Add(thingDef.ingestible.tasteThought);
            }
            CompIngredients compIngredients = t.TryGetComp<CompIngredients>();
            if (FoodUtility.IsHumanlikeMeat(thingDef) && ingester.RaceProps.Humanlike)
            {
                ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatDirect : ThoughtDefOf.AteHumanlikeMeatDirectCannibal);
            }
            else if (compIngredients != null)
            {
                for (int i = 0; i < compIngredients.ingredients.Count; i++)
                {
                    ThingDef thingDef2 = compIngredients.ingredients[i];
                    if (thingDef2.ingestible != null)
                    {
                        if (ingester.RaceProps.Humanlike && FoodUtility.IsHumanlikeMeat(thingDef2))
                        {
                            ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatAsIngredient : ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal);
                        }
                        else if (thingDef2.ingestible.specialThoughtAsIngredient != null)
                        {
                            ingestThoughts.Add(thingDef2.ingestible.specialThoughtAsIngredient);
                        }
                    }
                }
            }
            else if (thingDef.ingestible.specialThoughtDirect != null)
            {
                ingestThoughts.Add(thingDef.ingestible.specialThoughtDirect);
            }
            if (t.IsNotFresh())
            {
                ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
            }
            return ingestThoughts;
        }
    }
}