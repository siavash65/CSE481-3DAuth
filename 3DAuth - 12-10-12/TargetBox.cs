using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Projects pixel-space coordinates into pixel space coordinates with the goal
    // of making every point on the screen reachable with the same arm position
    // regardless of depth. (Think lensing... sort of)
    class TargetBox
    {
        /* Moved to MainWindow.xaml.cs
        public static Tuple<int, int> ProjectPoint(Tuple<int, int> basePoint,
                                                   DepthPoint torsoPosition, 
                                                   double armLength, 
                                                   short planeDepthFromTorso, 
                                                   double alpha,
                                                   int windowWidth,
                                                   int windowHeight)
        {
            armLength *= 1000; // Convert meters to mm

            // We wish to find how many millimeters away from the corner of the target box our torso center is

            // If we're at torsoPosition and the plane is planeDepthFromTorso away from us,
            // then we construct a right triangle where one side is the vector
            // of our torso to the plane,
            // the other side is the vector from the center of the plane to the corner of the plane
            // and the hypotenuse is our required arm
            
            // Vector of torso to plane has length planeDepthFromTorso
            // Vector of our required arm length is alpha * armLength
            // By the pythagorean theorem, (alpha * armLength)^2 = (planeDepthFromTorso)^2 + (distance to corner)^2
            // so distance to corner = + sqrt( (alpha * armLength)^2 - (planeDepthFromTorso)^2 )

            double tempValue = Math.Pow(alpha * armLength, 2) - Math.Pow(planeDepthFromTorso, 2);
            // If this tempValue is less than 0 then holding the arm straight out infront of you 
            // (with the given alpha) will not cross the plane and we have bigger problems
            if (tempValue < 0)
            {
                // Sanity check
                return null;
            }

            double distanceToCornermm = Math.Sqrt( tempValue );

            // Now find the x and y offset required to get to the corner
            // if we consider our torso position as at the center of the box (they should be standing there)
            // then we consider the angle the vector to the upper right corner forms with the horizontal plane 
            // to be a value theta

            // our x offset to the corner will be cos(theta) * distanceToCornermm
            // (cos(angle) = adj / hyp, so adj = cos(angle) * hyp)

            // our y offset to the corner will be sin(theta) * distanceToCornermm
            // (sin(angle) = adj / hyp, so adj = sin(angle) * hyp)

            // By similar triangles (rectangles), tan(theta) = windowHeight / windowWidth, both of which we know
            // so theta = arctan(windowHeight / windowWidth);

            double theta = Math.Atan((double)windowHeight / (double)windowWidth);
            double xOffset = Math.Cos(theta) * distanceToCornermm;
            double yOffset = Math.Sin(theta) * distanceToCornermm;

            Microsoft.Kinect.SkeletonPoint cornerSkeletonPoint = new Microsoft.Kinect.SkeletonPoint();
            cornerSkeletonPoint.X = (float) (torsoPosition.x + xOffset);
            cornerSkeletonPoint.Y = (float) (torsoPosition.y + yOffset);
            DepthPoint lowerRightCornerDepthPoint = Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.SSkeletonPointToScreen(cornerSkeletonPoint);

            // This cornerPoint should represent the bottom right corner more or less
            // So its x value should be our pre-projected target box width,
            // and its y value should be our pre-projected target box height
            double xScale = (double)lowerRightCornerDepthPoint.x / (double)windowWidth;
            double yScale = (double)lowerRightCornerDepthPoint.y / (double)windowHeight;

            int xProj = (int) Math.Floor(xScale * basePoint.Item1);
            int yProj = (int) Math.Floor(yScale * basePoint.Item2);

            //Sanity check
            if (xProj > windowWidth) xProj = windowWidth - 1;
            if (yProj > windowHeight) yProj = windowHeight - 1;

            return new Tuple<int, int>(xProj, yProj);
        }
    }*/
    }

    /*
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
     * */
}
