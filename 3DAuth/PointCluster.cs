using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class PointCluster
    {
        public HashSet<Point3d> points { get; set; }

        public PointCluster() : this(new HashSet<Point3d>()) { }

        public PointCluster(HashSet<Point3d> points)
        {
            this.points = points; ;
        }

        public void addPoint(Point3d point)
        {
            points.Add(point);
        }

        public void removePoint(Point3d point)
        {
            points.Remove(point);
        }

        public Point3d getNearestPoint(Point3d other)
        {
            Point3d nearestPoint = null;
            foreach (Point3d pt in points)
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

        // Simple euclidean distance
        private float distance(Point3d x, Point3d y)
        {
            return (float) Math.Sqrt( Math.Pow((x.X - y.X), 2) +
                                      Math.Pow((x.Y - y.Y), 2) +
                                      Math.Pow((x.Z - y.Z), 2) );
        }
    }
}
