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
        private static List<Building_ReplimatTerminal> tmpReplimatTerminals = new List<Building_ReplimatTerminal>();

        private List<Building_ReplimatTerminal> ReplimatCandidates(Map map)
        {
            IncidentWorker_ReplimatMalfunctionKibble.tmpReplimatTerminals.Clear();
            List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Building_ReplimatTerminal building_ReplimatTerminal = allBuildingsColonist[i] as Building_ReplimatTerminal;
                if (building_ReplimatTerminal != null && building_ReplimatTerminal.powerComp.PowerOn)
                {
                    IncidentWorker_ReplimatMalfunctionKibble.tmpReplimatTerminals.Add(building_ReplimatTerminal);
                }
            }
            return IncidentWorker_ReplimatMalfunctionKibble.tmpReplimatTerminals;
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return this.ReplimatCandidates(map).Any<Building_ReplimatTerminal>();
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Building_ReplimatTerminal> list = this.ReplimatCandidates(map);
            if (!list.Any<Building_ReplimatTerminal>())
            {
                return false;
            }
            Building_ReplimatTerminal building_ReplimatTerminal = list.RandomElement<Building_ReplimatTerminal>();

            Thing t = ThingMaker.MakeThing(ThingDef.Named("Kibble"), null);
            t.stackCount = 75 * 3;
            GenPlace.TryPlaceThing(t, building_ReplimatTerminal.InteractionCell, map, ThingPlaceMode.Near);

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(building_ReplimatTerminal.Position, map, false), null);
            return true;
        }
    }
}
