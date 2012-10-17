using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    interface IPoint3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    class GenericPoint : IPoint3f { }

    class DepthPoint : GenericPoint { }

    class PointCluster
    {
        private HashSet<IPoint3f> points;

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
