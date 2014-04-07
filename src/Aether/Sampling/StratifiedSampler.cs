using System;
using System.Collections.Generic;
using Aether.Films;
using Aether.MonteCarlo;

namespace Aether.Sampling
{
    public class StratifiedSampler : Sampler
    {
        private readonly int _xPixelSamples;
        private readonly int _yPixelSamples;
        private readonly bool _jitter;
        private int _xPos, _yPos;
        private readonly float[] _sampleBuffer;

        public StratifiedSampler(
            FilmExtent sampleExtent, 
            int xPixelSamples, int yPixelSamples, bool jitter,
            float shutterOpen, float shutterClose) 
            : base(sampleExtent, xPixelSamples * yPixelSamples, shutterOpen, shutterClose)
        {
            _xPixelSamples = xPixelSamples;
            _yPixelSamples = yPixelSamples;
            _jitter = jitter;
            _xPos = sampleExtent.XStart;
            _yPos = sampleExtent.YStart;
            _sampleBuffer = new float[5 * xPixelSamples * yPixelSamples];
        }

        public override int MaximumSampleCount
        {
            get { return _xPixelSamples * _yPixelSamples; }
        }

        public override int RoundSize(int size)
        {
            return size;
        }

        public override int GetMoreSamples(Sample[] samples, Random rng)
        {
            if (_yPos == YEnd) return 0;
            int nSamples = _xPixelSamples * _yPixelSamples;
            // Generate stratified camera samples for _(xPos, yPos)_

            // Generate initial stratified samples into _sampleBuf_ memory
            IList<float> imageSamples = new ArraySegment<float>(_sampleBuffer, 0, 2 * nSamples);
            IList<float> lensSamples = new ArraySegment<float>(_sampleBuffer, 2 * nSamples, 2 * nSamples);
            IList<float> timeSamples = new ArraySegment<float>(_sampleBuffer, 4 * nSamples, nSamples);
            SamplingUtilities.StratifiedSample2D(imageSamples, _xPixelSamples, _yPixelSamples, rng, _jitter);
            SamplingUtilities.StratifiedSample2D(lensSamples, _xPixelSamples, _yPixelSamples, rng, _jitter);
            SamplingUtilities.StratifiedSample1D(timeSamples, _xPixelSamples * _yPixelSamples, rng, _jitter);

            // Shift stratified image samples to pixel coordinates
            for (int o = 0; o < 2 * _xPixelSamples * _yPixelSamples; o += 2)
            {
                imageSamples[o] += _xPos;
                imageSamples[o + 1] += _yPos;
            }

            // Decorrelate sample dimensions
            SamplingUtilities.Shuffle(lensSamples, _xPixelSamples * _yPixelSamples, 2, rng);
            SamplingUtilities.Shuffle(timeSamples, _xPixelSamples * _yPixelSamples, 1, rng);

            // Initialize stratified _samples_ with sample values
            for (int i = 0; i < nSamples; ++i)
            {
                samples[i].ImageX = imageSamples[2 * i];
                samples[i].ImageY = imageSamples[2 * i + 1];
                samples[i].LensU = lensSamples[2 * i];
                samples[i].LensV = lensSamples[2 * i + 1];
                samples[i].Time = MathUtility.Lerp(timeSamples[i], ShutterOpen, ShutterClose);
                // Generate stratified samples for integrators
                for (var j = 0; j < samples[i].Num1D.Count; ++j)
                    MonteCarloUtilities.LatinHypercube(samples[i].OneD[j], samples[i].Num1D[j], 1, rng);
                for (var j = 0; j < samples[i].Num2D.Count; ++j)
                    MonteCarloUtilities.LatinHypercube(samples[i].TwoD[j], samples[i].Num2D[j], 2, rng);
            }

            // Advance to next pixel for stratified sampling
            if (++_xPos == XEnd)
            {
                _xPos = XStart;
                ++_yPos;
            }
            return nSamples;
        }
    }
}