using System;
using Aether.Geometry;
using Aether.Reflection;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public static class IntegratorUtilities
    {
        public static Spectrum SpecularReflect(RayDifferential ray, Bsdf bsdf,
            Random rng, Intersection isect, Renderer renderer,
            Scene scene, Sample sample)
        {
            Vector wo = -ray.Direction, wi;
            float pdf;
            Point p = bsdf.DgShading.Point;
            Normal n = bsdf.DgShading.Normal;
            Spectrum f = bsdf.SampleF(wo, out wi, new BsdfSample(rng), out pdf,
                BxdfType.Reflection | BxdfType.Specular);
            Spectrum L = Spectrum.CreateBlack();
            if (pdf > 0.0f && !f.IsBlack && Vector.AbsDot(wi, n) != 0.0f)
            {
                // Compute ray differential _rd_ for specular reflection
                var rd = new RayDifferential(p, wi, ray, isect.RayEpsilon);
                if (ray.HasDifferentials)
                {
                    rd.HasDifferentials = true;
                    rd.RxOrigin = p + isect.DifferentialGeometry.DpDx;
                    rd.RyOrigin = p + isect.DifferentialGeometry.DpDy;
                    // Compute differential reflected directions
                    Normal dndx = bsdf.DgShading.DnDu * bsdf.DgShading.DuDx +
                        bsdf.DgShading.DnDv * bsdf.DgShading.DvDx;
                    Normal dndy = bsdf.DgShading.DnDu * bsdf.DgShading.DuDy +
                        bsdf.DgShading.DnDv * bsdf.DgShading.DvDy;
                    Vector dwodx = -ray.RxDirection - wo, dwody = -ray.RyDirection - wo;
                    float dDNdx = Vector.Dot(dwodx, n) + Vector.Dot(wo, dndx);
                    float dDNdy = Vector.Dot(dwody, n) + Vector.Dot(wo, dndy);
                    rd.RxDirection = wi - dwodx + 2 * (Vector) (Vector.Dot(wo, n) * dndx +
                        dDNdx * n);
                    rd.RyDirection = wi - dwody + 2 * (Vector) (Vector.Dot(wo, n) * dndy +
                        dDNdy * n);
                }
                Spectrum Li = renderer.Li(scene, rd, sample, rng);
                L = f * Li * Vector.AbsDot(wi, n) / pdf;
            }
            return L;
        }

        public static Spectrum SpecularTransmit(RayDifferential ray, Bsdf bsdf,
            Random rng, Intersection isect, Renderer renderer,
            Scene scene, Sample sample)
        {
            Vector wo = -ray.Direction, wi;
            float pdf;
            Point p = bsdf.DgShading.Point;
            Normal n = bsdf.DgShading.Normal;
            Spectrum f = bsdf.SampleF(wo, out wi, new BsdfSample(rng), out pdf,
                BxdfType.Transmission | BxdfType.Specular);
            Spectrum L = Spectrum.CreateBlack();
            if (pdf > 0.0f && !f.IsBlack && Vector.AbsDot(wi, n) != 0.0f)
            {
                // Compute ray differential _rd_ for specular transmission
                var rd = new RayDifferential(p, wi, ray, isect.RayEpsilon);
                if (ray.HasDifferentials)
                {
                    rd.HasDifferentials = true;
                    rd.RxOrigin = p + isect.DifferentialGeometry.DpDx;
                    rd.RyOrigin = p + isect.DifferentialGeometry.DpDy;

                    float eta = bsdf.Eta;
                    Vector w = -wo;
                    if (Vector.Dot(wo, n) < 0)
                        eta = 1.0f / eta;

                    Normal dndx = bsdf.DgShading.DnDu * bsdf.DgShading.DuDx +
                        bsdf.DgShading.DnDv * bsdf.DgShading.DvDx;
                    Normal dndy = bsdf.DgShading.DnDu * bsdf.DgShading.DuDy +
                        bsdf.DgShading.DnDv * bsdf.DgShading.DvDy;

                    Vector dwodx = -ray.RxDirection - wo, dwody = -ray.RyDirection - wo;
                    float dDNdx = Vector.Dot(dwodx, n) + Vector.Dot(wo, dndx);
                    float dDNdy = Vector.Dot(dwody, n) + Vector.Dot(wo, dndy);

                    float mu = eta * Vector.Dot(w, n) - Vector.Dot(wi, n);
                    float dmudx = (eta - (eta * eta * Vector.Dot(w, n)) / Vector.Dot(wi, n)) * dDNdx;
                    float dmudy = (eta - (eta * eta * Vector.Dot(w, n)) / Vector.Dot(wi, n)) * dDNdy;

                    rd.RxDirection = wi + eta * dwodx - (Vector) (mu * dndx + dmudx * n);
                    rd.RyDirection = wi + eta * dwody - (Vector) (mu * dndy + dmudy * n);
                }
                Spectrum Li = renderer.Li(scene, rd, sample, rng);
                L = f * Li * Vector.AbsDot(wi, n) / pdf;
            }
            return L;
        }
    }
}