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
    public class Building_ReplimatCorpseRecycler : Building, IStoreSettingsParent
    {
        public CompPowerTrader powerComp;

        public CompStateDependentPowerUse stateDependentPowerComp;

        public StorageSettings allowedCorpseFilterSettings;

        private Graphic cachedGraphicFull;

        private float corpseInitialMass;

        private float corpseRemainingMass;

        // 0.12 kg per 5 tick update interval = 60 kg human processed in one in-game hour 
        private readonly float defaultMassDecrementStepSize = ThingDefOf.Human.BaseMass / 500;

        public bool Empty => corpseRemainingMass <= 0;

        private bool Running;

        public bool StorageTabVisible => true; //Always show storage options tab

        private int dematerializingCycleInt;

        private Sustainer wickSustainer;

        public List<Building_ReplimatFeedTank> GetTanks => ReplimatUtility.GetTanks(powerComp.PowerNet);

        public override Graphic Graphic
        {
            get
            {
                if (!Empty)
                {
                    if (def.building.fullGraveGraphicData == null)
                    {
                        return base.Graphic;
                    }
                    if (cachedGraphicFull == null)
                    {
                        cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
                    }
                    return cachedGraphicFull;
                }
                return base.Graphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());

            if (ParentHolder != null && !(ParentHolder is Map))
            {
                // If minified, don't show computer and feedstock check Inspector messages
            }
            else
            {
                if (!ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
                {
                    stringBuilder.AppendLine("NotConnectedToComputer".Translate());
                }

                if (Empty)
                {
                    stringBuilder.AppendLine("CorpseRecyclerEmpty".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("CorpseRecyclerMassRemaining".Translate(corpseRemainingMass.ToString("0.00"), corpseInitialMass.ToString("0.00")));
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public StorageSettings GetStoreSettings()
        {
            return allowedCorpseFilterSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            StorageSettings recyclerAllowedCorpses = def.building.fixedStorageSettings;

            // Remove non-fleshy corpses from filter if Humanoid Alien Races mod is active
            if (ModCompatibility.AlienRacesIsActive)
            {
                recyclerAllowedCorpses.filter.allowedDefs.RemoveWhere(def => !ModCompatibility.AlienCorpseHasOrganicFlesh(def));
            }

            // Remove non-fleshy corpses from filter for non-HAR humanoid robot or hologram races
            recyclerAllowedCorpses.filter.allowedDefs.RemoveWhere(def => ThingDef.Named(def.ToString().Substring("Corpse_".Length)).GetStatValueAbstract(StatDefOf.MeatAmount) == 0);

            return recyclerAllowedCorpses;
        }

        public override void PostMake()
        {
            base.PostMake();
            allowedCorpseFilterSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                allowedCorpseFilterSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref allowedCorpseFilterSettings, "allowedCorpseFilterSettings", this);
            Scribe_Values.Look(ref corpseInitialMass, "corpseInitialMass", 0f);
            Scribe_Values.Look(ref corpseRemainingMass, "corpseRemaningMass", 0f);
        }

        public void LoadCorpse(Corpse corpse)
        {
            // Initial mass should account for missing body parts
            corpseInitialMass = corpse.InnerPawn.def.BaseMass * corpse.InnerPawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(corpse.InnerPawn.RaceProps.body.corePart);
            corpseRemainingMass = corpseInitialMass;            
            corpse.Destroy();
        }

        public void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            wickSustainer = def.building.soundDispense.TrySpawnSustainer(info);
        }

        public override void Draw()
        {
            base.Draw();

            Vector3 replimatCorpseRecyclerGlowDrawPos = DrawPos;
            replimatCorpseRecyclerGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

            if (Running)
            {
                Graphics.DrawMesh(GraphicsLoader.replimatCorpseRecyclerGlow[dematerializingCycleInt].MeshAt(Rotation), replimatCorpseRecyclerGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatCorpseRecyclerGlow[dematerializingCycleInt].MatAt(Rotation, null), 1), 0);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;

            List<Building_ReplimatFeedTank> tanks = GetTanks;

            if (this.IsHashIntervalTick(5))
            {
                if (Empty)
                {
                    corpseInitialMass = 0;
                    Running = false;
                }
                else
                {
                    float massDecrementStepSize = Mathf.Min(defaultMassDecrementStepSize, corpseRemainingMass);
                    float feedstockVolume = ReplimatUtility.ConvertMassToFeedstockVolume(massDecrementStepSize);
                    float freeSpaceInTanks = tanks.Sum(x => x.AmountCanAccept);

                    if (powerComp.PowerOn && freeSpaceInTanks >= feedstockVolume)
                    {
                        Running = true;

                        powerComp.PowerOutput = -Math.Max(stateDependentPowerComp.ActiveModePowerConsumption, powerComp.Props.basePowerConsumption);

                        float buffer = feedstockVolume;

                        foreach (var tank in tanks.InRandomOrder())
                        {
                            if (buffer > 0f)
                            {
                                float sent = Mathf.Min(buffer, tank.AmountCanAccept);
                                buffer -= sent;
                                tank.AddFeedstock(sent);
                                corpseRemainingMass -= Mathf.Min(defaultMassDecrementStepSize, corpseRemainingMass);
                            }
                        }

                        dematerializingCycleInt++;
                        if (dematerializingCycleInt > 3)
                        {
                            dematerializingCycleInt = 0;
                        }
                    }
                    else
                    {
                        Running = false;
                    }
                }
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
            }

            if (Running)
            {
                if (wickSustainer == null || wickSustainer.Ended)
                {
                    StartWickSustainer();
                }
                else
                {
                    wickSustainer.Maintain();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(allowedCorpseFilterSettings))
            {
                yield return item;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        corpseRemainingMass = 0;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -10kg",
                    action = delegate
                    {
                        corpseRemainingMass = (corpseRemainingMass > 10) ? (corpseRemainingMass - 10) : 0;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -1kg",
                    action = delegate
                    {
                        corpseRemainingMass = (corpseRemainingMass > 1) ? (corpseRemainingMass - 1) : 0;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +1kg",
                    action = delegate
                    {
                        if (Empty)
                        {
                            corpseInitialMass = ThingDefOf.Human.BaseMass;
                        }

                        corpseRemainingMass = (corpseInitialMass - corpseRemainingMass > 1) ? (corpseRemainingMass + 1) : corpseInitialMass;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +10kg",
                    action = delegate
                    {
                        if (Empty)
                        {
                            corpseInitialMass = ThingDefOf.Human.BaseMass;
                        }

                        corpseRemainingMass = (corpseInitialMass - corpseRemainingMass > 10) ? (corpseRemainingMass + 10) : corpseInitialMass;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Load Corpse",
                    action = delegate
                    {
                        corpseInitialMass = ThingDefOf.Human.BaseMass;
                        corpseRemainingMass = corpseInitialMass;
                    }
                };
            }
        }
    }
}