using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Replimat
{
    class Building_ReplimatAnimalFeeder : Building_Storage
    {
        public static int KibbleReplicateDuration = GenTicks.SecondsToTicks(2f);

        public CompPowerTrader powerComp;

        public int ReplicatingTicks = 0;

        public float volumePerKibble = ReplimatUtility.convertMassToFeedstockVolume(ThingDefOf.Kibble.BaseMass);

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputerDef).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return Position;
        }

        public bool HasEnoughFeedstockInHopperForIncident(float stockNeeded)
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            string copyStr = "CommandCopyZoneSettingsLabel".Translate();
            string pasteStr = "CommandPasteZoneSettingsLabel".Translate();
            foreach (Gizmo g in base.GetGizmos())
            {
                if (g is Command_Action act && (act.defaultLabel == copyStr || act.defaultLabel == pasteStr))
                {
                    continue;
                }
                yield return g;
            }
        }

        public bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.convertMassToFeedstockVolume(1f);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override void Draw()
        {
            base.Draw();

            if (ReplicatingTicks > 0)
            {
                float alpha;
                float quart = KibbleReplicateDuration * 0.25f;
                if (ReplicatingTicks < quart)
                {
                    alpha = Mathf.InverseLerp(0, quart, ReplicatingTicks);
                }
                else if (ReplicatingTicks > quart * 3f)
                {
                    alpha = Mathf.InverseLerp(KibbleReplicateDuration, quart * 3f, ReplicatingTicks);
                }
                else
                {
                    alpha = 1f;
                }

                Graphics.DrawMesh(GraphicsLoader.replimatAnimalFeederGlow.MeshAt(base.Rotation), this.DrawPos + Altitudes.AltIncVect, Quaternion.identity,
                    FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatAnimalFeederGlow.MatAt(base.Rotation, null), alpha), 0);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            if (this.IsHashIntervalTick(60))
            {
                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);
                Thing foodInFeeder = list.FirstOrDefault(x => x.def.IsNutritionGivingIngestible);

                if (foodInFeeder == null)
                {
                    Log.Message(this.ThingID.ToString() + " is empty");

                    int maxKib = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerKibble);
                    maxKib = Mathf.Min(maxKib, 75);

                    if (maxKib > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerKibble * maxKib))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);
                        this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

                        Thing t = ThingMaker.MakeThing(ThingDefOf.Kibble, null);
                        t.stackCount = maxKib;
                        GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Direct);
                    }

                }
                else if (foodInFeeder.def == ThingDefOf.Kibble && foodInFeeder.stackCount < 20)
                {
                    Log.Message(this.ThingID.ToString() + " currently has " + foodInFeeder.stackCount.ToString() + " units of " + foodInFeeder.def.label.ToString());

                    int refill = Mathf.Min(foodInFeeder.def.stackLimit - foodInFeeder.stackCount, 75);
                    int maxKib = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerKibble);
                    maxKib = Mathf.Min(maxKib, refill);

                    if (maxKib > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerKibble * maxKib))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);
                        this.def.building.soundDispense.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

                        foodInFeeder.stackCount += maxKib;
                    }
                }
            }

            powerComp.PowerOutput = -125f;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -1500f;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (!HasComputer)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Requires connection to Replimat Computer");
            }
            else if (!HasEnoughFeedstockInHoppers())
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Insufficient Feedstock");
            }
            else
            { }

            return stringBuilder.ToString();
        }
    }
}
