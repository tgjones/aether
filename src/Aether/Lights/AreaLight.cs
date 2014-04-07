using Aether.Geometry;

namespace Aether.Lights
{
    public abstract class AreaLight : Light
    {
        protected AreaLight(Transform lightToWorld, int numSamples)
            : base(lightToWorld, numSamples)
        {
            
        }

        public abstract Spectrum L(Point p, Normal n, Vector w);
    }
}