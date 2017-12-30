using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace Replimat
{
    class HarmonyPatches
    {
        [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap"), StaticConstructorOnStartup]
        static class Patch_BestFoodSourceOnMap
        {
            static Patch_BestFoodSourceOnMap()
            {
                var harmonyInstance = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }

            static void Prefix(ref Pawn getter, ref Pawn eater, ref bool desperate, ref FoodPreferability maxPref, ref bool allowDrug, ref bool allowCorpse, ref bool allowDispenserFull, ref bool allowDispenserEmpty, ref bool allowForbidden, ref bool allowSociallyImproper)
            {
                Patch_SpawnedFoodSearchInnerScan.getter = getter;
                Patch_SpawnedFoodSearchInnerScan.eater = eater;
                Patch_SpawnedFoodSearchInnerScan.desperate = desperate;
                Patch_SpawnedFoodSearchInnerScan.maxPref = maxPref;
                Patch_SpawnedFoodSearchInnerScan.allowDrug = allowDrug;
                Patch_SpawnedFoodSearchInnerScan.allowCorpse = allowCorpse;
                Patch_SpawnedFoodSearchInnerScan.allowDispenserFull = allowDispenserFull;
                Patch_SpawnedFoodSearchInnerScan.allowDispenserEmpty = allowDispenserEmpty;
                Patch_SpawnedFoodSearchInnerScan.allowForbidden = allowForbidden;
                Patch_SpawnedFoodSearchInnerScan.allowSociallyImproper = allowSociallyImproper;
            }
        }



        [HarmonyPatch(typeof(FoodUtility), "SpawnedFoodSearchInnerScan"), StaticConstructorOnStartup]
        static class Patch_SpawnedFoodSearchInnerScan
        {
            static Patch_SpawnedFoodSearchInnerScan()
            {
                var harmonyInstance = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                poop = AccessTools.Method(typo, "IsFoodSourceOnMapSociallyProper");
            }


            public static bool allowForbidden;
            public static bool allowDispenserFull;
            public static Pawn getter;
            public static Pawn eater;
            public static FoodPreferability maxPref;
            public static FoodPreferability minPref;
            public static bool getterCanManipulate;
            public static bool desperate;
            public static bool allowCorpse;
            public static bool allowDrug;
            public static bool allowSociallyImproper;
            public static bool allowDispenserEmpty;

            public static Type typo = typeof(FoodUtility);
            public static MethodInfo poop;

            static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
            {          
                var rawValue = poop.Invoke(typo, new object[] { t, getter, eater, allowSociallyImproper });

                bool x = false;
                if (rawValue is bool)
                {
                    x = (bool)rawValue;
                }
                return x;
            }

            static void Prefix(ref Predicate<Thing> validator)
            {

                getterCanManipulate = (getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));

                if (eater.NonHumanlikeOrWildMan())
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

                validator = delegate (Thing t)
                            {
                                if (!allowForbidden && t.IsForbidden(getter))
                                {
                                    return false;
                                }
                                Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                                if (building_NutrientPasteDispenser != null)
                                {
                                    if (!allowDispenserFull || building_NutrientPasteDispenser.DispensableDef.ingestible.preferability < minPref || building_NutrientPasteDispenser.DispensableDef.ingestible.preferability > maxPref || !getterCanManipulate || getter.IsWildMan() || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !t.InteractionCell.Standable(t.Map) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
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
            }
        }


        [HarmonyPatch(typeof(ThingListGroupHelper), "Includes"), StaticConstructorOnStartup]
        static class Patch_Includes
        {
            static Patch_Includes()
            {
                var harmonyInstance = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }

            static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
            {
                if (group == ThingRequestGroup.FoodSource || group == ThingRequestGroup.FoodSourceNotPlantOrTree)
                {
                    //def.thingClass.GetType().IsAssignableFrom(typeof(Building_NutrientPasteDispenser))
                    if (def.thingClass == typeof(Building_ReplimatTerminal))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("AmbientTemperature", PropertyMethod.Getter)]
        static class Patch_Thing_AmbientTemperature
        {

            static Patch_Thing_AmbientTemperature()
            {
                var harmonyInstance = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }

            static void Postfix(Thing __instance, ref float __result)
            {
                if (__instance.Spawned && __instance.def.IsNutritionGivingIngestible)
                {
                    List<Thing> list = __instance.Map.thingGrid.ThingsListAtFast(__instance.Position);
                    Building_ReplimatHopper hop = list.OfType<Building_ReplimatHopper>().FirstOrDefault();
                    if (hop != null)
                    {
                        __result = hop.freezerTemp;
                    }
                }
            }
        }
    }
}