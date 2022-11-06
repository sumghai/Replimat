using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Replimat
{
    [HarmonyPatch(typeof(Trait), nameof(Trait.TipString))]
    public static class Harmony_Trait_TipString
    {
        public static void Postfix(ref string __result, Trait __instance, Pawn pawn)
        {
            List<TraitDef> sensitiveTasterTraits = ReplimatDef.ReplimatComputer.GetCompProperties<CompProperties_ReplimatRestrictions>().sensitiveTasterTraits;

            if (sensitiveTasterTraits.Contains(__instance.def))
            {
                TraitDegreeData currentData = __instance.CurrentData;
                string originalDesc = currentData.description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn).Resolve();
                string appendedDesc = originalDesc + "\n\n" + "AteReplicatedMealTraitDescAppendix".Translate().Formatted(pawn.Named("PAWN")).AdjustedFor(pawn).Resolve();
                __result = __result.Replace(originalDesc, appendedDesc);
            }
        }
    }
}
