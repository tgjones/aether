using System;

namespace Aether.Geometry
{
    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;

        public Vector(float x, float y, float z)
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
            set
            {
                switch (index)
                {
                    case 0 :
                        X = value;
                        break;
                    case 1 :
                        Y = value;
                        break;
                    case 2 :
                        Z = value;
                        break;
                    default:
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

        public static readonly Vector Zero = new Vector(0, 0, 0);

        #endregion

        #region Static methods

        public static float Dot(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static float Dot(Vector v1, Normal v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static float AbsDot(Vector v1, Vector v2)
        {
            return Math.Abs(Dot(v1, v2));
        }

        public static float AbsDot(Vector v1, Normal v2)
        {
            return Math.Abs(Dot(v1, v2));
        }

        public static Vector Cross(Vector v1, Vector v2)
        {
            float v1x = v1.X, v1y = v1.Y, v1z = v1.Z;
            float v2x = v2.X, v2y = v2.Y, v2z = v2.Z;
            return new Vector(
                (v1y * v2z) - (v1z * v2y),
                (v1z * v2x) - (v1x * v2z),
                (v1x * v2y) - (v1y * v2x));
        }

        public static Vector Cross(Vector v1, Normal v2)
        {
            float v1x = v1.X, v1y = v1.Y, v1z = v1.Z;
            float v2x = v2.X, v2y = v2.Y, v2z = v2.Z;
            return new Vector(
                (v1y * v2z) - (v1z * v2y),
                (v1z * v2x) - (v1x * v2z),
                (v1x * v2y) - (v1y * v2x));
        }

        public static Vector Normalize(Vector v)
        {
            return v / v.Length();
        }

        public static void CoordinateSystem(Vector v1, out Vector v2, out Vector v3)
        {
            if (Math.Abs(v1.X) > Math.Abs(v1.Y))
            {
                float invLen = 1.0f / MathUtility.Sqrt(v1.X * v1.X + v1.Z * v1.Z);
                v2 = new Vector(-v1.Z * invLen, 0.0f, v1.X * invLen);
            }
            else
            {
                float invLen = 1.0f / MathUtility.Sqrt(v1.Y * v1.Y + v1.Z * v1.Z);
                v2 = new Vector(0.0f, v1.Z * invLen, -v1.Y * invLen);
            }
            v3 = Cross(v1, v2);
        }

        public static Vector FaceForward(Vector n, Vector v)
        {
            return (Dot(n, v) < 0.0f) ? -n : n;
        }

        public static Vector FaceForward(Vector n, Normal v)
        {
            return (Dot(n, v) < 0.0f) ? -n : n;
        }

        public static Vector SphericalDirection(float sintheta, float costheta, float phi)
        {
            return new Vector(
                sintheta * MathUtility.Cos(phi),
                sintheta * MathUtility.Sin(phi),
                costheta);
        }

        public static Vector SphericalDirection(
            float sintheta, float costheta, float phi,
            Vector x, Vector y, Vector z)
        {
            return sintheta * MathUtility.Cos(phi) * x
                + sintheta * MathUtility.Sin(phi) * y 
                + costheta * z;
        }

        public static float SphericalTheta(Vector v)
        {
            return MathUtility.Acos(MathUtility.Clamp(v.Z, -1.0f, 1.0f));
        }

        public static float SphericalPhi(Vector v)
        {
            float p = MathUtility.Atan2(v.Y, v.X);
            return (p < 0.0f) ? p + 2.0f * MathUtility.Pi : p;
        }

        #endregion

        #region Operators

        public static Vector operator +(Vector l, Vector r)
        {
            return new Vector(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        public static Vector operator -(Vector l, Vector r)
        {
            return new Vector(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y, -v.Z);
        }

        public static Vector operator *(Vector l, float r)
        {
            return new Vector(l.X * r, l.Y * r, l.Z * r);
        }

        public static Vector operator *(float l, Vector r)
        {
            return new Vector(l * r.X, l * r.Y, l * r.Z);
        }

        public static Vector operator /(Vector l, float r)
        {
            return new Vector(l.X / r, l.Y / r, l.Z / r);
        }

        public static explicit operator Vector(Point p)
        {
            return new Vector(p.X, p.Y, p.Z);
        }

        public static explicit operator Vector(Normal n)
        {
            return new Vector(n.X, n.Y, n.Z);
        }

        #endregion
    }
}