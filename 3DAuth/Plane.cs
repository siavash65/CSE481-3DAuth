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
        public bool crossingPlane(PointCluster points);
        public bool crossesPlane(IPoint3f point);
    }

    class FlatPlane : IPlane
    {
        private float depth; // distance to torso

        public FlatPlane(float depth)
        {
            this.depth = depth;
        }
    }

    class CylindricalPlane : IPlane
    {
        private float depth; // distance to torso

        public CylindricalPlane(float depth)
        {
            this.depth = depth;
        }
    }
}
