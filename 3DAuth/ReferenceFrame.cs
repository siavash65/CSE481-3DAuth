using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
namespace ThreeDAuth
{
    class ReferenceFrame
    {
        private static double[] armLengths = null;
        public double armLength { get; protected set; }
        private static Point3d[] torsoPositions = null;
        public Point3d torsoPosition { get; protected set; }
        private static int armCounter = 0;
        private static int torsoCounter = 0;
        /*The minimum amount of data point we need to compute an accurate value and cancel
        OutOfMemoryException the nois in ThemeDictionaryExtension raw data*/
        private const int MINDATAPOINTSREQ = 20;

        /// <summary>
        /// 
        /// </summary>
        public ReferenceFrame()
        {
            if (armLengths == null)
            {
                armLengths = new double[MINDATAPOINTSREQ];
            }
            if (torsoPositions == null)
            {
                torsoPositions = new Point3d[MINDATAPOINTSREQ];
                for (int i = 0; i < MINDATAPOINTSREQ; i++)
                {
                    torsoPositions[i] = new Point3d();

                }
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="leftWristJoint"></param>
        /// <param name="leftShoulderJoint"></param>
        /// <param name="rightWristJoint"></param>
        /// <param name="rightShoulderJoint"></param>
        public void computeArmLength(Joint leftWristJoint, Joint leftShoulderJoint, Joint rightWristJoint, Joint rightShoulderJoint)
        {

            double leftArmX = leftWristJoint.Position.X - leftShoulderJoint.Position.X;
            double leftArmY = leftWristJoint.Position.Y - leftShoulderJoint.Position.Y;
            double leftArmZ = leftWristJoint.Position.Z - leftShoulderJoint.Position.Z;
            double rightArmX = rightWristJoint.Position.X - rightShoulderJoint.Position.X;
            double rightArmY = rightWristJoint.Position.Y - rightShoulderJoint.Position.Y;
            double rightArmZ = rightWristJoint.Position.Z - rightShoulderJoint.Position.Z;
            double leftArm = Math.Sqrt(leftArmX * leftArmX + leftArmY * leftArmY + leftArmZ * leftArmZ);
            double rightArm = Math.Sqrt(rightArmX * rightArmX + rightArmY * rightArmY + rightArmZ * rightArmZ);


            /*
             * The length of both arms changes depending on the location of the person due to noise in data 
             * and also inaccuracy of depth cammera at too far and too close postions.
             * so we collect at least 50 data points of the four joint we need and calculate the avregae
             * 
             */
            if (armCounter < MINDATAPOINTSREQ)
            {
                armLengths[armCounter] = (leftArm + rightArm) / 2;
                armCounter++;
            }
            else
            {
                for (int i = 0; i < armCounter; i++)
                {
                    this.armLength += armLengths[i];
                }
                this.armLength /= (armCounter);
                //System.Console.WriteLine("Arm length is: " + this.armLength);
            }
            //System.Console.WriteLine("x is: " + leftArmX + "  y is: " + leftArmY + " z is: " + leftArmZ);
            //System.Console.WriteLine("Left Arm: " + leftArm + " Right Arm: " + rightArm + " Average length : " + (leftArm+rightArm)/2);




        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shoulderCenter"></param>
        /// <param name="spine"></param>
        /// <param name="hipCenter"></param>
        internal void computerTorsoDepth(Joint shoulderCenter, Joint spine, Joint hipCenter)
        {
            double shoulderCenterZ = shoulderCenter.Position.Z;
            double spineZ = spine.Position.Z;
            double hipCenterZ = hipCenter.Position.Z;

            //System.Console.WriteLine("shoulder z is: " + shoulderCenterZ + " spine z is: " + spineZ + " hipCenter z is: " + hipCenterZ);
            if (torsoCounter < MINDATAPOINTSREQ)
            {
                torsoPositions[torsoCounter].X = (shoulderCenter.Position.X + spine.Position.X + hipCenter.Position.X) / 3;
                torsoPositions[torsoCounter].Y = (shoulderCenter.Position.Y + spine.Position.Y + hipCenter.Position.Y) / 3;
                torsoPositions[torsoCounter].Z = (shoulderCenter.Position.Z + spine.Position.Z + hipCenter.Position.Z) / 3;
                torsoCounter++;
            }
            else
            {
                this.torsoPosition = new Point3d();
                for (int i = 0; i < torsoCounter; i++)
                {
                    this.torsoPosition =
                        this.torsoPosition + torsoPositions[i];
                }
                this.torsoPosition.X /= (torsoCounter);
                this.torsoPosition.Y /= (torsoCounter);
                this.torsoPosition.Z /= (torsoCounter);
                //System.Console.WriteLine("The depth of torso is: " + this.torsoPosition);
            }
        }
    }
}
