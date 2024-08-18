using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replimat
{
    public class IncidentWorker_ReplimatMalfunctionSmokeleaf : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return ReplimatMod.Settings.EnableIncidentSmokeleaf &&
                map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal).Any();
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> list = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal);

            if (!list.Any())
            {
                // If there are no Terminals or Animal Feeders, break out of execution early
                return false;
            }

            Thing building_ReplimatTerminal = list.RandomElement();
            Building_ReplimatTerminal currentTerminal = building_ReplimatTerminal as Building_ReplimatTerminal;

            ThingDef smokeleaf = ThingDef.Named("SmokeleafJoint");
            int smokeleafJoints = 500;
            float volumeOfFeedstockToWaste = ReplimatUtility.ConvertMassToFeedstockVolume(smokeleafJoints * smokeleaf.BaseMass);

            if (currentTerminal.powerComp.PowerOn && currentTerminal.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToWaste))
            {
                currentTerminal.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToWaste);
                Thing t = ThingMaker.MakeThing(smokeleaf, null);
                t.stackCount = smokeleafJoints;
                GenPlace.TryPlaceThing(t, building_ReplimatTerminal.InteractionCell, map, ThingPlaceMode.Near);

                TaggedString letterLabel = def.letterLabel;
                TaggedString letterText = def.letterText.Formatted(currentTerminal.def.label).Resolve();

                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(building_ReplimatTerminal.Position, map, false), null);
                return true;
            }
            return false;
        }
    }
}
