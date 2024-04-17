using AlienRace;
using Verse;

namespace Replimat
{
    public static class AlienRacesCompatibility
    {
        public static bool RaceHasOrganicFlesh(Pawn pawn)
        {
            ThingDef_AlienRace raceDef = pawn.def as ThingDef_AlienRace;
            return raceDef?.alienRace.compatibility.IsFlesh ?? true;
        }

        public static bool CorpseHasOrganicFlesh(ThingDef def)
        {
            ThingDef_AlienRace raceDef = CachedData.GetRaceFromRaceProps(def.race) as ThingDef_AlienRace;
            return raceDef?.alienRace.compatibility.IsFlesh ?? true;
        }

    }
}
