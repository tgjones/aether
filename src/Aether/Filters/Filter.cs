namespace Aether.Filters
{
    public abstract class Filter
    {
        private readonly float _xWidth;
        private readonly float _yWidth;
        private readonly float _inverseXWidth;
        private readonly float _inverseYWidth;

        protected Filter(float xWidth, float yWidth)
        {
            _xWidth = xWidth;
            _yWidth = yWidth;
            _inverseXWidth = 1.0f / xWidth;
            _inverseYWidth = 1.0f / yWidth;
        }

        public float XWidth
        {
            get { return _xWidth; }
        }

        public float YWidth
        {
            get { return _yWidth; }
        }

        public float InverseXWidth
        {
            get { return _inverseXWidth; }
        }

        public float InverseYWidth
        {
            get { return _inverseYWidth; }
        }

        public abstract float Evaluate(float x, float y);
    }
}