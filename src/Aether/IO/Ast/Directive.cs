namespace Aether.IO.Ast
{
    public abstract class Directive
    {
        public abstract void Process(SceneReaderContext context);
    }
}