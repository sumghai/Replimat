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
            ThingDef_AlienRace corpseAlienRace = ThingDef.Named(def.ToString().Substring("Corpse_".Length)) as ThingDef_AlienRace;
            return !corpseAlienRace.alienRace.compatibility.IsFlesh;
        }
    }
}