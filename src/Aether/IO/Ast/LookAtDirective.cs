using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class LookAtDirective : Directive
    {
        public Point Eye { get; set; }
        public Point LookAt { get; set; }
        public Vector Up { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => t * Transform.LookAt(Eye, LookAt, Up));
        }
    }
}