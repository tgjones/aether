using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using Aether.Cameras;
using Aether.Geometry;
using Aether.Integrators;
using Aether.Sampling;

namespace Aether.Renderers
{
    public class SamplerRenderer : Renderer
    {
        private readonly Sampler _sampler;
        private readonly Camera _camera;
        private readonly SurfaceIntegrator _surfaceIntegrator;
        private readonly VolumeIntegrator _volumeIntegrator;

        public SamplerRenderer(Sampler sampler, Camera camera,
            SurfaceIntegrator surfaceIntegrator,
            VolumeIntegrator volumeIntegrator)
        {
            _sampler = sampler;
            _camera = camera;
            _surfaceIntegrator = surfaceIntegrator;
            _volumeIntegrator = volumeIntegrator;
        }

        public override WriteableBitmap Output
        {
            get { return _camera.Film.Bitmap; }
        }

        public override void Render(Scene scene, CancellationToken cancellationToken)
        {
            // Allow integrators to do preprocessing for the scene
            _surfaceIntegrator.PreProcess(scene, _camera, this);
            _volumeIntegrator.PreProcess(scene, _camera, this);
            // Allocate and initialize _sample_
            var sample = new Sample(_sampler, _surfaceIntegrator, _volumeIntegrator, scene);

            Render(scene, sample, cancellationToken);

            // Clean up after rendering and store final image
            _camera.Film.WriteImage();
        }

        private void Render(Scene scene, Sample origSample, CancellationToken cancellationToken)
        {
            var rng = new Random(0);

            // Allocate space for samples and intersections
            int maxSamples = _sampler.MaximumSampleCount;
            var samples = origSample.Duplicate(maxSamples);
            var rays = new RayDifferential[maxSamples];
            var Ls = new Spectrum[maxSamples];
            var Ts = new Spectrum[maxSamples];
            var isects = new Intersection[maxSamples];

            // Get samples from _Sampler_ and update image
            int sampleCount;
            while ((sampleCount = _sampler.GetMoreSamples(samples, rng)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Generate camera rays and compute radiance along rays
                for (int i = 0; i < sampleCount; ++i)
                {
                    // Find camera ray for _sample[i]_
                    float rayWeight = _camera.GenerateRayDifferential(samples[i], out rays[i]);
                    rays[i].ScaleDifferentials(1.0f / MathUtility.Sqrt(_sampler.SamplesPerPixel));

                    // Evaluate radiance along camera ray
                    if (rayWeight > 0.0f)
                    {
                        Ls[i] = rayWeight * Li(scene, rays[i], samples[i], rng, ref isects[i], out Ts[i]);
                    }
                    else
                    {
                        Ls[i] = new Spectrum(0.0f);
                        Ts[i] = new Spectrum(1.0f);
                    }
                }

                // Report sample results to _Sampler_, add contributions to image
                if (_sampler.ReportResults(samples, rays, Ls, isects, sampleCount))
                    for (int i = 0; i < sampleCount; ++i)
                        _camera.Film.AddSample(samples[i], Ls[i]);

                _camera.Film.UpdateDisplay(
                    (int) samples.Min(s => s.ImageX),
                    (int) samples.Min(s => s.ImageY),
                    (int) samples.Max(s => s.ImageX),
                    (int) samples.Max(s => s.ImageY));
            }
        }

        public override Spectrum Li(Scene scene, RayDifferential ray, Sample sample, Random rng, ref Intersection intersection, out Spectrum t)
        {
            Debug.Assert(ray.Time == sample.Time);
            Spectrum Li = Spectrum.CreateBlack();
            if (scene.TryIntersect(ray, ref intersection))
                Li = _surfaceIntegrator.Li(scene, this, ray, intersection, sample, rng);
            else
            {
                // Handle ray that doesn't intersect any geometry
                foreach (var light in scene.Lights)
                    Li += light.Le(ray);
            }
            Spectrum Lvi = _volumeIntegrator.Li(scene, this, ray, sample, rng, out t);
            return t * Li + Lvi;
        }

        public override Spectrum Transmittance(Scene scene, RayDifferential ray, Sample sample, Random rng)
        {
            return _volumeIntegrator.Transmittance(scene, this, ray, sample, rng);
        }
    }
}