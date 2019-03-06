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
        public bool RandomMeals = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref RandomMeals, "RandomMeals", false);
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

            MethodInfo qarth = AccessTools.Method("RimWorld.FoodUtility+<BestFoodSourceOnMap>c__AnonStorey0:<>m__0");
            HarmonyMethod asshai = new HarmonyMethod(typeof(ReplimatMod).GetMethod("RepDelSwap"));
            harmony.Patch(qarth, asshai);
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(canvas);
            listingStandard.CheckboxLabeled(Translator.Translate("Replimat_Settings_EnableRandomMeal_Title"), ref Settings.RandomMeals, Translator.Translate("Replimat_Settings_EnableRandomMeal_Desc"));
            listingStandard.End();
            base.DoSettingsWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Replimat".Translate();
        }

        static bool allowForbidden;
        static bool allowDispenserFull;
        static Pawn getter;
        static Pawn eater;
        static bool allowSociallyImproper;

        [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap"), StaticConstructorOnStartup]
        static class Patch_BestFoodSourceOnMap
        {
            static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull, ref bool allowDispenserEmpty, ref bool allowForbidden, ref bool allowSociallyImproper)
            {
                ReplimatMod.getter = getter;
                ReplimatMod.eater = eater;
                ReplimatMod.allowDispenserFull = allowDispenserFull;
                ReplimatMod.allowForbidden = allowForbidden;
                ReplimatMod.allowSociallyImproper = allowSociallyImproper;
            }
        }

        static bool IsFoodSourceOnMapSociallyProper(Thing t)
        {
            var trav = AccessTools.Method(typeof(FoodUtility), "IsFoodSourceOnMapSociallyProper");
            if (trav.Invoke(null, new object[] { t, getter, eater, allowSociallyImproper }) is bool b) return b;
            return true;
        }

        static bool RepDelSwap(ref Thing t, ref bool __result)
        {
            if (t is Building_ReplimatTerminal term)
            {
                __result = true;
                if (
                    !allowDispenserFull
                    || !(getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    || (t.Faction != getter.Faction && t.Faction != getter.HostFaction)
                    || (!allowForbidden && t.IsForbidden(getter))
                    || !term.powerComp.PowerOn
                    || !t.InteractionCell.Standable(t.Map)
                    || !IsFoodSourceOnMapSociallyProper(t)
                    || getter.IsWildMan()
                    || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                {
                    __result = false;
                }
                return false;
            }
            return true;
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