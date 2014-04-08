namespace Aether.IO.Ast
{
    public class NamedMaterialDirective : Directive
    {
        public string MaterialName { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("NamedMaterial");
            context.GraphicsState.CurrentNamedMaterial = MaterialName;
        }
    }
}