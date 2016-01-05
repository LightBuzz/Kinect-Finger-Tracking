using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// Detects human hands in the 3D and 2D space.
    /// </summary>
    public class HandsController
    {
        private readonly int DEFAULT_DEPTH_WIDTH = 512;
        private readonly int DEFAULT_DEPTH_HEIGHT = 424;
        private readonly ushort MIN_DEPTH = 500;
        private readonly ushort MAX_DEPTH = ushort.MaxValue;
        private readonly float DEPTH_THRESHOLD = 80; // 8cm

        private GrahamScan _grahamScan = new GrahamScan();
        private PointFilter _lineThinner = new PointFilter();

        private byte[] _handPixelsLeft = null;
        private byte[] _handPixelsRight = null;

        /// <summary>
        /// The width of the depth frame.
        /// </summary>
        public int DepthWidth { get; set; }

        /// <summary>
        /// the height of the depth frame.
        /// </summary>
        public int DepthHeight { get; set; }

        /// <summary>
        /// The coordinate mapper that will be used during the finger detection process.
        /// </summary>
        public CoordinateMapper CoordinateMapper { get; set; }

        /// <summary>
        /// Determines whether the algorithm will detect the left hand.
        /// </summary>
        public bool DetectLeftHand { get; set; }

        /// <summary>
        /// Determines whether the algorithm will detect the right hand.
        /// </summary>
        public bool DetectRightHand { get; set; }

        /// <summary>
        /// Raised when a new pair of hands is detected.
        /// </summary>
        public event EventHandler<HandCollection> HandsDetected;

        /// <summary>
        /// Creates a new instance of <see cref="HandsController"/>.
        /// </summary>
        public HandsController()
        {
            CoordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
            DetectLeftHand = true;
            DetectRightHand = true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="HandsController"/> with the specified coordinate mapper.
        /// </summary>
        /// <param name="coordinateMapper">The coordinate mapper that will be used during the finger detection process.</param>
        public HandsController(CoordinateMapper coordinateMapper)
        {
            CoordinateMapper = coordinateMapper;
        }

        /// <summary>
        /// Updates the finger-detection engine with the new data.
        /// </summary>
        /// <param name="data">An array of depth values.</param>
        /// <param name="body">The body to search for hands and fingers.</param>
        public unsafe void Update(ushort[] data, Body body)
        {
            fixed (ushort* frameData = data)
            {
                Update(frameData, body);
            }
        }

        /// <summary>
        /// Updates the finger-detection engine with the new data.
        /// </summary>
        /// <param name="data">An IntPtr that describes depth values.</param>
        /// <param name="body">The body to search for hands and fingers.</param>
        public unsafe void Update(IntPtr data, Body body)
        {
            ushort* frameData = (ushort*)data;

            Update(frameData, body);
        }

        /// <summary>
        /// Updates the finger-detection engine with the new data.
        /// </summary>
        /// <param name="data">A pointer to an array of depth data.</param>
        /// <param name="body">The body to search for hands and fingers.</param>
        public unsafe void Update(ushort* data, Body body)
        {
            if (data == null || body == null) return;

            if (DepthWidth == 0)
            {
                DepthWidth = DEFAULT_DEPTH_WIDTH;
            }

            if (DepthHeight == 0)
            {
                DepthHeight = DEFAULT_DEPTH_HEIGHT;
            }

            if (_handPixelsLeft == null)
            {
                _handPixelsLeft = new byte[DepthWidth * DepthHeight];
            }

            if (_handPixelsRight == null)
            {
                _handPixelsRight = new byte[DepthWidth * DepthHeight];
            }

            Hand handLeft = null;
            Hand handRight = null;

            Joint jointHandLeft = body.Joints[JointType.HandLeft];
            Joint jointHandRight = body.Joints[JointType.HandRight];
            Joint jointWristLeft = body.Joints[JointType.WristLeft];
            Joint jointWristRight = body.Joints[JointType.WristRight];
            Joint jointTipLeft = body.Joints[JointType.HandTipLeft];
            Joint jointTipRight = body.Joints[JointType.HandTipRight];
            Joint jointThumbLeft = body.Joints[JointType.ThumbLeft];
            Joint jointThumbRight = body.Joints[JointType.ThumbRight];

            DepthSpacePoint depthPointHandLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointHandLeft.Position);
            DepthSpacePoint depthPointWristLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointWristLeft.Position);
            DepthSpacePoint depthPointTipLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointTipLeft.Position);
            DepthSpacePoint depthPointThumbLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointThumbLeft.Position);

            DepthSpacePoint depthPointHandRight = CoordinateMapper.MapCameraPointToDepthSpace(jointHandRight.Position);
            DepthSpacePoint depthPointWristRight = CoordinateMapper.MapCameraPointToDepthSpace(jointWristRight.Position);
            DepthSpacePoint depthPointTipRight = CoordinateMapper.MapCameraPointToDepthSpace(jointTipRight.Position);
            DepthSpacePoint depthPointThumbRight = CoordinateMapper.MapCameraPointToDepthSpace(jointThumbRight.Position);

            float handLeftX = depthPointHandLeft.X;
            float handLeftY = depthPointHandLeft.Y;
            float wristLeftX = depthPointWristLeft.X;
            float wristLeftY = depthPointWristLeft.Y;
            float tipLeftX = depthPointTipLeft.X;
            float tipLeftY = depthPointTipLeft.Y;
            float thumbLeftX = depthPointThumbLeft.X;
            float thumbLeftY = depthPointThumbLeft.Y;

            float handRightX = depthPointHandRight.X;
            float handRightY = depthPointHandRight.Y;
            float wristRightX = depthPointWristRight.X;
            float wristRightY = depthPointWristRight.Y;
            float tipRightX = depthPointTipRight.X;
            float tipRightY = depthPointTipRight.Y;
            float thumbRightX = depthPointThumbRight.X;
            float thumbRightY = depthPointThumbRight.Y;

            bool searchForLeftHand = DetectLeftHand && !float.IsInfinity(handLeftX) && !float.IsInfinity(handLeftY) && !float.IsInfinity(wristLeftX) && !float.IsInfinity(wristLeftY) && !float.IsInfinity(tipLeftX) && !float.IsInfinity(tipLeftY) && !float.IsInfinity(thumbLeftX) && !float.IsInfinity(thumbLeftY);
            bool searchForRightHand = DetectRightHand && !float.IsInfinity(handRightX) && !float.IsInfinity(handRightY) && !float.IsInfinity(wristRightX) && !float.IsInfinity(wristRightY) && !float.IsInfinity(tipRightX) && !float.IsInfinity(tipRightY) && !float.IsInfinity(thumbRightX) && !float.IsInfinity(thumbRightY);

            if (searchForLeftHand || searchForRightHand)
            {
                double distanceLeft = searchForLeftHand ? CalculateDistance(handLeftX, handLeftY, tipLeftX, tipLeftY, thumbLeftX, thumbLeftY) : 0.0;
                double distanceRight = searchForRightHand ? CalculateDistance(handRightX, handRightY, tipRightX, tipRightY, thumbRightX, thumbRightY) : 0.0;

                double angleLeft = searchForLeftHand ? DepthPointEx.Angle(wristLeftX, wristLeftY, wristLeftX, 0, handLeftX, handLeftY) : 0.0;
                double angleRight = searchForRightHand ? DepthPointEx.Angle(wristRightX, wristRightY, wristRightX, 0, handRightX, handRightY) : 0.0;

                int minLeftX = searchForLeftHand ? (int)(handLeftX - distanceLeft) : 0;
                int minLeftY = searchForLeftHand ? (int)(handLeftY - distanceLeft) : 0;
                int maxLeftX = searchForLeftHand ? (int)(handLeftX + distanceLeft) : 0;
                int maxLeftY = searchForLeftHand ? (int)(handLeftY + distanceLeft) : 0;

                int minRightX = searchForRightHand ? (int)(handRightX - distanceRight) : 0;
                int minRightY = searchForRightHand ? (int)(handRightY - distanceRight) : 0;
                int maxRightX = searchForRightHand ? (int)(handRightX + distanceRight) : 0;
                int maxRightY = searchForRightHand ? (int)(handRightY + distanceRight) : 0;

                float depthLeft = jointHandLeft.Position.Z * 1000; // m to mm
                float depthRight = jointHandRight.Position.Z * 1000;
                
                for (int i = 0; i < DepthWidth * DepthHeight; ++i)
                {
                    ushort depth = data[i];

                    int depthX = i % DepthWidth;
                    int depthY = i / DepthWidth;

                    bool isInBounds = depth >= MIN_DEPTH && depth <= MAX_DEPTH;

                    bool conditionLeft = depth >= depthLeft - DEPTH_THRESHOLD &&
                                         depth <= depthLeft + DEPTH_THRESHOLD &&
                                         depthX >= minLeftX && depthX <= maxLeftX &&
                                         depthY >= minLeftY && depthY <= maxLeftY;

                    bool conditionRight = depth >= depthRight - DEPTH_THRESHOLD &&
                                          depth <= depthRight + DEPTH_THRESHOLD &&
                                          depthX >= minRightX && depthX <= maxRightX &&
                                          depthY >= minRightY && depthY <= maxRightY;

                    _handPixelsLeft[i] = (byte)(isInBounds && searchForLeftHand && conditionLeft ? 255 : 0);
                    _handPixelsRight[i] = (byte)(isInBounds && searchForRightHand && conditionRight ? 255 : 0);
                }

                List<DepthPointEx> contourLeft = new List<DepthPointEx>();
                List<DepthPointEx> contourRight = new List<DepthPointEx>();

                for (int i = 0; i < DepthWidth * DepthHeight; ++i)
                {
                    ushort depth = data[i];

                    int depthX = i % DepthWidth;
                    int depthY = i / DepthWidth;

                    if (searchForLeftHand)
                    {
                        if (_handPixelsLeft[i] != 0)
                        {
                            byte top = i - DepthWidth >= 0 ? _handPixelsLeft[i - DepthWidth] : (byte)0;
                            byte bottom = i + DepthWidth < _handPixelsLeft.Length ? _handPixelsLeft[i + DepthWidth] : (byte)0;
                            byte left = i - 1 >= 0 ? _handPixelsLeft[i - 1] : (byte)0;
                            byte right = i + 1 < _handPixelsLeft.Length ? _handPixelsLeft[i + 1] : (byte)0;

                            bool isInContour = top == 0 || bottom == 0 || left == 0 || right == 0;

                            if (isInContour)
                            {
                                contourLeft.Add(new DepthPointEx { X = depthX, Y = depthY, Z = depth });
                            }
                        }
                    }

                    if (searchForRightHand)
                    {
                        if (_handPixelsRight[i] != 0)
                        {
                            byte top = i - DepthWidth >= 0 ? _handPixelsRight[i - DepthWidth] : (byte)0;
                            byte bottom = i + DepthWidth < _handPixelsRight.Length ? _handPixelsRight[i + DepthWidth] : (byte)0;
                            byte left = i - 1 >= 0 ? _handPixelsRight[i - 1] : (byte)0;
                            byte right = i + 1 < _handPixelsRight.Length ? _handPixelsRight[i + 1] : (byte)0;

                            bool isInContour = top == 0 || bottom == 0 || left == 0 || right == 0;

                            if (isInContour)
                            {
                                contourRight.Add(new DepthPointEx { X = depthX, Y = depthY, Z = depth });
                            }
                        }
                    }
                }

                if (searchForLeftHand)
                {
                    handLeft = GetHand(body.TrackingId, body.HandLeftState, contourLeft, angleLeft, wristLeftX, wristLeftY);
                }

                if (searchForRightHand)
                {
                    handRight = GetHand(body.TrackingId, body.HandRightState, contourRight, angleRight, wristRightX, wristRightY);
                }
            }

            if (handLeft != null || handRight != null)
            {
                HandCollection hands = new HandCollection
                {
                    TrackingId = body.TrackingId,
                    HandLeft = handLeft,
                    HandRight = handRight
                };

                if (HandsDetected != null)
                {
                    HandsDetected(this, hands);
                }
            }
        }

        private double CalculateDistance(float handLeftX, float handLeftY, float tipLeftX, float tipLeftY, float thumbLeftX, float thumbLeftY)
        {
            double distanceLeftHandTip = Math.Sqrt(Math.Pow(tipLeftX - handLeftX, 2) + Math.Pow(tipLeftY - handLeftY, 2)) * 2;
            double distanceLeftHandThumb = Math.Sqrt(Math.Pow(thumbLeftX - handLeftX, 2) + Math.Pow(thumbLeftY - handLeftY, 2)) * 2;

            return Math.Max(distanceLeftHandTip, distanceLeftHandThumb);
        }

        private Hand GetHand(ulong trackingID, HandState state, List<DepthPointEx> contour, double angle, float wristX, float wristY)
        {
            IList<DepthPointEx> convexHull = _grahamScan.ConvexHull(contour);
            IList<DepthPointEx> filtered = _lineThinner.Filter(convexHull);
            IList<DepthPointEx> fingers = new List<DepthPointEx>();

            if (angle > -90.0 && angle < 30.0)
            {
                // Hand "up".
                fingers = filtered.Where(p => p.Y < wristY).Take(5).ToList();
            }
            else if (angle >= 30.0 && angle < 90.0)
            {
                // Thumb below wrist (sometimes).
                fingers = filtered.Where(p => p.X > wristX).Take(5).ToList();
            }
            else if (angle >= 90.0 && angle < 180.0)
            {
                fingers = filtered.Where(p => p.Y > wristY).Take(5).ToList();
            }
            else
            {
                fingers = filtered.Where(p => p.X < wristX).Take(5).ToList();
            }

            if (contour.Count > 0 && fingers.Count > 0)
            {
                return new Hand(trackingID, state, contour, fingers, CoordinateMapper);
            }

            return null;
        }
    }
}
