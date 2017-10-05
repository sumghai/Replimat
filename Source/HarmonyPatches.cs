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

        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob", new Type[] { typeof(Pawn) }), StaticConstructorOnStartup]
        static class Patch_JobGiver_GetFood
        {
            static Patch_JobGiver_GetFood()
            {
                var harmonyInstance = HarmonyInstance.Create("com.Sumghai.Replimat.patches");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            static bool Prefix(JobGiver_GetFood __instance, Pawn pawn, ref Job __result)
            {
               
                bool getterCanManipulate = pawn.RaceProps.ToolUser && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
                List<Thing> Replimats = pawn.Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminalDef);

                if (!Replimats.NullOrEmpty() && getterCanManipulate)
                {
                    Traverse traverse = Traverse.Create(__instance);
                    HungerCategory minCategory = traverse.Field("minCategory").GetValue<HungerCategory>();

                    Need_Food food = pawn.needs.food;
                    if (food == null || food.CurCategory < minCategory)
                    {
                        __result = null;
                        return false;
                    }

                    bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;

                    Thing thing;
                    ThingDef def;
                    if (!ReplimatUtility.TryFindBestFoodSourceFor(Replimats, pawn, pawn, desperate, out thing, out def, true, true, false, false, false))
                    {
                        __result = null;
                        return false;
                    }

                    if (thing is Building_ReplimatTerminal rep)
                    {
                        __result = new Job(ReplimatDef.ingestReplimatDef, thing)
                        {
                            count = FoodUtility.WillIngestStackCountOf(pawn, def)
                        };
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("AmbientTemperature", PropertyMethod.Getter)]
        class Patch_Thing_AmbientTemperature
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