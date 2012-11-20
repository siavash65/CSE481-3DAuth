using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Scheme for determining whether a point is in the target box or not
    interface ITargetBoxScheme
    {
        bool pointInTargetBox(DepthPoint point, DepthPoint center, double armLength);

        DepthPoint[] getCorners(DepthPoint center, double armLength);
    }

    class RigidTargetBoxScheme : ITargetBoxScheme
    {
        private double height;
        private double width;

        public RigidTargetBoxScheme(double height, double width)
        {
            this.height = height;
            this.width = width;
        }

        public bool pointInTargetBox(DepthPoint point, DepthPoint center, double armLength)
        {
            return Math.Abs(point.x - center.x) <= (width / 2) &&
                   Math.Abs(point.y - center.y) <= (height / 2);
        }

        public DepthPoint[] getCorners(DepthPoint center, double armLength)
        {
            DepthPoint[] corners = new DepthPoint[4];
            corners[0] = new DepthPoint((int) (center.x - (width / 2)), (int) (center.y - (height / 2)), center.depth);
            corners[1] = new DepthPoint((int) (center.x - (width / 2)), (int) (center.y + (height / 2)), center.depth);
            corners[2] = new DepthPoint((int) (center.x + (width / 2)), (int) (center.y + (height / 2)), center.depth);
            corners[3] = new DepthPoint((int) (center.x + (width / 2)), (int) (center.y - (height / 2)), center.depth);
            return corners;
        }
    }

    class ArmLengthTargetBoxScheme : ITargetBoxScheme
    {
        private double widthPercentage;
        private double heightPercentage;

        // The target percentage are the percentage of arm length to use for making the target box
        public ArmLengthTargetBoxScheme(double targetWidthPercentage, double targetHeightPercentage)
        {
            widthPercentage = targetWidthPercentage;
            heightPercentage = targetHeightPercentage;
        }

        public ArmLengthTargetBoxScheme(double targetPercentage) : this(targetPercentage, targetPercentage) { }

        public bool pointInTargetBox(DepthPoint point, DepthPoint center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            return Math.Abs(point.x - center.x) <= (width / 2) &&
                   Math.Abs(point.y - center.y) <= (height / 2);
        }

        public DepthPoint[] getCorners(DepthPoint center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            DepthPoint[] corners = new DepthPoint[4];
            corners[0] = new DepthPoint((int)(center.x - (width / 2)), (int)(center.y - (height / 2)), center.depth);
            corners[1] = new DepthPoint((int)(center.x - (width / 2)), (int)(center.y + (height / 2)), center.depth);
            corners[2] = new DepthPoint((int)(center.x + (width / 2)), (int)(center.y + (height / 2)), center.depth);
            corners[3] = new DepthPoint((int)(center.x + (width / 2)), (int)(center.y - (height / 2)), center.depth);
            return corners;
        }
    }

    class TargetBox
    {
        private ITargetBoxScheme targetBoxScheme { get; set; }
        private DepthPoint center { get; set; }
        private double armLength { get; set; }
        private double torsoDepth { get; set; }

        public TargetBox(ITargetBoxScheme targetBoxScheme, DepthPoint center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
            this.targetBoxScheme = targetBoxScheme;
        }

        public TargetBox() : this(new RigidTargetBoxScheme(0f, 0f), new DepthPoint(), 0f, 0f) { }

        public void setBox(DepthPoint center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
        }

        public void setTargetBoxScheme(ITargetBoxScheme scheme)
        {
            targetBoxScheme = scheme;
        }

        public bool pointInTargetBox(DepthPoint point)
        {
            return targetBoxScheme.pointInTargetBox(point, center, armLength);
        }

        public DepthPoint[] getCorners()
        {
            return targetBoxScheme.getCorners(center, armLength);
        }

        public Vec2d[] getBoxLines()
        {
            DepthPoint[] corners = getCorners();
            Vec2d[] lines = new Vec2d[4];
            lines[0] = new Vec2d(corners[0], corners[1]);
            lines[1] = new Vec2d(corners[1], corners[2]);
            lines[2] = new Vec2d(corners[2], corners[3]);
            lines[3] = new Vec2d(corners[3], corners[0]);
            return lines;
        }
    }

    // Union of the targetbox with the plane
    class TargetArea
    {
        private TargetBox targetBox;
        private IPlane plane;

        public TargetArea(TargetBox targetBox, IPlane plane)
        {
            this.targetBox = targetBox;
            this.plane = plane;
        }

        public void setPlane(IPlane plane)
        {
            this.plane = plane;
        }

        public void setTargetBox(TargetBox targetBox)
        {
            this.targetBox = targetBox;
        }

        public bool pointInTargetArea(DepthPoint point)
        {
            return plane.crossesPlane(point) && targetBox.pointInTargetBox(point);
        }
    }
}
