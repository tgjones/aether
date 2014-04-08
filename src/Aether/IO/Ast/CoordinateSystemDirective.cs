namespace Aether.IO.Ast
{
    public class CoordinateSystemDirective : Directive
    {
        public string Name { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.NamedCoordinateSystems[Name] = context.CurrentTransform;
        }
    }
}