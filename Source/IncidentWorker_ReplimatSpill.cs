using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Replimat
{
    public class IncidentWorker_ReplimatSpill : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return ReplimatMod.Settings.EnableIncidentSpill && 
                (map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal).Any() |
                map.listerThings.ThingsOfDef(ReplimatDef.ReplimatAnimalFeeder).Any());
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            List<Thing> listOfTerminals = map.listerThings.AllThings.Where(t => t.def == ReplimatDef.ReplimatTerminal || t.def == ReplimatDef.ReplimatAnimalFeeder).ToList();

            if (!listOfTerminals.Any())
            {
                // If there are no Terminals or Animal Feeders, break out of execution early
                return false;
            }

            Thing targetTerminal = listOfTerminals.RandomElement();

            int volumeOfFeedstockToSpill = 3; // Volume in litres

            if (targetTerminal.def == ReplimatDef.ReplimatTerminal)
            {
                if (targetTerminal is Building_ReplimatTerminal currentTerminal && currentTerminal.powerComp.PowerOn && currentTerminal.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentTerminal.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.TryMakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterLabel = "LetterLabelReplimatSpill".Translate();

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentTerminal.def.label
                    });

                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map));
                    return true;
                }

            }
            else if (targetTerminal.def == ReplimatDef.ReplimatAnimalFeeder)
            {
                if (targetTerminal is Building_ReplimatAnimalFeeder currentAnimalFeeder && currentAnimalFeeder.powerComp.PowerOn && currentAnimalFeeder.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentAnimalFeeder.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.TryMakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterLabel = "LetterLabelReplimatSpill".Translate();

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentAnimalFeeder.def.label
                    });

                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map));
                    return true;
                }
            }

            return false;
        }
    }
}
