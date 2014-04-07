using System;
using System.Collections.Generic;

namespace Aether.MonteCarlo
{
    public static class SamplingUtilities
    {
        public static void StratifiedSample1D(IList<float> samples, int nSamples, Random rng, bool jitter = true)
        {
            float invTot = 1.0f / nSamples;
            var index = 0;
            for (int i = 0; i < nSamples; i++)
            {
                float delta = jitter ? rng.NextSingle() : 0.5f;
                samples[index++] = Math.Min((i + delta) * invTot, MonteCarloUtilities.OneMinusEpsilon);
            }
        }

        public static void StratifiedSample2D(IList<float> samples, int nx, int ny, Random rng, bool jitter = true)
        {
            float dx = 1.0f / nx, dy = 1.0f / ny;
            var index = 0;
            for (int y = 0; y < ny; ++y)
                for (int x = 0; x < nx; ++x)
                {
                    float jx = jitter ? rng.NextSingle() : 0.5f;
                    float jy = jitter ? rng.NextSingle() : 0.5f;
                    samples[index++] = Math.Min((x + jx) * dx, MonteCarloUtilities.OneMinusEpsilon);
                    samples[index++] = Math.Min((y + jy) * dy, MonteCarloUtilities.OneMinusEpsilon);
                }
        }

        public static void Shuffle<T>(IList<T> samp, int count, int dims, Random rng)
        {
            for (var i = 0; i < count; ++i)
            {
                var other = i + (rng.Next() % (count - i));
                for (var j = 0; j < dims; ++j)
                {
                    var temp = samp[dims * i + j];
                    samp[dims * i + j] = samp[dims * other + j];
                    samp[dims * other + j] = temp;
                }
            }
        }
    }
}