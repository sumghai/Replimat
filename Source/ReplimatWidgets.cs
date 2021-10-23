using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public static class ReplimatWidgets
    {
		public static void IntEntry(Rect rect, ref int value, ref string editBuffer, int multiplier = 1, int min = 1, int max = 100)
		{			
			int num = (int)rect.width / 8;
			int change;
			if (Widgets.ButtonText(new Rect(rect.xMin, rect.yMin, num, rect.height), "min".Translate().CapitalizeFirst()))
			{
				if (value != min)
				{
					value = min;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
				else 
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
				
			}
			if (Widgets.ButtonText(new Rect(rect.xMin + (float)num, rect.yMin, num, rect.height), (-10 * multiplier).ToStringCached()))
			{
				change = 10 * multiplier * GenUI.CurrentAdjustmentMultiplier();
				if (value - change >= min)
				{
					value -= change;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
				else 
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			if (Widgets.ButtonText(new Rect(rect.xMin + (float)(num * 2), rect.yMin, num, rect.height), (-1 * multiplier).ToStringCached()))
			{
				change = multiplier * GenUI.CurrentAdjustmentMultiplier();
				if (value - change >= min)
				{
					value -= change;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			if (Widgets.ButtonText(new Rect(rect.xMax - (float)(num  * 3), rect.yMin, num, rect.height), "+" + multiplier.ToStringCached()))
			{
				change = multiplier * GenUI.CurrentAdjustmentMultiplier();
				if (value + change <= max)
				{
					value += change;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			if (Widgets.ButtonText(new Rect(rect.xMax - (float)(num * 2), rect.yMin, num, rect.height), "+" + (10 * multiplier).ToStringCached()))
			{
				change = 10 * multiplier * GenUI.CurrentAdjustmentMultiplier();
				if (value + change <= max)
				{
					value += change;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			if (Widgets.ButtonText(new Rect(rect.xMax - (float)num, rect.yMin, num, rect.height), "max".Translate().CapitalizeFirst()))
			{
				if (value != max)
				{
					value = max;
					editBuffer = value.ToStringCached();
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			Widgets.TextFieldNumeric(new Rect(rect.xMin + (float)(num * 3), rect.yMin, rect.width - (float)(num * 6), rect.height), ref value, ref editBuffer, min, max);
		}
	}
}