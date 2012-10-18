using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class PointCluster
    {
        public HashSet<IPoint3f> points { get; set; }

        public PointCluster() : this(new HashSet<IPoint3f>()) { }

        public PointCluster(HashSet<IPoint3f> points)
        {
            this.points = points; ;
        }

        public void addPoint(IPoint3f point)
        {
            points.Add(point);
        }

        public void removePoint(IPoint3f point)
        {
            points.Remove(point);
        }

        public IPoint3f getNearestPoint(IPoint3f other)
        {
            IPoint3f nearestPoint = null;
            foreach (IPoint3f pt in points)
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
        private float distance(IPoint3f x, IPoint3f y)
        {
            return (float) Math.Sqrt( Math.Pow((x.X - y.X), 2) +
                                      Math.Pow((x.Y - y.Y), 2) +
                                      Math.Pow((x.Z - y.Z), 2) );
        }
    }
}
