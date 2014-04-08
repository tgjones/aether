using System;

namespace Aether.IO.Ast
{
    public class TextureDirective : Directive
    {
        public string Name { get; set; }
        public string TextureType { get; set; }
        public string TextureClass { get; set; }
        public ParamSet Parameters { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("Texture");

            var textureParams = new TextureParams(Parameters, Parameters,
                context.GraphicsState.FloatTextures,
                context.GraphicsState.SpectrumTextures);

            switch (TextureType)
            {
                case "float" :
                {
                    context.GraphicsState.FloatTextures[Name] = Factories.MakeFloatTexture(
                        TextureClass, context.CurrentTransform[0],
                        textureParams); ;
                    break;
                }
                case "color" :
                case "spectrum" :
                {
                    context.GraphicsState.SpectrumTextures[Name] = Factories.MakeSpectrumTexture(
                        TextureClass, context.CurrentTransform[0],
                        textureParams); ;
                    break;
                }
                default :
                    throw new InvalidOperationException("Texture type '" + TextureType +" ' unknown.");
            }
        }
    }
}