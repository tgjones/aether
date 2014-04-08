using System;

namespace Aether.IO.Ast
{
    public class ObjectEndDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("ObjectEnd");
            if (context.RenderOptions.CurrentInstance == null)
                throw new InvalidOperationException("ObjectEnd called outside of instance definition.");

            context.RenderOptions.CurrentInstance = null;
            new AttributeEndDirective().Process(context);
        }
    }
}