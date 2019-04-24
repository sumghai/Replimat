using RimWorld;
using System;
using Verse;

namespace Replimat
{
    internal class CompSecondLayer : ThingComp
    {
        private Graphic graphicInt;

        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce("[Replimat] " + parent.def + " has no SecondLayer graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    graphicInt = Props.graphicData.GraphicColoredFor(parent);
                }
                return graphicInt;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
        }
    }
}
