using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class Settings : ModSettings
    {
        public bool PrioritizeFoodQuality = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PrioritizeFoodQuality, "PrioritizeFoodQuality", true, true);
        }
    }

    public class ReplimatMod : Mod
    {
        public static Settings Settings;

        public ReplimatMod(ModContentPack content) : base(content)
        {
            Settings = base.GetSettings<Settings>();
            var harmony = HarmonyInstance.Create("com.Replimat.patches");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(canvas);
            listingStandard.CheckboxLabeled(Translator.Translate("Replimat_Settings_PrioritizeFoodQuality_Title"), ref Settings.PrioritizeFoodQuality, Translator.Translate("Replimat_Settings_PrioritizeFoodQuality_Desc"));
            listingStandard.End();
            base.DoSettingsWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Replimat_SettingsCategory_Heading".Translate();
        }

        static bool allowForbidden;
        static bool allowDispenserFull;
        static Pawn getter;
        static Pawn eater;
        static bool allowSociallyImproper;
        static bool BestFoodSourceOnMap = false;
        [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap"), StaticConstructorOnStartup]
        static class Patch_BestFoodSourceOnMap
        {
            static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull, ref bool allowDispenserEmpty, ref bool allowForbidden, ref bool allowSociallyImproper)
            {
                BestFoodSourceOnMap = true;
                ReplimatMod.getter = getter;
                ReplimatMod.eater = eater;
                ReplimatMod.allowDispenserFull = allowDispenserFull;
                ReplimatMod.allowForbidden = allowForbidden;
                ReplimatMod.allowSociallyImproper = allowSociallyImproper;
            }

            static void Postfix()
            {
                BestFoodSourceOnMap = false;
            }
        }

        static bool RepDel(Building_ReplimatTerminal t)
        {
            if (
                !allowDispenserFull
                || !(getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                || (t.Faction != getter.Faction && t.Faction != getter.HostFaction)
                || (!allowForbidden && t.IsForbidden(getter))
                || !t.powerComp.PowerOn
                || !t.InteractionCell.Standable(t.Map)
                || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)
                || getter.IsWildMan()
                || t.PickMeal(eater) == null
                || !t.HasStockFor(t.PickMeal(eater))
                || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FoodUtility), "SpawnedFoodSearchInnerScan")]
        static class Patch_SpawnedFoodSearchInnerScan
        {
            static bool Prefix(ref Predicate<Thing> validator)
            {
                Predicate<Thing> malidator = validator;
                Predicate<Thing> salivator = x => (x is Building_ReplimatTerminal rep) ? RepDel(rep) : malidator(x);
                validator = salivator;
                return true;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
        static class Patch_FoodOptimality
        {
            static bool Prefix(ref ThingDef foodDef, ref float __result)
            {
                if (foodDef == null)
                {
                    __result = -9999999f;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "GetFinalIngestibleDef")]
        static class Patch_GetFinalIngestibleDef
        {
            static bool Prefix(ref Thing foodSource, ref ThingDef __result)
            {
                if (foodSource is Building_ReplimatTerminal d)
                {
                    if (BestFoodSourceOnMap)
                    {
                        //    Log.Warning("BestFoodSourceOnMap");
                        __result = d.PickMeal(eater);
                        return false;
                    }

                    //   Log.Warning("NOT BestFoodSourceOnMap");
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Toils_Ingest), "TakeMealFromDispenser")]
        static class Patch_TakeMealFromDispenser
        {
            static bool Prefix(ref TargetIndex ind, ref Pawn eater, ref Toil __result)
            {
                if (eater.jobs.curJob.GetTarget(ind).Thing is Building_ReplimatTerminal)
                {
                    TargetIndex windex = ind;
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;
                        Job curJob = actor.jobs.curJob;
                        var repmat = (Building_ReplimatTerminal)curJob.GetTarget(windex).Thing;

                        Pawn PawnForMealScan = actor;
                        if (curJob.GetTarget(TargetIndex.B).Thing is Pawn p)
                        {
                            PawnForMealScan = p;
                        }

                        Thing thing = repmat.TryDispenseFood(PawnForMealScan);
                        if (thing == null)
                        {
                            actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                        actor.carryTracker.TryStartCarry(thing);
                        actor.CurJob.SetTarget(windex, actor.carryTracker.CarriedThing);
                    };
                    toil.FailOnCannotTouch(ind, PathEndMode.Touch);
                    toil.defaultCompleteMode = ToilCompleteMode.Delay;
                    toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;
                    __result = toil;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(JobDriver_FoodDeliver), "GetReport")]
        static class Patch_JobDriver_FoodDeliver_GetReport
        {
            static void Postfix(JobDriver_FoodDeliver __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal && (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "Replicated Meal").Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), "GetReport")]
        static class Patch_JobDriver_FoodFeedPatient_GetReport
        {
            static void Postfix(JobDriver_FoodFeedPatient __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal && (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "Replicated Meal").Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_Ingest), "GetReport")]
        static class Patch_JobDriver_Ingest_GetReport
        {
            static void Postfix(JobDriver_Ingest __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "Replicated Meal");
                }
            }
        }

        [HarmonyPatch(typeof(ThingListGroupHelper), "Includes")]
        static class Patch_Includes
        {
            static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
            {
                if (group == ThingRequestGroup.FoodSource || group == ThingRequestGroup.FoodSourceNotPlantOrTree)
                {
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
            static void Postfix(Thing __instance, ref float __result)
            {
                if (__instance.Spawned && __instance.def.IsNutritionGivingIngestible)
                {
                    var hop = __instance.Map.thingGrid.ThingsListAtFast(__instance.Position).OfType<Building_ReplimatHopper>().FirstOrDefault(x => x.powerComp.PowerOn);
                    if (hop != null)
                    {
                        __result = hop.freezerTemp;
                    }
                }
            }
        }

        // Hacky workaround
        // Trick the game into thinking that Replimat Terminals are not food dispensers
        // so that they do not trigger the "Need food hopper" alert
        // Will not affect stock nutrient paste dispensers or pawn food optimality
        [HarmonyPatch(typeof(ThingDef), "get_IsFoodDispenser")]
        static class Building
        {
            [HarmonyPrefix]
            static bool IsFoodDispenserPrefix(ThingDef __instance, ref bool __result)
            {
                if (__instance.thingClass == typeof(Building_ReplimatTerminal))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}