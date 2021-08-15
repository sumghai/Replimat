using HarmonyLib;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class ReplimatMod : Mod
    {
        public static Settings Settings;

        public static bool allowForbidden;
        public static bool allowDispenserFull;
        public static Pawn getter;
        public static Pawn eater;
        public static bool allowSociallyImproper;
        public static bool BestFoodSourceOnMap;

        public ReplimatMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            var harmony = new Harmony("com.Replimat.patches");
            harmony.PatchAll();

            if (ModCompatibility.SaveOurShip2IsActive)
            {
                Log.Message("Replimat :: Save Our Ship 2 detected!");
            }

            if (ModCompatibility.AlienRacesIsActive)
            {
                Log.Message("Replimat :: Humanoid Alien Races detected!");
            }

            MP_Util.Bootup(harmony);
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.Draw(canvas);
            base.DoSettingsWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Replimat_SettingsCategory_Heading".Translate();
        }
    }
}
