using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aether.IO.Ast
{
    public class SceneFile : Collection<Directive>
    {
        public SceneFile(IEnumerable<Directive> directives)
            : base(directives.ToList())
        {
            
        }
    }
}