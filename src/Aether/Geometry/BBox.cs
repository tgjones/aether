using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Aether.Geometry
{
    public struct BBox
    {
        public Point Min;
        public Point Max;

        public BBox(Point min, Point max)
        {
            Min = min;
            Max = max;
        }

        /// Empty bounding box.
        public static readonly BBox Empty = new BBox(
            new Point(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
            new Point(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity));

        /// Creates a new bounding box large enough to contain both the original
        /// bounding box and the given point.
        public static BBox Union(BBox b, Point p)
        {
            var min = new Point(Math.Min(b.Min.X, p.X), Math.Min(b.Min.Y, p.Y), Math.Min(b.Min.Z, p.Z));
            var max = new Point(Math.Max(b.Max.X, p.X), Math.Max(b.Max.Y, p.Y), Math.Max(b.Max.Z, p.Z));
            return new BBox(min, max);
        }

        /// Creates a new bounding box large enough to contain the given two
        /// bounding boxes.
        public static BBox Union(BBox b1, BBox b2)
        {
            var min = new Point(Math.Min(b1.Min.X, b2.Min.X), Math.Min(b1.Min.Y, b2.Min.Y), Math.Min(b1.Min.Z, b2.Min.Z));
            var max = new Point(Math.Max(b1.Max.X, b2.Max.X), Math.Max(b1.Max.Y, b2.Max.Y), Math.Max(b1.Max.Z, b2.Max.Z));
            return new BBox(min, max);
        }

        public static BBox Union(IEnumerable<BBox> boxes)
        {
            var result = Empty;
            foreach (var box in boxes)
                result = Union(result, box);
            return result;
        }

        /// Creates a bounding box from two points.
        public static BBox FromPoints(Point p1, Point p2)
        {
            return new BBox(
                new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Min(p1.Z, p2.Z)),
                new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), Math.Max(p1.Z, p2.Z)));
        }

        /// Creates a bounding box from one point.
        public static BBox FromPoint(Point p)
        {
            return FromPoints(p, p);
        }

        /// Creates a bounding box from the given point list.
        public static BBox FromPoints(IEnumerable<Point> points)
        {
            var result = Empty;
            foreach (var point in points)
                result = Union(result, point);
            return result;
        }

        /// Returns true if any part of the two bounding boxes overlap.
        public bool Overlaps(BBox b2)
        {
            var x = Max.X >= b2.Min.X && Min.X <= b2.Max.X;
            var y = Max.Y >= b2.Min.Y && Min.Y <= b2.Max.Y;
            var z = Max.Z >= b2.Min.Z && Min.Z <= b2.Max.Z;
            return x && y && z;
        }

        /// Returns true if the point is inside the bounding box.
        [Pure]
        public bool Inside(Point p)
        {
            return p.X >= Min.X && p.X <= Max.X
                && p.Y >= Min.Y && p.Y <= Max.Y
                && p.Z >= Min.Z && p.Z <= Max.Z;
        }

        /// Creates a new bounding box padded by the given amount.
        public static BBox Expand(BBox b, float delta)
        {
            return new BBox(
                b.Min - new Vector(delta, delta, delta),
                b.Max + new Vector(delta, delta, delta));
        }

        /// Returns the diagonal distance between the min and max.
        private Vector Diag()
        {
            return Max - Min;
        }

        /// Returns the surface area of the bounding box faces.
        public float SurfaceArea()
        {
            var d = Diag();
            return 2.0f * (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
        }

        /// Returns the volume inside the bounding box.
        public float Volume()
        {
            var d = Diag();
            return d.X * d.Y * d.Z;
        }

        /// Returns the largest axis (X=0, Y=1, Z=2) of the bounding box.
        public int MaximumExtent()
        {
            var d = Diag();
            if (d.X > d.Y && d.X > d.Z)
                return 0;
            if (d.Y > d.Z)
                return 1;
            return 2;
        }

        /// Returns the center and radius of a sphere that bounds the given bounding box.
        public void BoundingSphere(out Point center, out float radius)
        {
            center = 0.5f * Min + 0.5f * Max;
            radius = Inside(center) ? Point.Distance(center, this.Max) : 0.0f;
        }

        /// Tests for intersection of the given ray with the given bounding box.
        [Pure]
        public bool TryIntersect(Ray ray, out float t0, out float t1)
        {
            t0 = ray.MinT;
            t1 = ray.MaxT;

            for (int i = 0; i < 3; ++i)
            {
                // Update interval for _i_th bounding box slab
                float invRayDir = 1.0f / ray.Direction[i];
                float tNear = (Min[i] - ray.Origin[i]) * invRayDir;
                float tFar = (Max[i] - ray.Origin[i]) * invRayDir;

                // Update parametric interval from slab intersection $t$s
                if (tNear > tFar) MathUtility.Swap(ref tNear, ref tFar);
                t0 = tNear > t0 ? tNear : t0;
                t1 = tFar < t1 ? tFar : t1;
                if (t0 > t1)
                    return false;
            }
            return true;
        }

        [Pure]
        public bool TryIntersect(Ray ray, out float t0)
        {
            float t1;
            return TryIntersect(ray, out t0, out t1);
        }

        [Pure]
        public bool Intersects(Ray ray)
        {
            float t0, t1;
            return TryIntersect(ray, out t0, out t1);
        }

        public override string ToString()
        {
            return string.Format("{{Min:{0} Max:{1}}}", Min, Max);
        }
    }
}