using System.Linq;
using Verse;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public class ModCompatibility
    {
        public static bool AlienRacesIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("erdelf.HumanoidAlienRaces".ToLower())).Any();

        public static bool VanillaCookingExpandedIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("VanillaExpanded.VCookE".ToLower())).Any();

        public static bool DbhIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("Dubwise.DubsBadHygiene".ToLower())).Any();
    }
}