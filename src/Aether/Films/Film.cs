using System.Windows.Media.Imaging;
using Aether.Sampling;

namespace Aether.Films
{
    public abstract class Film
    {
        private readonly int _xResolution;
        private readonly int _yResolution;

        protected Film(int xResolution, int yResolution)
        {
            _xResolution = xResolution;
            _yResolution = yResolution;
        }

        public int XResolution
        {
            get { return _xResolution; }
        }

        public int YResolution
        {
            get { return _yResolution; }
        }

        public float AspectRatio
        {
            get { return XResolution / (float) YResolution; }
        }

        /// Allows the film to specify an extent for sampling that is different
        /// from the film resolution, in order to support sampling beyond the
        /// edges of the final image.
        public abstract FilmExtent SampleExtent { get; }

        /// Returns the range of pixels in the actual image.
        public abstract FilmExtent PixelExtent { get; }

        public abstract WriteableBitmap Bitmap { get; }

        /// Updates the stored image with a given sample and corresponding radiance.
        /// The selected reconstruction filter will be applied.
        public abstract void AddSample(CameraSample sample, Spectrum l);

        /// Updates the stored image. Splatted values are summed, rather than
        /// a weighted average as is the case with AddSample.
        public abstract void Splat(CameraSample sample, Spectrum l);

        /// Notifies the film that a region of pixels has recently been updated.
        public abstract void UpdateDisplay(int x0, int y0, int x1, int y1, float splatScale = 1.0f);

        /// Called when the film should do any processing necessary to create,
        /// and then display or store, the final image.
        public abstract void WriteImage(float splatScale = 1.0f);
    }
}