using System;
using Aether.Geometry;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Lights
{
    public class VisibilityTester
    {
        public Ray Ray { get; private set; }

        public VisibilityTester(Point p1, float eps1, Point p2, float eps2, float time)
        {
            float dist = Point.Distance(p1, p2);
            Ray = new Ray(p1, (p2 - p1) / dist, eps1, dist * (1.0f - eps2), time);
        }

        public VisibilityTester(Point p, float eps, Vector w, float time)
        {
            Ray = new Ray(p, w, eps, float.PositiveInfinity, time);
        }

        public bool Unoccluded(Scene scene)
        {
            return !scene.Intersects(Ray);
        }

        public Spectrum Transmittance(Scene scene, Renderer renderer,
            Sample sample, Random rng)
        {
            return renderer.Transmittance(
                scene, RayDifferential.FromRay(Ray),
                sample, rng);
        }
    }
}