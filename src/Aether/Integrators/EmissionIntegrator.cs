using System;
using Aether.Geometry;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public class EmissionIntegrator : VolumeIntegrator
    {
        public override Spectrum Li(Scene scene, Renderer renderer, RayDifferential ray, Sample sample, Random rng, out Spectrum transmittance)
        {
            // TODO
            transmittance = new Spectrum(1.0f);
            return Spectrum.CreateBlack();
        }

        public override Spectrum Transmittance(Scene scene, Renderer renderer, RayDifferential ray, Sample sample, Random rng)
        {
            // TODO
            return new Spectrum(1.0f);
        }
    }
}