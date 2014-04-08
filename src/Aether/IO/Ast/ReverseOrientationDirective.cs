namespace Aether.IO.Ast
{
    public class ReverseOrientationDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("ReverseOrientation");
            context.GraphicsState.ReverseOrientation = !context.GraphicsState.ReverseOrientation;
        }
    }
}