namespace Aether.IO.Ast
{
    public class StandardDirective : Directive
    {
        public StandardDirectiveType DirectiveType { get; set; }
        public string ImplementationType { get; set; }
        public ParamSet Parameters { get; set; }
    }
}