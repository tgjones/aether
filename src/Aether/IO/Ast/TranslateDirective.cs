using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class TranslateDirective : Directive
    {
        public Vector Translation { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => t * Transform.Translate(Translation));
        }
    }
}