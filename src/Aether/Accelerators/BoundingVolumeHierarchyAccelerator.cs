using System.Collections.Generic;
using Aether.Geometry;
using Aether.Primitives;

namespace Aether.Accelerators
{
    public class BoundingVolumeHierarchyAccelerator : Aggregate
    {
        public BoundingVolumeHierarchyAccelerator(IEnumerable<Primitive> primitives,
            int maxPrimitives = 1)
        {
            
        }

        public override BBox WorldBound
        {
            get { throw new System.NotImplementedException(); ; }
        }

        public override bool TryIntersect(Ray ray, out Intersection intersection)
        {
            throw new System.NotImplementedException();
        }

        public override bool Intersects(Ray ray)
        {
            throw new System.NotImplementedException();
        }
    }
}