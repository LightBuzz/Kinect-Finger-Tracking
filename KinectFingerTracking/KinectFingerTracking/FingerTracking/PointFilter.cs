using System.Collections.Generic;
using System.Linq;

namespace LightBuzz.Vitruvius.FingerTracking
{
    internal class PointFilter
    {
        private readonly float MINIMUM_DISTANCE = 18f;

        public IList<DepthPointEx> Filter(IList<DepthPointEx> points)
        {
            IList<DepthPointEx> result = new List<DepthPointEx>();

            if (points.Count > 0)
            {
                var point = new DepthPointEx(points.First());
                result.Add(point);

                foreach (var currentSourcePoint in points.Skip(1))
                {
                    if (!PointsAreClose(currentSourcePoint, point))
                    {
                        point = new DepthPointEx(currentSourcePoint);
                        result.Add(point);
                    }
                }

                if (result.Count > 1)
                {
                    CheckFirstAndLastPoint(result);
                }
            }

            return result;
        }

        private void CheckFirstAndLastPoint(IList<DepthPointEx> points)
        {
            if (PointsAreClose(points.Last(), points.First()))
            {
                points.RemoveAt(points.Count - 1);
            }
        }

        private bool PointsAreClose(DepthPointEx sourcePoint, DepthPointEx destPoint)
        {
            return DepthPointEx.Distance(sourcePoint, destPoint) < MINIMUM_DISTANCE;
        }
    }
}
