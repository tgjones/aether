using Aether.Geometry;
using Aether.MonteCarlo;

namespace Aether.Lights
{
    public class DistantLight : Light
    {
        private readonly Spectrum _radiance;
        private readonly Vector _direction;

        public DistantLight(Transform lightToWorld, Spectrum radiance, Vector direction)
            : base(lightToWorld)
        {
            _radiance = radiance;
            _direction = Vector.Normalize(lightToWorld.TransformVector(ref direction));
        }

        public override bool IsDeltaLight
        {
            get { return true; }
        }

        public override Spectrum SampleL(Point p, float pEpsilon, LightSample ls,
            float time, out Vector wi, out float pdf,
            out VisibilityTester vis)
        {
            wi = _direction;
            pdf = 1.0f;
            vis = new VisibilityTester(p, pEpsilon, wi, time);
            return _radiance;
        }

        public override Spectrum Power(Scene scene)
        {
            Point worldCenter;
            float worldRadius;
            scene.WorldBound.BoundingSphere(out worldCenter, out worldRadius);
            return _radiance * MathUtility.Pi * worldRadius * worldRadius;
        }

        public override float Pdf(Point p, Vector wi)
        {
            return 0.0f;
        }

        public override Spectrum SampleL(Scene scene, LightSample ls, float u1, float u2, float time, out Ray ray, out Normal ns, out float pdf)
        {
            // Choose point on disk oriented toward infinite light direction
            Point worldCenter;
            float worldRadius;
            scene.WorldBound.BoundingSphere(out worldCenter, out worldRadius);
            Vector v1, v2;
            Vector.CoordinateSystem(_direction, out v1, out v2);
            float d1, d2;
            MonteCarloUtilities.ConcentricSampleDisk(ls.UPos0, ls.UPos1, out d1, out d2);
            Point Pdisk = worldCenter + worldRadius * (d1 * v1 + d2 * v2);

            // Set ray origin and direction for infinite light ray
            ray = new Ray(Pdisk + worldRadius * _direction, -_direction, 0.0f, float.PositiveInfinity, time);
            ns = (Normal) ray.Direction;

            pdf = 1.0f / (MathUtility.Pi * worldRadius * worldRadius);
            return _radiance;
        }
    }
}