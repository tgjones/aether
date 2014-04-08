namespace Aether.IO.Ast
{
    public class MakeNamedMaterialDirective : Directive
    {
        public string MaterialName { get; set; }
        public string MaterialType { get; set; }
        public ParamSet Parameters { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("MakeNamedMaterial");

            var textureParams = new TextureParams(Parameters, context.GraphicsState.MaterialParams,
                context.GraphicsState.FloatTextures, context.GraphicsState.SpectrumTextures);
            context.GraphicsState.NamedMaterials[MaterialName] = Factories.MakeMaterial(
                MaterialType, context.CurrentTransform[0], textureParams);
        }
    }
}