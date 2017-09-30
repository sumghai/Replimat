using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Replimat
{
    class Building_ReplimatHopper : Building_Storage
    {
        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return IntVec3.FromVector3(this.DrawPos);
        }
    }
}
