using System;

namespace Aether.Filters
{
    public class MitchellFilter : Filter
    {
        private readonly float _b;
        private readonly float _c;

        public MitchellFilter(float xWidth, float yWidth, float b, float c) 
            : base(xWidth, yWidth)
        {
            _b = b;
            _c = c;
        }

        public override float Evaluate(float x, float y)
        {
            return Mitchell1D(x * InverseXWidth) * Mitchell1D(y * InverseYWidth);
        }

        private float Mitchell1D(float x)
        {
            x = Math.Abs(2.0f * x);
            if (x > 1.0f)
                return ((-_b - 6 * _c) * x * x * x + (6 * _b + 30 * _c) * x * x +
                    (-12 * _b - 48 * _c) * x + (8 * _b + 24 * _c)) * (1.0f / 6.0f);
            return ((12 - 9 * _b - 6 * _c) * x * x * x +
                (-18 + 12 * _b + 6 * _c) * x * x +
                (6 - 2 * _b)) * (1.0f / 6.0f);
        }
    }
}