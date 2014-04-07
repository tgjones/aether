using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class ConcatTransformDirective : Directive
    {
        public Matrix4x4 Transform { get; set; }
    }
}