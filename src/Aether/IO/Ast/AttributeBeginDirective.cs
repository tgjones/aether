namespace Aether.IO.Ast
{
    public class AttributeBeginDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("AttributeBegin");
            context.GraphicsStateStack.Push(context.GraphicsState.Clone());
            context.TransformStack.Push(context.CurrentTransform.Clone());
            context.ActiveTransformBitsStack.Push(context.ActiveTransformBits);
        }
    }
}