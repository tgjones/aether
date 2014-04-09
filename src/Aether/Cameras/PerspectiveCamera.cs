using Aether.Films;
using Aether.Geometry;
using Aether.MonteCarlo;
using Aether.Sampling;

namespace Aether.Cameras
{
    public class PerspectiveCamera : ProjectiveCamera
    {
        private readonly Vector _dxCamera;
        private readonly Vector _dyCamera;

        public PerspectiveCamera(
            AnimatedTransform cameraToWorld, float[] screenWindow, 
            float shutterOpen, float shutterClose, 
            float lensRadius, float focalDistance, 
            float fieldOfView,
            Film film)
            : base(cameraToWorld, Transform.Perspective(fieldOfView, 1e-2f, 1000.0f), screenWindow, shutterOpen, shutterClose, lensRadius, focalDistance, film)
        {
            // Compute differential changes in origin for perspective camera rays
            _dxCamera = RasterToCamera.TransformPoint(new Point(1, 0, 0)) - RasterToCamera.TransformPoint(new Point(0, 0, 0));
            _dyCamera = RasterToCamera.TransformPoint(new Point(0, 1, 0)) - RasterToCamera.TransformPoint(new Point(0, 0, 0));
        }

        public override float GenerateRay(CameraSample sample, out Ray ray)
        {
            // Generate raster and camera samples
            Point Pras = new Point(sample.ImageX, sample.ImageY, 0);
            Point Pcamera = RasterToCamera.TransformPoint(ref Pras);
            ray = new Ray(new Point(0, 0, 0), Vector.Normalize((Vector) Pcamera), 0.0f);
            // Modify ray for depth of field
            if (LensRadius > 0.0f)
            {
                // Sample point on lens
                float lensU, lensV;
                MonteCarloUtilities.ConcentricSampleDisk(sample.LensU, sample.LensV, out lensU, out lensV);
                lensU *= LensRadius;
                lensV *= LensRadius;

                // Compute point on plane of focus
                float ft = FocalDistance / ray.Direction.Z;
                Point Pfocus = ray.Evaluate(ft);

                // Update ray for effect of lens
                ray.Origin = new Point(lensU, lensV, 0.0f);
                ray.Direction = Vector.Normalize(Pfocus - ray.Origin);
            }
            ray.Time = sample.Time;
            ray = CameraToWorld.TransformRay(ray);
            return 1.0f;
        }

        public override float GenerateRayDifferential(CameraSample sample, out RayDifferential ray)
        {
            // Generate raster and camera samples
            Point Pras = new Point(sample.ImageX, sample.ImageY, 0);
            Point Pcamera = RasterToCamera.TransformPoint(ref Pras);
            ray = new RayDifferential(new Point(0, 0, 0), Vector.Normalize((Vector) Pcamera), 0.0f);
            // Modify ray for depth of field
            if (LensRadius > 0.0f)
            {
                // Sample point on lens
                float lensU, lensV;
                MonteCarloUtilities.ConcentricSampleDisk(sample.LensU, sample.LensV, out lensU, out lensV);
                lensU *= LensRadius;
                lensV *= LensRadius;

                // Compute point on plane of focus
                float ft = FocalDistance / ray.Direction.Z;
                Point Pfocus = ray.Evaluate(ft);

                // Update ray for effect of lens
                ray.Origin = new Point(lensU, lensV, 0.0f);
                ray.Direction = Vector.Normalize(Pfocus - ray.Origin);
            }
            // Compute offset rays for _PerspectiveCamera_ ray differentials
            ray.RxOrigin = ray.RyOrigin = ray.Origin;
            ray.RxDirection = Vector.Normalize((Vector) Pcamera + _dxCamera);
            ray.RyDirection = Vector.Normalize((Vector) Pcamera + _dyCamera);
            ray.Time = sample.Time;
            ray = CameraToWorld.TransformRayDifferential(ray);
            return 1.0f;
        }
    }
}