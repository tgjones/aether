using Aether.Shapes;

namespace Aether.Textures
{
    public abstract class Texture<T>
    {
        public abstract T Evaluate(DifferentialGeometry dg);
    }
}