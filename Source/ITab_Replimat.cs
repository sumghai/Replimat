using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Replimat
{
    public class ITab_Replimat : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        public ITab_Replimat()
        {
            this.labelKey = "TabReplimat";
        }

        protected override void FillTab()
        {
            Rect position = new Rect(0f, 0f, ITab_Replimat.WinSize.x, ITab_Replimat.WinSize.y).ContractedBy(10f);
        }
    }
}
