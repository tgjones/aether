using System;
using System.Collections.Generic;
using Aether.Cameras;
using Aether.Geometry;
using Aether.IO.Ast;
using Aether.Lights;
using Aether.Primitives;
using Aether.Renderers;
using Aether.Volumes;

namespace Aether.IO
{
    public class RenderOptions
    {
        public float TransformStartTime = 0.0f;
        public float TransformEndTime = 1.0f;
        public string FilterName = "box";
        public ParamSet FilterParams = new ParamSet();
        public string FilmName = "image";
        public ParamSet FilmParams = new ParamSet();
        public string SamplerName = "lowdiscrepancy";
        public ParamSet SamplerParams = new ParamSet();
        public string AcceleratorName = "bvh";
        public ParamSet AcceleratorParams = new ParamSet();
        public string RendererName = "sampler";
        public ParamSet RendererParams = new ParamSet();
        public string SurfaceIntegratorName = "directlighting";
        public ParamSet SurfaceIntegratorParams = new ParamSet();
        public string VolumeIntegratorName = "emission";
        public ParamSet VolumeIntegratorParams = new ParamSet();
        public string CameraName = "perspective";
        public ParamSet CameraParams = new ParamSet();
        public TransformSet CameraToWorld = new TransformSet();
        public List<Light> Lights = new List<Light>();
        public List<Primitive> Primitives = new List<Primitive>();
        public List<VolumeRegion> VolumeRegions = new List<VolumeRegion>(); 
        public Dictionary<string, List<Primitive>> Instances = new Dictionary<string, List<Primitive>>();
        public List<Primitive> CurrentInstance;

        public Camera MakeCamera()
        {
            var filter = Factories.MakeFilter(FilterName, FilterParams);
            var film = Factories.MakeFilm(FilmName, FilmParams, filter);
            return Factories.MakeCamera(CameraName, CameraParams, CameraToWorld,
                TransformStartTime, TransformEndTime, film);
        }

        public Renderer MakeRenderer()
        {
            var camera = MakeCamera();

            switch (RendererName)
            {
                case "sampler" :
                {
                    var sampler = Factories.MakeSampler(SamplerName, SamplerParams, camera.Film, camera);
                    var surfaceIntegrator = Factories.MakeSurfaceIntegrator(
                        SurfaceIntegratorName, SurfaceIntegratorParams);
                    var volumeIntegrator = Factories.MakeVolumeIntegrator(
                        VolumeIntegratorName, VolumeIntegratorParams);
                    return new SamplerRenderer(sampler, camera, surfaceIntegrator, volumeIntegrator);
                }
                default :
                    throw new ArgumentException("Unknown renderer: " + RendererName);
            }
        }

        public Scene MakeScene()
        {
            VolumeRegion volumeRegion;
            switch (VolumeRegions.Count)
            {
                case 0:
                    volumeRegion = null;
                    break;
                case 1:
                    volumeRegion = VolumeRegions[0];
                    break;
                default:
                    volumeRegion = new AggregateVolume(VolumeRegions);
                    break;
            }
            var accelerator = Factories.MakeAccelerator(AcceleratorName, AcceleratorParams, Primitives);
            var scene = new Scene(accelerator, Lights, volumeRegion);
            return scene;
        }
    }
}