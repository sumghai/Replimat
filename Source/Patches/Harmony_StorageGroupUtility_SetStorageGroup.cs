using HarmonyLib;
using RimWorld;

namespace Replimat
{
    // For Animal Feeders, Synchronize the animal feed type with that of the first Feeder in the storage group
    [HarmonyPatch(typeof(StorageGroupUtility), nameof(StorageGroupUtility.SetStorageGroup))]
    public static class Harmony_StorageGroupUtility_SetStorageGroup
    {
        public static void Postfix(this IStorageGroupMember member, StorageGroup newGroup)
        {
            if (member is Building_ReplimatAnimalFeeder feeder && newGroup != null)
            {
                Building_ReplimatAnimalFeeder leader = member.Group.members[0] as Building_ReplimatAnimalFeeder;
                feeder.CurrentAnimalFeedDef = leader.CurrentAnimalFeedDef;       
            }
        }
    }
}
