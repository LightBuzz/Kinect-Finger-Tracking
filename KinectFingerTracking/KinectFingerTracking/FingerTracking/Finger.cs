using Microsoft.Kinect;

namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// Represents a finger tip.
    /// </summary>
    public class Finger
    {
        /// <summary>
        /// The position of the fingertip in the 3D Camera space.
        /// </summary>
        public CameraSpacePoint CameraPoint { get; set; }

        /// <summary>
        /// The position of the fingertip in the 2D Depth space.
        /// </summary>
        public DepthSpacePoint DepthPoint { get; set; }

        /// <summary>
        /// The position of the fingertip in the 2D Color space.
        /// </summary>
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
