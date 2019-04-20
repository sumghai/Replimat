using RimWorld;
using System;
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

        public static ThingDef ReplimatComputer;
        public static ThingDef ReplimatTerminal;
        public static ThingDef ReplimatAnimalFeeder;
        public static ThingDef ReplimatFeedTank;
        public static ThingDef ReplimatHopper;
    }
}
