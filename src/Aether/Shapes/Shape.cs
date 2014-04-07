using System;
using System.Collections.Generic;
using Aether.Geometry;

namespace Aether.Shapes
{
    public abstract class Shape
    {
        protected Shape(Transform objectToWorld, bool reverseOrientation)
        {
            ObjectToWorld = objectToWorld;
            WorldToObject = Transform.Invert(objectToWorld);
            ReverseOrientation = reverseOrientation;
            TransformSwapsHandedness = objectToWorld.SwapsHandedness();
        }

        public Transform ObjectToWorld { get; private set; }
        public Transform WorldToObject { get; private set; }
        public bool ReverseOrientation { get; private set; }
        public bool TransformSwapsHandedness { get; private set; }

        public abstract BBox ObjectBound { get; }

        public virtual BBox WorldBound
        {
            get { return ObjectToWorld.TransformBBox(ObjectBound); }
        }

        public virtual bool CanIntersect
        {
            get { return true; }
        }

        public virtual float Area
        {
            get { throw new InvalidOperationException("Unimplemented Shape.Area property called."); }
        }

        public virtual IEnumerable<Shape> Refine()
        {
            throw new InvalidOperationException("Unimplemented Shape.Refine() method called.");
        }

        public virtual bool TryIntersect(Ray ray,
            out float tHit, out float rayEpsilon,
            out DifferentialGeometry dg)
        {
            throw new InvalidOperationException("Unimplemented Shape.TryIntersect() method called.");
        }

        public virtual bool Intersects(Ray ray)
        {
            throw new InvalidOperationException("Unimplemented Shape.Intersects() method called.");
        }

        public virtual DifferentialGeometry GetShadingGeometry(
            Transform objectToWorld, DifferentialGeometry dg)
        {
            return dg;
        }

        public virtual Point Sample(float u1, float u2, out Normal ns)
        {
            throw new InvalidOperationException("Unimplemented Shape.Sample() method called.");
        }

        public virtual float Pdf(Point pShape)
        {
            return 1.0f / Area;
        }

        public virtual Point Sample(Point p, float u1, float u2, out Normal ns)
        {
            return Sample(u1, u2, out ns);
        }

        public virtual float Pdf(Point p, Vector wi)
        {
            // Intersect sample ray with area light geometry
            var ray = new Ray(p, wi, 1e-3f);
            ray.Depth = -1; // temporary hack to ignore alpha mask
            float thit, rayEpsilon;
            DifferentialGeometry dgLight;
            if (!TryIntersect(ray, out thit, out rayEpsilon, out dgLight))
                return 0.0f;

            // Convert light sample weight to solid angle measure
            float pdf = Point.DistanceSquared(p, ray.Evaluate(thit)) / (Normal.AbsDot(dgLight.Normal, -wi) * Area);
            if (float.IsInfinity(pdf))
                pdf = 0.0f;
            return pdf;
        }
    }
}