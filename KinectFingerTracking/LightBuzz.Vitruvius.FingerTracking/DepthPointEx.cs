using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LightBuzz.Vitruvius.FingerTracking
{
    internal struct DepthPointEx
    {
        public float X;
        public float Y;
        public float Z;

        public static DepthPointEx Zero = new DepthPointEx(0, 0, 0);

        public DepthPointEx(DepthPointEx point) : this()
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }

        public DepthPointEx(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static double Distance(DepthPointEx p1, DepthPointEx p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static DepthPointEx Center(DepthPointEx p1, DepthPointEx p2)
        {
            return new DepthPointEx((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
        }

        public static DepthPointEx Center(IList<DepthPointEx> points)
        {
            var center = DepthPointEx.Zero;
            if (points.Count > 0)
            {
                for (int index = 0; index < points.Count; index++)
                {
                    var p = points[index];
                    center.X += p.X;
                    center.Y += p.Y;
                    center.Z += p.Z;
                }

                center.X /= points.Count;
                center.Y /= points.Count;
                center.Z /= points.Count;
            }
            return center;
        }

        public static DepthPointEx FindNearestPoint(DepthPointEx target, IEnumerable<DepthPointEx> points)
        {
            var pointList = points.ToList();
            return pointList[FindIndexOfNearestPoint(target, pointList)];
        }

        public static int FindIndexOfNearestPoint(DepthPointEx target, IList<DepthPointEx> points)
        {
            int index = 0;
            int resultIndex = -1;
            double minDist = double.MaxValue;
            foreach (DepthPointEx p in points)
            {
                var distance = Distance(p.X, p.Y, target.X, target.Y);
                if (distance < minDist)
                {
                    resultIndex = index;
                    minDist = distance;
                }
                index++;
            }
            return resultIndex;
        }

        public static double Angle(DepthPointEx center, DepthPointEx start, DepthPointEx end)
        {
            return Angle(center.X, center.Y, start.X, start.Y, end.X, end.Y);
        }

        public static double Angle(float centerX, float centerY, float startX, float startY, float endX, float endY)
        {
            Vector first = new Vector(startX - centerX, startY - centerY);
            Vector second = new Vector(endX - centerX, endY - centerY);

            return Vector.AngleBetween(first, second);
        }
    }
}
