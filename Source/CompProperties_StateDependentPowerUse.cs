using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace Replimat
{
    public class CompProperties_StateDependentPowerUse : CompProperties
    {
        // Define defaults if not specified in XML defs

        public float activeModePowerConsumption = 0f;

        public CompProperties_StateDependentPowerUse()
        {
            compClass = typeof(CompStateDependentPowerUse);
        }

        [DebuggerHidden]
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
            {
                yield return s;
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Building, "Replimat_Stat_ActiveModePowerConsumption_Label".Translate(), activeModePowerConsumption.ToString("F0") + " W", "Replimat_Stat_ActiveModePowerConsumption_Desc".Translate(), 4994);
        }
    }
}
