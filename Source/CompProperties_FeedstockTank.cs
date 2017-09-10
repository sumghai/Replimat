using Verse;

namespace Replimat
{
    public class CompProperties_FeedstockTank : CompProperties
    {
        public float storedFeedstockMax = 8000f; //8000L capacity for a 2m diameter and 2m high liquid tank

        public CompProperties_FeedstockTank()
        {
            compClass = typeof(CompFeedstockTank);
        }
    }
}