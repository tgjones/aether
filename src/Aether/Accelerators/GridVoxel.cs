using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aether.Geometry;
using Aether.Primitives;

namespace Aether.Accelerators
{
    public class GridVoxel
    {
        private readonly List<Primitive> _primitives;
        private bool _allCanIntersect;

        public GridVoxel(Primitive op)
        {
            _primitives = new List<Primitive>();
            _primitives.Add(op);
            _allCanIntersect = false;
        }

        public int Count
        {
            get { return _primitives.Count; }
        }

        public void AddPrimitive(Primitive prim)
        {
            _primitives.Add(prim);
        }

        public bool TryIntersect(Ray ray, out Intersection intersection)
        {
            RefineIfNeeded();

            // Loop over primitives in voxel and find intersections
            intersection = null;
            bool hitSomething = false;
            for (var i = 0; i < _primitives.Count; ++i)
            {
                var prim = _primitives[i];
                if (prim.TryIntersect(ray, out intersection))
                {
                    hitSomething = true;
                }
            }
            return hitSomething;
        }

        public bool Intersects(Ray ray)
        {
            RefineIfNeeded();
            return _primitives.Any(prim => prim.Intersects(ray));
        }

        private void RefineIfNeeded()
        {
            // Refine primitives in voxel if needed
            if (!_allCanIntersect)
            {
                for (var i = 0; i < _primitives.Count; ++i)
                {
                    var prim = _primitives[i];
                    // Refine primitive _prim_ if it's not intersectable
                    if (!prim.CanIntersect)
                    {
                        var p = prim.FullyRefine();
                        Debug.Assert(p.Count > 0);
                        if (p.Count == 1)
                            _primitives[i] = p[0];
                        else
                            _primitives[i] = new GridAccelerator(p, false);
                    }
                }
                _allCanIntersect = true;
            }
        }
    }
}