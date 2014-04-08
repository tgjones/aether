using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class WorldBeginDirective : Directive
    {
        public override void Process(SceneReaderContext context)
        {
            context.VerifyOptions("WorldBegin");
            context.CurrentState = SceneReaderState.WorldBlock;
            for (var i = 0; i < TransformSet.MaxTransforms; i++)
                context.CurrentTransform[i] = new Transform();
            context.ActiveTransformBits = TransformSet.AllTransformsBits;
            context.NamedCoordinateSystems["world"] = context.CurrentTransform;
        }
    }
}