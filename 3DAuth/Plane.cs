using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    /*
     * Abstraction of the idea of a plane to break with the hands
     */
    interface IPlane
    {
        bool crossesPlane(PointCluster points);
        bool crossesPlane(DepthPoint point);
    }

    class FlatPlane : IPlane
    {
        private double depth; // distance to torso
        private DepthPoint center;

        public FlatPlane(DepthPoint center, double depth)
        {
            this.depth = depth * 1000; // Assuming we get depth in meters from the skeleton tracker
            this.center = center;
        }

        public void setCenter(DepthPoint center)
        {
            this.center = center;
        }
        public bool crossesPlane(DepthPoint point)
        {
            //Console.WriteLine("Difference: " + (point.depth - (center.depth - depth)));
            return point.depth < (center.depth - depth);
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (DepthPoint pt in points.points)
            {
                if (crossesPlane(pt)) return true;
            }
            return false;
        }
    }

    class CylindricalPlane : IPlane
    {
        private double depth; // distance to torso
        private DepthPoint center;

        public CylindricalPlane(DepthPoint center, double depth)
        {
            this.depth = depth;
            this.center = center;
        }

        public bool crossesPlane(DepthPoint point)
        {
            // Ignore Y axis data
            // Return whether the length of the projected vector into xz-space is greater than the cutoff
            return Math.Sqrt(Math.Pow(point.x - center.x, 2) + Math.Pow(point.depth - center.depth, 2)) > depth;
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (DepthPoint pt in points.points)
            {
                if (crossesPlane(pt)) return true;
            }
            return false;
        }
    }
    
    // Spherical plane placed around the center of the shoulders
    class SphericalPlane : IPlane
    {
        private double depth; // distance to torso
        private DepthPoint center;

        public SphericalPlane(DepthPoint center, double depth)
        {
            this.depth = depth;
            this.center = center;
        }

        public bool crossesPlane(DepthPoint point)
        {
            // Return whether the length of the vector is greater than the cutoff
            return Math.Sqrt(Math.Pow(point.x - center.x, 2) + Math.Pow(point.depth - center.depth, 2)) > depth;
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (DepthPoint pt in points.points)
            {
                if (crossesPlane(pt)) return true;
            }
            return false;
        }
    }
}
