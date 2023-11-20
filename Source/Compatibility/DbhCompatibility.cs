using System.Linq;
using UnityEngine;
using Verse;

namespace Replimat
{
    public static class DbhCompatibility
    {
        public static float GetAvailableSewage(ThingComp pipeComp)
        {
            float total = 0;

            foreach (var sewer in ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.Sewers)
            {
                total += sewer.sewageBuffer;
            }

            return total;
        }

        public static void ConsumeSewage(ThingComp pipeComp, float volume)
        {
            var sewageTanks = ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.Sewers;

            if (sewageTanks?.Count() > 0)
            {
                float sewageToConsumePerTank = volume / sewageTanks.Count();

                foreach (DubsBadHygiene.CompSewageHandler currSewageTank in sewageTanks)
                {
                    currSewageTank.sewageBuffer = Mathf.Max(currSewageTank.sewageBuffer - sewageToConsumePerTank, 0);
                }
            }
        }
    }
}
