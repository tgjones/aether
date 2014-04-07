using Aether.Sampling;

namespace Aether.Reflection
{
    public struct BsdfSampleOffsets
    {
        public int NumSamples;
        public int ComponentOffset;
        public int PosOffset;

        public BsdfSampleOffsets(int numSamples, Sample sample)
        {
            NumSamples = numSamples;
            ComponentOffset = sample.Add1D(numSamples);
            PosOffset = sample.Add2D(numSamples);
        }
    }
}