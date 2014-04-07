using Aether.Cameras;
using Aether.Renderers;
using Aether.Sampling;

namespace Aether.Integrators
{
    public abstract class Integrator
    {
        public virtual void PreProcess(Scene scene, Camera camera, Renderer renderer)
        {
            
        }

        public virtual void RequestSamples(Sampler sampler, Sample sample, Scene scene)
        {
            
        }
    }
}