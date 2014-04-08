using System;

namespace Aether.IO.Ast
{
    public class WorldEndDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("WorldEnd");

            if (context.GraphicsStateStack.Count > 0)
                throw new InvalidOperationException("Missing end to AttributeBegin");
            if (context.TransformStack.Count > 0)
                throw new InvalidOperationException("Missing end to TransformBegin");
        }
    }
}