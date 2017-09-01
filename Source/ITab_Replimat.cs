using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace Replimat
{
    public class ITab_Replimat : ITab
    {
        private const float TopAreaHeight = 35f;

        private Vector2 scrollPosition = default(Vector2);

        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        public override bool IsVisible
        {
            get
            {
                return true;
            }
        }

        public ITab_Replimat()
        {
            this.size = ITab_Replimat.WinSize;
            this.labelKey = "TabReplimat";
        }

        protected override void FillTab()
        {

            Rect position = new Rect(0f, 0f, ITab_Replimat.WinSize.x, ITab_Replimat.WinSize.y).ContractedBy(10f);
            Widgets.Label(position, "Meals available from this Terminal:");
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;

            //List<ThingDef> allMeals = ThingCategoryDefOf.FoodMeals.DescendantThingDefs.ToList();

            //String text = "Meal list: ";

            //foreach (var currentMeal in allMeals)
            //{
            //    text += currentMeal.defName + ", ";
            //}

            //Log.Message(text);

            GUI.EndGroup();
        }
    }
}
