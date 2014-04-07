using System;

namespace Aether.Geometry
{
    public class AnimatedTransform
    {
        private readonly Transform _startTransform;
        private readonly float _startTime;
        private readonly Transform _endTransform;
        private readonly float _endTime;
        private readonly bool _actuallyAnimated;
        private readonly Vector[] _t;
        private readonly Quaternion[] _r;
        private readonly Matrix4x4[] _s;

        public AnimatedTransform(
            Transform startTransform, float startTime,
            Transform endTransform, float endTime)
        {
            _startTransform = startTransform;
            _startTime = startTime;
            _endTransform = endTransform;
            _endTime = endTime;
            _actuallyAnimated = !startTransform.Equals(endTransform);

            _t = new Vector[2];
            _r = new Quaternion[2];
            _s = new Matrix4x4[2];
            Decompose(startTransform.Matrix, out _t[0], out _r[0], out _s[0]);
            Decompose(endTransform.Matrix, out _t[1], out _r[1], out _s[1]);
        }

        private static void Decompose(Matrix4x4 m, out Vector t, out Quaternion r, out Matrix4x4 s)
        {
            // Extract translation _T_ from transformation matrix
            t.X = m.M[0, 3];
            t.Y = m.M[1, 3];
            t.Z = m.M[2, 3];

            // Compute new transformation matrix _M_ without translation
            Matrix4x4 M = m.Clone();
            for (int i = 0; i < 3; ++i)
                M.M[i, 3] = M.M[3, i] = 0.0f;
            M.M[3, 3] = 1.0f;

            // Extract rotation _R_ from transformation matrix
            float norm;
            int count = 0;
            Matrix4x4 R = M.Clone();
            do
            {
                // Compute next matrix _Rnext_ in series
                var Rnext = new Matrix4x4();
                var Rit = Matrix4x4.Invert(Matrix4x4.Transpose(R));
                for (int i = 0; i < 4; ++i)
                    for (int j = 0; j < 4; ++j)
                        Rnext.M[i, j] = 0.5f * (R.M[i, j] + Rit.M[i, j]);

                // Compute norm of difference between _R_ and _Rnext_
                norm = 0.0f;
                for (int i = 0; i < 3; ++i)
                {
                    float n = Math.Abs(R.M[i, 0] - Rnext.M[i, 0]) +
                        Math.Abs(R.M[i, 1] - Rnext.M[i, 1]) +
                        Math.Abs(R.M[i, 2] - Rnext.M[i, 2]);
                    norm = Math.Max(norm, n);
                }
                R = Rnext;
            } while (++count < 100 && norm > .0001f);
            // XXX TODO FIXME deal with flip...
            r = (Quaternion) new Transform(R);

            // Compute scale _S_ using rotation and original matrix
            s = Matrix4x4.Mul(Matrix4x4.Invert(R), M);
        }

        public void Interpolate(float time, out Transform t)
        {
            // Handle boundary conditions for matrix interpolation
            if (!_actuallyAnimated || time <= _startTime)
            {
                t = _startTransform;
                return;
            }
            if (time >= _endTime)
            {
                t = _endTransform;
                return;
            }
            float dt = (time - _startTime) / (_endTime - _startTime);
            // Interpolate translation at _dt_
            Vector trans = (1.0f - dt) * _t[0] + dt * _t[1];

            // Interpolate rotation at _dt_
            Quaternion rotate = Quaternion.Slerp(dt, _r[0], _r[1]);

            // Interpolate scale at _dt_
            var scale = new Matrix4x4();
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    scale.M[i, j] = MathUtility.Lerp(dt, _s[0].M[i, j], _s[1].M[i, j]);

            // Compute interpolated matrix as product of interpolated components
            t = Transform.Translate(trans) * rotate.ToTransform() * new Transform(scale);
        }

        public BBox MotionBounds(BBox b, bool useInverse)
        {
            if (!_actuallyAnimated)
                return Transform.Invert(_startTransform).TransformBBox(b);
            BBox ret = BBox.Empty;
            const int nSteps = 128;
            for (int i = 0; i < nSteps; ++i)
            {
                Transform t;
                float time = MathUtility.Lerp(i / (float) (nSteps - 1), _startTime, _endTime);
                Interpolate(time, out t);
                if (useInverse)
                    t = Transform.Invert(t);
                ret = BBox.Union(ret, t.TransformBBox(b));
            }
            return ret;
        }

        public bool HasScale()
        {
            return _startTransform.HasScale() || _endTransform.HasScale();
        }

        public Ray TransformRay(Ray r)
        {
            Ray tr;
            if (!_actuallyAnimated || r.Time <= _startTime)
                tr = _startTransform.TransformRay(r);
            else if (r.Time >= _endTime)
                tr = _endTransform.TransformRay(r);
            else
            {
                Transform t;
                Interpolate(r.Time, out t);
                tr = t.TransformRay(r);
            }
            tr.Time = r.Time;
            return tr;
        }

        public RayDifferential TransformRayDifferential(RayDifferential r)
        {
            RayDifferential tr;
            if (!_actuallyAnimated || r.Time <= _startTime)
                tr = _startTransform.TransformRayDifferential(r);
            else if (r.Time >= _endTime)
                tr = _endTransform.TransformRayDifferential(r);
            else
            {
                Transform t;
                Interpolate(r.Time, out t);
                tr = t.TransformRayDifferential(r);
            }
            tr.Time = r.Time;
            return tr;
        }

        public Point TransformPoint(float time, Point p)
        {
            if (!_actuallyAnimated || time <= _startTime)
                return _startTransform.TransformPoint(p);
            if (time >= _endTime)
                return _endTransform.TransformPoint(p);
            Transform t;
            Interpolate(time, out t);
            return t.TransformPoint(p);
        }

        public Vector TransformVector(float time, Vector v)
        {
            if (!_actuallyAnimated || time <= _startTime)
                return _startTransform.TransformVector(v);
            if (time >= _endTime)
                return _endTransform.TransformVector(v);
            Transform t;
            Interpolate(time, out t);
            return t.TransformVector(v);
        }
    }
}