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

    [StaticConstructorOnStartup]
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

        public static List<Building_ReplimatFeedTank> GetTanks(this PowerNet net)
        {
            List<Building_ReplimatFeedTank> tanks;
            tanks = net.Map.listerThings.ThingsOfDef(ReplimatDef.FeedTankDef).OfType<Building_ReplimatFeedTank>().Where(x => x.PowerComp.PowerNet == net && x.HasComputer).ToList();
            return tanks;
        }

        public static bool TryConsumeFeedstock(this PowerNet net, float feedstockNeeded)
        {

            if (feedstockNeeded <= 0f)
            {
                Log.Warning("Replimat: Tried to draw 0 feedstock!");
                return false;
            }

            List<Building_ReplimatFeedTank> feedstockTanks = net.GetTanks();

            if (!feedstockTanks.NullOrEmpty())
            {
                float totalAvailableFeedstock = feedstockTanks.Sum(x => x.storedFeedstock);

                feedstockTanks.Shuffle();

                if (totalAvailableFeedstock >= feedstockNeeded)
                {
                    float feedstockLeftToConsume = feedstockNeeded;

                    foreach (var currentTank in feedstockTanks)
                    {
                        if (feedstockLeftToConsume > 0)
                        {
                            float num = Math.Min(feedstockLeftToConsume, currentTank.StoredFeedstock);
                            currentTank.DrawFeedstock(num);

                            feedstockLeftToConsume -= num;

                            if (feedstockLeftToConsume <= 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }

                    Log.Warning("Replimat: Tried but tanks ran out of feedstock, needed:" + feedstockNeeded);
                    return false;
                }
                else
                {
                    Log.Warning("Replimat: Tanks didn't have enough feedstock, needed:" + feedstockNeeded);
                    return false;
                }
            }

            Log.Warning("Replimat: Tried to draw feedstock from non-existent tanks!");
            return false;

        }
    }
}
