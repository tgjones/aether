using System;
using System.Diagnostics;
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
        public abstract Sampler GetSubSampler(int num, int count);

        public virtual bool ReportResults(Sample[] samples, RayDifferential[] rays,
            Spectrum[] ls, Intersection[] intersections, int count)
        {
            return true;
        }

        protected FilmExtent ComputeSubWindow(int num, int count)
        {
            // Determine how many tiles to use in each dimension, nx and ny.
            int dx = _xEnd - _xStart, dy = _yEnd - _yStart;
            int nx = count, ny = 1;
            while ((nx & 0x1) == 0 && 2 * dx * ny < dy * nx)
            {
                nx >>= 1;
                ny <<= 1;
            }
            Debug.Assert(nx * ny == count);

            // Compute $x$ and $y$ pixel sample range for sub-window
            int xo = num % nx, yo = num / nx;
            float tx0 = xo / (float) nx, tx1 = (xo + 1) / (float) nx;
            float ty0 = yo / (float) ny, ty1 = (yo + 1) / (float) ny;
            return new FilmExtent
            {
                XStart = MathUtility.Floor(MathUtility.Lerp(tx0, _xStart, _xEnd)),
                XEnd = MathUtility.Floor(MathUtility.Lerp(tx1, _xStart, _xEnd)),
                YStart = MathUtility.Floor(MathUtility.Lerp(ty0, _yStart, _yEnd)),
                YEnd = MathUtility.Floor(MathUtility.Lerp(ty1, _yStart, _yEnd))
            };
        }
    }
}