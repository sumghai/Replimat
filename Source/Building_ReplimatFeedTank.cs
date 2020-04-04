using System.Text;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Replimat
{
    public class Building_ReplimatFeedTank : Building
    {
        public virtual float storedFeedstockMax => 250f; // 250L capacity for an *insulated* 0.5m diameter and 1.5m high liquid tank
                                                         // Originally 8000L capacity for a 2m diameter and 2m high liquid tank

        public float storedFeedstock;


        public float AmountCanAccept => this.IsBrokenDown() ? 0f : (storedFeedstockMax - storedFeedstock);

        public bool HasComputer
        {
            get
            {
                return Map.listerThings.ThingsOfDef(ReplimatDef.ReplimatComputer).OfType<Building_ReplimatComputer>().Any(x => x.PowerComp.PowerNet == this.PowerComp.PowerNet && x.Working);

            }
        }

        public float StoredFeedstock => storedFeedstock;
        

        public float StoredFeedstockPct => storedFeedstock / storedFeedstockMax;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<float>(ref storedFeedstock, "storedFeedstock", 0f, false);

        }

        public void AddFeedstock(float amount)
        {
            if (amount < 0f)
            {
                //Log.Error("[Replimat] " + "Cannot add negative feedstock " + amount);
                return;
            }
            if (amount > AmountCanAccept)
            {
                amount = AmountCanAccept;
            }
            storedFeedstock += amount;
        }

        public void DrawFeedstock(float amount)
        {
            storedFeedstock -= amount;
            if (storedFeedstock < 0f)
            {
                //Log.Error("[Replimat] " + "Drawing feedstock we don't have from " + this);
                storedFeedstock = 0f;
            }
        }

        public void SetStoredFeedstockPct(float pct)
        {
            pct = Mathf.Clamp01(pct);
            storedFeedstock = storedFeedstockMax * pct;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.Append("FeedstockStored".Translate(storedFeedstock, storedFeedstockMax));

            if (ParentHolder != null && !(ParentHolder is Map))
            {
                // If minified, don't show computer and feedstock check Inspector messages
            }
            else
            {
                if (!HasComputer)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotConnectedToComputer".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        this.SetStoredFeedstockPct(0f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -10L",
                    action = delegate
                    {
                        this.DrawFeedstock(10f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -1L",
                    action = delegate
                    {
                        this.DrawFeedstock(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +1L",
                    action = delegate
                    {
                        this.AddFeedstock(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +10L",
                    action = delegate
                    {
                        this.AddFeedstock(10f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate
                    {
                        this.SetStoredFeedstockPct(1f);
                    }
                };
            }
        }
    }
}

