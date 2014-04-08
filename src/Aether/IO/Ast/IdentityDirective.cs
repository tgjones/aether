using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class IdentityDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.ForActiveTransforms(t => new Transform());
        }
    }
}