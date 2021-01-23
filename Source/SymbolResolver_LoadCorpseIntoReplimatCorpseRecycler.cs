using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace Replimat
{
    public class SymbolResolver_LoadCorpseIntoReplimatCorpseRecycler : SymbolResolver
    {
        private static List<Building_ReplimatCorpseRecycler> corpseRecyclers = new List<Building_ReplimatCorpseRecycler>();

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            corpseRecyclers.Clear();
            foreach (IntVec3 item in rp.rect)
            {
                List<Thing> thingList = item.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Building_ReplimatCorpseRecycler building_ReplimatCorpseRecycler = thingList[i] as Building_ReplimatCorpseRecycler;
                    if (building_ReplimatCorpseRecycler != null && !corpseRecyclers.Contains(building_ReplimatCorpseRecycler))
                    {
                        corpseRecyclers.Add(building_ReplimatCorpseRecycler);
                    }
                }
            }

            for (int j = 0; j < corpseRecyclers.Count; j++)
            {
                /**if (!corpseRecyclers[j].CorpseFinishedProcessing)
                {
                    Log.Warning("Replimat :: SymbolResolver_LoadCorpseIntoReplimatCorpseRecycler - Do something here for " + corpseRecyclers[j].ToString() + " that has not yet finished processing?" );
                }*/
            }

            corpseRecyclers.Clear();
        }
    }
}
