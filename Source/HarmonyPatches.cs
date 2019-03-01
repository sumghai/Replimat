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

    public class SumghaiReplimatMod : Mod
    {
        public SumghaiReplimatMod(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(JobDriver), "TryActuallyStartNextToil")]
        static class Patch_TryActuallyStartNextToil
        {
            static void Prefix(JobDriver __instance)
            {
                Building_ReplimatTerminal.MealSearcher = __instance.pawn;
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