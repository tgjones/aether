using System;

namespace Aether.IO.Ast
{
    public class TransformEndDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("TransformEnd");

            if (context.TransformStack.Count == 0)
                throw new InvalidOperationException("Unmatched TransformEnd encountered.");

            context.CurrentTransform = context.TransformStack.Pop();
            context.ActiveTransformBits = context.ActiveTransformBitsStack.Pop();
        }
    }
}