using System;
using System.Diagnostics;
using System.Linq;

namespace Aether.MonteCarlo
{
    public class Distribution1D
    {
        private readonly float[] _func;
        private readonly float[] _cdf;
        private readonly float _funcInt;
        private readonly int _count;

        public Distribution1D(ArraySegment<float> f)
        {
            _count = f.Count;
            _func = f.ToArray();
            _cdf = new float[Count + 1];
            // Compute integral of step function at x_i.
            _cdf[0] = 0.0f;
            for (int i = 1; i < Count + 1; ++i)
                _cdf[i] = _cdf[i - 1] + Func[i - 1] / Count;

            // Transform step function integral into CDF
            _funcInt = _cdf[Count];
            if (FuncInt == 0.0f)
                for (int i = 1; i < Count + 1; ++i)
                    _cdf[i] = (float) i / Count;
            else
                for (int i = 1; i < Count + 1; ++i)
                    _cdf[i] /= FuncInt;
        }

        public float[] Func
        {
            get { return _func; }
        }

        public float FuncInt
        {
            get { return _funcInt; }
        }

        public int Count
        {
            get { return _count; }
        }

        public float SampleContinuous(float u, out float pdf, out int off)
        {
            // Find surrounding CDF segments and _offset_
            var offset = Array.FindIndex(_cdf, x => x > u);
            off = offset;
            Debug.Assert(offset < Count);
            Debug.Assert(u >= _cdf[offset] && u < _cdf[offset + 1]);

            // Compute offset along CDF segment
            float du = (u - _cdf[offset]) / (_cdf[offset + 1] - _cdf[offset]);
            Debug.Assert(!float.IsNaN(du));

            // Compute PDF for sampled offset
            pdf = Func[offset] / FuncInt;

            // Return $x\in{}[0,1)$ corresponding to sample
            return (offset + du) / Count;
        }

        public int SampleDiscrete(float u, out float pdf)
        {
            // Find surrounding CDF segments and offset.
            var offset = Array.FindIndex(_cdf, x => x > u);
            Debug.Assert(offset < Count);
            Debug.Assert(u >= _cdf[offset] && u < _cdf[offset + 1]);
            pdf = Func[offset] / (FuncInt * Count);
            return offset;
        }
    }
}