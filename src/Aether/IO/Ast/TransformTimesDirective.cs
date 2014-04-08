namespace Aether.IO.Ast
{
    public class TransformTimesDirective : Directive
    {
        public float Start { get; set; }
        public float End { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyOptions("TransformTimes");
            context.RenderOptions.TransformStartTime = Start;
            context.RenderOptions.TransformEndTime = End;
        }
    }
}