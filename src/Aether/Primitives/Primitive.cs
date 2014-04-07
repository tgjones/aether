using System;
using System.Collections.Generic;
using Aether.Geometry;
using Aether.Lights;
using Aether.Reflection;
using Aether.Shapes;

namespace Aether.Primitives
{
    public abstract class Primitive
    {
        public abstract BBox WorldBound { get; }

        public virtual bool CanIntersect
        {
            get { return true; }
        }

        public abstract bool TryIntersect(Ray ray, out Intersection intersection);
        public abstract bool Intersects(Ray ray);

        public virtual IEnumerable<Primitive> Refine()
        {
            throw new InvalidOperationException("Unimplemented Primitive.Refine() method called.");
        }

        public List<Primitive> FullyRefine()
        {
            var refined = new List<Primitive>();
            var todo = new Stack<Primitive>(new[] { this });
            while (todo.Count > 0)
            {
                // Refine last primitive in todo list.
                var prim = todo.Pop();
                if (prim.CanIntersect)
                    refined.Add(prim);
                else
                    foreach (var temp in prim.Refine())
                        todo.Push(temp);
            }
            return refined;
        }

        public abstract AreaLight GetAreaLight();
        public abstract Bsdf GetBsdf(DifferentialGeometry dg, Transform objectToWorld);
        public abstract Bssrdf GetBssrdf(DifferentialGeometry dg, Transform objectToWorld);
    }
}