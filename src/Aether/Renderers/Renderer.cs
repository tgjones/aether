using System;
using System.Threading;
using System.Windows.Media.Imaging;
using Aether.Geometry;
using Aether.Sampling;

namespace Aether.Renderers
{
    public abstract class Renderer
    {
        public abstract WriteableBitmap Output { get; }

        public abstract void Render(Scene scene, CancellationToken cancellationToken);

        public abstract Spectrum Li(Scene scene, RayDifferential ray, Sample sample, Random rng,
            out Intersection intersection, out Spectrum t);

        public Spectrum Li(Scene scene, RayDifferential ray, Sample sample, Random rng)
        {
            Intersection intersection;
            Spectrum t;
            return Li(scene, ray, sample, rng, out intersection, out t);
        }

        public abstract Spectrum Transmittance(Scene scene, RayDifferential ray, Sample sample,
            Random rng);
    }
}