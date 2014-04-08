using System.Collections.Generic;
using System.Linq;
using Aether.Geometry;

namespace Aether.Volumes
{
    public class AggregateVolume : VolumeRegion
    {
        private readonly List<VolumeRegion> _regions;
        private readonly BBox _worldBound;

        public AggregateVolume(IEnumerable<VolumeRegion> regions)
        {
            _regions = regions.ToList();
            _worldBound = BBox.Union(_regions.Select(x => x.WorldBound));
        }

        public override BBox WorldBound
        {
            get { return _worldBound; }
        }
    }
}