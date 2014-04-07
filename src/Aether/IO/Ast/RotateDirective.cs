using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class RotateDirective : Directive
    {
        public float Angle { get; set; }
        public Vector Axis { get; set; }
    }
}