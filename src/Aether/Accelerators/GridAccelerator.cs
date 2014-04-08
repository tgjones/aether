using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aether.Geometry;
using Aether.Primitives;

namespace Aether.Accelerators
{
    public class GridAccelerator : Aggregate
    {
        private readonly Primitive[] _primitives;
        private readonly int[] _numVoxels;
        private readonly BBox _bounds;
        private readonly Vector _width, _inverseWidth;
        private readonly GridVoxel[] _voxels;

        public GridAccelerator(IEnumerable<Primitive> primitives, bool refineImmediately)
        {
            // Initialize _primitives_ with primitives for grid
            _primitives = refineImmediately
                ? primitives.SelectMany(prim => prim.FullyRefine()).ToArray()
                : primitives.ToArray();

            // Compute bounds and choose grid resolution
            _bounds = BBox.Empty;
            for (var i = 0; i < _primitives.Length; ++i)
                _bounds = BBox.Union(_bounds, _primitives[i].WorldBound);
            Vector delta = _bounds.Max - _bounds.Min;

            // Find _voxelsPerUnitDist_ for grid
            int maxAxis = _bounds.MaximumExtent();
            float invMaxWidth = 1.0f / delta[maxAxis];
            Debug.Assert(invMaxWidth > 0.0f);
            float cubeRoot = 3.0f * MathUtility.Pow(_primitives.Length, 1.0f / 3.0f);
            float voxelsPerUnitDist = cubeRoot * invMaxWidth;
            _numVoxels = new int[3];
            for (int axis = 0; axis < 3; ++axis)
            {
                _numVoxels[axis] = MathUtility.Round(delta[axis] * voxelsPerUnitDist);
                _numVoxels[axis] = MathUtility.Clamp(_numVoxels[axis], 1, 64);
            }

            // Compute voxel widths and allocate voxels
            for (int axis = 0; axis < 3; ++axis)
            {
                _width[axis] = delta[axis] / _numVoxels[axis];
                _inverseWidth[axis] = (_width[axis] == 0.0f) ? 0.0f : 1.0f / _width[axis];
            }
            int nv = _numVoxels[0] * _numVoxels[1] * _numVoxels[2];
            _voxels = new GridVoxel[nv];

            // Add primitives to grid voxels
            for (var i = 0; i < _primitives.Length; ++i)
            {
                // Find voxel extent of primitive
                BBox pb = _primitives[i].WorldBound;
                int[] vmin = new int[3], vmax = new int[3];
                for (int axis = 0; axis < 3; ++axis)
                {
                    vmin[axis] = PosToVoxel(ref pb.Min, axis);
                    vmax[axis] = PosToVoxel(ref pb.Max, axis);
                }

                // Add primitive to overlapping voxels
                for (int z = vmin[2]; z <= vmax[2]; ++z)
                    for (int y = vmin[1]; y <= vmax[1]; ++y)
                        for (int x = vmin[0]; x <= vmax[0]; ++x)
                        {
                            int o = Offset(x, y, z);
                            if (_voxels[o] == null)
                            {
                                // Allocate new voxel and store primitive in it
                                _voxels[o] = new GridVoxel(_primitives[i]);
                            }
                            else
                            {
                                // Add primitive to already-allocated voxel
                                _voxels[o].AddPrimitive(_primitives[i]);
                            }
                        }
            }
        }

        public override BBox WorldBound
        {
            get { return _bounds; }
        }

        public override bool TryIntersect(Ray ray, ref Intersection intersection)
        {
            // Walk ray through voxel grid
            bool hitSomething = false;
            Intersection tempIntersection = null;
            DoIntersect(ray, voxel =>
            {
                hitSomething |= voxel.TryIntersect(ray, ref tempIntersection);
                return false;
            });
            if (hitSomething)
                intersection = tempIntersection;
            return hitSomething;
        }

        public override bool Intersects(Ray ray)
        {
            var result = false;
            DoIntersect(ray, voxel => result = voxel.Intersects(ray));
            return result;
        }

        private int PosToVoxel(ref Point p, int axis)
        {
            var v = (int) ((p[axis] - _bounds.Min[axis]) * _inverseWidth[axis]);
            return MathUtility.Clamp(v, 0, _numVoxels[axis] - 1);
        }

        private float VoxelToPos(int p, int axis)
        {
            return _bounds.Min[axis] + p * _width[axis];
        }

        private int Offset(int x, int y, int z)
        {
            return z * _numVoxels[0] * _numVoxels[1] + y * _numVoxels[0] + x;
        }

        private void DoIntersect(Ray ray, Func<GridVoxel, bool> callback)
        {
            // Check ray against overall grid bounds
            float rayT;
            if (_bounds.Inside(ray.Evaluate(ray.MinT)))
                rayT = ray.MinT;
            else if (!_bounds.TryIntersect(ray, out rayT))
                return;
            Point gridIntersect = ray.Evaluate(rayT);

            // Set up 3D DDA for ray
            float[] NextCrossingT = new float[3], DeltaT = new float[3];
            int[] Step = new int[3], Out = new int[3], Pos = new int[3];
            for (int axis = 0; axis < 3; ++axis)
            {
                // Compute current voxel for axis
                Pos[axis] = PosToVoxel(ref gridIntersect, axis);
                if (ray.Direction[axis] >= 0)
                {
                    // Handle ray with positive direction for voxel stepping
                    NextCrossingT[axis] = rayT +
                        (VoxelToPos(Pos[axis] + 1, axis) - gridIntersect[axis]) / ray.Direction[axis];
                    DeltaT[axis] = _width[axis] / ray.Direction[axis];
                    Step[axis] = 1;
                    Out[axis] = _numVoxels[axis];
                }
                else
                {
                    // Handle ray with negative direction for voxel stepping
                    NextCrossingT[axis] = rayT +
                        (VoxelToPos(Pos[axis], axis) - gridIntersect[axis]) / ray.Direction[axis];
                    DeltaT[axis] = -_width[axis] / ray.Direction[axis];
                    Step[axis] = -1;
                    Out[axis] = -1;
                }
            }

            // Walk ray through voxel grid
            while (true)
            {
                // Check for intersection in current voxel and advance to next
                var voxel = _voxels[Offset(Pos[0], Pos[1], Pos[2])];
                if (voxel != null)
                    if (callback(voxel))
                        break;

                // Advance to next voxel

                // Find _stepAxis_ for stepping to next voxel
                int bits = (((NextCrossingT[0] < NextCrossingT[1]) ? 1 : 0) << 2) +
                    (((NextCrossingT[0] < NextCrossingT[2]) ? 1 : 0) << 1) +
                    (((NextCrossingT[1] < NextCrossingT[2]) ? 1 : 0));
                int[] cmpToAxis = { 2, 1, 2, 1, 2, 2, 0, 0 };
                int stepAxis = cmpToAxis[bits];
                if (ray.MaxT < NextCrossingT[stepAxis])
                    break;
                Pos[stepAxis] += Step[stepAxis];
                if (Pos[stepAxis] == Out[stepAxis])
                    break;
                NextCrossingT[stepAxis] += DeltaT[stepAxis];
            }
        }
    }
}