using HarmonyLib;
using System.Text;
using Verse;

namespace Replimat.Patches
{
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.GetInspectString))]
    public static class Harmony_Thing_GetInspectString
    {
        public static void Postfix(ThingWithComps __instance, ref string __result)
        {
            if (__instance.def.IsIngestible && ReplimatUtility.IsReplicatedFood(__instance))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(__result);
                stringBuilder.AppendInNewLine("ReplicatedMeal".Translate().CapitalizeFirst());
                __result = stringBuilder.ToString();
            }
        }
    }
}
