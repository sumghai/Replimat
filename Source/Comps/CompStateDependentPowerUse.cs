using Verse;

namespace Replimat
{
    public class CompStateDependentPowerUse : ThingComp
    {
        public CompProperties_StateDependentPowerUse Props
        {
            get
            {
                return (CompProperties_StateDependentPowerUse)props;
            }
        }
    }
}