using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;

namespace Replimat
{
    public class IncidentWorker_ReplimatMalfunctionKibble : IncidentWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal).Any();
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> list = map.listerThings.ThingsOfDef(ReplimatDef.ReplimatTerminal);
            if (!list.Any())
            {
                return false;
            }
            Thing building_ReplimatTerminal = list.RandomElement();

            Thing t = ThingMaker.MakeThing(ThingDef.Named("Kibble"), null);
            t.stackCount = 75 * 3;
            GenPlace.TryPlaceThing(t, building_ReplimatTerminal.InteractionCell, map, ThingPlaceMode.Near);

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(building_ReplimatTerminal.Position, map, false), null);
            return true;
        }
    }
}
