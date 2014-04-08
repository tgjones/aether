using System;

namespace Aether.IO.Ast
{
    public class CoordSysTransformDirective : Directive
    {
        public string Name { get; set; }

        public override void Process(SceneReaderContext context)
        {
            if (context.NamedCoordinateSystems.ContainsKey(Name))
                context.CurrentTransform = context.NamedCoordinateSystems[Name];
            else
                throw new InvalidOperationException("Could not find named coordinate system '" + Name + "'.");
        }
    }
}