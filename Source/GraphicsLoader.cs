﻿using UnityEngine;
using Verse;

namespace Replimat
{
    [StaticConstructorOnStartup]
    public static class GraphicsLoader
    {
        // This mod uses Dubwise-style legacy hardcoded graphics loading, for historical reasons
        // Future mods will use more robust XML-accessible methods
        
        public static readonly Graphic replimatTerminalGlow = GraphicDatabase.Get<Graphic_Multi>("FX/replimatTerminalGlow", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic replimatTerminalWallGlow = GraphicDatabase.Get<Graphic_Multi>("FX/replimatTerminalWallGlow", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic replimatTerminalScreenGlow = GraphicDatabase.Get<Graphic_Single>("FX/replimatTerminalScreenGlow_north", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic replimatTerminalWallScreenGlow = GraphicDatabase.Get<Graphic_Single>("FX/replimatTerminalWallScreenGlow_north", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic replimatAnimalFeederGlow = GraphicDatabase.Get<Graphic_Single>("FX/replimatAnimalFeederGlow", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic[] replimatHopperGlow = new Graphic[]
        {
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatHopperGlow0", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white),
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatHopperGlow1", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white),
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatHopperGlow2", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white)
        };

        public static readonly Graphic replimatHopperScreenGlow = GraphicDatabase.Get<Graphic_Multi>("FX/replimatHopperScreenGlow", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Graphic[] replimatCorpseRecyclerGlow = new Graphic[]
        {
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatCorpseRecyclerGlow0", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white),
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatCorpseRecyclerGlow1", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white),
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatCorpseRecyclerGlow2", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white),
            GraphicDatabase.Get<Graphic_Multi>("FX/replimatCorpseRecyclerGlow3", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white)
        };

        public static readonly Graphic replimatComputerScreenGlow = GraphicDatabase.Get<Graphic_Single>("FX/replimatComputerScreenGlow_north", ShaderDatabase.MoteGlow, new Vector2(3f, 3f), Color.white);

        public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);
        public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(38, 169, 224).ToColor);
        public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(249, 236, 49).ToColor);
        public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);
        public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(41, 180, 115).ToColor);
        public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 11, 12).ToColor);
    }
}