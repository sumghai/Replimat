using AlienRace;
using System.Linq;
using UnityEngine;
using Verse;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public class ModCompatibility
    {
        public static bool AlienRacesIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("erdelf.HumanoidAlienRaces".ToLower())).Any();

        public static bool VanillaCookingExpandedIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("VanillaExpanded.VCookE".ToLower())).Any();

        public static bool DbhIsActive => ModLister.AllInstalledMods.Where(x => x.Active && x.PackageId.Equals("Dubwise.DubsBadHygiene".ToLower())).Any();

        public static bool AlienRaceHasOrganicFlesh(Pawn pawn)
        {
            ThingDef_AlienRace raceDef = pawn.def as ThingDef_AlienRace;
            return raceDef?.alienRace.compatibility.IsFlesh ?? true;
        }

        public static bool AlienCorpseHasOrganicFlesh(ThingDef def)
        {
            ThingDef corpseAlienRace = ThingDef.Named(def.ToString().Substring("Corpse_".Length));

            if (corpseAlienRace.race.Humanlike && corpseAlienRace is ThingDef_AlienRace raceDef)
            {                
                return raceDef?.alienRace.compatibility.IsFlesh ?? true;
            }

            return false;
        }

        public static float DbhGetAvailableSewage(ThingComp pipeComp)
        {
            float total = 0;

            foreach (var sewer in ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.Sewers)
            {
                total += sewer.sewageBuffer;
            }

            return total;
        }

        public static void DbhConsumeSewage(ThingComp pipeComp, float volume)
        {
            var sewageTanks = ((DubsBadHygiene.CompPipe)pipeComp).pipeNet.Sewers;

            if (sewageTanks?.Count() > 0)
            {
                float sewageToConsumePerTank = volume / sewageTanks.Count();

                foreach (DubsBadHygiene.CompSewageHandler currSewageTank in sewageTanks)
                {
                    currSewageTank.sewageBuffer = Mathf.Max(currSewageTank.sewageBuffer - sewageToConsumePerTank, 0);
                }
            }
        }
    }
}