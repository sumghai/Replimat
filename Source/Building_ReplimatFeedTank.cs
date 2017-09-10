using System.Text;
using Verse;
using RimWorld;

namespace Replimat
{
    public class Building_ReplimatFeedTank : Building
    {
        public float storedFeedstockMax = 8000f; //8000L capacity for a 2m diameter and 2m high liquid tank

        public float storedFeedstock;
    }
}

