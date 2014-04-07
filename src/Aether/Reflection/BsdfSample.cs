using System;
using System.Diagnostics;
using Aether.Sampling;

namespace Aether.Reflection
{
    public struct BsdfSample
    {
        public float UDir0;
        public float UDir1;
        public float UComponent;

        public BsdfSample(float uDir0, float uDir1, float uComponent)
        {
            Debug.Assert(uDir0 >= 0.0f && uDir0 < 1.0f);
            Debug.Assert(uDir1 >= 0.0f && uDir1 < 1.0f);
            Debug.Assert(uComponent >= 0.0f && uComponent < 1.0f);

            UDir0 = uDir0;
            UDir1 = uDir1;
            UComponent = uComponent;
        }

        public BsdfSample(Sample sample, BsdfSampleOffsets offsets, int n)
        {
            Debug.Assert(n < sample.Num2D[offsets.PosOffset]);
            Debug.Assert(n < sample.Num1D[offsets.ComponentOffset]);
            UDir0 = sample.TwoD[offsets.PosOffset][2 * n];
            UDir1 = sample.TwoD[offsets.PosOffset][2 * n + 1];
            UComponent = sample.OneD[offsets.ComponentOffset][n];
            Debug.Assert(UDir0 >= 0.0f && UDir0 < 1.0f);
            Debug.Assert(UDir1 >= 0.0f && UDir1 < 1.0f);
            Debug.Assert(UComponent >= 0.0f && UComponent < 1.0f);
        }

        public BsdfSample(Random rng)
        {
            UDir0 = rng.NextSingle();
            UDir1 = rng.NextSingle();
            UComponent = rng.NextSingle();
        }
    }
}