using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Replimat
{
    class Building_ReplimatAnimalFeeder : Building_Storage
    {
        public static int AnimalFeedReplicateDuration = GenTicks.SecondsToTicks(2f);

        public CompPowerTrader powerComp;

        public CompStateDependentPowerUse stateDependentPowerComp;

        public int ReplicatingTicks = 0;

        public List<ThingDef> AllowedAnimalFeedDefs;

        public ThingDef CurrentAnimalFeedDef = ThingDefOf.Kibble; // Default is Kibble

        public float volumePerAnimalFeed;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
            AllowedAnimalFeedDefs = def.building.fixedStorageSettings.filter.allowedDefs.ToList();
            volumePerAnimalFeed = ReplimatUtility.ConvertMassToFeedstockVolume(CurrentAnimalFeedDef.BaseMass);
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

            yield return new Command_Action
            {
                defaultLabel = "CommandGizmoSwitchAnimalFeed_Label".Translate(),
                defaultDesc = "CommandGizmoSwitchAnimalFeed_Desc".Translate(CurrentAnimalFeedDef.LabelCap),
                action = ToggleAnimalFeedDef,
                icon = CurrentAnimalFeedDef.uiIcon
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref CurrentAnimalFeedDef, "currentAnimalFeedDef");
        }

        public void ToggleAnimalFeedDef()
        {           
            int currentAnimalFeedDefNum = AllowedAnimalFeedDefs.IndexOf(CurrentAnimalFeedDef);
            currentAnimalFeedDefNum = (currentAnimalFeedDefNum + 1) % AllowedAnimalFeedDefs.Count;
            CurrentAnimalFeedDef = AllowedAnimalFeedDefs.ElementAt(currentAnimalFeedDefNum);
        }

        public bool HasEnoughFeedstockInHoppers()
        {
            float totalAvailableFeedstock = powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock);
            float stockNeeded = ReplimatUtility.ConvertMassToFeedstockVolume(1f);
            return totalAvailableFeedstock >= stockNeeded;
        }

        public override void Draw()
        {
            base.Draw();

            if (ReplicatingTicks > 0)
            {
                float alpha;
                float quart = AnimalFeedReplicateDuration * 0.25f;
                if (ReplicatingTicks < quart)
                {
                    alpha = Mathf.InverseLerp(0, quart, ReplicatingTicks);
                }
                else if (ReplicatingTicks > quart * 3f)
                {
                    alpha = Mathf.InverseLerp(AnimalFeedReplicateDuration, quart * 3f, ReplicatingTicks);
                }
                else
                {
                    alpha = 1f;
                }

                Vector3 replimatAnimalFeederGlowDrawPos = DrawPos;
                replimatAnimalFeederGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

                Graphics.DrawMesh(GraphicsLoader.replimatAnimalFeederGlow.MeshAt(Rotation), replimatAnimalFeederGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatAnimalFeederGlow.MatAt(Rotation, null), alpha), 0);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn || !ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
            {
                return;
            }

            if (this.IsHashIntervalTick(60))
            {
                List<Thing> list = Map.thingGrid.ThingsListAtFast(Position);
                Thing foodInFeeder = list.FirstOrDefault(x => x.def.IsNutritionGivingIngestible);

                if (foodInFeeder == null)
                {
                    int maxAnimFeed = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerAnimalFeed);
                    maxAnimFeed = Mathf.Min(maxAnimFeed, CurrentAnimalFeedDef.stackLimit);

                    if (maxAnimFeed > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerAnimalFeed * maxAnimFeed))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);
                        def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map, false));

                        Thing t = ThingMaker.MakeThing(CurrentAnimalFeedDef, null);
                        t.stackCount = maxAnimFeed;
                        GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Direct);
                    }

                }
                else if (foodInFeeder.def == CurrentAnimalFeedDef && foodInFeeder.stackCount < (CurrentAnimalFeedDef.stackLimit * 0.25))
                {
                    int refill = Mathf.Min(foodInFeeder.def.stackLimit - foodInFeeder.stackCount, foodInFeeder.def.stackLimit);
                    int maxAnimFeed = Mathf.FloorToInt(powerComp.PowerNet.GetTanks().Sum(x => x.storedFeedstock) / volumePerAnimalFeed);
                    maxAnimFeed = Mathf.Min(maxAnimFeed, refill);

                    if (maxAnimFeed > 0 && powerComp.PowerNet.TryConsumeFeedstock(volumePerAnimalFeed * maxAnimFeed))
                    {
                        ReplicatingTicks = GenTicks.SecondsToTicks(2f);
                        def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map, false));

                        foodInFeeder.stackCount += maxAnimFeed;
                    }
                }
            }

            powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;

            if (ReplicatingTicks > 0)
            {
                ReplicatingTicks--;
                powerComp.PowerOutput = -Math.Max(stateDependentPowerComp.ActiveModePowerConsumption, powerComp.Props.basePowerConsumption);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (ParentHolder != null && !(ParentHolder is Map))
            {
                // If minified, don't show computer and feedstock check Inspector messages
            }
            else
            {
                if (!ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotConnectedToComputer".Translate());
                }
                else if (!HasEnoughFeedstockInHoppers())
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotEnoughFeedstock".Translate());
                }
                else
                { }
            }

            return stringBuilder.ToString();
        }
    }
}
