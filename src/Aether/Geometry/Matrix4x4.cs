using System;

namespace Aether.Geometry
{
    public class Matrix4x4
    {
        public readonly float[,] M;

        public Matrix4x4()
        {
            M = new float[4, 4];
            M[0, 0] = M[1, 1] = M[2, 2] = M[3, 3] = 1.0f;
        }

        public Matrix4x4(float[,] values)
        {
            M = values;
        }

        public Matrix4x4(
            float t00, float t01, float t02, float t03,
            float t10, float t11, float t12, float t13,
            float t20, float t21, float t22, float t23,
            float t30, float t31, float t32, float t33)
        {
            M = new float[4, 4];
            M[0, 0] = t00;
            M[0, 1] = t01;
            M[0, 2] = t02;
            M[0, 3] = t03;
            M[1, 0] = t10;
            M[1, 1] = t11;
            M[1, 2] = t12;
            M[1, 3] = t13;
            M[2, 0] = t20;
            M[2, 1] = t21;
            M[2, 2] = t22;
            M[2, 3] = t23;
            M[3, 0] = t30;
            M[3, 1] = t31;
            M[3, 2] = t32;
            M[3, 3] = t33;
        }

        public override bool Equals(object obj)
        {
            var m2 = obj as Matrix4x4;
            if (m2 == null)
                return false;

            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    if (M[i,j] != m2.M[i,j])
                        return false;
            return true;
        }

        public override int GetHashCode()
        {
            return M.GetHashCode();
        }

        public Matrix4x4 Clone()
        {
            var values = new float[4, 4];
            Array.Copy(M, values, M.LongLength);
            return new Matrix4x4(values);
        }

        #region Static methods

        public static Matrix4x4 Transpose(Matrix4x4 m)
        {
            return new Matrix4x4(
                m.M[0, 0], m.M[1, 0], m.M[2, 0], m.M[3, 0],
                m.M[0, 1], m.M[1, 1], m.M[2, 1], m.M[3, 1],
                m.M[0, 2], m.M[1, 2], m.M[2, 2], m.M[3, 2],
                m.M[0, 3], m.M[1, 3], m.M[2, 3], m.M[3, 3]);
        }

        public static Matrix4x4 Invert(Matrix4x4 m)
        {
            int[] indxc = new int[4], indxr = new int[4];
            int[] ipiv = { 0, 0, 0, 0 };
            var minv = new float[4, 4];
            Array.Copy(m.M, minv, m.M.LongLength);
            for (int i = 0; i < 4; i++)
            {
                int irow = -1, icol = -1;
                float big = 0.0f;
                // Choose pivot
                for (int j = 0; j < 4; j++)
                    if (ipiv[j] != 1)
                        for (int k = 0; k < 4; k++)
                            if (ipiv[k] == 0)
                            {
                                if (Math.Abs(minv[j, k]) >= big)
                                {
                                    big = Math.Abs(minv[j, k]);
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (ipiv[k] > 1)
                                throw new InvalidOperationException("Singular matrix in MatrixInvert");
                ++ipiv[icol];
                // Swap rows _irow_ and _icol_ for pivot
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                        MathUtility.Swap(ref minv[irow, k], ref minv[icol, k]);
                }
                indxr[i] = irow;
                indxc[i] = icol;
                if (minv[icol, icol] == 0.0f)
                    throw new InvalidOperationException("Singular matrix in MatrixInvert");

                // Set $m[icol][icol]$ to one by scaling row _icol_ appropriately
                float pivinv = 1.0f / minv[icol, icol];
                minv[icol, icol] = 1.0f;
                for (int j = 0; j < 4; j++)
                    minv[icol, j] *= pivinv;

                // Subtract this row from others to zero out their columns
                for (int j = 0; j < 4; j++)
                    if (j != icol)
                    {
                        float save = minv[j, icol];
                        minv[j, icol] = 0;
                        for (int k = 0; k < 4; k++)
                            minv[j, k] -= minv[icol, k] * save;
                    }
            }
            // Swap columns to reflect permutation
            for (int j = 3; j >= 0; j--)
            {
                if (indxr[j] != indxc[j])
                {
                    for (int k = 0; k < 4; k++)
                        MathUtility.Swap(ref minv[k, indxr[j]], ref minv[k, indxc[j]]);
                }
            }
            return new Matrix4x4(minv);
        }

        public static Matrix4x4 Mul(Matrix4x4 m1, Matrix4x4 m2)
        {
            var r = new Matrix4x4();
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    r.M[i,j] =
                        m1.M[i,0] * m2.M[0,j] +
                        m1.M[i,1] * m2.M[1,j] +
                        m1.M[i,2] * m2.M[2,j] +
                        m1.M[i,3] * m2.M[3,j];
            return r;
        }

        public static bool SolveLinearSystem2x2(float[,] A, float[] B, out float x0, out float x1)
        {
            float det = A[0, 0] * A[1, 1] - A[0, 1] * A[1, 0];
            if (Math.Abs(det) < 1e-10f)
            {
                x0 = x1 = float.NaN;
                return false;
            }
            x0 = (A[1, 1] * B[0] - A[0, 1] * B[1]) / det;
            x1 = (A[0, 0] * B[1] - A[1, 0] * B[0]) / det;
            if (float.IsNaN(x0) || float.IsNaN(x1))
                return false;
            return true;
        }

        #endregion
    }
}