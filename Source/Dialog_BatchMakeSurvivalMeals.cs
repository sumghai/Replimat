using System;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class Dialog_BatchMakeSurvivalMeals : Window
    {
		public string title;

		public int min;

		public int max;

		public float roundTo = 1f;

		private Action<int> confirmAction;

		private int curValue;

		private string mealCountEditBuffer;

		public override Vector2 InitialSize => new Vector2(450f, 150f);

		public Dialog_BatchMakeSurvivalMeals(string title, int min, int max, Action<int> confirmAction, int startingValue = int.MinValue)
		{
			this.title = title;
			this.min = min;
			this.max = max;
			this.confirmAction = confirmAction;
			forcePause = true;
			closeOnClickedOutside = true;
			curValue = (startingValue == int.MinValue) ? min : startingValue;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect1 = new Rect(inRect.x, inRect.y, inRect.width, Text.CalcSize(title).y);
			Widgets.Label(rect1, title);
			Text.Anchor = TextAnchor.UpperLeft;

			Rect rect2 = new Rect(inRect.x, inRect.height / 2f - 15f, inRect.width, 30f);
			ReplimatWidgets.IntEntry(rect2, ref curValue, ref mealCountEditBuffer, 1, min, max);
			if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, inRect.width / 2f, 30f), "CancelButton".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f, inRect.yMax - 30f, inRect.width / 2f, 30f), "OK".Translate()))
			{
				Close();
				confirmAction(curValue);
			}
		}
	}
}
