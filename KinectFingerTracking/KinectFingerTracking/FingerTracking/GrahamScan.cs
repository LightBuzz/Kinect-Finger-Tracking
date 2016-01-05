using System.Collections.Generic;
using System.Linq;

namespace LightBuzz.Vitruvius.FingerTracking
{
    internal class GrahamScan
    {
        private IList<DepthPointEx> _points;

        public IList<DepthPointEx> ConvexHull(IList<DepthPointEx> points)
        {
            if (points.Count <= 3)
            {
                return points;
            }

            _points = points;

            var pointsSortedByAngle = SortPoints();
            int index = 1;

            while (index + 1 < pointsSortedByAngle.Count)
            {
                var value = PointAngleComparer.Compare(pointsSortedByAngle[index - 1], pointsSortedByAngle[index + 1], pointsSortedByAngle[index]);
                if (value < 0)
                {
                    index++;
                }
                else
                {
                    pointsSortedByAngle.RemoveAt(index);
                    if (index > 1)
                    {
                        index--;
                    }
                }
            }

            pointsSortedByAngle.Add(pointsSortedByAngle.First());

            return pointsSortedByAngle;
        }

        private DepthPointEx GetMinimumPoint()
        {
            var minPoint = _points[0];

            for (int index = 1; index < _points.Count; index++)
            {
                minPoint = GetMinimumPoint(minPoint, _points[index]);
            }

            return minPoint;
        }

        private DepthPointEx GetMinimumPoint(DepthPointEx p1, DepthPointEx p2)
        {
            if (p1.Y < p2.Y)
            {
                return p1;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X < p2.X)
                {
                    return p1;
                }
            }

            return p2;
        }

        private IList<DepthPointEx> SortPoints()
        {
            var p0 = GetMinimumPoint();

            var comparer = new PointAngleComparer(p0);

            var sortedPoints = new List<DepthPointEx>(_points);
            sortedPoints.Remove(p0);
            sortedPoints.Insert(0, p0);
            sortedPoints.Sort(1, sortedPoints.Count - 1, comparer);

            return sortedPoints;
        }
    }

    internal class PointAngleComparer : IComparer<DepthPointEx>
    {
        private DepthPointEx p0;

        public PointAngleComparer(DepthPointEx zeroPoint)
        {
            p0 = zeroPoint;
        }

        public int Compare(DepthPointEx p1, DepthPointEx p2)
        {
            if (p1.Equals(p2))
            {
                return 0;
            }

            float value = Compare(p0, p1, p2);

            if (value == 0)
            {
                return 0;
            }
            if (value < 0)
            {
                return 1;
            }
            return -1;
        }

        public static float Compare(DepthPointEx p0, DepthPointEx p1, DepthPointEx p2)
        {
            return (p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);
        }
    }
}
