using System;
using Aether.Geometry;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public abstract class VolumeIntegrator : Integrator
    {
        public abstract Spectrum Li(Scene scene, Renderer renderer, RayDifferential ray,
            Sample sample, Random rng, out Spectrum transmittance);

        public abstract Spectrum Transmittance(Scene scene, Renderer renderer,
            RayDifferential ray, Sample sample, Random rng);
    }
}