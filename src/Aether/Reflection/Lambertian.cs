using Aether.Geometry;

namespace Aether.Reflection
{
    public class Lambertian : Bxdf
    {
        private readonly Spectrum _reflectance;

        public Lambertian(Spectrum reflectance)
            : base(BxdfType.Reflection | BxdfType.Diffuse)
        {
            _reflectance = reflectance;
        }

        public override Spectrum F(Vector wo, Vector wi)
        {
            return _reflectance * MathUtility.InvPi;
        }

        public override Spectrum Rho(Vector wo, int numSamples, float[] samples)
        {
            return _reflectance;
        }

        public override Spectrum Rho(int numSamples, float[] samples1, float[] samples2)
        {
            return _reflectance;
        }
    }
}