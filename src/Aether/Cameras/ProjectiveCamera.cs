using Aether.Films;
using Aether.Geometry;

namespace Aether.Cameras
{
    public abstract class ProjectiveCamera : Camera
    {
        private readonly Transform _cameraToScreen;
        private readonly float _lensRadius;
        private readonly float _focalDistance;

        private readonly Transform _screenToRaster;
        private readonly Transform _rasterToScreen;
        private readonly Transform _rasterToCamera;

        protected ProjectiveCamera(AnimatedTransform cameraToWorld, Transform cameraToScreen,
            float[] screenWindow, float shutterOpen, float shutterClose,
            float lensRadius, float focalDistance, Film film)
            : base(cameraToWorld, shutterOpen, shutterClose, film)
        {
            _cameraToScreen = cameraToScreen;
            _lensRadius = lensRadius;
            _focalDistance = focalDistance;

            // Compute projective camera screen transformations
            _screenToRaster = Transform.Scale(film.XResolution, film.YResolution, 1.0f)
                * Transform.Scale(1.0f / (screenWindow[1] - screenWindow[0]), 1.0f / (screenWindow[2] - screenWindow[3]), 1.0f)
                * Transform.Translate(new Vector(-screenWindow[0], -screenWindow[3], 0.0f));
            _rasterToScreen = Transform.Invert(_screenToRaster);
            _rasterToCamera = Transform.Invert(_cameraToScreen) * _rasterToScreen;
        }

        protected float LensRadius
        {
            get { return _lensRadius; }
        }

        protected float FocalDistance
        {
            get { return _focalDistance; }
        }

        protected Transform RasterToCamera
        {
            get { return _rasterToCamera; }
        }
    }
}