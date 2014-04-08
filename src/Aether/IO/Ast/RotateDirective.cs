using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class RotateDirective : Directive
    {
        public float Angle { get; set; }
        public Vector Axis { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => t * Transform.Rotate(Angle, Axis));
        }
    }
}