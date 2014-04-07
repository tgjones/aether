using Aether.Reflection;
using Aether.Shapes;
using Aether.Textures;

namespace Aether.Materials
{
    public class MatteMaterial : Material
    {
        private readonly Texture<Spectrum> _kd;
        private readonly Texture<float> _sigma;
        private readonly Texture<float> _bumpMap;

        public MatteMaterial(Texture<Spectrum> kd, Texture<float> sigma, Texture<float> bumpMap)
        {
            _kd = kd;
            _sigma = sigma;
            _bumpMap = bumpMap;
        }

        public override Bsdf GetBsdf(DifferentialGeometry dgGeom, DifferentialGeometry dgShading)
        {
            // Allocate _BSDF_, possibly doing bump mapping with _bumpMap_
            var dgs = (_bumpMap != null)
                ? Bump(_bumpMap, dgGeom, dgShading)
                : dgShading;

            var bsdf = new Bsdf(dgs, dgGeom.Normal);

            // Evaluate textures for _MatteMaterial_ material and allocate BRDF
            Spectrum r = Spectrum.Clamp(_kd.Evaluate(dgs));
            float sig = MathUtility.Clamp(_sigma.Evaluate(dgs), 0.0f, 90.0f);
            if (!r.IsBlack)
            {
                if (sig == 0.0f)
                    bsdf.Add(new Lambertian(r));
                else
                    bsdf.Add(new OrenNayar(r, sig));
            }
            return bsdf;
        }
    }
}