using System;
using System.Collections.Generic;
using Aether.Primitives;

namespace Aether.IO.Ast
{
    public class ObjectBeginDirective : Directive
    {
        public string Name { get; set; }

        public override void Process(SceneReaderContext context)
        {
            context.VerifyWorld("ObjectBegin");
            new AttributeBeginDirective().Process(context);
            if (context.RenderOptions.CurrentInstance != null)
                throw new InvalidOperationException("ObjectBegin called inside of instance definition.");

            context.RenderOptions.Instances[Name] = new List<Primitive>();
            context.RenderOptions.CurrentInstance = context.RenderOptions.Instances[Name];
        }
    }
}