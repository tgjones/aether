using Aether.Geometry;
using Aether.MonteCarlo;

namespace Aether.Reflection
{
    public abstract class Bxdf
    {
        public BxdfType Type { get; private set; }

        protected Bxdf(BxdfType type)
        {
            Type = type;
        }

        public bool MatchesFlags(BxdfType flags)
        {
            return (Type & flags) == Type;
        }

        public abstract Spectrum F(Vector wo, Vector wi);

        public virtual Spectrum SampleF(Vector wo, out Vector wi, float u1, float u2, out float pdf)
        {
            // Cosine-sample the hemisphere, flipping the direction if necessary
            wi = MonteCarloUtilities.CosineSampleHemisphere(u1, u2);
            if (wo.Z < 0.0f) wi.Z *= -1.0f;
            pdf = Pdf(wo, wi);
            return F(wo, wi);
        }

        public virtual Spectrum Rho(Vector wo, int numSamples, float[] samples)
        {
            Spectrum r = Spectrum.CreateBlack();
            for (int i = 0; i < numSamples; ++i)
            {
                // Estimate one term of $\rho_\roman{hd}$
                Vector wi;
                float pdf;
                Spectrum f = SampleF(wo, out wi, samples[2 * i], samples[2 * i + 1], out pdf);
                if (pdf > 0.0f) r += f * ReflectionUtilities.AbsCosTheta(ref wi) / pdf;
            }
            return r / numSamples;
        }

        public virtual Spectrum Rho(int numSamples, float[] samples1, float[] samples2)
        {
            Spectrum r = Spectrum.CreateBlack();
            for (int i = 0; i < numSamples; ++i)
            {
                // Estimate one term of $\rho_\roman{hh}$
                Vector wo, wi;
                wo = MonteCarloUtilities.UniformSampleHemisphere(samples1[2 * i], samples1[2 * i + 1]);
                float pdf_o = MathUtility.InvTwoPi, pdf_i;
                Spectrum f = SampleF(wo, out wi, samples2[2 * i], samples2[2 * i + 1], out pdf_i);
                if (pdf_i > 0.0f)
                    r += f * ReflectionUtilities.AbsCosTheta(ref wi) * ReflectionUtilities.AbsCosTheta(ref wo) / (pdf_o * pdf_i);
            }
            return r / (MathUtility.Pi * numSamples);
        }

        public virtual float Pdf(Vector wi, Vector wo)
        {
            return ReflectionUtilities.SameHemisphere(ref wo, ref wi)
                ? ReflectionUtilities.AbsCosTheta(ref wi) * MathUtility.InvPi
                : 0.0f;
        }
    }
}