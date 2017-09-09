using System;
using UnityEngine;
using Verse;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public static class GraphicsLoader
    {
        public static readonly Graphic replimatTerminalGlow = GraphicDatabase.Get<Graphic_Multi>("FX/replimatTerminalGlow", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);
    }
}
