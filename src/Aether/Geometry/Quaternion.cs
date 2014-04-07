using System;

namespace Aether.Geometry
{
    public struct Quaternion
    {
        public Vector V;
        public float W;

        public Quaternion(Vector v, float w)
        {
            V = v;
            W = w;
        }

        public static Quaternion Identity
        {
            get { return new Quaternion(new Vector(0, 0, 0), 1); }
        }

        public static Quaternion operator +(Quaternion l, Quaternion r)
        {
            return new Quaternion(l.V + r.V, l.W + r.W);
        }

        public static Quaternion operator -(Quaternion l, Quaternion r)
        {
            return new Quaternion(l.V - r.V, l.W - r.W);
        }

        public static Quaternion operator *(Quaternion l, float r)
        {
            return new Quaternion(l.V * r, l.W * r);
        }

        public static Quaternion operator *(float l, Quaternion r)
        {
            return new Quaternion(l * r.V, l * r.W);
        }

        public static Quaternion operator /(Quaternion l, float r)
        {
            return new Quaternion(l.V / r, l.W / r);
        }

        public Transform ToTransform()
        {
            float xx = V.X * V.X, yy = V.Y * V.Y, zz = V.Z * V.Z;
            float xy = V.X * V.Y, xz = V.X * V.Z, yz = V.Y * V.Z;
            float wx = V.X * W, wy = V.Y * W, wz = V.Z * W;

            var m = new Matrix4x4();
            m.M[0, 0] = 1.0f - 2.0f * (yy + zz);
            m.M[0, 1] = 2.0f * (xy + wz);
            m.M[0, 2] = 2.0f * (xz - wy);
            m.M[1, 0] = 2.0f * (xy - wz);
            m.M[1, 1] = 1.0f - 2.0f * (xx + zz);
            m.M[1, 2] = 2.0f * (yz + wx);
            m.M[2, 0] = 2.0f * (xz + wy);
            m.M[2, 1] = 2.0f * (yz - wx);
            m.M[2, 2] = 1.0f - 2.0f * (xx + yy);

            // Transpose since we are left-handed.  Ugh.
            return new Transform(Matrix4x4.Transpose(m), m);
        }

        public static explicit operator Quaternion(Transform t)
        {
            var ret = new Quaternion();

            var m = t.Matrix;
            float trace = m.M[0, 0] + m.M[1, 1] + m.M[2, 2];
            if (trace > 0.0f)
            {
                // Compute w from matrix trace, then xyz
                // 4w^2 = m[0,0] + m[1,1] + m[2,2] + m[3,3] (but m[3,3] == 1)
                float s = MathUtility.Sqrt(trace + 1.0f);
                ret.W = s / 2.0f;
                s = 0.5f / s;
                ret.V.X = (m.M[2, 1] - m.M[1, 2]) * s;
                ret.V.Y = (m.M[0, 2] - m.M[2, 0]) * s;
                ret.V.Z = (m.M[1, 0] - m.M[0, 1]) * s;
            }
            else
            {
                // Compute largest of $x$, $y$, or $z$, then remaining components
                int[] nxt = { 1, 2, 0 };
                var q = new float[3];
                int i = 0;
                if (m.M[1, 1] > m.M[0, 0]) i = 1;
                if (m.M[2, 2] > m.M[i, i]) i = 2;
                int j = nxt[i];
                int k = nxt[j];
                float s = MathUtility.Sqrt((m.M[i, i] - (m.M[j, j] + m.M[k, k])) + 1.0f);
                q[i] = s * 0.5f;
                if (s != 0.0f)
                    s = 0.5f / s;
                ret.W = (m.M[k, j] - m.M[j, k]) * s;
                q[j] = (m.M[j, i] + m.M[i, j]) * s;
                q[k] = (m.M[k, i] + m.M[i, k]) * s;
                ret.V.X = q[0];
                ret.V.Y = q[1];
                ret.V.Z = q[2];
            }
            return ret;
        }

        public static Quaternion Slerp(float t, Quaternion q1, Quaternion q2)
        {
            float cosTheta = Dot(q1, q2);
            if (cosTheta > .9995f)
                return Normalize((1.0f - t) * q1 + t * q2);
            float theta = MathUtility.Acos(MathUtility.Clamp(cosTheta, -1.0f, 1.0f));
            float thetap = theta * t;
            Quaternion qperp = Normalize(q2 - q1 * cosTheta);
            return q1 * MathUtility.Cos(thetap) + qperp * MathUtility.Sin(thetap);
        }

        public static float Dot(Quaternion q1, Quaternion q2)
        {
            return Vector.Dot(q1.V, q2.V) + q1.W * q2.W;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            return q / MathUtility.Sqrt(Dot(q, q));
        }
    }
}