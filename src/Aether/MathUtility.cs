using System;

namespace Aether
{
    internal static class MathUtility
    {
        /// <summary>
        /// PI
        /// </summary>
        public static readonly float Pi = (float) Math.PI;

        /// <summary>
        /// 1 / PI
        /// </summary>
        public static readonly float InvPi = 1.0f / Pi;

        /// <summary>
        /// 1 / (2 * PI)
        /// </summary>
        public static readonly float InvTwoPi = 1.0f / (2.0f * Pi);

        /// <summary>
        /// 1 / (4 * PI)
        /// </summary>
        public static readonly float InvFourPi = 1.0f / (4.0f * Pi);

        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        public static float Lerp(float t, float v1, float v2)
        {
            return (1.0f - t) * v1 + (t * v2);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        public static float Sqrt(float value)
        {
            return (float) Math.Sqrt(value);
        }

        /// <summary>
        /// Returns e raised to the specified power.
        /// </summary>
        public static float Exp(float value)
        {
            return (float) Math.Exp(value);
        }

        public static float Pow(float x, float y)
        {
            return (float) Math.Pow(x, y);
        }

        public static float Sin(float value)
        {
            return (float) Math.Sin(value);
        }

        public static float Cos(float value)
        {
            return (float) Math.Cos(value);
        }

        public static float Acos(float value)
        {
            return (float) Math.Acos(value);
        }

        public static float Atan2(float y, float x)
        {
            return (float) Math.Atan2(y, x);
        }

        public static float Tan(float value)
        {
            return (float) Math.Tan(value);
        }

        /// <summary>
        /// Restricts a value to lie between a lower and higher bound.
        /// </summary>
        public static float Clamp(float value, float low, float high)
        {
            if (value < low)
                return low;
            if (value > high)
                return high;
            return value;
        }

        /// <summary>
        /// Restricts a value to lie between a lower and higher bound.
        /// </summary>
        public static int Clamp(int value, int low, int high)
        {
            if (value < low)
                return low;
            if (value > high)
                return high;
            return value;
        }

        /// <summary>
        /// Converts an angle in degrees to an angle in radians.
        /// </summary>
        public static float ToRadians(float degrees)
        {
            return Pi / 180.0f * degrees;
        }

        /// <summary>
        /// Converts an angle in radians to an angle in degrees.
        /// </summary>
        public static float ToDegrees(float radians)
        {
            return 180.0f / Pi * radians;
        }

        /// <summary>
        /// 1 / (log 2)
        /// </summary>
        public static readonly float InvLog2 = (float) (1.0f / (Math.Log(2.0f)));

        /// <summary>
        /// Calculates the binary logarithm of x.
        /// The binary logarithm of x is the power to which the number 2 must be
        /// raised to obtain the value x.
        /// </summary>
        public static float Log2(float x)
        {
            return (float) (Math.Log(x) * InvLog2);
        }

        /// <summary>
        /// Floor of the given number, cast to an integer.
        /// </summary>
        public static int Floor(float value)
        {
            return (int) Math.Floor(value);
        }

        /// <summary>
        /// Rounds the given number to the nearest integer.
        /// </summary>
        public static int Round(float value)
        {
            return Floor(value + 0.5f);
        }

        /// <summary>
        /// Ceiling of the given number, cast to an integer.
        /// </summary>
        public static int Ceiling(float value)
        {
            return (int) Math.Ceiling(value);
        }

        /// <summary>
        /// Binary logarithm of the given number, floored to an integer.
        /// </summary>
        public static int Log2Int(float value)
        {
            return Floor(Log2(value));
        }

        /// <summary>
        /// Returns true if the given number is a power of 2.
        /// </summary>
        public static bool IsPowerOf2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        public static uint RoundUpPow2(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            return v + 1;
        }

        /// <summary>
        /// Swaps the left and right values.
        /// </summary>
        public static void Swap<T>(ref T left, ref T right)
        {
            var temp = left;
            left = right;
            right = temp;
        }

        /// <summary>
        /// Solves a quadratic equation where a, b and c are the coefficients.
        /// (Quadratic equations have the form ax^2 + bx + c = 0.)
        /// </summary>
        public static bool TryQuadratic(float a, float b, float c, out float t0, out float t1)
        {
            // Find quadratic discriminant.
            var discrim = b * b - 4.0f * a * c;

            if (discrim < 0.0f)
            {
                t0 = t1 = float.NaN;
                return false;
            }

            var rootDiscrim = Sqrt(discrim);

            // Compute quadratic t values.
            var q = (b < 0.0f)
                ? -0.5f * (b - rootDiscrim)
                : -0.5f * (b + rootDiscrim);
            t0 = q / a;
            t1 = c / q;
            if (t0 > t1)
                Swap(ref t0, ref t1);
            return true;
        }

        public static float NextSingle(this Random random)
        {
            return (float) random.NextDouble();
        }
    }
}