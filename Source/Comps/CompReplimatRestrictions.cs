using System.Collections.Generic;
using Verse;

namespace Replimat
{
    public class CompReplimatRestrictions : ThingComp
    {
        private CompProperties_ReplimatRestrictions Props
        {
            get 
            {
                return (CompProperties_ReplimatRestrictions)props;
            }
        }

        public List<ThingDef> DisallowedMeals
        {
            get
            {
                return Props.disallowedMeals;
            }
        }

        public List<ThingDef> DisallowedIngredients
        {
            get
            {
                return Props.disallowedIngredients;
            }
        }

        public List<ThingDef> BatchReplicableSurvivalMeals
        {
            get
            {
                return Props.batchReplicableSurvivalMeals;
            }
        }
    }
}