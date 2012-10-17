using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Scheme for determining whether a point is in the target box or not
    interface ITargetBoxScheme
    {
        public bool pointInTargetBox(IPoint3f point, IPoint3f center, float armLength, float torsoDepth);
    }

    class RigidTargetBoxScheme : ITargetBoxScheme
    {
        private float height;
        private float width;

        public RigidTargetBoxScheme(float height, float width)
        {
            this.height = height;
            this.width = width;
        }

        public bool pointInTargetBox(IPoint3f point, IPoint3f center, float armLength, float torsoDepth)
        {
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }
    }

    class ArmLengthTargetBoxScheme : ITargetBoxScheme
    {
        private float widthPercentage;
        private float heightPercentage;

        // The target percentage are the percentage of arm length to use for making the target box
        public ArmLengthTargetBoxScheme(float targetWidthPercentage, float targetHeightPercentage)
        {
            widthPercentage = targetWidthPercentage;
            heightPercentage = targetHeightPercentage;
        }

        public ArmLengthTargetBoxScheme(float targetPercentage) : this(targetPercentage, targetPercentage) { }

        public bool pointInTargetBox(IPoint3f point, IPoint3f center, float armLength, float torsoDepth)
        {
            float width = armLength * widthPercentage;
            float height = armLength * heightPercentage;
            return Math.Abs(point.X - center.X) <= (width / 2) &&
                   Math.Abs(point.Y - center.Y) <= (height / 2);
        }
    }

    class TargetBox
    {
        private ITargetBoxScheme targetBoxScheme;
        private IPoint3f center;
        private float armLength;
        private float torsoDepth;

        public TargetBox(ITargetBoxScheme targetBoxScheme, IPoint3f center, float armLength, float torsoDepth)
        {
            this.center = center;
            this.armLength = armLength;
            this.torsoDepth = torsoDepth;
            this.targetBoxScheme = targetBoxScheme;
        }

        public TargetBox() : this(new RigidTargetBoxScheme(0f, 0f), new GenericPoint(), 0f, 0f) { }

        public void setBox(IPoint3f center, float armLength, float torsoDepth)
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
            return targetBoxScheme.pointInTargetBox(point, center, armLength, torsoDepth);
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
