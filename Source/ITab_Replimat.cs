using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using Verse.Sound;

namespace Replimat
{
    public class ITab_Replimat : ITab
    {
        private static readonly Color FulfilledPrerequisiteColor = Color.green;

        private static readonly Color MissingPrerequisiteColor = Color.red;

        private const float InteractivityDelay = 0.5f;
        public Action closeAction;

        public Color screenFillColor = Color.clear;
        public Vector2 scrollPos = Vector2.zero;

        private static readonly Vector2 WinSize = new Vector2(500f, 500f);

        private Building_ReplimatTerminal Terminal
        {
            get
            {
                return (base.SelObject as Building_ReplimatTerminal);
            }
        }

        public ITab_Replimat()
        {
            this.size = WinSize;
            this.labelKey = "Managefuel";
        }

        protected override void FillTab()
        {
            ThingFilter defaults = Terminal.def.building.fixedStorageSettings.filter;

            Rect rect1 = new Rect(10f, 10f, 100f, 30f);
            Widgets.DrawMenuSection(rect1);
            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(rect1, "FoodQuality".Translate(Terminal.MaxPreferability), true, false, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (FoodPreferability item in FoodPreferability.GetValues(typeof(FoodPreferability)))
                {
                    // List<FloatMenuOption> benAffleck = list;

                    list.Add(new FloatMenuOption(item.ToString(), delegate
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        Terminal.MaxPreferability = item;

                    }, MenuOptionPriority.Default));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Rect OutRect = new Rect(0f, 50f, WinSize.x, WinSize.y - 50f).ContractedBy(20f);

            Widgets.DrawMenuSection(OutRect, true);

            float height = 30f * defaults.AllowedDefCount + 50f;
            Rect rect = new Rect(0f, 0f, OutRect.width - 16f, height);

            Widgets.BeginScrollView(OutRect, ref scrollPos, rect);

            Rect position = rect.ContractedBy(10f);

            GUI.BeginGroup(position);
            int num = 0;

            foreach (var item in defaults.AllowedThingDefs.Where(x => x.ingestible.preferability <= Terminal.MaxPreferability))
            {
                Rect rect2 = new Rect(0f, num, position.width, 30f);

                Rect rect3 = new Rect(rect2);
                rect3.x += 6f;
                rect3.width -= 6f;

                DoAreaRow(rect3, item);

                num += 30;
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DoAreaRow(Rect rect, ThingDef meal)
        {
            rect = rect.ContractedBy(4f);
            if (Mouse.IsOver(rect))
            {
                GUI.color = Color.blue;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }

            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            widgetRow.Icon(meal.uiIcon, null);
            widgetRow.Gap(4f);
            widgetRow.Label(meal.label, 150f);

            Rect rectum = new Rect(widgetRow.FinalX, 0, 150f, rect.height);
            float num2 = rectum.width / 2f;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            Rect rect2 = new Rect(rectum.x, rectum.y, num2, rectum.height);
            DoAreaSelector(rect2, meal, true);

            Rect rect3 = new Rect(rectum.x + num2, rectum.y, num2, rectum.height);
            DoAreaSelector(rect3, meal, false);
            Text.Font = GameFont.Small;
            Text.WordWrap = true;

            GUI.EndGroup();
        }


        private void DoAreaSelector(Rect rect, ThingDef meal, bool selection)
        {
            ThingFilter parentFilter = Terminal.MealFilter.filter;

            GUI.DrawTexture(rect, selection ? GraphicsLoader.grey : GraphicsLoader.red);
            Text.Anchor = TextAnchor.MiddleLeft;
            string text = selection ? "Allow".Translate() : "Restrict".Translate();
            Rect rect2 = rect;
            rect2.xMin += 3f;
            rect2.yMin += 2f;
            Widgets.Label(rect2, text);
            if (parentFilter.Allows(meal) == selection)
            {
                Widgets.DrawBox(rect, 2);
            }
            if (Mouse.IsOver(rect))
            {
                if (Input.GetMouseButton(0) && parentFilter.Allows(meal) != selection)
                {
                    Terminal.MealFilter.filter.SetAllow(meal, selection);
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}