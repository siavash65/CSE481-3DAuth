using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Scheme for determining whether a point is in the target box or not
    interface ITargetBoxScheme
    {
        bool pointInTargetBox(Point3d point, Point3d center, double armLength);

        Point3d[] getCorners(Point3d center, double armLength);
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

        public bool pointInTargetBox(Point3d point, Point3d center, double armLength)
        {
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }

        public Point3d[] getCorners(Point3d center, double armLength)
        {
            Point3d[] corners = new Point3d[4];
            corners[0] = new Point3d(center.X - (width / 2), center.Y - (height / 2), center.Z);
            corners[1] = new Point3d(center.X - (width / 2), center.Y + (height / 2), center.Z);
            corners[2] = new Point3d(center.X + (width / 2), center.Y + (height / 2), center.Z);
            corners[3] = new Point3d(center.X + (width / 2), center.Y - (height / 2), center.Z);
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

        public bool pointInTargetBox(Point3d point, Point3d center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }

        public Point3d[] getCorners(Point3d center, double armLength)
        {
            double width = armLength * widthPercentage;
            double height = armLength * heightPercentage;
            Point3d[] corners = new Point3d[4];
            corners[0] = new Point3d(center.X - (width / 2), center.Y - (height / 2), center.Z);
            corners[1] = new Point3d(center.X - (width / 2), center.Y + (height / 2), center.Z);
            corners[2] = new Point3d(center.X + (width / 2), center.Y + (height / 2), center.Z);
            corners[3] = new Point3d(center.X + (width / 2), center.Y - (height / 2), center.Z);
            return corners;
        }
    }

    class TargetBox
    {
        private ITargetBoxScheme targetBoxScheme { get; set; }
        private Point3d center { get; set; }
        private double armLength { get; set; }
        private double torsoDepth { get; set; }

        public TargetBox(ITargetBoxScheme targetBoxScheme, Point3d center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
            this.targetBoxScheme = targetBoxScheme;
        }

        public TargetBox() : this(new RigidTargetBoxScheme(0f, 0f), new Point3d(), 0f, 0f) { }

        public void setBox(Point3d center, double armLength, double torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
        }

        public void setTargetBoxScheme(ITargetBoxScheme scheme)
        {
            targetBoxScheme = scheme;
        }

        public bool pointInTargetBox(Point3d point)
        {
            return targetBoxScheme.pointInTargetBox(point, center, armLength);
        }

        public Point3d[] getCorners()
        {
            return targetBoxScheme.getCorners(center, armLength);
        }

        public Vec2d[] getBoxLines()
        {
            Point3d[] corners = getCorners();
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

        public bool pointInTargetArea(Point3d point)
        {
            return plane.crossesPlane(point) && targetBox.pointInTargetBox(point);
        }
    }
}
