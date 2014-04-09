using Aether.Geometry;
using Aether.Lights;
using Aether.Reflection;
using Aether.Shapes;

namespace Aether.Primitives
{
    public class TransformedPrimitive : Primitive
    {
        private readonly Primitive _primitive;
        private readonly AnimatedTransform _worldToPrimitive;

        public TransformedPrimitive(Primitive primitive, AnimatedTransform worldToPrimitive)
        {
            _primitive = primitive;
            _worldToPrimitive = worldToPrimitive;
        }

        public override BBox WorldBound
        {
            get { return _worldToPrimitive.MotionBounds(_primitive.WorldBound, true); }
        }

        public override bool TryIntersect(Ray r, ref Intersection intersection)
        {
            Transform w2p;
            _worldToPrimitive.Interpolate(r.Time, out w2p);
            Ray ray = w2p.TransformRay(r);
            if (!_primitive.TryIntersect(ray, ref intersection))
                return false;
            r.MaxT = ray.MaxT;
            if (!w2p.IsIdentity())
            {
                // Compute world-to-object transformation for instance
                intersection.WorldToObject = intersection.WorldToObject * w2p;
                intersection.ObjectToWorld = Transform.Invert(intersection.WorldToObject);

                // Transform instance's differential geometry to world space
                Transform primitiveToWorld = Transform.Invert(w2p);
                var dg = intersection.DifferentialGeometry;
                dg.Point = primitiveToWorld.TransformPoint(ref dg.Point);
                dg.Normal = Normal.Normalize(primitiveToWorld.TransformNormal(ref dg.Normal));
                dg.DpDu = primitiveToWorld.TransformVector(ref dg.DpDu);
                dg.DpDv = primitiveToWorld.TransformVector(ref dg.DpDv);
                dg.DnDu = primitiveToWorld.TransformNormal(ref dg.DnDu);
                dg.DnDv = primitiveToWorld.TransformNormal(ref dg.DnDv);
            }
            return true;
        }

        public override bool Intersects(Ray ray)
        {
            return _primitive.Intersects(_worldToPrimitive.TransformRay(ray));
        }

        public override AreaLight GetAreaLight()
        {
            return null;
        }

        public override Bsdf GetBsdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            return null;
        }

        public override Bssrdf GetBssrdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            return null;
        }
    }
}