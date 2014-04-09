using System.Diagnostics;

namespace Aether.Geometry
{
    public class Transform
    {
        private readonly Matrix4x4 _m, _mInv;

        public Transform(float[,] mat)
            : this(new Matrix4x4(mat))
        {
            
        }

        public Transform(Matrix4x4 mat, Matrix4x4 minv = null)
        {
            _m = mat;
            _mInv = minv ?? Matrix4x4.Invert(_m);
        }

        public Transform()
            : this(new Matrix4x4())
        {
            
        }

        public Matrix4x4 Matrix
        {
            get { return _m; }
        }

        public Matrix4x4 InverseMatrix
        {
            get { return _mInv; }
        }

        public bool IsIdentity()
        {
            return
                _m.M[0, 0] == 1.0f && _m.M[0, 1] == 0.0f &&
                _m.M[0, 2] == 0.0f && _m.M[0, 3] == 0.0f &&
                _m.M[1, 0] == 0.0f && _m.M[1, 1] == 1.0f &&
                _m.M[1, 2] == 0.0f && _m.M[1, 3] == 0.0f &&
                _m.M[2, 0] == 0.0f && _m.M[2, 1] == 0.0f &&
                _m.M[2, 2] == 1.0f && _m.M[2, 3] == 0.0f &&
                _m.M[3, 0] == 0.0f && _m.M[3, 1] == 0.0f &&
                _m.M[3, 2] == 0.0f && _m.M[3, 3] == 1.0f;
        }

        private static bool NotOne(float x)
        {
            return x < .999f || x > 1.001f;
        }

        public bool HasScale()
        {
            float la2 = TransformVector(new Vector(1, 0, 0)).LengthSquared();
            float lb2 = TransformVector(new Vector(0, 1, 0)).LengthSquared();
            float lc2 = TransformVector(new Vector(0, 0, 1)).LengthSquared();
            return NotOne(la2) || NotOne(lb2) || NotOne(lc2);
        }

        public Point TransformPoint(Point pt)
        {
            return TransformPoint(ref pt);
        }

        public Point TransformPoint(ref Point pt)
        {
            float x = pt.X, y = pt.Y, z = pt.Z;
            float xp = _m.M[0, 0] * x + _m.M[0, 1] * y + _m.M[0, 2] * z + _m.M[0, 3];
            float yp = _m.M[1, 0] * x + _m.M[1, 1] * y + _m.M[1, 2] * z + _m.M[1, 3];
            float zp = _m.M[2, 0] * x + _m.M[2, 1] * y + _m.M[2, 2] * z + _m.M[2, 3];
            float wp = _m.M[3, 0] * x + _m.M[3, 1] * y + _m.M[3, 2] * z + _m.M[3, 3];
            Debug.Assert(wp != 0);
            if (wp == 1.0f)
                return new Point(xp, yp, zp);
            return new Point(xp, yp, zp) / wp;
        }

        public Vector TransformVector(Vector v)
        {
            return TransformVector(ref v);
        }

        public Vector TransformVector(ref Vector v)
        {
            float x = v.X, y = v.Y, z = v.Z;
            return new Vector(
                _m.M[0, 0] * x + _m.M[0, 1] * y + _m.M[0, 2] * z,
                _m.M[1, 0] * x + _m.M[1, 1] * y + _m.M[1, 2] * z,
                _m.M[2, 0] * x + _m.M[2, 1] * y + _m.M[2, 2] * z);
        }

        public Normal TransformNormal(Normal n)
        {
            return TransformNormal(ref n);
        }

        public Normal TransformNormal(ref Normal n)
        {
            float x = n.X, y = n.Y, z = n.Z;
            return new Normal(
                _mInv.M[0, 0] * x + _mInv.M[1, 0] * y + _mInv.M[2, 0] * z,
                _mInv.M[0, 1] * x + _mInv.M[1, 1] * y + _mInv.M[2, 1] * z,
                _mInv.M[0, 2] * x + _mInv.M[1, 2] * y + _mInv.M[2, 2] * z);
        }

        public Ray TransformRay(Ray r)
        {
            var ret = r.Clone();
            ret.Origin = TransformPoint(ref r.Origin);
            ret.Direction = TransformVector(ref r.Direction);
            return ret;
        }

        public RayDifferential TransformRayDifferential(RayDifferential r)
        {
            var ret = r.Clone();
            ret.Origin = TransformPoint(ref r.Origin);
            ret.Direction = TransformVector(ref r.Direction);
            ret.RxOrigin = TransformPoint(ref r.RxOrigin);
            ret.RyOrigin = TransformPoint(ref r.RyOrigin);
            ret.RxDirection = TransformVector(ref r.RxDirection);
            ret.RyDirection = TransformVector(ref r.RyDirection);
            return ret;
        }

        public BBox TransformBBox(BBox b)
        {
            var ret = BBox.FromPoint(TransformPoint(new Point(b.Min.X, b.Min.Y, b.Min.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Max.X, b.Min.Y, b.Min.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Min.X, b.Max.Y, b.Min.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Min.X, b.Min.Y, b.Max.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Min.X, b.Max.Y, b.Max.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Max.X, b.Max.Y, b.Min.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Max.X, b.Min.Y, b.Max.Z)));
            ret = BBox.Union(ret, TransformPoint(new Point(b.Max.X, b.Max.Y, b.Max.Z)));
            return ret;
        }

        public static Transform operator *(Transform t1, Transform t2)
        {
            Matrix4x4 m1 = Matrix4x4.Mul(t1._m, t2._m);
            Matrix4x4 m2 = Matrix4x4.Mul(t2._mInv, t1._mInv);
            return new Transform(m1, m2);
        }

        public bool SwapsHandedness()
        {
            float det =
                ((_m.M[0, 0] *
                 (_m.M[1, 1] * _m.M[2, 2] -
                  _m.M[1, 2] * _m.M[2, 1])) -
                 (_m.M[0, 1] *
                  (_m.M[1, 0] * _m.M[2, 2] -
                   _m.M[1, 2] * _m.M[2, 0])) +
                 (_m.M[0, 2] *
                  (_m.M[1, 0] * _m.M[2, 1] -
                   _m.M[1, 1] * _m.M[2, 0])));
            return det < 0.0f;
        }

        public override bool Equals(object obj)
        {
            var t = obj as Transform;
            return t != null && t._m.Equals(_m) && t._mInv.Equals(_mInv);
        }

        public override int GetHashCode()
        {
            return _m.GetHashCode();
        }

        #region Static methods

        public static Transform Invert(Transform t)
        {
            return new Transform(t._mInv, t._m);
        }

        public static Transform Transpose(Transform t)
        {
            return new Transform(
                Matrix4x4.Transpose(t._m),
                Matrix4x4.Transpose(t._mInv));
        }

        public static Transform Translate(Vector delta)
        {
            var m = new Matrix4x4(
                1, 0, 0, delta.X,
                0, 1, 0, delta.Y,
                0, 0, 1, delta.Z,
                0, 0, 0, 1);
            var minv = new Matrix4x4(
                1, 0, 0, -delta.X,
                0, 1, 0, -delta.Y,
                0, 0, 1, -delta.Z,
                0, 0, 0, 1);
            return new Transform(m, minv);
        }

        public static Transform Scale(float x, float y, float z)
        {
            var m = new Matrix4x4(
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1);
            var minv = new Matrix4x4(
                1.0f / x, 0, 0, 0,
                0, 1.0f / y, 0, 0,
                0, 0, 1.0f / z, 0,
                0, 0, 0, 1);
            return new Transform(m, minv);
        }

        public static Transform RotateX(float angle)
        {
            float sinT = MathUtility.Sin(MathUtility.ToRadians(angle));
            float cosT = MathUtility.Cos(MathUtility.ToRadians(angle));
            var m = new Matrix4x4(
                1, 0, 0, 0,
                0, cosT, -sinT, 0,
                0, sinT, cosT, 0,
                0, 0, 0, 1);
            return new Transform(m, Matrix4x4.Transpose(m));
        }

        public static Transform RotateY(float angle)
        {
            float sinT = MathUtility.Sin(MathUtility.ToRadians(angle));
            float cosT = MathUtility.Cos(MathUtility.ToRadians(angle));
            var m = new Matrix4x4(
                cosT, 0, sinT, 0,
                0, 1, 0, 0,
                -sinT, 0, cosT, 0,
                0, 0, 0, 1);
            return new Transform(m, Matrix4x4.Transpose(m));
        }

        public static Transform RotateZ(float angle)
        {
            float sinT = MathUtility.Sin(MathUtility.ToRadians(angle));
            float cosT = MathUtility.Cos(MathUtility.ToRadians(angle));
            var m = new Matrix4x4(
                cosT, -sinT, 0, 0,
                sinT, cosT, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
            return new Transform(m, Matrix4x4.Transpose(m));
        }

        public static Transform Rotate(float angle, Vector axis)
        {
            Vector a = Vector.Normalize(axis);
            float s = MathUtility.Sin(MathUtility.ToRadians(angle));
            float c = MathUtility.Cos(MathUtility.ToRadians(angle));
            var m = new float[4, 4];

            m[0, 0] = a.X * a.X + (1.0f - a.X * a.X) * c;
            m[0, 1] = a.X * a.Y * (1.0f - c) - a.Z * s;
            m[0, 2] = a.X * a.Z * (1.0f - c) + a.Y * s;
            m[0, 3] = 0;

            m[1, 0] = a.X * a.Y * (1.0f - c) + a.Z * s;
            m[1, 1] = a.Y * a.Y + (1.0f - a.Y * a.Y) * c;
            m[1, 2] = a.Y * a.Z * (1.0f - c) - a.X * s;
            m[1, 3] = 0;

            m[2, 0] = a.X * a.Z * (1.0f - c) - a.Y * s;
            m[2, 1] = a.Y * a.Z * (1.0f - c) + a.X * s;
            m[2, 2] = a.Z * a.Z + (1.0f - a.Z * a.Z) * c;
            m[2, 3] = 0;

            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = 0;
            m[3, 3] = 1;

            var mat = new Matrix4x4(m);
            return new Transform(mat, Matrix4x4.Transpose(mat));
        }

        public static Transform LookAt(Point pos, Point look, Vector up)
        {
            var m = new float[4, 4];
            // Initialize fourth column of viewing matrix
            m[0, 3] = pos.X;
            m[1, 3] = pos.Y;
            m[2, 3] = pos.Z;
            m[3, 3] = 1;

            // Initialize first three columns of viewing matrix
            Vector dir = Vector.Normalize(look - pos);
            Vector left = Vector.Normalize(Vector.Cross(Vector.Normalize(up), dir));
            Vector newUp = Vector.Cross(dir, left);
            m[0, 0] = left.X;
            m[1, 0] = left.Y;
            m[2, 0] = left.Z;
            m[3, 0] = 0.0f;
            m[0, 1] = newUp.X;
            m[1, 1] = newUp.Y;
            m[2, 1] = newUp.Z;
            m[3, 1] = 0.0f;
            m[0, 2] = dir.X;
            m[1, 2] = dir.Y;
            m[2, 2] = dir.Z;
            m[3, 2] = 0.0f;
            var camToWorld = new Matrix4x4(m);
            return new Transform(Matrix4x4.Invert(camToWorld), camToWorld);
        }

        public static Transform Orthographic(float znear, float zfar)
        {
            return Scale(1.0f, 1.0f, 1.0f / (zfar - znear)) *
                Translate(new Vector(0.0f, 0.0f, -znear));
        }

        public static Transform Perspective(float fov, float n, float f)
        {
            // Perform projective divide
            var persp = new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, f / (f - n), -f * n / (f - n),
                0, 0, 1, 0);

            // Scale to canonical viewing volume
            float invTanAng = 1.0f / MathUtility.Tan(MathUtility.ToRadians(fov) / 2.0f);
            return Scale(invTanAng, invTanAng, 1) * new Transform(persp);
        }

        #endregion
    }
}