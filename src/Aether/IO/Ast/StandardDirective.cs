using System;
using Aether.Accelerators;
using Aether.Geometry;
using Aether.Lights;
using Aether.Primitives;

namespace Aether.IO.Ast
{
    public class StandardDirective : Directive
    {
        public StandardDirectiveType DirectiveType { get; set; }
        public string ImplementationType { get; set; }
        public ParamSet Parameters { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyOptions(DirectiveType.ToString());

            switch (DirectiveType)
            {
                case StandardDirectiveType.Accelerator:
                    context.VerifyOptions("Accelerator");
                    context.RenderOptions.AcceleratorName = ImplementationType;
                    context.RenderOptions.AcceleratorParams = Parameters;
                    break;
                case StandardDirectiveType.AreaLightSource:
                    context.VerifyWorld("AreaLightSource");
                    context.GraphicsState.AreaLight = ImplementationType;
                    context.GraphicsState.AreaLightParams = Parameters;
                    break;
                case StandardDirectiveType.Camera:
                    context.VerifyOptions("Camera");
                    context.RenderOptions.CameraName = ImplementationType;
                    context.RenderOptions.CameraParams = Parameters;
                    context.RenderOptions.CameraToWorld = TransformSet.Invert(context.CurrentTransform);
                    context.NamedCoordinateSystems["camera"] = context.RenderOptions.CameraToWorld;
                    break;
                case StandardDirectiveType.Film:
                    context.VerifyOptions("Film");
                    context.RenderOptions.FilmName = ImplementationType;
                    context.RenderOptions.FilmParams = Parameters;
                    break;
                case StandardDirectiveType.LightSource:
                    context.VerifyWorld("LightSource");
                    context.RenderOptions.Lights.Add(Factories.MakeLight(
                        ImplementationType, context.CurrentTransform[0], Parameters));
                    break;
                case StandardDirectiveType.Material:
                    context.VerifyOptions("Material");
                    context.GraphicsState.Material = ImplementationType;
                    context.GraphicsState.MaterialParams = Parameters;
                    context.GraphicsState.CurrentNamedMaterial = "";
                    break;
                case StandardDirectiveType.PixelFilter:
                    context.VerifyOptions("PixelFilter");
                    context.RenderOptions.FilterName = ImplementationType;
                    context.RenderOptions.FilterParams = Parameters;
                    break;
                case StandardDirectiveType.Renderer:
                    context.VerifyOptions("Renderer");
                    context.RenderOptions.RendererName = ImplementationType;
                    context.RenderOptions.RendererParams = Parameters;
                    break;
                case StandardDirectiveType.Sampler:
                    context.VerifyOptions("Sampler");
                    context.RenderOptions.SamplerName = ImplementationType;
                    context.RenderOptions.SamplerParams = Parameters;
                    break;
                case StandardDirectiveType.Shape:
                    context.VerifyWorld("Shape");
                    ProcessShape(context);
                    break;
                case StandardDirectiveType.SurfaceIntegrator:
                    context.VerifyOptions("SurfaceIntegrator");
                    context.RenderOptions.SurfaceIntegratorName = ImplementationType;
                    context.RenderOptions.SurfaceIntegratorParams = Parameters;
                    break;
                case StandardDirectiveType.Volume:
                    context.VerifyWorld("Volume");
                    context.RenderOptions.VolumeRegions.Add(Factories.MakeVolumeRegion(
                        ImplementationType, context.CurrentTransform[0], Parameters));
                    break;
                case StandardDirectiveType.VolumeIntegrator:
                    context.VerifyOptions("VolumeIntegrator");
                    context.RenderOptions.VolumeIntegratorName = ImplementationType;
                    context.RenderOptions.VolumeIntegratorParams = Parameters;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessShape(SceneReaderContext context)
        {
            Primitive prim = null;
            AreaLight area = null;
            if (!context.CurrentTransform.IsAnimated)
            {
                // Create primitive for static shape.

                var shape = Factories.MakeShape(ImplementationType,
                    context.CurrentTransform[0], context.GraphicsState.ReverseOrientation,
                    Parameters);
                var material = context.GraphicsState.CreateMaterial(
                    Parameters, context.CurrentTransform[0]);

                // Possibly create area light for shape.
                if (!string.IsNullOrEmpty(context.GraphicsState.AreaLight))
                    area = Factories.MakeAreaLight(
                        context.GraphicsState.AreaLight,
                        context.CurrentTransform[0],
                        context.GraphicsState.AreaLightParams,
                        shape);
                prim = new GeometricPrimitive(shape, material, area);
            }
            else
            {
                // Create primitive for animated shape.

                var shape = Factories.MakeShape(ImplementationType, new Transform(),
                    context.GraphicsState.ReverseOrientation, Parameters);
                var material = context.GraphicsState.CreateMaterial(
                    Parameters, context.CurrentTransform[0]);

                // Get animatedWorldToObject transform for shape.
                var worldToObj0 = Transform.Invert(context.CurrentTransform[0]);
                var worldToObj1 = Transform.Invert(context.CurrentTransform[1]);
                var animatedWorldToObject = new AnimatedTransform(
                    worldToObj0, context.RenderOptions.TransformStartTime,
                    worldToObj1, context.RenderOptions.TransformEndTime);

                Primitive basePrim = new GeometricPrimitive(shape, material, null);
                if (!basePrim.CanIntersect)
                {
                    // Refine animated shape and create BVH if more than one shape created.
                    var refinedPrimitives = basePrim.FullyRefine();
                    if (refinedPrimitives.Count == 0)
                        return;
                    basePrim = (refinedPrimitives.Count > 1)
                        ? new BoundingVolumeHierarchyAccelerator(refinedPrimitives) 
                        : refinedPrimitives[0];
                }
                prim = new TransformedPrimitive(basePrim, animatedWorldToObject);
            }

            // Add primitive to scene or current instance.
            if (context.RenderOptions.CurrentInstance != null)
            {
                context.RenderOptions.CurrentInstance.Add(prim);
            }
            else
            {
                context.RenderOptions.Primitives.Add(prim);
                if (area != null)
                    context.RenderOptions.Lights.Add(area);
            }
        }
    }
}