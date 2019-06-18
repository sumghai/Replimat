using Verse;

namespace Replimat
{
    internal class CompProperties_SecondLayer : CompProperties
    {
        public GraphicData graphicData;

        public AltitudeLayer altitudeLayer;

        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);

        public CompProperties_SecondLayer()
        {
            compClass = typeof(CompSecondLayer);
        }
    }
}