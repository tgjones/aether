using System;
using Aether.Films;
using Aether.Geometry;

namespace Aether.Sampling
{
    public abstract class Sampler
    {
        private readonly int _xStart;
        private readonly int _xEnd;
        private readonly int _yStart;
        private readonly int _yEnd;
        private readonly int _samplesPerPixel;
        private readonly float _shutterOpen;
        private readonly float _shutterClose;

        protected Sampler(FilmExtent sampleExtent,
            int samplesPerPixel, float shutterOpen, float shutterClose)
        {
            _xStart = sampleExtent.XStart;
            _xEnd = sampleExtent.XEnd;
            _yStart = sampleExtent.YStart;
            _yEnd = sampleExtent.YEnd;
            _samplesPerPixel = samplesPerPixel;
            _shutterOpen = shutterOpen;
            _shutterClose = shutterClose;
        }

        public int SamplesPerPixel
        {
            get { return _samplesPerPixel; }
        }

        public int XStart
        {
            get { return _xStart; }
        }

        public int XEnd
        {
            get { return _xEnd; }
        }

        public int YStart
        {
            get { return _yStart; }
        }

        public int YEnd
        {
            get { return _yEnd; }
        }

        public float ShutterOpen
        {
            get { return _shutterOpen; }
        }

        public float ShutterClose
        {
            get { return _shutterClose; }
        }

        public abstract int MaximumSampleCount { get; }

        public abstract int GetMoreSamples(Sample[] samples, Random rng);
        public abstract int RoundSize(int size);

        public virtual bool ReportResults(Sample[] samples, RayDifferential[] rays,
            Spectrum[] ls, Intersection[] intersections, int count)
        {
            return true;
        }
    }
}