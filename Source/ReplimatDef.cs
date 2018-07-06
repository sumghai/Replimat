using RimWorld;
using System;
using Verse;

namespace Replimat
{
    public class ReplimatDef
    {
        public static ThingDef ReplimatComputerDef = ThingDef.Named("ReplimatComputer");

        public static ThingDef ReplimatTerminalDef = ThingDef.Named("ReplimatTerminal");
        public static ThingDef ReplimatAnimalFeederDef = ThingDef.Named("ReplimatAnimalFeeder");
        public static ThingDef FeedTankDef = ThingDef.Named("ReplimatFeedTank");
        public static ThingDef ReplimatHopper = ThingDef.Named("ReplimatHopper");


        public static TraitDef SensitiveTaster = TraitDef.Named("SensitiveTaster");

        public static ThoughtDef AteReplicatedFood = ThoughtDef.Named("AteReplicatedFood");
    }
}
