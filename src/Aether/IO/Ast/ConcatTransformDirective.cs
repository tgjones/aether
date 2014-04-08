using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class ConcatTransformDirective : Directive
    {
        public Matrix4x4 Transform { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => t * new Transform(Transform));
        }
    }
}