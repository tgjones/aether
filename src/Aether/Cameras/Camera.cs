using Aether.Films;
using Aether.Geometry;
using Aether.Sampling;

namespace Aether.Cameras
{
    public abstract class Camera
    {
        private readonly AnimatedTransform _cameraToWorld;
        private readonly float _shutterOpen;
        private readonly float _shutterClose;
        private readonly Film _film;

        protected Camera(AnimatedTransform cameraToWorld,
            float shutterOpen, float shutterClose,
            Film film)
        {
            _cameraToWorld = cameraToWorld;
            _shutterOpen = shutterOpen;
            _shutterClose = shutterClose;
            _film = film;
        }

        public float ShutterOpen
        {
            get { return _shutterOpen; }
        }

        public float ShutterClose
        {
            get { return _shutterClose; }
        }

        protected AnimatedTransform CameraToWorld
        {
            get { return _cameraToWorld; }
        }

        public Film Film
        {
            get { return _film; }
        }

        public abstract float GenerateRay(CameraSample sample, out Ray ray);

        public virtual float GenerateRayDifferential(CameraSample sample, out RayDifferential rd)
        {
            Ray ray;
            float wt = GenerateRay(sample, out ray);
            rd = RayDifferential.FromRay(ray);

            // Find ray after shifting one pixel in the $x$ direction
            CameraSample sshift = sample;
            ++(sshift.ImageX);
            Ray rx;
            float wtx = GenerateRay(sshift, out rx);
            rd.RxOrigin = rx.Origin;
            rd.RxDirection = rx.Direction;

            // Find ray after shifting one pixel in the $y$ direction
            --(sshift.ImageX);
            ++(sshift.ImageY);
            Ray ry;
            float wty = GenerateRay(sshift, out ry);
            rd.RyOrigin = ry.Origin;
            rd.RyDirection = ry.Direction;

            if (wtx == 0.0f || wty == 0.0f)
                return 0.0f;

            rd.HasDifferentials = true;
            return wt;
        }
    }
}