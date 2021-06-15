using AlienRace;
using Verse;

namespace Replimat
{
    public class ModCompatibility
    {
        public static bool AlienRacesIsActive => ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");

        public static bool AlienRaceHasOrganicFlesh(Pawn pawn)
        {
            ThingDef_AlienRace raceDef = pawn.def as ThingDef_AlienRace;
            return raceDef?.alienRace.compatibility.IsFlesh ?? true;
        }

        public static bool AlienCorpseHasOrganicFlesh(ThingDef def)
        {
            ThingDef corpseAlienRace = ThingDef.Named(def.ToString().Substring("Corpse_".Length));

            if (corpseAlienRace.race.Humanlike)
            {
                ThingDef_AlienRace test = corpseAlienRace as ThingDef_AlienRace;
                
                return test.alienRace.compatibility.IsFlesh;
            }

            return false;
        }
    }
}