using Aether.Shapes;

namespace Aether.Textures
{
    public class ConstantTexture<T> : Texture<T>
    {
        private readonly T _value;

        public ConstantTexture(T value)
        {
            _value = value;
        }

        public override T Evaluate(DifferentialGeometry dg)
        {
            return _value;
        }
    }
}