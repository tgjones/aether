using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

            // Create and launch _SamplerRendererTask_s for rendering image

            // Compute number of _SamplerRendererTask_s to create for rendering
            var numPixels = _camera.Film.XResolution * _camera.Film.YResolution;
            var numTasks = Math.Max(32 * Environment.ProcessorCount, numPixels / (16 * 16));
            numTasks = (int) MathUtility.RoundUpPow2((uint) numTasks);

            var renderTasks = Enumerable.Range(0, numTasks)
                .Select(i => CreateRenderTask(scene, sample, cancellationToken,
                    numTasks - 1 - i, numTasks))
                .ToArray();

            // Organise tasks such that we do several passes over the whole image,
            // of increasing quality.
            var groupedRenderTasks = renderTasks.Select((t, i) => new { t, i })
                .GroupBy(a => a.i % 6, a => a.t)
                .ToArray();
            foreach (var groupedTasks in groupedRenderTasks)
                foreach (var task in groupedTasks)
                    task.Start();

            Task.WaitAll(renderTasks, cancellationToken);

            // Clean up after rendering and store final image
            _camera.Film.WriteImage();
        }

        private Task CreateRenderTask(
            Scene scene, Sample origSample, CancellationToken cancellationToken,
            int taskNumber, int taskCount)
        {
            return new Task(() =>
            {
                var sampler = _sampler.GetSubSampler(taskNumber, taskCount);
                if (sampler == null)
                    return;

                var rng = new Random(taskNumber);

                // Allocate space for samples and intersections
                int maxSamples = _sampler.MaximumSampleCount;
                var samples = origSample.Duplicate(maxSamples);
                var rays = new RayDifferential[maxSamples];
                var Ls = new Spectrum[maxSamples];
                var Ts = new Spectrum[maxSamples];
                var isects = new Intersection[maxSamples];

                // Get samples from _Sampler_ and update image
                int sampleCount;
                while ((sampleCount = sampler.GetMoreSamples(samples, rng)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Generate camera rays and compute radiance along rays
                    for (int i = 0; i < sampleCount; ++i)
                    {
                        // Find camera ray for _sample[i]_
                        float rayWeight = _camera.GenerateRayDifferential(samples[i], out rays[i]);
                        rays[i].ScaleDifferentials(1.0f / MathUtility.Sqrt(sampler.SamplesPerPixel));

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
                    if (sampler.ReportResults(samples, rays, Ls, isects, sampleCount))
                        for (int i = 0; i < sampleCount; ++i)
                            _camera.Film.AddSample(samples[i], Ls[i]);
                }

                _camera.Film.UpdateDisplay(
                    sampler.XStart, sampler.YStart,
                    sampler.XEnd + 1, sampler.YEnd + 1);
            }, cancellationToken);
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