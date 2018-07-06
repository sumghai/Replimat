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
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminalDef).Any() &&
                map.listerThings.ThingsOfDef(ReplimatDef.ReplimatAnimalFeederDef).Any();


        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> listOfTerminals = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminalDef);
            List<Thing> listOfAnimalFeeders = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatAnimalFeederDef);
            List<Thing> list = listOfTerminals.Concat(listOfAnimalFeeders).ToList();


            if (!list.Any())
            {
                return false;
            }
            else {
                //DEBUG
                Log.Message("Replimat - Terminals available for spill incident" );
            }

            Thing targetTerminal = list.RandomElement();

            int volumeOfFeedstockToSpill = 3; // Volume in litres

            if (targetTerminal.def == ReplimatDef.ReplimatTerminalDef)
            {
                Building_ReplimatTerminal currentTerminal = targetTerminal as Building_ReplimatTerminal;

                //DEBUG
                Log.Message("Replimat - " + currentTerminal.GetUniqueLoadID().ToString() +" selected for spill incident");

                if (currentTerminal.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentTerminal.ConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.MakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentTerminal.def.label
                    });

                    Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map, false), null);
                    return true;
                }

            }
            else if (targetTerminal.def == ReplimatDef.ReplimatAnimalFeederDef)
            {
                Building_ReplimatAnimalFeeder currentAnimalFeeder = targetTerminal as Building_ReplimatAnimalFeeder;

                //DEBUG
                Log.Message("Replimat - " + currentAnimalFeeder.GetUniqueLoadID().ToString() + " selected for spill incident");

                if (currentAnimalFeeder.HasEnoughFeedstockInHopperForIncident(volumeOfFeedstockToSpill))
                {
                    currentAnimalFeeder.ConsumeFeedstock(volumeOfFeedstockToSpill);
                    ThingDef filthSlime = ThingDefOf.Filth_Slime;
                    FilthMaker.MakeFilth(targetTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

                    string letterText = "LetterTextReplimatSpill".Translate(new object[]
                    {
                        currentAnimalFeeder.def.label
                    });

                    Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, LetterDefOf.NegativeEvent, new TargetInfo(targetTerminal.Position, map, false), null);
                    return true;
                }
            }
            else {

            }
            
            return false;
        }
    }
}