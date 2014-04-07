using System;
using Aether.Geometry;

namespace Aether.Reflection
{
    public class OrenNayar : Bxdf
    {
        private readonly Spectrum _reflectance;
        private readonly float _a, _b;

        public OrenNayar(Spectrum reflectance, float sig)
            : base(BxdfType.Reflection | BxdfType.Diffuse)
        {
            _reflectance = reflectance;

            float sigma = MathUtility.ToRadians(sig);
            float sigma2 = sigma * sigma;
            _a = 1.0f - (sigma2 / (2.0f * (sigma2 + 0.33f)));
            _b = 0.45f * sigma2 / (sigma2 + 0.09f);
        }

        public override Spectrum F(Vector wo, Vector wi)
        {
            float sinthetai = ReflectionUtilities.SinTheta(ref wi);
            float sinthetao = ReflectionUtilities.SinTheta(ref wo);
            // Compute cosine term of Oren-Nayar model
            float maxcos = 0.0f;
            if (sinthetai > 1e-4 && sinthetao > 1e-4)
            {
                float sinphii = ReflectionUtilities.SinPhi(ref wi);
                float cosphii = ReflectionUtilities.CosPhi(ref wi);
                float sinphio = ReflectionUtilities.SinPhi(ref wo);
                float cosphio = ReflectionUtilities.CosPhi(ref wo);
                float dcos = cosphii * cosphio + sinphii * sinphio;
                maxcos = Math.Max(0.0f, dcos);
            }

            // Compute sine and tangent terms of Oren-Nayar model
            float sinalpha, tanbeta;
            if (ReflectionUtilities.AbsCosTheta(ref wi) > ReflectionUtilities.AbsCosTheta(ref wo))
            {
                sinalpha = sinthetao;
                tanbeta = sinthetai / ReflectionUtilities.AbsCosTheta(ref wi);
            }
            else
            {
                sinalpha = sinthetai;
                tanbeta = sinthetao / ReflectionUtilities.AbsCosTheta(ref wo);
            }
            return _reflectance * MathUtility.InvPi * (_a + _b * maxcos * sinalpha * tanbeta);
        }
    }
}