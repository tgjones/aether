using System.Collections.Generic;
using System.Linq;
using Aether.Geometry;
using Aether.Lights;
using Aether.Materials;
using Aether.Reflection;
using Aether.Shapes;

namespace Aether.Primitives
{
    public class GeometricPrimitive : Primitive
    {
        private readonly Shape _shape;
        private readonly Material _material;
        private readonly AreaLight _areaLight;

        public GeometricPrimitive(Shape shape, Material material, AreaLight areaLight)
        {
            _shape = shape;
            _material = material;
            _areaLight = areaLight;
        }

        public override BBox WorldBound
        {
            get { return _shape.WorldBound; }
        }

        public override bool CanIntersect
        {
            get { return _shape.CanIntersect; }
        }

        public override IEnumerable<Primitive> Refine()
        {
            return _shape.Refine().Select(x => new GeometricPrimitive(x, _material, _areaLight));
        }

        public override bool TryIntersect(Ray ray, ref Intersection intersection)
        {
            float thit, rayEpsilon;
            DifferentialGeometry dg;
            if (!_shape.TryIntersect(ray, out thit, out rayEpsilon, out dg))
                return false;
            intersection = new Intersection(dg, this,
                _shape.ObjectToWorld, _shape.WorldToObject,
                rayEpsilon);
            ray.MaxT = thit;
            return true;
        }

        public override bool Intersects(Ray ray)
        {
            return _shape.Intersects(ray);
        }

        public override AreaLight GetAreaLight()
        {
            return _areaLight;
        }

        public override Bsdf GetBsdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            var dgs = _shape.GetShadingGeometry(objectToWorld, dg);
            return _material.GetBsdf(dg, dgs);
        }

        public override Bssrdf GetBssrdf(DifferentialGeometry dg, Transform objectToWorld)
        {
            var dgs = _shape.GetShadingGeometry(objectToWorld, dg);
            return _material.GetBssrdf(dg, dgs);
        }
    }
}