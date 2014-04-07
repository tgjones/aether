using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aether.Geometry;
using Aether.MonteCarlo;
using Aether.Shapes;

namespace Aether.Reflection
{
    public class Bsdf
    {
        private readonly DifferentialGeometry _dgShading;
        private readonly Normal _ng;
        private readonly float _eta;
        private readonly Normal _nn;
        private readonly Vector _sn;
        private readonly Vector _tn;
        private readonly List<Bxdf> _bxdfs;

        public Bsdf(DifferentialGeometry dgShading, Normal nGeom, float eta = 1.0f)
        {
            _dgShading = dgShading;
            _ng = nGeom;
            _eta = eta;
            _nn = dgShading.Normal;
            _sn = Vector.Normalize(dgShading.DpDu);
            _tn = Normal.Cross(_nn, _sn);
            _bxdfs = new List<Bxdf>();
        }

        public DifferentialGeometry DgShading
        {
            get { return _dgShading; }
        }

        public float Eta
        {
            get { return _eta; }
        }

        public Spectrum SampleF(Vector woW, out Vector wiW, BsdfSample bsdfSample,
            out float pdf, BxdfType flags = BxdfType.All)
        {
            BxdfType sampledType;
            return SampleF(woW, out wiW, bsdfSample, out pdf, out sampledType, flags);
        }

        public Spectrum SampleF(Vector woW, out Vector wiW, BsdfSample bsdfSample,
            out float pdf, out BxdfType sampledType, BxdfType flags = BxdfType.All)
        {
            // Choose which _BxDF_ to sample
            int matchingComps = NumComponents(flags);
            if (matchingComps == 0)
            {
                wiW = Vector.Zero;
                pdf = 0.0f;
                sampledType = 0;
                return Spectrum.CreateBlack();
            }
            int which = Math.Min(
                MathUtility.Floor(bsdfSample.UComponent * matchingComps),
                matchingComps - 1);
            Bxdf bxdf = null;
            int count = which;
            foreach (var eachBxdf in _bxdfs)
                if (eachBxdf.MatchesFlags(flags) && count-- == 0)
                {
                    bxdf = eachBxdf;
                    break;
                }
            Debug.Assert(bxdf != null);

            // Sample chosen _BxDF_
            Vector wo = WorldToLocal(woW);
            Vector wi;
            pdf = 0.0f;
            Spectrum f = bxdf.SampleF(wo, out wi, bsdfSample.UDir0, bsdfSample.UDir1, out pdf);
            if (pdf == 0.0f)
            {
                wiW = Vector.Zero;
                sampledType = 0;
                return Spectrum.CreateBlack();
            }
            sampledType = bxdf.Type;
            wiW = LocalToWorld(wi);

            // Compute overall PDF with all matching _BxDF_s
            if (!bxdf.Type.HasFlag(BxdfType.Specular) && matchingComps > 1)
                foreach (var eachBxdf in _bxdfs)
                    if (eachBxdf != bxdf && eachBxdf.MatchesFlags(flags))
                        pdf += bxdf.Pdf(wo, wi);
            if (matchingComps > 1)
                pdf /= matchingComps;

            // Compute value of BSDF for sampled direction
            if (!bxdf.Type.HasFlag(BxdfType.Specular))
            {
                f = Spectrum.CreateBlack();
                if (Vector.Dot(wiW, _ng) * Vector.Dot(woW, _ng) > 0) // ignore BTDFs
                    flags &= ~BxdfType.Transmission;
                else // ignore BRDFs
                    flags &= ~BxdfType.Reflection;
                foreach (var eachBxdf in _bxdfs)
                    if (eachBxdf.MatchesFlags(flags))
                        f += eachBxdf.F(wo, wi);
            }
            return f;
        }

        public float Pdf(Vector woW, Vector wiW, BxdfType flags = BxdfType.All)
        {
            if (_bxdfs.Count == 0)
                return 0.0f;
            Vector wo = WorldToLocal(woW), wi = WorldToLocal(wiW);
            float pdf = 0.0f;
            int matchingComps = 0;
            foreach (var bxdf in _bxdfs)
                if (bxdf.MatchesFlags(flags))
                {
                    ++matchingComps;
                    pdf += bxdf.Pdf(wo, wi);
                }
            float v = matchingComps > 0 ? pdf / matchingComps : 0.0f;
            return v;
        }

        public void Add(Bxdf bxdf)
        {
            _bxdfs.Add(bxdf);
        }

        public int NumComponents()
        {
            return _bxdfs.Count;
        }

        public int NumComponents(BxdfType flags)
        {
            return _bxdfs.Count(x => x.MatchesFlags(flags));
        }

        public Vector WorldToLocal(Vector v)
        {
            return new Vector(
                Vector.Dot(v, _sn), 
                Vector.Dot(v, _tn), 
                Vector.Dot(v, _nn));
        }

        public Vector LocalToWorld(Vector v)
        {
            return new Vector(
                _sn.X * v.X + _tn.X * v.Y + _nn.X * v.Z,
                _sn.Y * v.X + _tn.Y * v.Y + _nn.Y * v.Z,
                _sn.Z * v.X + _tn.Z * v.Y + _nn.Z * v.Z);
        }

        public Spectrum F(Vector woW, Vector wiW, BxdfType flags = BxdfType.All)
        {
            Vector wi = WorldToLocal(wiW), wo = WorldToLocal(woW);
            if (Vector.Dot(wiW, _ng) * Vector.Dot(woW, _ng) > 0) // ignore BTDFs
                flags &= ~BxdfType.Transmission;
            else // ignore BRDFs
                flags &= ~BxdfType.Reflection;
            Spectrum f = Spectrum.CreateBlack();
            foreach (var bxdf in _bxdfs)
                if (bxdf.MatchesFlags(flags))
                    f += bxdf.F(wo, wi);
            return f;
        }

        public Spectrum Rho(Random rng, BxdfType flags = BxdfType.All, int sqrtSamples = 6)
        {
            int nSamples = sqrtSamples * sqrtSamples;
            var s1 = new float[2 * nSamples];
            SamplingUtilities.StratifiedSample2D(s1, sqrtSamples, sqrtSamples, rng);
            var s2 = new float[2 * nSamples];
            SamplingUtilities.StratifiedSample2D(s2, sqrtSamples, sqrtSamples, rng);

            Spectrum ret = Spectrum.CreateBlack();
            foreach (var bxdf in _bxdfs)
                if (bxdf.MatchesFlags(flags))
                    ret += bxdf.Rho(nSamples, s1, s2);
            return ret;
        }

        public Spectrum Rho(Vector wo, Random rng, BxdfType flags = BxdfType.All, int sqrtSamples = 6)
        {
            int nSamples = sqrtSamples * sqrtSamples;
            var s1 = new float[2 * nSamples];
            SamplingUtilities.StratifiedSample2D(s1, sqrtSamples, sqrtSamples, rng);
            Spectrum ret = Spectrum.CreateBlack();
            foreach (var bxdf in _bxdfs)
                if (bxdf.MatchesFlags(flags))
                    ret += bxdf.Rho(wo, nSamples, s1);
            return ret;
        }
    }
}