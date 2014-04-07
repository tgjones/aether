using System;
using Aether.Geometry;

namespace Aether.MonteCarlo
{
    public static class MonteCarloUtilities
    {
        /// <summary>
        /// Smallest floating point value less than one; all canonical random samples
        /// should be &lt;= this.
        /// </summary>
        public const float OneMinusEpsilon = 0.9999999403953552f;

        public static void LatinHypercube(float[] samples, int nSamples, int nDim, Random rng)
        {
            // Generate LHS samples along diagonal
            float delta = 1.0f / nSamples;
            for (var i = 0; i < nSamples; ++i)
                for (var j = 0; j < nDim; ++j)
                    samples[nDim * i + j] = Math.Min((i + (rng.NextSingle())) * delta, OneMinusEpsilon);

            // Permute LHS samples in each dimension
            for (var i = 0; i < nDim; ++i)
                for (var j = 0; j < nSamples; ++j)
                {
                    var other = j + (rng.Next() % (nSamples - j));
                    MathUtility.Swap(ref samples[nDim * j + i], ref samples[nDim * other + i]);
                }
        }

        public static Vector CosineSampleHemisphere(float u1, float u2)
        {
            var ret = new Vector();
            ConcentricSampleDisk(u1, u2, out ret.X, out ret.Y);
            ret.Z = MathUtility.Sqrt(Math.Max(0.0f, 1.0f - ret.X * ret.X - ret.Y * ret.Y));
            return ret;
        }

        public static Vector UniformSampleHemisphere(float u1, float u2)
        {
            float z = u1;
            float r = MathUtility.Sqrt(Math.Max(0.0f, 1.0f - z * z));
            float phi = 2 * MathUtility.Pi * u2;
            float x = r * MathUtility.Cos(phi);
            float y = r * MathUtility.Sin(phi);
            return new Vector(x, y, z);
        }

        public static float UniformHemispherePdf()
        {
            return MathUtility.InvTwoPi;
        }

        public static Vector UniformSampleSphere(float u1, float u2)
        {
            float z = 1.0f - 2.0f * u1;
            float r = MathUtility.Sqrt(Math.Max(0.0f, 1.0f - z * z));
            float phi = 2.0f * MathUtility.Pi * u2;
            float x = r * MathUtility.Cos(phi);
            float y = r * MathUtility.Sin(phi);
            return new Vector(x, y, z);
        }

        public static float UniformSpherePdf()
        {
            return 1.0f / (4.0f * MathUtility.Pi);
        }

        public static void UniformSampleDisk(float u1, float u2, out float x, out float y)
        {
            float r = MathUtility.Sqrt(u1);
            float theta = 2.0f * MathUtility.Pi * u2;
            x = r * MathUtility.Cos(theta);
            y = r * MathUtility.Sin(theta);
        }

        public static void ConcentricSampleDisk(float u1, float u2, out float dx, out float dy)
        {
            float r, theta;
            // Map uniform random numbers to $[-1,1]^2$
            float sx = 2 * u1 - 1;
            float sy = 2 * u2 - 1;

            // Map square to $(r,\theta)$

            // Handle degeneracy at the origin
            if (sx == 0.0f && sy == 0.0f)
            {
                dx = 0.0f;
                dy = 0.0f;
                return;
            }
            if (sx >= -sy)
            {
                if (sx > sy)
                {
                    // Handle first region of disk
                    r = sx;
                    if (sy > 0.0) theta = sy / r;
                    else theta = 8.0f + sy / r;
                }
                else
                {
                    // Handle second region of disk
                    r = sy;
                    theta = 2.0f - sx / r;
                }
            }
            else
            {
                if (sx <= sy)
                {
                    // Handle third region of disk
                    r = -sx;
                    theta = 4.0f - sy / r;
                }
                else
                {
                    // Handle fourth region of disk
                    r = -sy;
                    theta = 6.0f + sx / r;
                }
            }
            theta *= MathUtility.Pi / 4.0f;
            dx = r * MathUtility.Cos(theta);
            dy = r * MathUtility.Sin(theta);
        }

        public static void UniformSampleTriangle(float u1, float u2, out float u, out float v)
        {
            float su1 = MathUtility.Sqrt(u1);
            u = 1.0f - su1;
            v = u2 * su1;
        }

        public static float UniformConePdf(float cosThetaMax)
        {
            return 1.0f / (2.0f * MathUtility.Pi * (1.0f - cosThetaMax));
        }

        public static Vector UniformSampleCone(float u1, float u2, float costhetamax)
        {
            float costheta = (1.0f - u1) + u1 * costhetamax;
            float sintheta = MathUtility.Sqrt(1.0f - costheta * costheta);
            float phi = u2 * 2.0f * MathUtility.Pi;
            return new Vector(
                MathUtility.Cos(phi) * sintheta, 
                MathUtility.Sin(phi) * sintheta, 
                costheta);
        }

        public static Vector UniformSampleCone(
            float u1, float u2, float costhetamax,
            ref Vector x, ref Vector y, ref Vector z)
        {
            float costheta = MathUtility.Lerp(u1, costhetamax, 1.0f);
            float sintheta = MathUtility.Sqrt(1.0f - costheta * costheta);
            float phi = u2 * 2.0f * MathUtility.Pi;
            return MathUtility.Cos(phi) * sintheta * x + MathUtility.Sin(phi) * sintheta * y + costheta * z;
        }
    }
}