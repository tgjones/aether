using System;
using Aether.Geometry;
using Aether.Primitives;

namespace Aether.IO.Ast
{
    public class ObjectInstanceDirective : Directive
    {
        public string Name { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("ObjectInstance");

            if (context.RenderOptions.CurrentInstance != null)
                throw new InvalidOperationException("ObjectInstance can't be called inside instance definition.");
            if (!context.RenderOptions.Instances.ContainsKey(Name))
                throw new InvalidOperationException("Unable to find instance named '" + Name + "'.");

            var instances = context.RenderOptions.Instances[Name];
            if (instances.Count == 0)
                return;

            if (instances.Count > 1 || !instances[0].CanIntersect)
            {
                // Refine instance primitives and create aggregate.
                var accelerator = Factories.MakeAccelerator(
                    context.RenderOptions.AcceleratorName,
                    context.RenderOptions.AcceleratorParams,
                    instances);
                instances.Clear();
                instances.Add(accelerator);
            }

            var worldToInstance0 = Transform.Invert(context.CurrentTransform[0]);
            var worldToInstance1 = Transform.Invert(context.CurrentTransform[1]);
            var animatedWorldToInstance = new AnimatedTransform(
                worldToInstance0, context.RenderOptions.TransformStartTime,
                worldToInstance1, context.RenderOptions.TransformEndTime);
            context.RenderOptions.Primitives.Add(new TransformedPrimitive(
                instances[0], animatedWorldToInstance));
        }
    }
}