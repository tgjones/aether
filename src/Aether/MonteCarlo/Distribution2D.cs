using System;
using System.Linq;

namespace Aether.MonteCarlo
{
    public class Distribution2D
    {
        private readonly Distribution1D[] _conditionalV;
        private readonly Distribution1D _marginal;

        public Distribution2D(float[] func, int nu, int nv)
        {
            // Compute conditional sampling distribution for $\tilde{v}$
            _conditionalV = Enumerable.Range(0, nv)
                .Select(v => new Distribution1D(new ArraySegment<float>(func, v * nu, nu)))
                .ToArray();

            // Compute marginal sampling distribution $p[\tilde{v}]$
            _marginal = new Distribution1D(new ArraySegment<float>(
                Enumerable.Range(0, nv).Select(v => _conditionalV[v].FuncInt).ToArray()));
        }

        public void SampleContinuous(float u0, float u1, out float u, out float v, out float pdf)
        {
            float pdf0, pdf1;
            int offset;
            v = _marginal.SampleContinuous(u1, out pdf1, out offset);
            u = _conditionalV[offset].SampleContinuous(u0, out pdf0, out offset);
            pdf = pdf0 * pdf1;
        }

        public float Pdf(float u, float v)
        {
            var iu = MathUtility.Clamp((int) (u * _conditionalV[0].Count), 0, _conditionalV[0].Count - 1);
            var iv = MathUtility.Clamp((int) (v * _marginal.Count), 0, _marginal.Count - 1);
            if (_conditionalV[iv].FuncInt * _marginal.FuncInt == 0.0f)
                return 0.0f;
            return (_conditionalV[iv].Func[iu] * _marginal.Func[iv]) /
                (_conditionalV[iv].FuncInt * _marginal.FuncInt);
        }
    }
}