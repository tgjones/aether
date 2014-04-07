using System.Collections.Generic;
using Aether.Geometry;
using Aether.Lights;
using Aether.Primitives;
using Aether.Volumes;

namespace Aether
{
    public class Scene
    {
        private readonly Primitive _aggregate;
        private readonly IEnumerable<Light> _lights;
        private readonly VolumeRegion _volumeRegion;
        private readonly BBox _worldBound;

        public Scene(Primitive aggregate, IEnumerable<Light> lights, VolumeRegion volumeRegion)
        {
            _aggregate = aggregate;
            _lights = lights;
            _volumeRegion = volumeRegion;

            _worldBound = aggregate.WorldBound;
            if (volumeRegion != null)
                _worldBound = BBox.Union(_worldBound, volumeRegion.WorldBound);
        }

        public IEnumerable<Light> Lights
        {
            get { return _lights; }
        }

        public BBox WorldBound
        {
            get { return _worldBound; }
        }

        public bool TryIntersect(Ray ray, out Intersection intersection)
        {
            return _aggregate.TryIntersect(ray, out intersection);
        }

        public bool Intersects(Ray ray)
        {
            return _aggregate.Intersects(ray);
        }
    }
}