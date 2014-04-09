using System;
using Aether.Geometry;
using Aether.MonteCarlo;

namespace Aether.Shapes
{
    public class Sphere : Shape
    {
        private readonly float _radius;
        private readonly float _zMin, _zMax;
        private readonly float _thetaMin, _thetaMax;
        private readonly float _phiMax;

        public Sphere(Transform objectToWorld, bool reverseOrientation,
            float radius, float z0, float z1, float phiMax)
            : base(objectToWorld, reverseOrientation)
        {
            _radius = radius;
            _zMin = MathUtility.Clamp(Math.Min(z0, z1), -radius, radius);
            _zMax = MathUtility.Clamp(Math.Max(z0, z1), -radius, radius);
            _thetaMin = MathUtility.Acos(MathUtility.Clamp(_zMin / radius, -1.0f, 1.0f));
            _thetaMax = MathUtility.Acos(MathUtility.Clamp(_zMax / radius, -1.0f, 1.0f));
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

        public override float Area
        {
            get { return _phiMax * _radius * (_zMax - _zMin); }
        }

        public override bool TryIntersect(Ray r, out float tHit, out float rayEpsilon, out DifferentialGeometry dg)
        {
            tHit = float.NegativeInfinity;
            rayEpsilon = 0.0f;
            dg = null;

            float phi;
            Point phit;
            float thit;
            if (!DoIntersection(r, out phi, out phit, out thit))
                return false;

            // Find parametric representation of sphere hit
            float u = phi / _phiMax;
            float theta = MathUtility.Acos(MathUtility.Clamp(phit.Z / _radius, -1.0f, 1.0f));
            float v = (theta - _thetaMin) / (_thetaMax - _thetaMin);

            // Compute sphere $\dpdu$ and $\dpdv$
            float zradius = MathUtility.Sqrt(phit.X * phit.X + phit.Y * phit.Y);
            float invzradius = 1.0f / zradius;
            float cosphi = phit.X * invzradius;
            float sinphi = phit.Y * invzradius;
            var dpdu = new Vector(-_phiMax * phit.Y, _phiMax * phit.X, 0);
            var dpdv = (_thetaMax - _thetaMin) *
                new Vector(phit.Z * cosphi, phit.Z * sinphi, -_radius * MathUtility.Sin(theta));

            // Compute sphere $\dndu$ and $\dndv$
            Vector d2Pduu = -_phiMax * _phiMax * new Vector(phit.X, phit.Y, 0);
            Vector d2Pduv = (_thetaMax - _thetaMin) * phit.Z * _phiMax * new Vector(-sinphi, cosphi, 0.0f);
            Vector d2Pdvv = -(_thetaMax - _thetaMin) * (_thetaMax - _thetaMin) * new Vector(phit.X, phit.Y, phit.Z);

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
            var dndu = (Normal) ((f * F - e * G) * invEGF2 * dpdu + (e * F - f * E) * invEGF2 * dpdv);
            var dndv = (Normal) ((g * F - f * G) * invEGF2 * dpdu + (f * F - g * E) * invEGF2 * dpdv);

            // Initialize _DifferentialGeometry_ from parametric information
            var o2w = ObjectToWorld;
            dg = new DifferentialGeometry(
                o2w.TransformPoint(ref phit),
                o2w.TransformVector(ref dpdu),
                o2w.TransformVector(ref dpdv),
                o2w.TransformNormal(ref dndu),
                o2w.TransformNormal(ref dndv),
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

        public override Point Sample(float u1, float u2, out Normal ns)
        {
            Point p = new Point(0, 0, 0) + _radius * MonteCarloUtilities.UniformSampleSphere(u1, u2);
            ns = Normal.Normalize(ObjectToWorld.TransformNormal(new Normal(p.X, p.Y, p.Z)));
            if (ReverseOrientation)
                ns *= -1.0f;
            return ObjectToWorld.TransformPoint(ref p);
        }

        public override Point Sample(Point p, float u1, float u2, out Normal ns)
        {
            // Compute coordinate system for sphere sampling
            Point Pcenter = ObjectToWorld.TransformPoint(Point.Zero);
            Vector wc = Vector.Normalize(Pcenter - p);
            Vector wcX, wcY;
            Vector.CoordinateSystem(wc, out wcX, out wcY);

            // Sample uniformly on sphere if $\pt{}$ is inside it
            if (Point.DistanceSquared(p, Pcenter) - _radius * _radius < 1e-4f)
                return Sample(u1, u2, out ns);

            // Sample sphere uniformly inside subtended cone
            float sinThetaMax2 = _radius * _radius / Point.DistanceSquared(p, Pcenter);
            float cosThetaMax = MathUtility.Sqrt(Math.Max(0.0f, 1.0f - sinThetaMax2));
            DifferentialGeometry dgSphere;
            float thit, rayEpsilon;
            Point ps;
            Ray r = new Ray(p, MonteCarloUtilities.UniformSampleCone(u1, u2, cosThetaMax, ref wcX, ref wcY, ref wc),
                1e-3f);
            if (!TryIntersect(r, out thit, out rayEpsilon, out dgSphere))
                thit = Vector.Dot(Pcenter - p, Vector.Normalize(r.Direction));
            ps = r.Evaluate(thit);
            ns = (Normal) Vector.Normalize(ps - Pcenter);
            if (ReverseOrientation)
                ns *= -1.0f;
            return ps;
        }

        public override float Pdf(Point p, Vector wi)
        {
            Point Pcenter = ObjectToWorld.TransformPoint(Point.Zero);
            // Return uniform weight if point inside sphere
            if (Point.DistanceSquared(p, Pcenter) - _radius * _radius < 1e-4f)
                return base.Pdf(p, wi);

            // Compute general sphere weight
            float sinThetaMax2 = _radius * _radius / Point.DistanceSquared(p, Pcenter);
            float cosThetaMax = MathUtility.Sqrt(Math.Max(0.0f, 1.0f - sinThetaMax2));
            return MonteCarloUtilities.UniformConePdf(cosThetaMax);
        }

        private bool DoIntersection(Ray r, out float phi, out Point phit, out float thit)
        {
            phi = 0.0f;
            phit = new Point();
            thit = 0.0f;

            // Transform _Ray_ to object space
            Ray ray = WorldToObject.TransformRay(r);

            // Compute quadratic sphere coefficients
            float A = ray.Direction.X * ray.Direction.X + ray.Direction.Y * ray.Direction.Y +
                ray.Direction.Z * ray.Direction.Z;
            float B = 2 *
                (ray.Direction.X * ray.Origin.X + ray.Direction.Y * ray.Origin.Y + ray.Direction.Z * ray.Origin.Z);
            float C = ray.Origin.X * ray.Origin.X + ray.Origin.Y * ray.Origin.Y +
                ray.Origin.Z * ray.Origin.Z - _radius * _radius;

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
                if (thit > ray.MaxT) return false;
            }

            // Compute sphere hit position and $\phi$
            phit = ray.Evaluate(thit);
            if (phit.X == 0.0f && phit.Y == 0.0f)
                phit.X = 1e-5f * _radius;
            phi = MathUtility.Atan2(phit.Y, phit.X);
            if (phi < 0.0f)
                phi += 2.0f * MathUtility.Pi;

            // Test sphere intersection against clipping parameters
            if ((_zMin > -_radius && phit.Z < _zMin) ||
                (_zMax < _radius && phit.Z > _zMax) || phi > _phiMax)
            {
                if (thit == t1) return false;
                if (t1 > ray.MaxT) return false;
                thit = t1;
                // Compute sphere hit position and $\phi$
                phit = ray.Evaluate(thit);
                if (phit.X == 0.0f && phit.Y == 0.0f)
                    phit.X = 1e-5f * _radius;
                phi = MathUtility.Atan2(phit.Y, phit.X);
                if (phi < 0.0f) phi += 2.0f * MathUtility.Pi;
                if ((_zMin > -_radius && phit.Z < _zMin) ||
                    (_zMax < _radius && phit.Z > _zMax) || phi > _phiMax)
                    return false;
            }

            return true;
        }
    }
}