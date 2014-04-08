using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class ScaleDirective : Directive
    {
        public Vector Scale { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => t * Transform.Scale(Scale.X, Scale.Y, Scale.Z));
        }
    }
}