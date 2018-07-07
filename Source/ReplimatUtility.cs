using Verse;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Text;
using Verse.AI;

namespace Replimat
{
    public static class ReplimatUtility
    {
        private static HashSet<Thing> filtered = new HashSet<Thing>();

        private const float nutrientFeedStockDensity = 1.07f; // 1.07 kg/L (equivalent to Abbott's Ensure Nutritional Shake)

        public static float convertMassToFeedstockVolume(float mass)
        {
            return mass / nutrientFeedStockDensity;
        }

        public static float convertFeedstockVolumeToMass(float volume)
        {
            return volume * nutrientFeedStockDensity;
        } 
    }
}