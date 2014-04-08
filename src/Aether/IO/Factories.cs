using System;
using System.Collections.Generic;
using Aether.Accelerators;
using Aether.Cameras;
using Aether.Films;
using Aether.Filters;
using Aether.Geometry;
using Aether.Integrators;
using Aether.IO.Ast;
using Aether.Lights;
using Aether.Materials;
using Aether.Primitives;
using Aether.Sampling;
using Aether.Shapes;
using Aether.Textures;
using Aether.Volumes;

namespace Aether.IO
{
    public static class Factories
    {
        public static Aggregate MakeAccelerator(string name, ParamSet parameters, 
            IEnumerable<Primitive> primitives)
        {
            switch (name)
            {
                case "grid" :
                {
                    var refineImmediately = parameters.FindBoolean("refineimmediately", false);
                    return new GridAccelerator(primitives, refineImmediately);
                }
                default:
                    throw new ArgumentException("Unknown accelerator: " + name);
            }
        }

        public static AreaLight MakeAreaLight(string name, Transform lightToWorld,
            ParamSet parameters, Shape shape)
        {
            switch (name)
            {
                case "area":
                case "diffuse":
                {
                    var l = parameters.FindSpectrum("L", new Spectrum(1.0f));
                    var scale = parameters.FindSpectrum("scale", new Spectrum(1.0f));
                    var numSamples = parameters.FindInt32("nsamples", 1);
                    return new DiffuseAreaLight(lightToWorld, l * scale, numSamples, shape);
                }
                default:
                    throw new ArgumentException("Unknown area light: " + name);
            }
        }

        public static Camera MakeCamera(string name, ParamSet parameters,
            TransformSet cameraToWorldSet, float transformStart, float transformEnd,
            Film film)
        {
            var animatedCameraToWorld = new AnimatedTransform(
                cameraToWorldSet[0], transformStart,
                cameraToWorldSet[1], transformEnd);

            switch (name)
            {
                case "perspective" :
                {
                    var shutterOpen = parameters.FindSingle("shutteropen", 0.0f);
                    var shutterClose = parameters.FindSingle("shutterclose", 1.0f);
                    if (shutterOpen < shutterClose)
                        MathUtility.Swap(ref shutterOpen, ref shutterClose);
                    var lensRadius = parameters.FindSingle("lensradius", 0.0f);
                    var focalDistance = parameters.FindSingle("focaldistance", 1e30f);
                    var frame = parameters.FindSingle("frameaspectratio", film.XResolution / (float) film.YResolution);
                    var screenWindow = parameters.FindSingleList("screenwindow");
                    if (screenWindow.Length != 4)
                        screenWindow = (frame > 1.0f)
                            ? new[] { -frame, frame, -1, 1 }
                            : new[] { -1, 1, -1 / frame, 1 / frame };
                    var fieldOfView = parameters.FindSingle("fov", 90.0f);

                    return new PerspectiveCamera(animatedCameraToWorld, screenWindow, shutterOpen, shutterClose,
                        lensRadius, focalDistance, fieldOfView, film);
                }
                default :
                    throw new ArgumentException("Unknown camera: " + name);
            }
        }

        public static Film MakeFilm(string name, ParamSet parameters, Filter filter)
        {
            switch (name)
            {
                case "image" :
                {
                    var xres = parameters.FindInt32("xresolution", 640);
                    var yres = parameters.FindInt32("yresolution", 480);
                    var cropWindow = parameters.FindSingleList("cropwindow");
                    if (cropWindow.Length == 0)
                        cropWindow = new float[] { 0, 1, 0, 1 };
                    return new ImageFilm(xres, yres, filter, cropWindow);
                }
                default:
                    throw new ArgumentException("Unknown film: " + name);
            }
        }

        public static Filter MakeFilter(string name, ParamSet parameters)
        {
            switch (name)
            {
                case "mitchell" :
                {
                    var xWidth = parameters.FindSingle("xwidth", 2.0f);
                    var yWidth = parameters.FindSingle("ywidth", 2.0f);
                    var b = parameters.FindSingle("B", 1.0f / 3.0f);
                    var c = parameters.FindSingle("C", 1.0f / 3.0f);
                    return new MitchellFilter(xWidth, yWidth, b, c);
                }
                default :
                    throw new ArgumentException("Unknown filter: " + name);
            }
        }

        public static Light MakeLight(string name, Transform lightToWorld, ParamSet parameters)
        {
            switch (name)
            {
                case "point":
                {
                    var i = parameters.FindSpectrum("I", new Spectrum(1.0f));
                    var scale = parameters.FindSpectrum("scale", new Spectrum(1.0f));
                    var from = parameters.FindPoint("from", Point.Zero);
                    var l2w = Transform.Translate((Vector) from) * lightToWorld;
                    return new PointLight(l2w, i * scale);
                    }
                default:
                    throw new ArgumentException("Unknown light: " + name);
            }
        }

        public static Material MakeMaterial(string name, Transform materialToWorld, TextureParams parameters)
        {
            switch (name)
            {
                case "matte" :
                {
                    var kd = parameters.GetSpectrumTexture("Kd", new Spectrum(0.5f));
                    var sigma = parameters.GetFloatTexture("sigma", 0.0f);
                    var bumpMap = parameters.GetOptionalFloatTexture("bumpmap");
                    return new MatteMaterial(kd, sigma, bumpMap);
                }
                default:
                    throw new ArgumentException("Unknown material: " + name);
            }
        }

        public static Sampler MakeSampler(string name, ParamSet parameters, Film film, Camera camera)
        {
            switch (name)
            {
                case "stratified":
                {
                    var jitter = parameters.FindBoolean("jitter", true);
                    var xSamples = parameters.FindInt32("xsamples", 2);
                    var ySamples = parameters.FindInt32("ysamples", 2);
                    var sampleExtent = film.SampleExtent;
                    return new StratifiedSampler(sampleExtent, xSamples, ySamples, jitter, 
                        camera.ShutterOpen, camera.ShutterClose);
                    }
                default:
                    throw new ArgumentException("Unknown sampler: " + name);
            }
        }

        public static Shape MakeShape(string name, Transform objectToWorld, 
            bool reverseOrientation, ParamSet parameters)
        {
            switch (name)
            {
                case "sphere":
                {
                    var radius = parameters.FindSingle("radius", 1.0f);
                    var zMin = parameters.FindSingle("zmin", -radius);
                    var zMax = parameters.FindSingle("zmax", radius);
                    var phiMax = parameters.FindSingle("phimax", 360.0f);
                    return new Sphere(objectToWorld, reverseOrientation, radius,
                        zMin, zMax, phiMax);
                }
                default:
                    throw new ArgumentException("Unknown shape: " + name);
            }
        }

        public static SurfaceIntegrator MakeSurfaceIntegrator(string name, ParamSet parameters)
        {
            switch (name)
            {
                case "whitted":
                {
                    var maxDepth = parameters.FindInt32("maxdepth", 5);
                    return new WhittedIntegrator(maxDepth);
                    }
                default:
                    throw new ArgumentException("Unknown surface integrator: " + name);
            }
        }

        public static VolumeIntegrator MakeVolumeIntegrator(string name, ParamSet parameters)
        {
            switch (name)
            {
                case "emission":
                    {
                        var stepSize = parameters.FindSingle("stepsize", 1.0f);
                        return new EmissionIntegrator(stepSize);
                    }
                default:
                    throw new ArgumentException("Unknown volume integrator: " + name);
            }
        }

        public static VolumeRegion MakeVolumeRegion(string name, Transform volumeToWorld, ParamSet parameters)
        {
            switch (name)
            {
                default:
                    throw new ArgumentException("Unknown volume region: " + name);
            }
        }

        public static Texture<float> MakeFloatTexture(string name, Transform textureToWorld, TextureParams textureParams)
        {
            switch (name)
            {
                case "constant":
                    {
                        return new ConstantTexture<float>(textureParams.FindSingle("value", 1.0f));
                    }
                default:
                    throw new ArgumentException("Unknown float texture: " + name);
            }
        }

        public static Texture<Spectrum> MakeSpectrumTexture(string name, Transform textureToWorld, TextureParams textureParams)
        {
            switch (name)
            {
                case "constant":
                    {
                        return new ConstantTexture<Spectrum>(textureParams.FindSpectrum("value", new Spectrum(1.0f)));
                    }
                default:
                    throw new ArgumentException("Unknown spectrum texture: " + name);
            }
        }
    }
}