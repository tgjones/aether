using System;
using Aether.Geometry;
using Aether.Reflection;
using Aether.Shapes;
using Aether.Textures;

namespace Aether.Materials
{
    public abstract class Material
    {
        public abstract Bsdf GetBsdf(DifferentialGeometry dgGeom, DifferentialGeometry dgShading);

        public virtual Bssrdf GetBssrdf(DifferentialGeometry dgGeom, DifferentialGeometry dgShading)
        {
            return null;
        }

        protected static DifferentialGeometry Bump(Texture<float> d,
            DifferentialGeometry dgGeom,
            DifferentialGeometry dgs)
        {
            // Compute offset positions and evaluate displacement texture
            DifferentialGeometry dgEval = dgs;

            // Shift _dgEval_ _du_ in the $u$ direction
            float du = .5f * (Math.Abs(dgs.DuDx) + Math.Abs(dgs.DuDy));
            if (du == 0.0f)
                du = .01f;
            dgEval.Point = dgs.Point + du * dgs.DpDu;
            dgEval.U = dgs.U + du;
            dgEval.Normal = Normal.Normalize((Normal) Vector.Cross(dgs.DpDu, dgs.DpDv) +
                du * dgs.DnDu);
            float uDisplace = d.Evaluate(dgEval);

            // Shift _dgEval_ _dv_ in the $v$ direction
            float dv = .5f * (Math.Abs(dgs.DvDx) + Math.Abs(dgs.DvDy));
            if (dv == 0.0f)
                dv = .01f;
            dgEval.Point = dgs.Point + dv * dgs.DpDv;
            dgEval.U = dgs.U;
            dgEval.V = dgs.V + dv;
            dgEval.Normal = Normal.Normalize((Normal) Vector.Cross(dgs.DpDu, dgs.DpDv) + dv * dgs.DnDv);
            float vDisplace = d.Evaluate(dgEval);
            float displace = d.Evaluate(dgs);

            // Compute bump-mapped differential geometry
            var dgBump = dgs.Clone();
            dgBump.DpDu = dgs.DpDu + (uDisplace - displace) / du * (Vector)dgs.Normal +
                displace * (Vector)dgs.DnDu;
            dgBump.DpDv = dgs.DpDv + (vDisplace - displace) / dv * (Vector)dgs.Normal +
                displace * (Vector)dgs.DnDv;
            dgBump.Normal = (Normal) Vector.Normalize(Vector.Cross(dgBump.DpDu, dgBump.DpDv));
            if (dgs.Shape.ReverseOrientation ^ dgs.Shape.TransformSwapsHandedness)
                dgBump.Normal *= -1.0f;

            // Orient shading normal to match geometric normal
            dgBump.Normal = Normal.FaceForward(dgBump.Normal, dgGeom.Normal);

            return dgBump;
        }
    }
}