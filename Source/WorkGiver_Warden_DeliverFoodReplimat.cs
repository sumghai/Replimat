using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class WorkGiver_Warden_DeliverFoodReplimat : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!base.ShouldTakeCareOfPrisoner(pawn, t))
            {
                return null;
            }
            Pawn pawn2 = (Pawn)t;
            if (!pawn2.guest.CanBeBroughtFood)
            {
                return null;
            }
            if (!pawn2.Position.IsInPrisonCell(pawn2.Map))
            {
                return null;
            }
            if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
            {
                return null;
            }
            if (WardenFeedUtility.ShouldBeFed(pawn2))
            {
                return null;
            }
            Thing thing;
            ThingDef def;
            if (!ReplimatUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out def, false, true, false, false, false))
            {
                return null;
            }
            if (thing.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
            {
                return null;
            }
            if (WorkGiver_Warden_DeliverFoodReplimat.FoodAvailableInRoomTo(pawn2))
            {
                return null;
            }
            return new Job(ReplimatDef.deliverFoodReplimatDef, thing, pawn2)
            {
                count = ReplimatUtility.WillIngestStackCountOf(pawn2, def),
                targetC = RCellFinder.SpotToChewStandingNear(pawn2, thing)
            };
        }

        public static bool FoodAvailableInRoomTo(Pawn prisoner)
        {
            if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverFoodReplimat.NutritionAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
            {
                return true;
            }
            var neededNutrition = 0.0f;
            var foodNutrition = 0.0f;
            var room = prisoner.GetRoom(RegionType.Set_Passable);
            if (room == null)
            {   // This should never actually happen...
                //Log.Message( "Prisoner is not in a room!" );
                return false;
            }
            for (int regionIndex = 0; regionIndex < room.RegionCount; ++regionIndex)
            {
                var region = room.Regions[regionIndex];

                var foodSources = region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
                if (
                    (prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) &&
                    (foodSources.Any((source) =>
                    {
                        
                            if (
                                (source is Building_NutrientPasteDispenser) &&
                                (((Building_NutrientPasteDispenser)source).CanDispenseNow)
                            )
                            {
                                return true;
                            }
                            if (
                                (source is Building_ReplimatTerminal) &&
                                (((Building_ReplimatTerminal)source).CanDispenseNow)
                            )
                            {
                                return true;
                            }
                        
                        return false;
                    }))
                )
                {
                    Log.Message( "Prisoner has access to a stocked food machine" );
                    return true;
                }
                for (int foodIndex = 0; foodIndex < foodSources.Count; ++foodIndex)
                {
                    var foodSource = foodSources[foodIndex];
                    if (!foodSource.def.IsIngestible || foodSource.def.ingestible.preferability > FoodPreferability.DesperateOnly)
                    {
                        foodNutrition += NutritionAvailableForFrom(prisoner, foodSource);
                    }
                }
                var pawns = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                for (int pawnIndex = 0; pawnIndex < pawns.Count; ++pawnIndex)
                {
                    var pawn = pawns[pawnIndex] as Pawn;
                    if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.RaceProps.WillAutomaticallyEat(pawn.carryTracker.CarriedThing)))
                    {
                        neededNutrition += pawn.needs.food.NutritionWanted;
                    }
                }
            }
            return foodNutrition + 0.5f >= neededNutrition;
        }

        private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
        {
            if (foodSource.def.IsNutritionGivingIngestible && p.RaceProps.WillAutomaticallyEat(foodSource))
            {
                return foodSource.def.ingestible.nutrition * (float)foodSource.stackCount;
            }
            if (p.RaceProps.ToolUser && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                Building_ReplimatTerminal building_ReplimatTerminal = foodSource as Building_ReplimatTerminal;
                if (building_ReplimatTerminal != null && building_ReplimatTerminal.CanDispenseNow)
                {
                    return 99999f;
                }

                Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
                if (building_NutrientPasteDispenser != null && building_NutrientPasteDispenser.CanDispenseNow)
                {
                    return 99999f;
                }
            }
            return 0f;
        }
    }
}
