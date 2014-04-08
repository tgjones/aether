using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class TransformDirective : Directive
    {
        public Matrix4x4 Transform { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => new Transform(Transform));
        }
    }
}