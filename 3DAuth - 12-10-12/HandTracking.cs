using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace ThreeDAuth
{
    class HandTracking
    {
        private static Point3d[] leftWristDepths = null;
        private static Point3d[] rightWristDepths = null;

        private Point3d leftWrist;
        private Point3d rightWrist;

        private const int MINDATAPOINTSREQ = 15;
        private static int leftWristCounter = 0;
        private static int rightWristCounter = 0;

        public HandTracking()
        {
            if (leftWristDepths == null)
            {
                leftWristDepths = new Point3d[MINDATAPOINTSREQ];
            }

            if (rightWristDepths == null)
            {
                rightWristDepths = new Point3d[MINDATAPOINTSREQ];
            }

            if (leftWrist == null)
            {
                leftWrist = new Point3d();
            }

            if (rightWrist == null)
            {
                rightWrist = new Point3d();
            }

        }

        private Point3d getPointFromJoint(Joint j)
        {
            return new Point3d(j.Position.X, j.Position.Y, j.Position.Z);
        }

        public Point3d getLeftWristPos()
        {
            return this.leftWrist;
        }

        public Point3d getRightWristPos()
        {
            return this.rightWrist;
        }

        public void updateWristDepths(Joint WristRight, Joint WristLeft)
        {


            Point3d WristRightZ = getPointFromJoint(WristRight);
            Point3d WristLeftZ = getPointFromJoint(WristLeft);

            if (leftWristCounter < MINDATAPOINTSREQ)
            {
                leftWristDepths[leftWristCounter] = WristLeftZ;
                leftWristCounter++;
            }
            else
            {
                for (int i = 0; i < leftWristCounter; i++)
                {
                    this.leftWrist.X += leftWristDepths[i].X;
                    this.leftWrist.Y += leftWristDepths[i].Y;
                    this.leftWrist.Z += leftWristDepths[i].Z;
                }

                this.leftWrist.X /= (leftWristCounter);
                this.leftWrist.Y /= (leftWristCounter);
                this.leftWrist.Z /= (leftWristCounter);
                
                System.Console.WriteLine("The depth of the left Wrist is : " + this.leftWrist.Z);

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
                    this.rightWrist.X += rightWristDepths[i].X;
                    this.rightWrist.Y += rightWristDepths[i].Y;
                    this.rightWrist.Z += rightWristDepths[i].Z;
                }

                this.rightWrist.X /= (rightWristCounter);
                this.rightWrist.Y /= (rightWristCounter);
                this.rightWrist.Z /= (rightWristCounter);

                System.Console.WriteLine("The depth of the right Wrist is : " + this.rightWrist.Z);

            }

        }
    }
}
