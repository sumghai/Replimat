using AlienRace;
using System.Linq;
using Verse;

namespace Replimat
{
    public class ModCompatibility
    {
        public static bool AlienRacesIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId == "erdelf.HumanoidAlienRaces".ToLower()).Any();

        public static bool SaveOurShip2IsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId == "kentington.saveourship2".ToLower()).Any();

        public static bool VanillaCookingExpandedIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId == "VanillaExpanded.VCookE".ToLower()).Any();

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