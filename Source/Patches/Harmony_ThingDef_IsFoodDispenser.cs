using HarmonyLib;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsFoodDispenser), MethodType.Getter)]
    public static class Harmony_ThingDef_IsFoodDispenser
    {
        // Hacky workaround
        // Trick the game into thinking that Replimat Terminals are not food dispensers
        // so that they do not trigger the "Need food hopper" alert
        // Will not affect stock nutrient paste dispensers or pawn food optimality
        
        static bool Prefix(ThingDef __instance, ref bool __result)
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
