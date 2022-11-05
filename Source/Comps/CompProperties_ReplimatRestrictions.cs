using System.Collections.Generic;
using Verse;

namespace Replimat
{
    public class CompProperties_ReplimatRestrictions : CompProperties
    {
        public List<ThingDef> disallowedMeals = null;

        public List<ThingDef> disallowedIngredients = null;

        public List<ThingDef> batchReplicableSurvivalMeals = null;

        public CompProperties_ReplimatRestrictions()
        {
            compClass = typeof(CompReplimatRestrictions);
        }
    }
}
