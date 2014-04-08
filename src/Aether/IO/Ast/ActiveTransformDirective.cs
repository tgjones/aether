using System;

namespace Aether.IO.Ast
{
    public class ActiveTransformDirective : Directive
    {
        public ActiveTransformType Type { get; set; }

        public override void Process(SceneReaderContext context)
        {
            switch (Type)
            {
                case ActiveTransformType.All:
                    context.ActiveTransformBits = TransformSet.AllTransformsBits;
                    break;
                case ActiveTransformType.Start:
                    context.ActiveTransformBits = TransformSet.StartTransformBits;
                    break;
                case ActiveTransformType.End:
                    context.ActiveTransformBits = TransformSet.EndTransformBits;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ActiveTransformType
    {
        All,
        Start,
        End
    }
}