namespace Aether.Geometry
{
    public class Ray
    {
        public Ray(Point origin, Vector direction,
            float start, float end = float.PositiveInfinity,
            float time = 0.0f, int depth = 0)
        {
            Origin = origin;
            Direction = direction;
            MinT = start;
            MaxT = end;
            Time = time;
            Depth = depth;
        }

        public Ray(Point origin, Vector direction, Ray parent,
            float start, float end = float.PositiveInfinity)
        {
            Origin = origin;
            Direction = direction;
            MinT = start;
            MaxT = end;
            Time = parent.Time;
            Depth = parent.Depth + 1;
        }

        public Point Origin { get; set; }
        public Vector Direction { get; set; }
        public float MinT { get; set; }
        public float MaxT { get; set; }
        public float Time { get; set; }
        public int Depth { get; set; }

        public Point Evaluate(float t)
        {
            return Origin + Direction * t;
        }

        public Ray Clone()
        {
            return new Ray(Origin, Direction, MinT, MaxT, Time, Depth);
        }
    }
}