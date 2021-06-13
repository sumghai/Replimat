using RimWorld.BaseGen;
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
                    if (thingList[i] is Building_ReplimatCorpseRecycler building_ReplimatCorpseRecycler && !corpseRecyclers.Contains(building_ReplimatCorpseRecycler))
                    {
                        corpseRecyclers.Add(building_ReplimatCorpseRecycler);
                    }
                }
            }

            corpseRecyclers.Clear();
        }
    }
}
