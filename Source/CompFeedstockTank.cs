using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Replimat
{
    public class CompFeedstockTank : ThingComp
    {
        private float storedFeedstock;

        public float AmountCanAccept
        {
            get => parent.IsBrokenDown() ? 0f :
                (Props.storedFeedstockMax - storedFeedstock);
        }

        public float StoredFeedstock
        {
            get => storedFeedstock;
        }

        public float StoredFeedstockPct
        {
            get => storedFeedstock / Props.storedFeedstockMax;
        }

        public new CompProperties_FeedstockTank Props
        {
            get => (CompProperties_FeedstockTank)props;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref storedFeedstock, "storedFeedstock", 0f, false);
            CompProperties_FeedstockTank props = Props;
            if (storedFeedstock > props.storedFeedstockMax)
            {
                storedFeedstock = props.storedFeedstockMax;
            }
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
                Log.Error("Drawing feedstock we don't have from " + parent);
                storedFeedstock = 0f;
            }
        }

        public void SetStoredFeedstockPct(float pct)
        {
            pct = Mathf.Clamp01(pct);
            storedFeedstock = Props.storedFeedstockMax * pct;
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown")
            {
                DrawFeedstock(StoredFeedstock);
            }
        }

        public override string CompInspectStringExtra()
        {
            return string.Concat(new string[]
            {
                "Feedstock stored: ",
                storedFeedstock.ToString("F0"),
                " / ",
                Props.storedFeedstockMax.ToString("F0"),
                " L"
            });
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
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