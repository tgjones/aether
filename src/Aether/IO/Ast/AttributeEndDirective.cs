using System;

namespace Aether.IO.Ast
{
    public class AttributeEndDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("AttributeEnd");

            if (context.GraphicsStateStack.Count == 0)
                throw new InvalidOperationException("Unmatched AttributeEnd encountered.");

            context.GraphicsState = context.GraphicsStateStack.Pop();
            context.CurrentTransform = context.TransformStack.Pop();
            context.ActiveTransformBits = context.ActiveTransformBitsStack.Pop();
        }
    }
}