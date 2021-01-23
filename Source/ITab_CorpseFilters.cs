using RimWorld;
using Verse;

namespace Replimat
{
    public class ITab_CorpseFilters : ITab_Storage
    {
        public override bool IsPrioritySettingVisible => false;

        public ITab_CorpseFilters()
        {
            labelKey = "TabCorpseFilters";
        }
    }
}
