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
        bool crossesPlane(IPoint3f point);
    }

    class FlatPlane : IPlane
    {
        private double depth; // distance to torso
        private IPoint3f center;

        public FlatPlane(IPoint3f center, double depth)
        {
            this.depth = depth;
            this.center = center;
        }

        public void setCenter(IPoint3f center)
        {
            this.center = center;
        }

        public bool crossesPlane(IPoint3f point)
        {
            return point.Z < (center.Z - depth);
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (IPoint3f pt in points.points)
            {
                if (crossesPlane(pt)) return true;
            }
            return false;
        }
    }

    class CylindricalPlane : IPlane
    {
        private double depth; // distance to torso
        private IPoint3f center;

        public CylindricalPlane(IPoint3f center, double depth)
        {
            this.depth = depth;
            this.center = center;
        }

        public bool crossesPlane(IPoint3f point)
        {
            // Ignore Y axis data
            // Return whether the length of the projected vector into xz-space is greater than the cutoff
            return Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Z - center.Z, 2)) > depth;
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (IPoint3f pt in points.points)
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
        private IPoint3f center;

        public SphericalPlane(IPoint3f center, double depth)
        {
            this.depth = depth;
            this.center = center;
        }

        public bool crossesPlane(IPoint3f point)
        {
            // Return whether the length of the vector is greater than the cutoff
            return Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Z - center.Z, 2)) > depth;
        }

        public bool crossesPlane(PointCluster points)
        {
            foreach (IPoint3f pt in points.points)
            {
                if (crossesPlane(pt)) return true;
            }
            return false;
        }
    }
}
