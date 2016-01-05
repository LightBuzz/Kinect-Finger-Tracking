using Microsoft.Kinect;

namespace LightBuzz.Vitruvius.FingerTracking
{
    public class Finger
    {
        public CameraSpacePoint CameraPoint { get; set; }

        public DepthSpacePoint DepthPoint { get; set; }

        public ColorSpacePoint ColorPoint { get; set; }

        internal Finger(DepthPointEx point, CoordinateMapper coordinateMapper)
        {
            ushort depth = (ushort)point.Z;

            DepthPoint = new DepthSpacePoint
            {
                X = point.X,
                Y = point.Y
            };

            ColorPoint = coordinateMapper.MapDepthPointToColorSpace(DepthPoint, (ushort)point.Z);

            CameraPoint = coordinateMapper.MapDepthPointToCameraSpace(DepthPoint, (ushort)point.Z);
        }
    }
}
