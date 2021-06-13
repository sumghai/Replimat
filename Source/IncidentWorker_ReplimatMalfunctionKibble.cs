using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Replimat
{
    public class IncidentWorker_ReplimatMalfunctionKibble : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return ReplimatMod.Settings.EnableIncidentKibble &&
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

            ThingDef kibble = ThingDef.Named("Kibble");
            int unitsOfKibble = 75 * 3;
            float volumeOfFeedstockToWaste = ReplimatUtility.ConvertMassToFeedstockVolume(unitsOfKibble * kibble.BaseMass);

            if (currentTerminal.powerComp.PowerOn && currentTerminal.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToWaste))
            {
                currentTerminal.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToWaste);
                Thing t = ThingMaker.MakeThing(kibble, null);
                t.stackCount = unitsOfKibble;
                GenPlace.TryPlaceThing(t, building_ReplimatTerminal.InteractionCell, map, ThingPlaceMode.Near);

                string letterLabel = "LetterLabelReplimatMalfunctionKibble".Translate();

                string letterText = "LetterTextReplimatMalfunctionKibble".Translate(new object[]
                {
                        currentTerminal.def.label
                });

                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(building_ReplimatTerminal.Position, map, false), null);
                return true;
            }
            return false;
        }
    }
}
