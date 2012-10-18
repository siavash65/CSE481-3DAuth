using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class PlaneInteraction
    {
        private static double[] leftWristDepths = null;
        public double leftWristDepth { get; protected set; }
        private static double[] rightWristDepths = null;
        public double rightWristDepth { get; protected set; }

        private const int MINDATAPOINTSREQ = 50;
        private static int leftWristCounter = 0;
        private static int rightWristCounter = 0;
        private double planeDepth;


        public PlaneInteraction(double planeDepth) 
        {
            if (leftWristDepths == null) {
                leftWristDepths = new double[MINDATAPOINTSREQ];
            }

            if (rightWristDepths == null) {
                rightWristDepths = new double[MINDATAPOINTSREQ];
            }

            this.planeDepth = planeDepth;
        }

        // returns an array with positions [left Wrist x, left Wrist y, right Wrist x, right Wrist y]
        // if values are zero, the plane is not broken
        public double[] updateWristDepths(Joint WristRight, Joint WristLeft)
        {

            double[] breakLocs = new double[4];

            double WristRightZ = WristRight.Position.Z;
            double WristLeftZ = WristLeft.Position.Z;
            
            if (leftWristCounter < MINDATAPOINTSREQ)
            {
                leftWristDepths[leftWristCounter] = WristLeftZ;
                leftWristCounter++;
            }
            else
            {
                for (int i = 0; i < leftWristCounter; i++)
                {
                    this.leftWristDepth += leftWristDepths[i];
                }
                this.leftWristDepth /= (leftWristCounter);
                System.Console.WriteLine("The depth of the left Wrist is : " + this.leftWristDepth);

                if (this.leftWristDepth <= this.planeDepth)
                {
                    breakLocs[0] = WristLeft.Position.X;
                    breakLocs[0] = WristLeft.Position.Y;
                }
            }


            if (rightWristCounter < MINDATAPOINTSREQ)
            {
                rightWristDepths[rightWristCounter] = WristRightZ;
                rightWristCounter++;
            }
            else
            {
                for (int i = 0; i < rightWristCounter; i++)
                {
                    this.rightWristDepth += rightWristDepths[i];
                }
                this.rightWristDepth /= (rightWristCounter);
                System.Console.WriteLine("The depth of the right Wrist is : " + this.rightWristDepth);

                if (this.rightWristDepth <= this.planeDepth)
                {
                    breakLocs[0] = WristRight.Position.X;
                    breakLocs[0] = WristRight.Position.Y;
                }
            }

            return breakLocs;
        }

    }
}
