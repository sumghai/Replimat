﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;

namespace Replimat
{
    public class IncidentWorker_ReplimatSpill : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal).Any() &&
                map.listerThings.ThingsOfDef(ReplimatDef.ReplimatAnimalFeeder).Any();
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> listOfTerminals = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal);
            List<Thing> listOfAnimalFeeders = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatAnimalFeeder);
            List<Thing> list = listOfTerminals.Concat(listOfAnimalFeeders).ToList();

            if (!list.Any())
            {
                // If there are no Terminals or Animal Feeders, break out of execution early
                return false;
            }

            Thing targetTerminal = list.RandomElement();

            int volumeOfFeedstockToSpill = 3; // Volume in litres

            if (targetTerminal.def == ReplimatDef.ReplimatTerminal)
            {
                Building_ReplimatTerminal currentTerminal = targetTerminal as Building_ReplimatTerminal;

                if (currentTerminal.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentTerminal.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.MakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterLabel = "LetterLabelReplimatSpill".Translate();

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentTerminal.def.label
                    });

                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map, false), null);
                    return true;
                }

            }
            else if (targetTerminal.def == ReplimatDef.ReplimatAnimalFeeder)
            {
                Building_ReplimatAnimalFeeder currentAnimalFeeder = targetTerminal as Building_ReplimatAnimalFeeder;

                if (currentAnimalFeeder.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentAnimalFeeder.powerComp.PowerNet.TryConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.MakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterLabel = "LetterLabelReplimatSpill".Translate();

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentAnimalFeeder.def.label
                    });

                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map, false), null);
                    return true;
                }
            }
            else {

            }
            
            return false;
        }
    }
}
