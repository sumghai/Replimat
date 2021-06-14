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

        public static ThoughtDef Thought_RecycledStrangerCorpseInReplimat;
        public static ThoughtDef Thought_RecycledColonistCorpseInReplimat;
        public static ThoughtDef Thought_RecycledCorpseInReplimatPsychopath;
        public static ThoughtDef Thought_KnowRecycledStrangerCorpseInReplimat;
        public static ThoughtDef Thought_KnowRecycledColonistCorpseInReplimat;
        public static ThoughtDef Thought_KnowRecycledCorpseInReplimatPsychopath;
    }
}
