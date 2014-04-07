using System;
using System.Diagnostics;
using Aether.Sampling;

namespace Aether.Lights
{
    public struct LightSample
    {
        public float UPos0;
        public float UPos1;
        public float UComponent;

        public LightSample(float uPos0, float uPos1, float uComponent)
        {
            Debug.Assert(uPos0 >= 0.0f && uPos0 < 1.0f);
            Debug.Assert(uPos1 >= 0.0f && uPos1 < 1.0f);
            Debug.Assert(uComponent >= 0.0f && uComponent < 1.0f);

            UPos0 = uPos0;
            UPos1 = uPos1;
            UComponent = uComponent;
        }

        public LightSample(Sample sample, LightSampleOffsets offsets, int n)
        {
            Debug.Assert(n < sample.Num2D[offsets.PosOffset]);
            Debug.Assert(n < sample.Num1D[offsets.ComponentOffset]);
            UPos0 = sample.TwoD[offsets.PosOffset][2 * n];
            UPos1 = sample.TwoD[offsets.PosOffset][2 * n + 1];
            UComponent = sample.OneD[offsets.ComponentOffset][n];
            Debug.Assert(UPos0 >= 0.0f && UPos0 < 1.0f);
            Debug.Assert(UPos1 >= 0.0f && UPos1 < 1.0f);
            Debug.Assert(UComponent >= 0.0f && UComponent < 1.0f);
        }

        public LightSample(Random rng)
        {
            UPos0 = rng.NextSingle();
            UPos1 = rng.NextSingle();
            UComponent = rng.NextSingle();
        }
    }
}