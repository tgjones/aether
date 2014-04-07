using System;
using System.Linq;
using Aether.Geometry;
using Aether.Lights;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public class WhittedIntegrator : SurfaceIntegrator
    {
        private readonly int _maxDepth;

        public WhittedIntegrator(int maxDepth)
        {
            _maxDepth = maxDepth;
        }

        public override Spectrum Li(Scene scene, Renderer renderer, RayDifferential ray, Intersection intersection,
            Sample sample, Random rng)
        {
            Spectrum L = Spectrum.CreateBlack();
            // Compute emitted and reflected light at ray intersection point

            // Evaluate BSDF at hit point
            var bsdf = intersection.GetBsdf(ray);

            // Initialize common variables for Whitted integrator
            Point p = bsdf.DgShading.Point;
            Normal n = bsdf.DgShading.Normal;
            Vector wo = -ray.Direction;

            // Compute emitted light if ray hit an area light source
            L += intersection.Le(wo);

            // Add contribution of each light source
            foreach (var light in scene.Lights)
            {
                Vector wi;
                float pdf;
                VisibilityTester visibility;
                Spectrum Li = light.SampleL(p, intersection.RayEpsilon,
                    new LightSample(rng), ray.Time, out wi, out pdf, out visibility);
                if (Li.IsBlack || pdf == 0.0f)
                    continue;
                Spectrum f = bsdf.F(wo, wi);
                if (!f.IsBlack && visibility.Unoccluded(scene))
                    L += f * Li * Vector.AbsDot(wi, n)
                        * visibility.Transmittance(scene, renderer, sample, rng)
                        / pdf;
            }
            if (ray.Depth + 1 < _maxDepth)
            {
                // Trace rays for specular reflection and refraction
                L += IntegratorUtilities.SpecularReflect(ray, bsdf, rng, intersection, renderer, scene, sample);
                L += IntegratorUtilities.SpecularTransmit(ray, bsdf, rng, intersection, renderer, scene, sample);
            }
            return L;
        }
    }
}