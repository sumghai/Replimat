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
            ThingDef corpseAlienRace = ThingDef.Named(def.ToString().Substring("Corpse_".Length));

            if (corpseAlienRace.race.Humanlike && corpseAlienRace is ThingDef_AlienRace raceDef)
            {
                return raceDef?.alienRace.compatibility.IsFlesh ?? true;
            }

            return false;
        }

    }
}
