using System.Collections.Generic;
using Aether.Geometry;
using Aether.IO.Ast;
using Aether.Materials;
using Aether.Textures;

namespace Aether.IO
{
    public class GraphicsState
    {
        public Dictionary<string, Texture<float>> FloatTextures = new Dictionary<string, Texture<float>>();
        public Dictionary<string, Texture<Spectrum>> SpectrumTextures = new Dictionary<string, Texture<Spectrum>>();
        public ParamSet MaterialParams = new ParamSet();
        public string Material = "matte";
        public Dictionary<string, Material> NamedMaterials = new Dictionary<string, Material>();
        public string CurrentNamedMaterial = "";
        public ParamSet AreaLightParams = new ParamSet();
        public string AreaLight = "";
        public bool ReverseOrientation = false;

        public Material CreateMaterial(ParamSet parameters, Transform transform)
        {
            var textureParams = new TextureParams(parameters, MaterialParams, FloatTextures, SpectrumTextures);

            if (!string.IsNullOrEmpty(CurrentNamedMaterial) && NamedMaterials.ContainsKey(CurrentNamedMaterial))
                return NamedMaterials[CurrentNamedMaterial];
            return Factories.MakeMaterial(Material, transform, textureParams);
        }

        public GraphicsState Clone()
        {
            return new GraphicsState
            {
                FloatTextures = new Dictionary<string, Texture<float>>(FloatTextures),
                SpectrumTextures = new Dictionary<string, Texture<Spectrum>>(SpectrumTextures),
                MaterialParams = new ParamSet(MaterialParams),
                Material = Material,
                NamedMaterials = new Dictionary<string, Material>(NamedMaterials),
                CurrentNamedMaterial = CurrentNamedMaterial,
                AreaLightParams = new ParamSet(AreaLightParams),
                AreaLight = AreaLight,
                ReverseOrientation = ReverseOrientation
            };
        }
    }
}