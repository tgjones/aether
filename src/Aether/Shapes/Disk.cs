using System;
using Aether.Geometry;
using Aether.MonteCarlo;

namespace Aether.Shapes
{
    public class Disk : Shape
    {
        private readonly float _height;
        private readonly float _radius;
        private readonly float _innerRadius;
        private readonly float _phiMax;

        public Disk(Transform objectToWorld, bool reverseOrientation,
            float height, float radius, float innerRadius, float phiMax)
            : base(objectToWorld, reverseOrientation)
        {
            _height = height;
            _radius = radius;
            _innerRadius = innerRadius;
            _phiMax = MathUtility.ToRadians(MathUtility.Clamp(phiMax, 0.0f, 360.0f));
        }

        public override BBox ObjectBound
        {
            get
            {
                return new BBox(
                    new Point(-_radius, -_radius, _height),
                    new Point(_radius, _radius, _height));
            }
        }

        public override float Area
        {
            get { return _phiMax * 0.5f * (_radius * _radius - _innerRadius * _innerRadius); }
        }

        public override bool TryIntersect(Ray r, out float tHit,
            out float rayEpsilon, out DifferentialGeometry dg)
        {
            tHit = float.NegativeInfinity;
            rayEpsilon = 0.0f;
            dg = null;

            float phi, dist2, thit;
            Point phit;
            if (!DoIntersection(r, out phi, out dist2, out phit, out thit))
                return false;

            // Find parametric representation of disk hit
            float u = phi / _phiMax;
            float oneMinusV = ((MathUtility.Sqrt(dist2) - _innerRadius) / (_radius - _innerRadius));
            float invOneMinusV = (oneMinusV > 0.0f) ? (1.0f / oneMinusV) : 0.0f;
            float v = 1.0f - oneMinusV;
            var dpdu = new Vector(-_phiMax * phit.Y, _phiMax * phit.X, 0.0f);
            var dpdv = new Vector(-phit.X * invOneMinusV, -phit.Y * invOneMinusV, 0.0f);
            dpdu *= _phiMax * MathUtility.InvTwoPi;
            dpdv *= (_radius - _innerRadius) / _radius;
            Normal dndu = Normal.Zero, dndv = Normal.Zero;

            // Initialize _DifferentialGeometry_ from parametric information
            var o2w = ObjectToWorld;
            dg = new DifferentialGeometry(o2w.TransformPoint(ref phit),
                o2w.TransformVector(ref dpdu), o2w.TransformVector(ref dpdv),
                o2w.TransformNormal(ref dndu), o2w.TransformNormal(ref dndv),
                u, v, this);

            // Update _tHit_ for quadric intersection
            tHit = thit;

            // Compute _rayEpsilon_ for quadric intersection
            rayEpsilon = 5e-4f * tHit;
            return true;
        }

        public override bool Intersects(Ray ray)
        {
            float phi, dist2, thit;
            Point phit;
            return DoIntersection(ray, out phi, out dist2, out phit, out thit);
        }

        private bool DoIntersection(Ray r, out float phi, out float dist2, out Point phit, out float thit)
        {
            phi = dist2 = thit = 0;
            phit = Point.Zero;

            // Transform _Ray_ to object space
            Ray ray = WorldToObject.TransformRay(r);

            // Compute plane intersection for disk
            if (Math.Abs(ray.Direction.Z) < 1e-7)
                return false;
            thit = (_height - ray.Origin.Z) / ray.Direction.Z;
            if (thit < ray.MinT || thit > ray.MaxT)
                return false;

            // See if hit point is inside disk radii and $\phimax$
            phit = ray.Evaluate(thit);
            dist2 = phit.X * phit.X + phit.Y * phit.Y;
            if (dist2 > _radius * _radius || dist2 < _innerRadius * _innerRadius)
                return false;

            // Test disk $\phi$ value against $\phimax$
            phi = MathUtility.Atan2(phit.Y, phit.X);
            if (phi < 0)
                phi += 2.0f * MathUtility.Pi;
            if (phi > _phiMax)
                return false;

            return true;
        }

        public override Point Sample(float u1, float u2, out Normal ns)
        {
            var p = new Point();
            MonteCarloUtilities.ConcentricSampleDisk(u1, u2, out p.X, out p.Y);
            p.X *= _radius;
            p.Y *= _radius;
            p.Z = _height;
            ns = Normal.Normalize(ObjectToWorld.TransformNormal(new Normal(0, 0, 1)));
            if (ReverseOrientation)
                ns *= -1.0f;
            return ObjectToWorld.TransformPoint(ref p);
        }
    }
}