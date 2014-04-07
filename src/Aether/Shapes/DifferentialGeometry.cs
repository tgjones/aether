using System;
using Aether.Geometry;

namespace Aether.Shapes
{
    public class DifferentialGeometry
    {
        public DifferentialGeometry(Point point, Vector dpdu, Vector dpdv,
            Normal dndu, Normal dndv, float u, float v, Shape shape)
        {
            Point = point;
            Normal = (Normal) Vector.Normalize(Vector.Cross(dpdu, dpdv));
            DpDu = dpdu;
            DpDv = dpdv;
            DnDu = dndu;
            DnDv = dndv;
            U = u;
            V = v;
            Shape = shape;

            if (shape != null && shape.ReverseOrientation != shape.TransformSwapsHandedness)
                Normal *= -1.0f;
        }

        private DifferentialGeometry()
        {
            
        }

        public Point Point;
        public Normal Normal;
        public float U;
        public float V;
        public Shape Shape { get; private set; }
        public Vector DpDu;
        public Vector DpDv;
        public Normal DnDu;
        public Normal DnDv;
        public Vector DpDx;
        public Vector DpDy;
        public float DuDx;
        public float DvDx;
        public float DuDy;
        public float DvDy;

        public DifferentialGeometry Clone()
        {
            return new DifferentialGeometry
            {
                Point = Point,
                Normal = Normal,
                U = U,
                V = V,
                Shape = Shape,
                DpDu = DpDu,
                DpDv = DpDv,
                DnDu = DnDu,
                DnDv = DnDv,
                DpDx = DpDx,
                DpDy = DpDy,
                DuDx = DuDx,
                DvDx = DvDx,
                DuDy = DuDy,
                DvDy = DvDy
            };
        }

        public void ComputeDifferentials(RayDifferential ray)
        {
            if (ray.HasDifferentials)
            {
                // Estimate screen space change in $\pt{}$ and $(u,v)$

                // Compute auxiliary intersection points with plane
                float d = -Normal.Dot(Normal, new Vector(Point.X, Point.Y, Point.Z));
                var rxv = new Vector(ray.RxOrigin.X, ray.RxOrigin.Y, ray.RxOrigin.Z);
                float tx = -(Normal.Dot(Normal, rxv) + d) / Normal.Dot(Normal, ray.RxDirection);
                if (float.IsNaN(tx))
                    throw new InvalidOperationException();
                Point px = ray.RxOrigin + tx * ray.RxDirection;
                var ryv = new Vector(ray.RyOrigin.X, ray.RyOrigin.Y, ray.RyOrigin.Z);
                float ty = -(Normal.Dot(Normal, ryv) + d) / Normal.Dot(Normal, ray.RyDirection);
                if (float.IsNaN(ty))
                    throw new InvalidOperationException();
                Point py = ray.RyOrigin + ty * ray.RyDirection;
                DpDx = px - Point;
                DpDy = py - Point;

                // Compute $(u,v)$ offsets at auxiliary points

                // Initialize _A_, _Bx_, and _By_ matrices for offset computation
                var A = new float[2, 2];
                var Bx = new float[2];
                var By = new float[2];
                var axes = new int[2];
                if (Math.Abs(Normal.X) > Math.Abs(Normal.Y) && Math.Abs(Normal.X) > Math.Abs(Normal.Z))
                {
                    axes[0] = 1;
                    axes[1] = 2;
                }
                else if (Math.Abs(Normal.Y) > Math.Abs(Normal.Z))
                {
                    axes[0] = 0;
                    axes[1] = 2;
                }
                else
                {
                    axes[0] = 0;
                    axes[1] = 1;
                }

                // Initialize matrices for chosen projection plane
                A[0, 0] = DpDu[axes[0]];
                A[0, 1] = DpDv[axes[0]];
                A[1, 0] = DpDu[axes[1]];
                A[1, 1] = DpDv[axes[1]];
                Bx[0] = px[axes[0]] - Point[axes[0]];
                Bx[1] = px[axes[1]] - Point[axes[1]];
                By[0] = py[axes[0]] - Point[axes[0]];
                By[1] = py[axes[1]] - Point[axes[1]];
                if (!Matrix4x4.SolveLinearSystem2x2(A, Bx, out DuDx, out DvDx))
                {
                    DuDx = 0.0f;
                    DvDx = 0.0f;
                }
                if (!Matrix4x4.SolveLinearSystem2x2(A, By, out DuDy, out DvDy))
                {
                    DuDy = 0.0f;
                    DvDy = 0.0f;
                }
            }
            else
            {
                DuDx = DvDx = 0.0f;
                DuDy = DvDy = 0.0f;
                DpDx = DpDy = Vector.Zero;
            }
        }
    }
}