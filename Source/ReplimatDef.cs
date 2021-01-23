using RimWorld;
using Verse;

namespace Replimat
{
    [DefOf]
    public class ReplimatDef
    {
        static ReplimatDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ReplimatDef));
        }

        public static JobDef LoadReplimatCorpseRecycler;

        public static ThingDef ReplimatComputer;
        public static ThingDef ReplimatTerminal;
        public static ThingDef ReplimatAnimalFeeder;
        public static ThingDef ReplimatFeedTank;
        public static ThingDef ReplimatHopper;
        public static ThingDef ReplimatCorpseRecycler;
    }
}
