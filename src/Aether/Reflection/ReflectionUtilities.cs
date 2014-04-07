using System;
using Aether.Geometry;

namespace Aether.Reflection
{
    public static class ReflectionUtilities
    {
        public static bool SameHemisphere(ref Vector w, ref Vector wp)
        {
            return w.Z * wp.Z > 0.0f;
        }

        public static float CosTheta(ref Vector w)
        {
            return w.Z;
        }

        public static float AbsCosTheta(ref Vector w)
        {
            return Math.Abs(w.Z);
        }

        public static float SinTheta2(ref Vector w)
        {
            return Math.Max(0.0f, 1.0f - CosTheta(ref w) * CosTheta(ref w));
        }

        public static float SinTheta(ref Vector w)
        {
            return MathUtility.Sqrt(SinTheta2(ref w));
        }

        public static float CosPhi(ref Vector w)
        {
            float sintheta = SinTheta(ref w);
            if (sintheta == 0.0f)
                return 1.0f;
            return MathUtility.Clamp(w.X / sintheta, -1.0f, 1.0f);
        }

        public static float SinPhi(ref Vector w)
        {
            float sintheta = SinTheta(ref w);
            if (sintheta == 0.0f)
                return 0.0f;
            return MathUtility.Clamp(w.Y / sintheta, -1.0f, 1.0f);
        }
    }
}