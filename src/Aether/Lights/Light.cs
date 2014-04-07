using System;
using Aether.Geometry;

namespace Aether.Lights
{
    public abstract class Light
    {
        private readonly Transform _lightToWorld;
        private readonly Transform _worldToLight;
        private readonly int _numSamples;

        protected Light(Transform lightToWorld, int numSamples = 1)
        {
            _lightToWorld = lightToWorld;
            _worldToLight = Transform.Invert(lightToWorld);
            _numSamples = Math.Max(numSamples, 1);
        }

        public abstract bool IsDeltaLight { get; }

        public abstract Spectrum SampleL(Point p, float pEpsilon, LightSample ls,
            float time, out Vector wi, out float pdf, out VisibilityTester vis);

        public abstract Spectrum Power(Scene scene);

        public virtual Spectrum Le(RayDifferential r)
        {
            return Spectrum.CreateBlack();
        }

        public abstract float Pdf(Point p, Vector wi);

        public abstract Spectrum SampleL(Scene scene, LightSample ls, float u1, float u2,
            float time, out Ray ray, out Normal ns, out float pdf);
    }
}