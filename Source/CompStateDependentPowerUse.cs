using Verse;

namespace Replimat
{
    public class CompStateDependentPowerUse : ThingComp
    {
        private CompProperties_StateDependentPowerUse Props
        {
            get
            {
                return (CompProperties_StateDependentPowerUse)props;
            }
        }

        public float ActiveModePowerConsumption
        {
            get
            {
                return Props.activeModePowerConsumption;
            }
        }
    }
}