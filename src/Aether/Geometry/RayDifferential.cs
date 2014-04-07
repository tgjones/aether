namespace Aether.Geometry
{
    public class RayDifferential : Ray
    {
        public bool HasDifferentials { get; set; }
        public Point RxOrigin { get; set; }
        public Point RyOrigin { get; set; }
        public Vector RxDirection { get; set; }
        public Vector RyDirection { get; set; }

        public RayDifferential(Point origin, Vector direction,
            float start, float end = float.PositiveInfinity,
            float time = 0.0f, int depth = 0)
            : base(origin, direction, start, end, time, depth)
        {
            HasDifferentials = false;
        }

        public RayDifferential(Point origin, Vector direction, Ray parent,
            float start, float end = float.PositiveInfinity)
            : base(origin, direction, parent, start, end)
        {
            HasDifferentials = false;
        }

        public void ScaleDifferentials(float s)
        {
            RxOrigin = Origin + (RxOrigin - Origin) * s;
            RyOrigin = Origin + (RyOrigin - Origin) * s;
            RxDirection = Direction + (RxDirection - Direction) * s;
            RyDirection = Direction + (RyDirection - Direction) * s;
        }

        public new RayDifferential Clone()
        {
            var result = new RayDifferential(Origin, Direction, MinT, MaxT, Time, Depth);
            result.HasDifferentials = HasDifferentials;
            result.RxOrigin = RxOrigin;
            result.RyOrigin = RyOrigin;
            result.RxDirection = RxDirection;
            result.RyDirection = RyDirection;
            return result;
        }

        public static RayDifferential FromRay(Ray ray)
        {
            return new RayDifferential(ray.Origin, ray.Direction, 
                ray.MinT, ray.MaxT, ray.Time, ray.Depth);
        }
    }
}