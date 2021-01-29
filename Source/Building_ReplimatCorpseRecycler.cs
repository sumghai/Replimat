using RimWorld;
using System;
using System.Text;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Replimat
{
    public class Building_ReplimatCorpseRecycler : Building, IStoreSettingsParent
    {
        public CompPowerTrader powerComp;

        public CompStateDependentPowerUse stateDependentPowerComp;

        public StorageSettings allowedCorpseFilterSettings;

        private float corpseInitialMass;

        private float corpseRemainingMass;

        // 0.12 kg per 5 tick update interval = 60 kg human processed in one in-game hour 
        private readonly float defaultMassDecrementStepSize = ThingDefOf.Human.BaseMass / 500;

        public bool Empty => corpseRemainingMass <= 0;

        public bool StorageTabVisible => true; //Always show storage options tab

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatFeedTank).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.HasComputer).ToList();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            stateDependentPowerComp = GetComp<CompStateDependentPowerUse>();
        }

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

            }
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
                if (Empty)
                {
                    stringBuilder.AppendLine("CorpseRecyclerEmpty".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("CorpseRecyclerMassRemaining".Translate(corpseRemainingMass, corpseInitialMass));
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
            StorageSettings foobar = def.building.fixedStorageSettings;

            // Remove non-fleshy corpses from filter if Humanoid Alien Races mod is active
            if (ModCompatibility.AlienRacesIsActive)
            {
                foobar.filter.allowedDefs.RemoveWhere(def => ModCompatibility.AlienCorpseHasOrganicFlesh(def));
            }

            return foobar;
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

        public override void Tick()
        {
            base.Tick();

            List<Building_ReplimatFeedTank> tanks = GetTanks;

            if (this.IsHashIntervalTick(5))
            {
                powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;

                if (Empty)
                {
                    corpseInitialMass = 0;
                }
                else
                {
                    float massDecrementStepSize = Mathf.Min(defaultMassDecrementStepSize, corpseRemainingMass);
                    float feedstockVolume = ReplimatUtility.convertMassToFeedstockVolume(massDecrementStepSize);
                    float freeSpaceInTanks = tanks.Sum(x => x.AmountCanAccept);

                    if (powerComp.PowerOn && freeSpaceInTanks >= feedstockVolume)
                    {
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
                    }
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
        }
    }
}
