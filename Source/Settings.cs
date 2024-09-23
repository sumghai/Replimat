using UnityEngine;
using Verse;

namespace Replimat
{
    public class Settings : ModSettings
    {
        public bool PrioritizeFoodQuality = true;
        public bool EnableIncidentSpill = true;
        public bool EnableIncidentKibble = true;
        public bool EnableIncidentSmokeleaf = true;
        public override void ExposeData()
        {
            base.ExposeData();
            var defaults = new Settings();
            Scribe_Values.Look(ref PrioritizeFoodQuality, "PrioritizeFoodQuality", true, true);
            Scribe_Values.Look(ref EnableIncidentSpill, "EnableIncidentSpill", true, true);
            Scribe_Values.Look(ref EnableIncidentKibble, "EnableIncidentKibble", true, true);
            Scribe_Values.Look(ref EnableIncidentSmokeleaf, "EnableIncidentSmokeleaf", true, true);
        }

        public void Draw(Rect canvas)
        {
            var listingStandard = new Listing_Standard()
            {
                ColumnWidth = (canvas.width - Window.StandardMargin) / 2
            };

            listingStandard.Begin(canvas);

            // First Column - general
            listingStandard.Gap();
            listingStandard.Header("Replimat_Settings_HeaderGeneral".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_PrioritizeFoodQuality_Title".Translate(),
                ref PrioritizeFoodQuality, "Replimat_Settings_PrioritizeFoodQuality_Desc".Translate());

            // Second Column - incidents
            listingStandard.NewColumn();
            listingStandard.Gap();

            listingStandard.Header("Replimat_Settings_HeaderIncidents".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentSpill_Title".Translate(), ref EnableIncidentSpill, "Replimat_Settings_EnableIncidentSpill_Desc".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentKibble_Title".Translate(), ref EnableIncidentKibble, "Replimat_Settings_EnableIncidentKibble_Desc".Translate());
            listingStandard.CheckboxLabeled("Replimat_Settings_EnableIncidentSmokeleaf_Title".Translate(), ref EnableIncidentSmokeleaf, "Replimat_Settings_EnableIncidentSmokeleaf_Desc".Translate());

            listingStandard.End();
        }
    }

    public static class UI_Extensions
    {
        public static void Header(this Listing_Standard list, TaggedString headerTitle)
        {
            Rect rect = list.GetRect(Text.CalcHeight(headerTitle, list.ColumnWidth));
            Widgets.DrawBoxSolid(rect, Widgets.OptionUnselectedBGFillColor);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, headerTitle);
            Text.Anchor = anchor;
            list.Gap(4);
        }
    }
}