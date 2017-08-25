using RimWorld;
using System;
using Verse;

namespace Replimat
{
    public class ReplimatDef
    {
        public static JobDef ingestReplimatDef = DefDatabase<JobDef>.GetNamed("IngestReplimat", true);

        public static JobDef feedPatientReplimatDef = DefDatabase<JobDef>.GetNamed("FeedPatientReplimat", true);

        public static JobDef deliverFoodReplimatDef = DefDatabase<JobDef>.GetNamed("DeliverFoodReplimat", true);
    }
}
