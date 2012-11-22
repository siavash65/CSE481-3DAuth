using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ThreeDAuth
{
    class PointCluster
    {
        public HashSet<DepthPoint> points { get; set; }

        private HashSet<System.Windows.Point> positionPoints;

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
            this.positionPoints = new HashSet<System.Windows.Point>();
            foreach (DepthPoint point in points)
            {
                positionPoints.Add(point.GetPoint());
            }
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

        public Cube GetBoundingCube()
        {
            return Cube.CreateBoundingCube(this);
        }

        public BoundingRectangle GetBoundingRectangle()
        {
            return BoundingRectangle.CreateBoundingRectangle(this);
        }


        public void Prune()
        {
            HashSet<System.Windows.Point> removePoints = new HashSet<System.Windows.Point>();
            foreach (DepthPoint basePoint in points)
            {
                if (!removePoints.Contains(basePoint.GetPoint()))
                {
                    System.Windows.Point[] neighbors = 
                    {
                        new System.Windows.Point(basePoint.x - 1, basePoint.y - 1),
                        new System.Windows.Point(basePoint.x - 1, basePoint.y),
                        new System.Windows.Point(basePoint.x - 1, basePoint.y + 1),
                        new System.Windows.Point(basePoint.x, basePoint.y - 1),
                        new System.Windows.Point(basePoint.x, basePoint.y + 1),
                        new System.Windows.Point(basePoint.x + 1, basePoint.y - 1),
                        new System.Windows.Point(basePoint.x + 1, basePoint.y),
                        new System.Windows.Point(basePoint.x + 1, basePoint.y + 1),
                    };

                    foreach (System.Windows.Point point in neighbors)
                    {
                        if (positionPoints.Contains(point)) removePoints.Add(point);
                    }
                }
            }
            HashSet<DepthPoint> removeDepthPoints = new HashSet<DepthPoint>();
            foreach (DepthPoint point in points)
            {
                if (removePoints.Contains(point.GetPoint())) removeDepthPoints.Add(point);
            }
            foreach (DepthPoint point in removeDepthPoints)
            {
                points.Remove(point);
                positionPoints.Remove(point.GetPoint());
            }
        }


        private int manhattanDistance2d(DepthPoint x, DepthPoint y)
        {
            return Math.Abs(x.x - y.x) + Math.Abs(x.y - y.y);
        }

        // Simple euclidean distance

        private double distance(DepthPoint x, DepthPoint y)
        {
            return Math.Sqrt(Math.Pow((x.x - y.x), 2) +
                             Math.Pow((x.y - y.y), 2) +
                             Math.Pow((x.depth - y.depth), 2));
        }
    }

}
