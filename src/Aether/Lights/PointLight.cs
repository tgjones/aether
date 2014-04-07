using Aether.Geometry;
using Aether.MonteCarlo;

namespace Aether.Lights
{
    public class PointLight : Light
    {
        private readonly Spectrum _intensity;
        private readonly Point _lightPosition;

        public PointLight(Transform lightToWorld, Spectrum intensity)
            : base(lightToWorld)
        {
            _intensity = intensity;
            _lightPosition = lightToWorld.TransformPoint(new Point(0, 0, 0));
        }

        public override bool IsDeltaLight
        {
            get { return true; }
        }

        public override Spectrum SampleL(
            Point p, float pEpsilon, LightSample ls, float time, out Vector wi,
            out float pdf, out VisibilityTester vis)
        {
            wi = Vector.Normalize(_lightPosition - p);
            pdf = 1.0f;
            vis = new VisibilityTester(p, pEpsilon, _lightPosition, 0.0f, time);
            return _intensity / Point.DistanceSquared(_lightPosition, p);
        }

        public override Spectrum Power(Scene scene)
        {
            return 4.0f * MathUtility.Pi * _intensity;
        }

        public override float Pdf(Point p, Vector wi)
        {
            return 0.0f;
        }

        public override Spectrum SampleL(Scene scene, LightSample ls, float u1, float u2, float time, out Ray ray, out Normal ns, out float pdf)
        {
            ray = new Ray(_lightPosition, 
                MonteCarloUtilities.UniformSampleSphere(ls.UPos0, ls.UPos1), 
                0.0f, float.PositiveInfinity, time);
            ns = (Normal) ray.Direction;
            pdf = MonteCarloUtilities.UniformSpherePdf();
            return _intensity;
        }
    }
}