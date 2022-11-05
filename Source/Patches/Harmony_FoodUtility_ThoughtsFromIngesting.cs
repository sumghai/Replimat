using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using static RimWorld.FoodUtility;

namespace Replimat
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting))]
    public static class Harmony_FoodUtility_ThoughtsFromIngesting
    {
        public static void Postfix(ref List<ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource, ThingDef foodDef)
        { 
            List<TraitDef> sensitiveTasterTraits = ReplimatDef.ReplimatComputer.GetCompProperties<CompProperties_ReplimatRestrictions>().sensitiveTasterTraits;

            if ((bool)(ingester.story?.traits?.allTraits.Any(x => sensitiveTasterTraits.Any(y => y == x.def))) && ReplimatUtility.IsReplicatedFood(foodSource))
            {
                MeatSourceCategory meatSourceCategory = MeatSourceCategory.NotMeat;
                meatSourceCategory = ((!foodSource.def.IsCorpse) ? GetMeatSourceCategory(foodDef) : GetMeatSourceCategoryFromCorpse(foodSource));

                TryAddIngestThought(ingester, ReplimatDef.Thought_AteReplicatedFood, null, __result, foodDef, meatSourceCategory);
            }
        }
    }
}
