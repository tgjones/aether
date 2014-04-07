using System;
using Aether.Geometry;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public abstract class SurfaceIntegrator : Integrator
    {
        public abstract Spectrum Li(Scene scene, Renderer renderer, RayDifferential ray,
            Intersection intersection, Sample sample, Random rng);
    }
}