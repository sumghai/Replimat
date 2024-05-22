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

            // Remove non-fleshy corpses from filter if Humanoid Alien Races mod is active
            if (ModCompatibility.AlienRacesIsActive)
            {
                def.building.fixedStorageSettings.filter.allowedDefs.RemoveWhere(def => !AlienRacesCompatibility.CorpseHasOrganicFlesh(def));
            }

            // Remove non-fleshy corpses from filter for non-HAR humanoid robot or hologram races
            def.building.fixedStorageSettings.filter.allowedDefs.RemoveWhere(def => def.GetStatValueAbstract(StatDefOf.MeatAmount) == 0);
        }
        public void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            wickSustainer = def.building.soundDispense.TrySpawnSustainer(info);
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 replimatCorpseRecyclerGlowDrawPos = drawLoc;
            replimatCorpseRecyclerGlowDrawPos.y = def.altitudeLayer.AltitudeFor() + 0.03f;

            if (Running && !Empty)
            {
                Graphics.DrawMesh(GraphicsLoader.replimatCorpseRecyclerGlow[dematerializingCycleInt].MeshAt(Rotation), replimatCorpseRecyclerGlowDrawPos, Quaternion.identity, FadedMaterialPool.FadedVersionOf(GraphicsLoader.replimatCorpseRecyclerGlow[dematerializingCycleInt].MatAt(Rotation, null), 1), 0);
            }
        }

        public void RefreshBuildingGraphic()
        {
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
        }

        public StorageSettings GetStoreSettings()
        {
            return allowedCorpseFilterSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        { 
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
            RefreshBuildingGraphic();
        }

        public override void Tick()
        {
            base.Tick();

            if (!powerComp.PowerOn)
            {
                return;
            }

            powerComp.PowerOutput = Running ? powerComp.PowerOutput = -Math.Max(stateDependentPowerComp.Props.activeModePowerConsumption, powerComp.Props.basePowerConsumption) : -powerComp.Props.basePowerConsumption;

            if (this.IsHashIntervalTick(15))
            {
                if (Empty)
                {   
                    corpseInitialMass = 0;
                    Running = false;
                }
                else
                {
                    List<Building_ReplimatFeedTank> tanks = GetTanks;

                    float massDecrementStepSize = Mathf.Min(defaultMassDecrementStepSize, corpseRemainingMass);
                    float feedstockVolume = ReplimatUtility.ConvertMassToFeedstockVolume(massDecrementStepSize);
                    float freeSpaceInTanks = tanks.Sum(x => x.AmountCanAccept);

                    if (powerComp.PowerOn && freeSpaceInTanks >= feedstockVolume)
                    {
                        Running = true;

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
                    }
                    else
                    {
                        Running = false;
                    }
                }
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

                if (this.IsHashIntervalTick(5))
                {
                    dematerializingCycleInt++;
                    if (dematerializingCycleInt > 3)
                    {
                        dematerializingCycleInt = 0;
                    }
                    if (Empty)
                    {
                        RefreshBuildingGraphic();
                    }
                }
            }
        }
        
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (ParentHolder == null || ParentHolder is Map)
            {
                if (!ReplimatUtility.CanFindComputer(this, PowerComp.PowerNet))
                {
                    stringBuilder.AppendLineIfNotEmpty().Append("NotConnectedToComputer".Translate());
                }
                else 
                {
                    if (Empty)
                    {
                        stringBuilder.AppendLineIfNotEmpty().Append("CorpseRecyclerEmpty".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLineIfNotEmpty().Append("CorpseRecyclerMassRemaining".Translate(corpseRemainingMass.ToString("0.00"), corpseInitialMass.ToString("0.00")));
                    }
                }
            }

            return stringBuilder.ToString();
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
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        corpseRemainingMass = 0;
                        RefreshBuildingGraphic();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -10kg",
                    action = delegate
                    {
                        corpseRemainingMass = (corpseRemainingMass > 10) ? (corpseRemainingMass - 10) : 0;
                        RefreshBuildingGraphic();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -1kg",
                    action = delegate
                    {
                        corpseRemainingMass = (corpseRemainingMass > 1) ? (corpseRemainingMass - 1) : 0;
                        RefreshBuildingGraphic();
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
                        RefreshBuildingGraphic();
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
                        RefreshBuildingGraphic();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Load Corpse",
                    action = delegate
                    {
                        corpseInitialMass = ThingDefOf.Human.BaseMass;
                        corpseRemainingMass = corpseInitialMass;
                        RefreshBuildingGraphic();
                    }
                };
            }
        }
    }
}