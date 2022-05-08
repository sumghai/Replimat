using AlienRace;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public class ModCompatibility
    {
        public static bool AlienRacesIsActive => ModsConfig.IsActive("erdelf.HumanoidAlienRaces");

        public static bool VanillaCookingExpandedIsActive => ModsConfig.IsActive("VanillaExpanded.VCookE");

        public static bool DbhIsActive => ModsConfig.IsActive("Dubwise.DubsBadHygiene");

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