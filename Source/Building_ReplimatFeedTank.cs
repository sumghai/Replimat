using System.Text;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Replimat
{
    public class Building_ReplimatFeedTank : Building
    {
        public virtual float storedFeedstockMax => 8000f; //8000L capacity for a 2m diameter and 2m high liquid tank

        public float storedFeedstock;


        public float AmountCanAccept => this.IsBrokenDown() ? 0f : (storedFeedstockMax - storedFeedstock);


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
                Log.Error("Cannot add negative feedstock " + amount);
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
                Log.Error("Drawing feedstock we don't have from " + this);
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
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append("FeedstockStored".Translate(storedFeedstock, storedFeedstockMax));
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
                    defaultLabel = "DEBUG: Fill",
                    action = delegate
                    {
                        this.SetStoredFeedstockPct(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        this.SetStoredFeedstockPct(0f);
                    }
                };
            }
        }
    }
}

