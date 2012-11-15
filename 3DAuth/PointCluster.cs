using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class PointCluster
    {
        public HashSet<DepthPoint> points { get; set; }

        public DepthPoint Centroid
        {
            get
            {
                if (points == null || points.Count == 0)
                {
                    return null;
                }
                DepthPoint sum = new DepthPoint(0, 0, 0);
                foreach (DepthPoint point in points)
                {
                    sum += point;
                }
                return sum / points.Count; 
            }
        }

        public double Radius
        {
            get
            {
                if (points == null | points.Count == 0)
                {
                    return 0.0;
                }
                // n^2 operation, could probably be improved
                double maxDist = double.MinValue;
                foreach (DepthPoint outerPoint in points)
                {
                    foreach (DepthPoint innerPoint in points)
                    {
                        maxDist = Math.Max(maxDist, distance(outerPoint, innerPoint));
                    }
                }
                return maxDist / 2.0;
            }
        }

        public PointCluster() : this(new HashSet<DepthPoint>()) { }

        public PointCluster(HashSet<DepthPoint> points)
        {
            this.points = points;
        }

        public void addPoint(DepthPoint point)
        {
            points.Add(point);
        }

        public void removePoint(DepthPoint point)
        {
            points.Remove(point);
        }

        public DepthPoint getNearestPoint(DepthPoint other)
        {
            DepthPoint nearestPoint = null;
            foreach (DepthPoint pt in points)
            {
                if (nearestPoint == null)
                {
                    nearestPoint = pt;
                }
                else
                {
                    if (distance(pt, other) < distance(nearestPoint, other))
                    {
                        nearestPoint = pt;
                    }
                }

            }
            return nearestPoint;
        }

        public Cube GetBoundingBox


        // Simple euclidean distance

        private float distance(DepthPoint x, DepthPoint y)
        {
            return (float)Math.Sqrt(Math.Pow((x.x - y.x), 2) +
                                      Math.Pow((x.y - y.y), 2) +
                                      Math.Pow((x.depth - y.depth), 2));
        }
    }

}
