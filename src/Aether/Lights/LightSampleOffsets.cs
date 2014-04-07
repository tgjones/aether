using Aether.Sampling;

namespace Aether.Lights
{
    public struct LightSampleOffsets
    {
        public int NumSamples;
        public int ComponentOffset;
        public int PosOffset;

        public LightSampleOffsets(int numSamples, Sample sample)
        {
            NumSamples = numSamples;
            ComponentOffset = sample.Add1D(numSamples);
            PosOffset = sample.Add2D(numSamples);
        }
    }
}