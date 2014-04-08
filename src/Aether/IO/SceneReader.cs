using System.IO;
using Aether.Renderers;

namespace Aether.IO
{
    public static class SceneReader
    {
        private static readonly SceneParser Parser = new SceneParser();

        public static void Read(TextReader reader, out Scene scene, out Renderer renderer)
        {
            var sceneFile = Parser.Parse(reader.ReadToEnd());

            var context = new SceneReaderContext();
            foreach (var directive in sceneFile)
                directive.Process(context);

            renderer = context.RenderOptions.MakeRenderer();
            scene = context.RenderOptions.MakeScene();
        }
    }
}