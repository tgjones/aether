using Aether.Geometry;
using Aether.Shapes;

namespace Aether.Lights
{
    public class DiffuseAreaLight : AreaLight
    {
        public DiffuseAreaLight(Transform lightToWorld, Spectrum lemit, int numSamples, Shape shape) 
            : base(lightToWorld, numSamples)
        {
        }

        public override bool IsDeltaLight
        {
            get { return false; }
        }

        public override Spectrum SampleL(Point p, float pEpsilon, LightSample ls, float time, out Vector wi, out float pdf,
            out VisibilityTester vis)
        {
            throw new System.NotImplementedException();
        }

        public override Spectrum Power(Scene scene)
        {
            throw new System.NotImplementedException();
        }

        public override float Pdf(Point p, Vector wi)
        {
            throw new System.NotImplementedException();
        }

        public override Spectrum SampleL(Scene scene, LightSample ls, float u1, float u2, float time, out Ray ray, out Normal ns, out float pdf)
        {
            throw new System.NotImplementedException();
        }

        public override Spectrum L(Point p, Normal n, Vector w)
        {
            throw new System.NotImplementedException();
        }
    }
}