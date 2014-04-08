using System;

namespace Aether.Geometry
{
    public struct Point
    {
        public float X;
        public float Y;
        public float Z;

        public Point(float x, float y, float z)
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

        #region Static properties

        public static readonly Point Zero = new Point(0, 0, 0);

        #endregion

        #region Static methods

        public static float Distance(Point p1, Point p2)
        {
            return (p1 - p2).Length();
        }

        public static float DistanceSquared(Point p1, Point p2)
        {
            return (p1 - p2).LengthSquared();
        }

        #endregion

        #region Operators

        public static Point operator +(Point l, Point r)
        {
            return new Point(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        public static Point operator +(Point l, Vector r)
        {
            return new Point(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        public static Vector operator -(Point l, Point r)
        {
            return new Vector(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        public static Point operator -(Point l, Vector r)
        {
            return new Point(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        public static Point operator *(Point l, float r)
        {
            return new Point(l.X * r, l.Y * r, l.Z * r);
        }

        public static Point operator *(float l, Point r)
        {
            return new Point(l * r.X, l * r.Y, l * r.Z);
        }

        public static Point operator /(Point l, float r)
        {
            return new Point(l.X / r, l.Y / r, l.Z / r);
        }

        #endregion
    }
}