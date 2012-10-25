using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace ThreeDAuth
{
    class HandTracking
    {
        private static GenericPoint[] leftWristDepths = null;
        private static GenericPoint[] rightWristDepths = null;

        private GenericPoint leftWrist;
        private GenericPoint rightWrist;

        private const int MINDATAPOINTSREQ = 15;
        private static int leftWristCounter = 0;
        private static int rightWristCounter = 0;

        public HandTracking()
        {
            if (leftWristDepths == null)
            {
                leftWristDepths = new GenericPoint[MINDATAPOINTSREQ];
            }

            if (rightWristDepths == null)
            {
                rightWristDepths = new GenericPoint[MINDATAPOINTSREQ];
            }

            if (leftWrist == null)
            {
                leftWrist = new GenericPoint();
            }

            if (rightWrist == null)
            {
                rightWrist = new GenericPoint();
            }

        }

        private GenericPoint getPointFromJoint(Joint j)
        {
            return new GenericPoint(j.Position.X, j.Position.Y, j.Position.Z);
        }

        public GenericPoint getLeftWristPos()
        {
            return this.leftWrist;
        }

        public GenericPoint getRightWristPos()
        {
            return this.rightWrist;
        }

        public void updateWristDepths(Joint WristRight, Joint WristLeft)
        {


            GenericPoint WristRightZ = getPointFromJoint(WristRight);
            GenericPoint WristLeftZ = getPointFromJoint(WristLeft);

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
