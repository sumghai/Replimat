using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;

namespace Replimat
{
    public class IncidentWorker_ReplimatSpill : IncidentWorker
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

            ThingDef filthSlime = ThingDefOf.FilthSlime;
            FilthMaker.MakeFilth(building_ReplimatTerminal.InteractionCell, map, filthSlime, Rand.Range(10, 12));

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(building_ReplimatTerminal.Position, map, false), null);
            return true;
        }
    }
}
