using System.Collections.Generic;
using System.Linq;
using Aether.Integrators;

namespace Aether.Sampling
{
    public class Sample : CameraSample
    {
        public List<int> Num1D { get; private set; }
        public List<int> Num2D { get; private set; }
        public float[][] OneD { get; private set; }
        public float[][] TwoD { get; private set; }

        public Sample(Sampler sampler, SurfaceIntegrator surfaceIntegrator,
            VolumeIntegrator volumeIntegrator, Scene scene)
        {
            Num1D = new List<int>();
            Num2D = new List<int>();

            if (surfaceIntegrator != null)
                surfaceIntegrator.RequestSamples(sampler, this, scene);
            if (volumeIntegrator != null)
                volumeIntegrator.RequestSamples(sampler, this, scene);
            AllocateSampleMemory();
        }

        private Sample()
        {
            
        }

        public int Add1D(int num)
        {
            Num1D.Add(num);
            return Num1D.Count - 1;
        }

        public int Add2D(int num)
        {
            Num2D.Add(num);
            return Num2D.Count - 1;
        }

        public Sample[] Duplicate(int count)
        {
            var ret = new Sample[count];
            for (var i = 0; i < count; ++i)
            {
                ret[i] = new Sample
                {
                    Num1D = Num1D.Select(x => x).ToList(),
                    Num2D = Num2D.Select(x => x).ToList()
                };
                ret[i].AllocateSampleMemory();
            }
            return ret;
        }

        private void AllocateSampleMemory()
        {
            OneD = new float[Num1D.Count][];
            for (var i = 0; i < Num1D.Count; i++)
                OneD[i] = new float[Num1D[i]];

            TwoD = new float[Num2D.Count][];
            for (var i = 0; i < Num2D.Count; i++)
                TwoD[i] = new float[Num2D[i]];
        }
    }
}