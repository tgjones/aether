namespace Aether.IO.Ast
{
    public class TextureDirective : Directive
    {
        public string Name { get; set; }
        public string TextureType { get; set; }
        public string TextureClass { get; set; }
        public ParamSet Parameters { get; set; }
    }
}