using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class Settings : ModSettings
    {
        public bool PrioritizeFoodQuality = true;
        public float HopperRefillThresholdPercent;
        public bool EnableIncidentSpill = true;
        public bool EnableIncidentKibble = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PrioritizeFoodQuality, "PrioritizeFoodQuality", true, true);
            Scribe_Values.Look(ref HopperRefillThresholdPercent, "HopperRefillThresholdPercent", 0.5f, true);
            Scribe_Values.Look(ref EnableIncidentSpill, "EnableIncidentSpill", true, true);
            Scribe_Values.Look(ref EnableIncidentKibble, "EnableIncidentKibble", true, true);
        }

        public void Draw(Rect canvas)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(canvas);

            // Do general settings
            Text.Font = GameFont.Medium;
            listingStandard.Label("Replimat_Settings_HeaderGeneral".Translate());
            Text.Font = GameFont.Small;
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Replimat_Settings_PrioritizeFoodQuality_Title".Translate(),
                ref PrioritizeFoodQuality, "Replimat_Settings_PrioritizeFoodQuality_Desc".Translate());
            listingStandard.Label("Replimat_Settings_HopperRefillThresholdPercent_Title".Translate() + ": " + HopperRefillThresholdPercent.ToStringPercent("F0"), -1f, "Replimat_Settings_HopperRefillThresholdPercent_Desc".Translate());
            HopperRefillThresholdPercent = (float)Math.Round((double)listingStandard.Slider(HopperRefillThresholdPercent, 0.05f, 1f), 2);            

            // Do incident settings
            Text.Font = GameFont.Medium;
            listingStandard.Label("Replimat_Settings_HeaderIncidents".Translate());
            Text.Font = GameFont.Small;
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentSpill_Title".Translate(), ref EnableIncidentSpill, "Replimat_Settings_EnableIncidentSpill_Desc".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentKibble_Title".Translate(), ref EnableIncidentKibble, "Replimat_Settings_EnableIncidentKibble_Desc".Translate());
            listingStandard.End();
        }
    }

    public class ReplimatMod : Mod
    {
        public static Settings Settings;

        private static bool allowForbidden;
        private static bool allowDispenserFull;
        private static Pawn getter;
        private static Pawn eater;
        private static bool allowSociallyImproper;
        private static bool BestFoodSourceOnMap;

        public static int minimumHopperRefillThresholdPercent = 10;


        public ReplimatMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            var harmony = new Harmony("com.Replimat.patches");
            harmony.PatchAll();

            MP_Util.Bootup(harmony);
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.Draw(canvas);
            base.DoSettingsWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Replimat_SettingsCategory_Heading".Translate();
        }



        private static bool RepDel(Building_ReplimatTerminal t)
        {
            if (
                !allowDispenserFull
                || !(getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                || t.Faction != getter.Faction && t.Faction != getter.HostFaction
                || !allowForbidden && t.IsForbidden(getter)
                || !t.powerComp.PowerOn
                || !t.InteractionCell.Standable(t.Map)
                || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)
                || getter.IsWildMan()
                || ReplimatUtility.PickMeal(eater, getter) == null
                || !t.HasStockFor(ReplimatUtility.PickMeal(eater, getter))
                || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map),
                    PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap")]
        [StaticConstructorOnStartup]
        private static class Patch_BestFoodSourceOnMap
        {
            private static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull,
                ref bool allowForbidden, ref bool allowSociallyImproper)
            {
                BestFoodSourceOnMap = true;
                ReplimatMod.getter = getter;
                ReplimatMod.eater = eater;
                ReplimatMod.allowDispenserFull = allowDispenserFull;
                ReplimatMod.allowForbidden = allowForbidden;
                ReplimatMod.allowSociallyImproper = allowSociallyImproper;
            }

            private static void Postfix()
            {
                BestFoodSourceOnMap = false;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "SpawnedFoodSearchInnerScan")]
        private static class Patch_SpawnedFoodSearchInnerScan
        {
            private static bool Prefix(ref Predicate<Thing> validator)
            {
                var malidator = validator;
                Predicate<Thing> salivator = x => x is Building_ReplimatTerminal rep ? RepDel(rep) : malidator(x);
                validator = salivator;
                return true;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
        private static class Patch_FoodOptimality
        {
            private static bool Prefix(ref ThingDef foodDef, ref float __result)
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
        private static class Patch_GetFinalIngestibleDef
        {
            private static bool Prefix(ref Thing foodSource, ref ThingDef __result)
            {
                if (foodSource is Building_ReplimatTerminal && BestFoodSourceOnMap)
                {
                    __result = ReplimatUtility.PickMeal(eater, getter);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Toils_Ingest), "TakeMealFromDispenser")]
        private static class Patch_TakeMealFromDispenser
        {
            private static bool Prefix(ref TargetIndex ind, ref Pawn eater, ref Toil __result)
            {
                if (eater.jobs.curJob.GetTarget(ind).Thing is Building_ReplimatTerminal)
                {
                    var windex = ind;
                    var toil = new Toil();
                    toil.initAction = delegate
                    {
                        var actor = toil.actor;
                        var curJob = actor.jobs.curJob;
                        var repmat = (Building_ReplimatTerminal)curJob.GetTarget(windex).Thing;

                        var PawnForMealScan = actor;
                        if (curJob.GetTarget(TargetIndex.B).Thing is Pawn p)
                        {
                            //  Log.Warning("for "+p.Label);
                            PawnForMealScan = p;
                        }
                       
                        var thing = repmat.TryDispenseFood(PawnForMealScan, actor);
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
        private static class Patch_JobDriver_FoodDeliver_GetReport
        {
            private static void Postfix(JobDriver_FoodDeliver __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal &&
                    (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "ReplicatedMeal".Translate())
                        .Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), "GetReport")]
        private static class Patch_JobDriver_FoodFeedPatient_GetReport
        {
            private static void Postfix(JobDriver_FoodFeedPatient __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal &&
                    (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "ReplicatedMeal".Translate())
                        .Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_Ingest), "GetReport")]
        private static class Patch_JobDriver_Ingest_GetReport
        {
            private static void Postfix(JobDriver_Ingest __instance, ref string __result)
            {
                if (__instance.usingNutrientPasteDispenser)
                {
                    if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_ReplimatTerminal)
                    {
                        __result = __instance.job.def.reportString.Replace("TargetA", "ReplicatedMeal".Translate());
                    }
                    else
                    {
                        __result = __instance.job.def.reportString.Replace("TargetA",
                            __instance.job.GetTarget(TargetIndex.A).Thing.Label);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ThingListGroupHelper), "Includes")]
        private static class Patch_Includes
        {
            private static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
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
        [HarmonyPatch("AmbientTemperature", MethodType.Getter)]
        private static class Patch_Thing_AmbientTemperature
        {
            private static void Postfix(Thing __instance, ref float __result)
            {
                if (__instance.Spawned && __instance.def.IsNutritionGivingIngestible)
                {
                    var hop = __instance.Map.thingGrid.ThingsListAtFast(__instance.Position)
                        .OfType<Building_ReplimatHopper>().FirstOrDefault(x => x.powerComp.PowerOn);
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
        private static class Building
        {
            [HarmonyPrefix]
            private static bool IsFoodDispenserPrefix(ThingDef __instance, ref bool __result)
            {
                if (__instance.thingClass == typeof(Building_ReplimatTerminal))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
        internal class StoreUtility_NoStorageBlockersIn
        {

            [HarmonyPostfix]
            public static void ReplimatHopperFilledEnough(ref bool __result, IntVec3 c, Map map, Thing thing)
            {
                if (__result)
                {

                    if (c.GetSlotGroup(map)?.parent?.GetType().ToString() == "Replimat.Building_ReplimatHopper")
                    {   
                        // Apply the minimum Hopper refilling threshold only to Replimat Hoppers
                        __result &= !map.thingGrid.ThingsListAt(c).Any(t => t.def.EverStorable(false) && t.stackCount >= thing.def.stackLimit * Settings.HopperRefillThresholdPercent);
                    }
                    else
                    {
                        // Ignore the threshold for all other storage buildings and stockpiles
                        // This ensures compatibility with other mods that control stack refilling thresholds on a more general level (e.g. Hauling Hysteresis, Satisfied Storage)
                        __result = __result;
                    }
                }
            }
        }

    }
}