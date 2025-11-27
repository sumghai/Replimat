using Verse;

namespace Replimat
{
    internal class CompProperties_SecondLayer : CompProperties
    {
        public GraphicData graphicData = null;

        public AltitudeLayer altitudeLayer = AltitudeLayer.Building;

        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);

        public CompProperties_SecondLayer()
        {
            compClass = typeof(CompSecondLayer);
        }
    }
}