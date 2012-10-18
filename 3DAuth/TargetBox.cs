using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Scheme for determining whether a point is in the target box or not
    interface ITargetBoxScheme
    {
        bool pointInTargetBox(IPoint3f point, IPoint3f center, double armLength);

        IPoint3f[] getCorners(IPoint3f center, double armLength);
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

        public bool pointInTargetBox(IPoint3f point, IPoint3f center, double armLength)
        {
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }

        public IPoint3f[] getCorners(IPoint3f center, double armLength)
        {
            IPoint3f[] corners = new GenericPoint[4];
            corners[0] = new GenericPoint(center.X - (width / 2), center.Y - (height / 2), center.Z);
            corners[1] = new GenericPoint(center.X - (width / 2), center.Y + (height / 2), center.Z);
            corners[2] = new GenericPoint(center.X + (width / 2), center.Y + (height / 2), center.Z);
            corners[3] = new GenericPoint(center.X + (width / 2), center.Y - (height / 2), center.Z);
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

        public bool pointInTargetBox(IPoint3f point, IPoint3f center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }

        public IPoint3f[] getCorners(IPoint3f center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            IPoint3f[] corners = new GenericPoint[4];
            corners[0] = new GenericPoint(center.X - (width / 2), center.Y - (height / 2), center.Z);
            corners[1] = new GenericPoint(center.X - (width / 2), center.Y + (height / 2), center.Z);
            corners[2] = new GenericPoint(center.X + (width / 2), center.Y + (height / 2), center.Z);
            corners[3] = new GenericPoint(center.X + (width / 2), center.Y - (height / 2), center.Z);
            return corners;
        }
    }

    class TargetBox
    {
        private ITargetBoxScheme targetBoxScheme { get; set; }
        private IPoint3f center { get; set; }
        private double armLength { get; set; }
        private double torsoDepth { get; set; }

        public TargetBox(ITargetBoxScheme targetBoxScheme, IPoint3f center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
            this.targetBoxScheme = targetBoxScheme;
        }

        public TargetBox() : this(new RigidTargetBoxScheme(0f, 0f), new GenericPoint(), 0f, 0f) { }

        public void setBox(IPoint3f center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
        }

        public void setTargetBoxScheme(ITargetBoxScheme scheme)
        {
            targetBoxScheme = scheme;
        }

        public bool pointInTargetBox(IPoint3f point)
        {
            return targetBoxScheme.pointInTargetBox(point, center, armLength);
        }

        public IPoint3f[] getCorners()
        {
            return targetBoxScheme.getCorners(center, armLength);
        }

        public Vec2f[] getBoxLines()
        {
            IPoint3f[] corners = getCorners();
            Vec2f[] lines = new Vec2f[4];
            lines[0] = new Vec2f(corners[0], corners[1]);
            lines[1] = new Vec2f(corners[1], corners[2]);
            lines[2] = new Vec2f(corners[2], corners[3]);
            lines[3] = new Vec2f(corners[3], corners[1]);
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

        public bool pointInTargetArea(IPoint3f point)
        {
            return plane.crossesPlane(point) && targetBox.pointInTargetBox(point);
        }
    }
}
