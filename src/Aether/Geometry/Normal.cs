using System;

namespace Aether.Geometry
{
    public struct Normal
    {
        public float X;
        public float Y;
        public float Z;

        public Normal(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0 :
                        return X;
                    case 1 :
                        return Y;
                    case 2 :
                        return Z;
                    default :
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public float LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public float Length()
        {
            return MathUtility.Sqrt(LengthSquared());
        }

        #region Static properties

        public static readonly Normal Zero = new Normal(0, 0, 0);

        #endregion

        #region Static methods

        public static Vector Cross(Normal v1, Vector v2)
        {
            float v1x = v1.X, v1y = v1.Y, v1z = v1.Z;
            float v2x = v2.X, v2y = v2.Y, v2z = v2.Z;
            return new Vector(
                (v1y * v2z) - (v1z * v2y),
                (v1z * v2x) - (v1x * v2z),
                (v1x * v2y) - (v1y * v2x));
        }

        public static Normal Normalize(Normal n)
        {
            return n / n.Length();
        }

        public static float Dot(Normal n1, Vector n2)
        {
            return n1.X * n2.X + n1.Y * n2.Y + n1.Z * n2.Z;
        }

        public static float Dot(Normal n1, Normal n2)
        {
            return n1.X * n2.X + n1.Y * n2.Y + n1.Z * n2.Z;
        }

        public static float AbsDot(Normal v1, Vector v2)
        {
            return Math.Abs(Dot(v1, v2));
        }

        public static float AbsDot(Normal v1, Normal v2)
        {
            return Math.Abs(Dot(v1, v2));
        }

        public static Normal FaceForward(Normal n, Vector v)
        {
            return (Dot(n, v) < 0.0f) ? -n : n;
        }

        public static Normal FaceForward(Normal n, Normal v)
        {
            return (Dot(n, v) < 0.0f) ? -n : n;
        }

        #endregion

        #region Operators

        public static Normal operator +(Normal l, Normal r)
        {
            return new Normal(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        public static Normal operator -(Normal l, Normal r)
        {
            return new Normal(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        public static Normal operator -(Normal v)
        {
            return new Normal(-v.X, -v.Y, -v.Z);
        }

        public static Normal operator *(Normal l, float r)
        {
            return new Normal(l.X * r, l.Y * r, l.Z * r);
        }

        public static Normal operator *(float l, Normal r)
        {
            return new Normal(l * r.X, l * r.Y, l * r.Z);
        }

        public static Normal operator /(Normal l, float r)
        {
            return new Normal(l.X / r, l.Y / r, l.Z / r);
        }

        public static explicit operator Normal(Vector v)
        {
            return new Normal(v.X, v.Y, v.Z);
        }

        #endregion
    }
}