namespace Aether.IO.Ast
{
    public class TransformBeginDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("TransformBegin");
            context.TransformStack.Push(context.CurrentTransform.Clone());
            context.ActiveTransformBitsStack.Push(context.ActiveTransformBits);
        }
    }
}