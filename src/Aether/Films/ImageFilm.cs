using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aether.Filters;
using Aether.Sampling;

namespace Aether.Films
{
    public class ImageFilm : Film
    {
        private const int FilterTableSize = 16;

        private readonly WriteableBitmap _bitmap;
        private readonly Filter _filter;
        private readonly float[] _cropWindow;
        private readonly int _xPixelStart;
        private readonly int _xPixelCount;
        private readonly int _yPixelStart;
        private readonly int _yPixelCount;
        private readonly ImagePixel[,] _pixels;
        private readonly float[] _filterTable;
        private readonly byte[] _bgra;

        public ImageFilm(int xResolution, int yResolution, Filter filter, float[] cropWindow)
            : base(xResolution, yResolution)
        {
            _filter = filter;
            _cropWindow = cropWindow;

            _bitmap = new WriteableBitmap(xResolution, yResolution, 96.0, 96.0, PixelFormats.Pbgra32, null);

            // Compute film image extent
            _xPixelStart = MathUtility.Ceiling(xResolution * cropWindow[0]);
            _xPixelCount = Math.Max(1, MathUtility.Ceiling(xResolution * cropWindow[1]) - _xPixelStart);
            _yPixelStart = MathUtility.Ceiling(yResolution * cropWindow[2]);
            _yPixelCount = Math.Max(1, MathUtility.Ceiling(yResolution * cropWindow[3]) - _yPixelStart);

            // Allocate film image storage
            _pixels = new ImagePixel[_xPixelCount, _yPixelCount];
            for (var x = 0; x < _xPixelCount; x++)
                for (var y = 0; y < _yPixelCount; y++)
                    _pixels[x, y] = new ImagePixel();

            // Precompute filter weight table
            _filterTable = new float[FilterTableSize * FilterTableSize];
            var index = 0;
            for (int y = 0; y < FilterTableSize; ++y)
            {
                float fy = (y + .5f) * filter.YWidth / FilterTableSize;
                for (int x = 0; x < FilterTableSize; ++x)
                {
                    float fx = (x + .5f) * filter.XWidth / FilterTableSize;
                    _filterTable[index++] = filter.Evaluate(fx, fy);
                }
            }

            _bgra = new byte[_xPixelCount * _yPixelCount * 4];
        }

        public override WriteableBitmap Bitmap
        {
            get { return _bitmap; }
        }

        public override FilmExtent SampleExtent
        {
            get
            {
                return new FilmExtent
                {
                    XStart = MathUtility.Floor(_xPixelStart + 0.5f - _filter.XWidth),
                    XEnd = MathUtility.Ceiling(_xPixelStart + 0.5f + _xPixelCount + _filter.XWidth),
                    YStart = MathUtility.Floor(_yPixelStart + 0.5f - _filter.YWidth),
                    YEnd = MathUtility.Ceiling(_yPixelStart + 0.5f + _yPixelCount + _filter.YWidth)
                };
            }
        }

        public override FilmExtent PixelExtent
        {
            get
            {
                return new FilmExtent
                {
                    XStart = _xPixelStart,
                    XEnd = _xPixelStart + _xPixelCount,
                    YStart = _yPixelStart,
                    YEnd = _yPixelStart + _yPixelCount
                };
            }
        }

        public override void AddSample(CameraSample sample, Spectrum l)
        {
            // Compute sample's raster extent
            float dimageX = sample.ImageX - 0.5f;
            float dimageY = sample.ImageY - 0.5f;
            int x0 = MathUtility.Ceiling(dimageX - _filter.XWidth);
            int x1 = MathUtility.Floor(dimageX + _filter.XWidth);
            int y0 = MathUtility.Ceiling(dimageY - _filter.YWidth);
            int y1 = MathUtility.Floor(dimageY + _filter.YWidth);
            x0 = Math.Max(x0, _xPixelStart);
            x1 = Math.Min(x1, _xPixelStart + _xPixelCount - 1);
            y0 = Math.Max(y0, _yPixelStart);
            y1 = Math.Min(y1, _yPixelStart + _yPixelCount - 1);
            if ((x1 - x0) < 0 || (y1 - y0) < 0)
                return;

            // Loop over filter support and add sample to pixel arrays
            var xyz = l.ToXyz();

            // Precompute $x$ and $y$ filter table offsets
            var ifx = new int[x1 - x0 + 1];
            for (int x = x0; x <= x1; ++x)
            {
                float fx = Math.Abs((x - dimageX) * _filter.InverseXWidth * FilterTableSize);
                ifx[x - x0] = Math.Min(MathUtility.Floor(fx), FilterTableSize - 1);
            }
            var ify = new int[y1 - y0 + 1];
            for (int y = y0; y <= y1; ++y)
            {
                float fy = Math.Abs((y - dimageY) * _filter.InverseYWidth * FilterTableSize);
                ify[y - y0] = Math.Min(MathUtility.Floor(fy), FilterTableSize - 1);
            }
            bool syncNeeded = (_filter.XWidth > 0.5f || _filter.YWidth > 0.5f);
            for (int y = y0; y <= y1; ++y)
            {
                for (int x = x0; x <= x1; ++x)
                {
                    // Evaluate filter value at $(x,y)$ pixel
                    int offset = ify[y - y0] * FilterTableSize + ifx[x - x0];
                    float filterWt = _filterTable[offset];

                    // Update pixel values with filtered sample contribution
                    var pixel = _pixels[x - _xPixelStart, y - _yPixelStart];
                    if (!syncNeeded)
                    {
                        pixel.Lxyz[0] += filterWt * xyz[0];
                        pixel.Lxyz[1] += filterWt * xyz[1];
                        pixel.Lxyz[2] += filterWt * xyz[2];
                        pixel.WeightSum += filterWt;
                    }
                    else
                    {
                        // Safely update _Lxyz_ and _weightSum_ even with concurrency
                        AtomicAdd(ref pixel.Lxyz[0], filterWt * xyz[0]);
                        AtomicAdd(ref pixel.Lxyz[1], filterWt * xyz[1]);
                        AtomicAdd(ref pixel.Lxyz[2], filterWt * xyz[2]);
                        AtomicAdd(ref pixel.WeightSum, filterWt);
                    }
                }
            }
        }

        public override void Splat(CameraSample sample, Spectrum l)
        {
            var xyz = l.ToXyz();
            int x = MathUtility.Floor(sample.ImageX), y = MathUtility.Floor(sample.ImageY);
            if (x < _xPixelStart || x - _xPixelStart >= _xPixelCount ||
                y < _yPixelStart || y - _yPixelStart >= _yPixelCount) return;
            var pixel = _pixels[x - _xPixelStart, y - _yPixelStart];
            AtomicAdd(ref pixel.SplatXyz[0], xyz[0]);
            AtomicAdd(ref pixel.SplatXyz[1], xyz[1]);
            AtomicAdd(ref pixel.SplatXyz[2], xyz[2]);
        }

        public override void WriteImage(float splatScale = 1)
        {
            UpdateDisplay(0, 0, _xPixelCount, _yPixelCount, splatScale);
        }

        private static byte ConvertToByte(float[] rgb, int index)
        {
            return (byte) MathUtility.Clamp(MathUtility.Pow(rgb[index], 1.0f / 1.8f) * 255.0f, 0, 255.0f);
        }

        public override void UpdateDisplay(int x0, int y0, int x1, int y1, float splatScale = 1)
        {
            x0 -= _xPixelStart;
            x1 -= _xPixelStart;
            y0 -= _yPixelStart;
            y1 -= _yPixelStart;
            x0 = MathUtility.Clamp(x0, 0, _xPixelCount);
            x1 = MathUtility.Clamp(x1, 0, _xPixelCount);
            y0 = MathUtility.Clamp(y0, 0, _xPixelCount);
            y1 = MathUtility.Clamp(y1, 0, _xPixelCount);

            // Convert image to RGB and compute final pixel values
            var rgb = new float[3];
            for (int y = y0; y < y1; ++y)
            {
                for (int x = x0; x < x1; ++x)
                {
                    var pixel = _pixels[x, y];

                    // Convert pixel XYZ color to RGB
                    Spectrum.XyzToRgb(pixel.Lxyz, rgb);

                    // Normalize pixel with weight sum
                    float weightSum = pixel.WeightSum;
                    if (weightSum != 0.0f)
                    {
                        float invWt = 1.0f / weightSum;
                        rgb[0] = Math.Max(0.0f, rgb[0] * invWt);
                        rgb[1] = Math.Max(0.0f, rgb[1] * invWt);
                        rgb[2] = Math.Max(0.0f, rgb[2] * invWt);
                    }

                    // Add splat value at pixel
                    var splatRgb = new float[3];
                    Spectrum.XyzToRgb(pixel.SplatXyz, splatRgb);
                    rgb[0] += splatScale * splatRgb[0];
                    rgb[1] += splatScale * splatRgb[1];
                    rgb[2] += splatScale * splatRgb[2];

                    var offset = ((y * _xPixelCount) + x) * 4;
                    _bgra[offset + 0] = ConvertToByte(rgb, 2); // B
                    _bgra[offset + 1] = ConvertToByte(rgb, 1); // G
                    _bgra[offset + 2] = ConvertToByte(rgb, 0); // R
                    _bgra[offset + 3] = 255; // A
                }
            }

            _bitmap.Dispatcher.Invoke(() =>
            {
                var stride = _bitmap.PixelWidth * _bitmap.Format.BitsPerPixel / 8;
                _bitmap.WritePixels(
                    new Int32Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1),
                    _bgra, stride, x0, y0);
            });
        }

        private static void AtomicAdd(ref float value, float delta)
        {
            // TODO: This isn't atomic.
            value += delta;
        }
    }
}