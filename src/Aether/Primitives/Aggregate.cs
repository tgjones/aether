using System;
using Aether.Geometry;
using Aether.Lights;
using Aether.Reflection;
using Aether.Shapes;

namespace Aether.Primitives
{
    public abstract class Aggregate : Primitive
    {
        public override AreaLight GetAreaLight()
        {
            throw new InvalidOperationException("Aggregate.GetAreaLight() method called; should have gone to GeometricPrimitive.");
        }

        public override Bsdf GetBsdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            throw new InvalidOperationException("Aggregate.GetBsdf() method called; should have gone to GeometricPrimitive.");
        }

        public override Bssrdf GetBssrdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            throw new InvalidOperationException("Aggregate.GetBssrdf() method called; should have gone to GeometricPrimitive.");
        }
    }
}