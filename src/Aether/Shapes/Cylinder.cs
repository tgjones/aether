using System;
using Aether.Geometry;

namespace Aether.Shapes
{
    public class Cylinder : Shape
    {
        private readonly float _radius;
        private readonly float _zMin;
        private readonly float _zMax;
        private readonly float _phiMax;

        public Cylinder(Transform objectToWorld, bool reverseOrientation,
            float radius, float zMin, float zMax, float phiMax)
            : base(objectToWorld, reverseOrientation)
        {
            _radius = radius;
            _zMin = Math.Min(zMin, zMax);
            _zMax = Math.Max(zMin, zMax);
            _phiMax = MathUtility.ToRadians(MathUtility.Clamp(phiMax, 0.0f, 360.0f));
        }

        public override BBox ObjectBound
        {
            get
            {
                return BBox.FromPoints(
                    new Point(-_radius, -_radius, _zMin),
                    new Point(_radius, _radius, _zMax));
            }
        }

        public override bool TryIntersect(Ray r, out float tHit,
            out float rayEpsilon, out DifferentialGeometry dg)
        {
            tHit = float.NegativeInfinity;
            rayEpsilon = 0.0f;
            dg = null;

            float phi;
            Point phit;
            float thit;
            if (!DoIntersection(r, out phi, out phit, out thit))
                return false;

            // Find parametric representation of cylinder hit
            float u = phi / _phiMax;
            float v = (phit.Z - _zMin) / (_zMax - _zMin);

            // Compute cylinder $\dpdu$ and $\dpdv$
            var dpdu = new Vector(-_phiMax * phit.Y, _phiMax * phit.X, 0);
            var dpdv = new Vector(0, 0, _zMax - _zMin);

            // Compute cylinder $\dndu$ and $\dndv$
            Vector d2Pduu = -_phiMax * _phiMax * new Vector(phit.X, phit.Y, 0);
            Vector d2Pduv = Vector.Zero, d2Pdvv = Vector.Zero;

            // Compute coefficients for fundamental forms
            float E = Vector.Dot(dpdu, dpdu);
            float F = Vector.Dot(dpdu, dpdv);
            float G = Vector.Dot(dpdv, dpdv);
            Vector N = Vector.Normalize(Vector.Cross(dpdu, dpdv));
            float e = Vector.Dot(N, d2Pduu);
            float f = Vector.Dot(N, d2Pduv);
            float g = Vector.Dot(N, d2Pdvv);

            // Compute $\dndu$ and $\dndv$ from fundamental form coefficients
            float invEGF2 = 1.0f / (E * G - F * F);
            var dndu = (Normal) ((f * F - e * G) * invEGF2 * dpdu +
                (e * F - f * E) * invEGF2 * dpdv);
            var dndv = (Normal) ((g * F - f * G) * invEGF2 * dpdu +
                (f * F - g * E) * invEGF2 * dpdv);

            // Initialize _DifferentialGeometry_ from parametric information
            var o2w = ObjectToWorld;
            dg = new DifferentialGeometry(o2w.TransformPoint(phit),
                o2w.TransformVector(dpdu), o2w.TransformVector(dpdv),
                o2w.TransformNormal(dndu), o2w.TransformNormal(dndv),
                u, v, this);

            // Update _tHit_ for quadric intersection
            tHit = thit;

            // Compute _rayEpsilon_ for quadric intersection
            rayEpsilon = 5e-4f * tHit;
            return true;
        }

        public override bool Intersects(Ray ray)
        {
            float phi;
            Point phit;
            float thit;
            return DoIntersection(ray, out phi, out phit, out thit);
        }

        private bool DoIntersection(Ray r, out float phi, out Point phit, out float thit)
        {
            phi = 0.0f;
            phit = new Point();
            thit = 0.0f;

            // Transform _Ray_ to object space
            Ray ray = WorldToObject.TransformRay(r);

            // Compute quadratic cylinder coefficients
            float A = ray.Direction.X * ray.Direction.X + ray.Direction.Y * ray.Direction.Y;
            float B = 2 * (ray.Direction.X * ray.Origin.X + ray.Direction.Y * ray.Origin.Y);
            float C = ray.Origin.X * ray.Origin.X + ray.Origin.Y * ray.Origin.Y - _radius * _radius;

            // Solve quadratic equation for _t_ values
            float t0, t1;
            if (!MathUtility.TryQuadratic(A, B, C, out t0, out t1))
                return false;

            // Compute intersection distance along ray
            if (t0 > ray.MaxT || t1 < ray.MinT)
                return false;
            thit = t0;
            if (t0 < ray.MinT)
            {
                thit = t1;
                if (thit > ray.MaxT)
                    return false;
            }

            // Compute cylinder hit point and $\phi$
            phit = ray.Evaluate(thit);
            phi = MathUtility.Atan2(phit.Y, phit.X);
            if (phi < 0.0)
                phi += 2.0f * MathUtility.Pi;

            // Test cylinder intersection against clipping parameters
            if (phit.Z < _zMin || phit.Z > _zMax || phi > _phiMax)
            {
                if (thit == t1) return false;
                thit = t1;
                if (t1 > ray.MaxT) return false;
                // Compute cylinder hit point and $\phi$
                phit = ray.Evaluate(thit);
                phi = MathUtility.Atan2(phit.Y, phit.X);
                if (phi < 0.0f) phi += 2.0f * MathUtility.Pi;
                if (phit.Z < _zMin || phit.Z > _zMax || phi > _phiMax)
                    return false;
            }

            return true;
        }

        public override float Area
        {
            get { return _phiMax * _radius * (_zMax - _zMin); }
        }

        public override Point Sample(float u1, float u2, out Normal ns)
        {
            var z = MathUtility.Lerp(u1, _zMin, _zMax);
            var t = u2 * _phiMax;
            var p = new Point(_radius * MathUtility.Cos(t), _radius * MathUtility.Sin(t), z);
            ns = Normal.Normalize(ObjectToWorld.TransformNormal(new Normal(p.X, p.Y, 0)));
            if (ReverseOrientation)
                ns *= -1.0f;
            return ObjectToWorld.TransformPoint(p);
        }
    }
}