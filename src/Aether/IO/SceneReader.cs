using System.IO;
using Aether.Accelerators;
using Aether.Cameras;
using Aether.Films;
using Aether.Filters;
using Aether.Geometry;
using Aether.Integrators;
using Aether.Lights;
using Aether.Materials;
using Aether.Primitives;
using Aether.Renderers;
using Aether.Sampling;
using Aether.Shapes;
using Aether.Textures;

namespace Aether.IO
{
    public static class SceneReader
    {
        private static readonly SceneParser Parser = new SceneParser();

        public static void Read(TextReader reader, out Scene scene, out Renderer renderer)
        {
            // TODO: Parse.
            var sceneFile = Parser.Parse(reader.ReadToEnd());

            var filter = new MitchellFilter(2.0f, 2.0f, 1.0f / 3.0f, 1.0f / 3.0f);
            var film = new ImageFilm(400, 400, filter, new float[] { 0, 1, 0, 1 });
            var aspectRatio = film.AspectRatio;

            var cameraTransform = Transform.Invert(Transform.LookAt(
                new Point(15, 7.5f, -15),
                new Point(0, 0, 0),
                new Vector(0, 1, 0)));
            var camera = new PerspectiveCamera(
                new AnimatedTransform(cameraTransform, 0, cameraTransform, 1),
                new[] { -1.0f, 1.0f, -1.0f / aspectRatio, 1.0f / aspectRatio },
                0.0f, 1.0f, 0.0f, 1e30f,
                60, film);

            var sampleExtent = film.SampleExtent;
            var sampler = new StratifiedSampler(sampleExtent, 2, 2, false,
                camera.ShutterOpen, camera.ShutterClose);

            var material = new MatteMaterial(
                new ConstantTexture<Spectrum>(new Spectrum(1, 1.5f, 3)),
                new ConstantTexture<float>(0.0f), null);
            var sphere = new Sphere(new Transform(new Matrix4x4()), false, 6.0f, -6.0f, 6.0f, 360.0f);

            var accelerator = new GridAccelerator(
                new Primitive[] { new TransformedPrimitive(new GeometricPrimitive(sphere, material, null),
                    new AnimatedTransform(new Transform(new Matrix4x4()), 0.0f, new Transform(new Matrix4x4()), 0.0f)) },
                false);

            var surfaceIntegrator = new WhittedIntegrator(5);
            var volumeIntegrator = new EmissionIntegrator();

            var lights = new Light[]
            {
                new PointLight(
                    Transform.Translate(new Vector(0, 20, -20)),
                    new Spectrum(1200))
            };

            scene = new Scene(accelerator, lights, null);
            renderer = new SamplerRenderer(sampler, camera, surfaceIntegrator, volumeIntegrator);
        }
    }
}