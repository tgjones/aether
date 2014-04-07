using Aether.Geometry;
using Aether.Primitives;
using Aether.Reflection;
using Aether.Shapes;

namespace Aether
{
    public class Intersection
    {
        private readonly DifferentialGeometry _dg;
        private readonly Primitive _primitive;
        private readonly float _rayEpsilon;

        public Intersection(DifferentialGeometry dg, Primitive primitive,
            Transform objectToWorld, Transform worldToObject,
            float rayEpsilon)
        {
            _dg = dg;
            _primitive = primitive;
            WorldToObject = worldToObject;
            ObjectToWorld = objectToWorld;
            _rayEpsilon = rayEpsilon;
        }

        public Transform ObjectToWorld { get; set; }
        public Transform WorldToObject { get; set; }

        public DifferentialGeometry DifferentialGeometry
        {
            get { return _dg; }
        }

        public float RayEpsilon
        {
            get { return _rayEpsilon; }
        }

        public Bsdf GetBsdf(RayDifferential ray)
        {
            _dg.ComputeDifferentials(ray);
            return _primitive.GetBsdf(_dg, ObjectToWorld);
        }

        public Bssrdf GetBssrdf(RayDifferential ray)
        {
            _dg.ComputeDifferentials(ray);
            return _primitive.GetBssrdf(_dg, ObjectToWorld);
        }

        public Spectrum Le(Vector wo)
        {
            var areaLight = _primitive.GetAreaLight();
            return (areaLight != null)
                ? areaLight.L(_dg.Point, _dg.Normal, wo)
                : Spectrum.CreateBlack();
        }
    }
}