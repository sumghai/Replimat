using System;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class Settings : ModSettings
    {
        public bool PrioritizeFoodQuality = true;
        public static float HopperRefillThresholdPercent = 0.1f;
        public bool EnableIncidentSpill = true;
        public bool EnableIncidentKibble = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PrioritizeFoodQuality, "PrioritizeFoodQuality", true, true);
            Scribe_Values.Look(ref HopperRefillThresholdPercent, "HopperRefillThresholdPercent", 0.5f, true);
            Scribe_Values.Look(ref EnableIncidentSpill, "EnableIncidentSpill", true, true);
            Scribe_Values.Look(ref EnableIncidentKibble, "EnableIncidentKibble", true, true);
        }

        public void Draw(Rect canvas)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(canvas);

            // Do general settings
            Text.Font = GameFont.Medium;
            listingStandard.Label("Replimat_Settings_HeaderGeneral".Translate());
            Text.Font = GameFont.Small;
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Replimat_Settings_PrioritizeFoodQuality_Title".Translate(),
                ref PrioritizeFoodQuality, "Replimat_Settings_PrioritizeFoodQuality_Desc".Translate());
            listingStandard.Label("Replimat_Settings_HopperRefillThresholdPercent_Title".Translate() + ": " + HopperRefillThresholdPercent.ToStringPercent("F0"), -1f, "Replimat_Settings_HopperRefillThresholdPercent_Desc".Translate());
            HopperRefillThresholdPercent = (float)Math.Round(listingStandard.Slider(HopperRefillThresholdPercent, 0.05f, 1f), 2);            

            // Do incident settings
            Text.Font = GameFont.Medium;
            listingStandard.Label("Replimat_Settings_HeaderIncidents".Translate());
            Text.Font = GameFont.Small;
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentSpill_Title".Translate(), ref EnableIncidentSpill, "Replimat_Settings_EnableIncidentSpill_Desc".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentKibble_Title".Translate(), ref EnableIncidentKibble, "Replimat_Settings_EnableIncidentKibble_Desc".Translate());
            listingStandard.End();
        }
    }
}