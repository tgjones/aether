namespace Aether.Films
{
    public class ImagePixel
    {
        public ImagePixel()
        {
            Lxyz = new float[3];
            SplatXyz = new float[3];
        }

        public float[] Lxyz;
        public float WeightSum;
        public float[] SplatXyz;
    }
}