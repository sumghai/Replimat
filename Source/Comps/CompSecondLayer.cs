using RimWorld;
using Verse;

namespace Replimat
{
    internal class CompSecondLayer : ThingComp
    {
        public Graphic graphicInt;

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
