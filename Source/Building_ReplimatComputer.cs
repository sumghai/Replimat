using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Replimat
{
    [StaticConstructorOnStartup]
    class Building_ReplimatComputer : Building
    {

        public CompPowerTrader powerComp;

        public static readonly Vector2 BarSize = new Vector2(1.3f, 0.2f);

        public static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.green);
         
        public static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

        public static readonly Material BatteryBarNoPowerMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.5f, 0.5f));

        public List<Building_ReplimatFeedTank> GetTanks => Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatFeedTank).Select(x => x as Building_ReplimatFeedTank).Where(x => x.PowerComp.PowerNet == PowerComp.PowerNet).ToList();

        public float GetFeedstockPercent()
        {
            float totalAvailableFeedstock = GetTanks.Sum(x => x.storedFeedstock);
            float totalSpace = GetTanks.Sum(x => x.storedFeedstockMax);
            return totalAvailableFeedstock / totalSpace;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = base.GetComp<CompPowerTrader>();
        }

        public bool Working
        {
            get
            {
                if (powerComp.PowerOn)
                {
                    return true;
                }
                return false;
            }
        }

        public override void Draw()
        {
            base.Draw();

            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);

            Vector3 currPos = DrawPos;

            currPos.y += 0.1f;
            currPos.z += 0.3f;

            r.center = currPos + Vector3.up * 0.1f;
            r.size = BarSize;
            if (Working)
            {
                r.fillPercent = GetFeedstockPercent();
                r.filledMat = BatteryBarFilledMat;
                r.unfilledMat = BatteryBarUnfilledMat;
            }
            else
            {
                r.fillPercent = 0;
                r.filledMat = BatteryBarFilledMat;
                r.unfilledMat = BatteryBarNoPowerMat;
            }
            
            r.margin = 0.15f;
            Rot4 rotation = base.Rotation;
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);

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
                if (Working)
                {
                    float totalAvailableFeedstock = GetTanks.Sum(x => x.storedFeedstock);
                    float totalSpace = GetTanks.Sum(x => x.storedFeedstockMax);
                    stringBuilder.Append("TotalFeedstockStored".Translate(totalAvailableFeedstock, totalSpace));
                }
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}