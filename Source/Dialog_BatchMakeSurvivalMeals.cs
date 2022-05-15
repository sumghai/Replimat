using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class Dialog_BatchMakeSurvivalMeals : Window
    {
		public float totalAvailableFeedstock;
		
		public ThingDef selectedSurvivalMealType;

		public float roundTo = 1f;

		private Action<int, ThingDef> confirmAction;

		private int curValue;

		private string mealCountEditBuffer;

		public override Vector2 InitialSize => new Vector2(450f, 180f);

		public Dialog_BatchMakeSurvivalMeals(float availableFeedstock, ThingDef selectedSurvivalMealType, Action<int, ThingDef> confirmAction, int startingValue = int.MinValue)
		{
			this.totalAvailableFeedstock = availableFeedstock;
			this.selectedSurvivalMealType = selectedSurvivalMealType;
			this.confirmAction = confirmAction;
			forcePause = true;
			closeOnClickedOutside = true;
			curValue = (startingValue == int.MinValue) ? 1 : startingValue;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperCenter;

			float totalAvailableFeedstockMass = ReplimatUtility.ConvertFeedstockVolumeToMass(totalAvailableFeedstock);
			int maxPossibleSurvivalMeals = (int)Math.Floor(totalAvailableFeedstockMass / selectedSurvivalMealType.BaseMass);

			string title = "SetSurvivalMealBatchSize".Translate(maxPossibleSurvivalMeals);

			Rect rect1 = new Rect(inRect.x, inRect.y, inRect.width, Text.CalcSize(title).y);
			Widgets.Label(rect1, title);
			Text.Anchor = TextAnchor.UpperLeft;

			Rect rect2 = new Rect(inRect.x, inRect.yMax - 100f, inRect.width, 30f);
			if (Widgets.ButtonText(rect2, "SetSurvivalMealType".Translate(selectedSurvivalMealType.LabelCap)))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef currentSurvivalMealType in ReplimatUtility.GetSurvivalMealChoices())
				{
					list.Add(new FloatMenuOption(currentSurvivalMealType.LabelCap, delegate 
					{
						selectedSurvivalMealType = currentSurvivalMealType;
					}, currentSurvivalMealType));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}

			Rect rect3 = new Rect(inRect.x, inRect.yMax - 65f, inRect.width, 30f);
			ReplimatWidgets.IntEntry(rect3, ref curValue, ref mealCountEditBuffer, 1, 1, maxPossibleSurvivalMeals);

			if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f, 30f), "CancelButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f, inRect.yMax - 30f, inRect.width / 2f, 30f), "OK".Translate()))
			{
				Close();
				confirmAction(curValue, selectedSurvivalMealType);
			}
		}
	}
}
