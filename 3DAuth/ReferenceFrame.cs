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
        public int armLengthPixels { get; protected set; }
        private static DepthPoint[] torsoPositions = null;
        public DepthPoint torsoPosition { get; protected set; }
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
                torsoPositions = new DepthPoint[MINDATAPOINTSREQ];
                for (int i = 0; i < MINDATAPOINTSREQ; i++)
                {
                    torsoPositions[i] = new DepthPoint();

                }
            }

            this.torsoPosition = new DepthPoint();
            leftWristPoints = new List<DepthPoint>();
            rightWristPoints = new List<DepthPoint>();
            leftShoulderPoints = new List<DepthPoint>();
            rightShoulderPoints = new List<DepthPoint>();
            totalArmPoints = 0;
            totalTorsoPoints = 0;
            
            shoulderCenterPoints = new List<DepthPoint>();
            spinePoints = new List<DepthPoint>();
            hipCenterPoints = new List<DepthPoint>();
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

            this.armLength = (leftArm + rightArm) / 2;

            /*
             * The length of both arms changes depending on the location of the person due to noise in data 
             * and also inaccuracy of depth cammera at too far and too close postions.
             * so we collect at least 50 data points of the four joint we need and calculate the avregae
             * 
             */
            /*
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
                //this.armLength /= (armCounter);
                this.armLength = (leftArm + rightArm) / 2;
            }*/
        }

        private List<DepthPoint> leftWristPoints;
        private List<DepthPoint> rightWristPoints;
        private List<DepthPoint> leftShoulderPoints;
        private List<DepthPoint> rightShoulderPoints;

        private List<DepthPoint> shoulderCenterPoints;
        private List<DepthPoint> spinePoints;
        private List<DepthPoint> hipCenterPoints;

        private int totalArmPoints;
        private int totalTorsoPoints;

        private const int MIN_POINTS_BEFORE_READING = 10;

        private const int MAX_POINTS_IN_READING = 50;

        public int AvgArmLengthPixels
        {
            get
            {
                double sumArmLengths = 0;
                for (int i = 0; i < leftWristPoints.Count; i++)
                {
                    int leftArmX = leftWristPoints.ElementAt(i).x - leftShoulderPoints.ElementAt(i).x;
                    int leftArmY = leftWristPoints.ElementAt(i).y - leftShoulderPoints.ElementAt(i).y;

                    int rightArmX = rightWristPoints.ElementAt(i).x - rightShoulderPoints.ElementAt(i).x;
                    int rightArmY = rightWristPoints.ElementAt(i).y - rightShoulderPoints.ElementAt(i).y;

                    double leftArm = Math.Sqrt(leftArmX * leftArmX + leftArmY * leftArmY);
                    double rightArm = Math.Sqrt(rightArmX * rightArmX + rightArmY * rightArmY);
                    double armLength = (leftArm + rightArm) / 2.0;
                    sumArmLengths += armLength;
                }
                return (int) (sumArmLengths / (double) leftWristPoints.Count);
            }
        }
        public DepthPoint AvgTorsoPosition
        {
            get
            {
                double shoulderCenterSumX = 0;
                double shoulderCenterSumY = 0;
                double shoulderCenterSumZ = 0;

                double hipCenterSumX = 0;
                double hipCenterSumY = 0;
                double hipCenterSumZ = 0;

                double spineSumX = 0;
                double spineSumY = 0;
                double spineSumZ = 0;

                for (int i = 0; i < shoulderCenterPoints.Count; i++)
                {
                    shoulderCenterSumX += shoulderCenterPoints.ElementAt(i).x;
                    shoulderCenterSumY += shoulderCenterPoints.ElementAt(i).y;
                    shoulderCenterSumZ += shoulderCenterPoints.ElementAt(i).depth;

                    hipCenterSumX += hipCenterPoints.ElementAt(i).x;
                    hipCenterSumY += hipCenterPoints.ElementAt(i).y;
                    hipCenterSumZ += hipCenterPoints.ElementAt(i).depth;

                    spineSumX += spinePoints.ElementAt(i).x;
                    spineSumY += spinePoints.ElementAt(i).y;
                    spineSumZ += spinePoints.ElementAt(i).depth;
                }

                double avgShoulderX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                double avgShoulderY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                double avgShoulderZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;

                double avgSpineX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                double avgSpineY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                double avgSpineZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;

                double avgHipX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                double avgHipY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                double avgHipZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;

                double avgTorsoX = (avgShoulderX + avgSpineX + avgHipX) / 3.0;
                double avgTorsoY = (avgShoulderY + avgSpineY + avgHipY) / 3.0;
                double avgTorsoZ = (avgShoulderZ + avgSpineZ + avgHipZ) / 3.0;

                return new DepthPoint((int)avgTorsoX, (int)avgTorsoY, (long)avgTorsoZ);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leftWristJoint"></param>
        /// <param name="leftShoulderJoint"></param>
        /// <param name="rightWristJoint"></param>
        /// <param name="rightShoulderJoint"></param>
        public void computeArmLengthPixels(DepthPoint leftWristPoint, DepthPoint leftShoulderPoint, DepthPoint rightWristPoint, DepthPoint rightShoulderPoint)
        {
            totalArmPoints++;
            if (totalArmPoints > MIN_POINTS_BEFORE_READING && leftWristPoints.Count < MAX_POINTS_IN_READING)
            {
                leftWristPoints.Add(leftWristPoint);
                leftShoulderPoints.Add(leftShoulderPoint);
                rightWristPoints.Add(rightWristPoint);
                rightShoulderPoints.Add(rightShoulderPoint);
            }


            int leftArmX = leftWristPoint.x - leftShoulderPoint.x;
            int leftArmY = leftWristPoint.y - leftShoulderPoint.y;
            //int leftArmZ = leftWristPoint.Position.Z - leftShoulderPoint.Position.Z;
            int rightArmX = rightWristPoint.x - rightShoulderPoint.x;
            int rightArmY = rightWristPoint.y - rightShoulderPoint.y;
            //int rightArmZ = rightWristPoint.Position.Z - rightShoulderPoint.Position.Z;
            double leftArm = Math.Sqrt(leftArmX * leftArmX + leftArmY * leftArmY /*+ leftArmZ * leftArmZ*/);
            double rightArm = Math.Sqrt(rightArmX * rightArmX + rightArmY * rightArmY /*+ rightArmZ * rightArmZ*/);

            this.armLengthPixels = (int) ((leftArm + rightArm) / 2.0);

            /*
             * The length of both arms changes depending on the location of the person due to noise in data 
             * and also inaccuracy of depth cammera at too far and too close postions.
             * so we collect at least 50 data points of the four joint we need and calculate the avregae
             * 
             */
            /*
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
                //this.armLength /= (armCounter);
                this.armLength = (leftArm + rightArm) / 2;
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shoulderCenter"></param>
        /// <param name="spine"></param>
        /// <param name="hipCenter"></param>
        internal void computerTorsoDepth(DepthPoint shoulderCenter, DepthPoint spine, DepthPoint hipCenter)
        {
            totalTorsoPoints++;
            if (totalTorsoPoints > MIN_POINTS_BEFORE_READING && shoulderCenterPoints.Count < MAX_POINTS_IN_READING)
            {
                shoulderCenterPoints.Add(shoulderCenter);
                hipCenterPoints.Add(hipCenter);
                spinePoints.Add(spine);
            }


            long shoulderCenterZ = shoulderCenter.depth;
            long spineZ = spine.depth;
            long hipCenterZ = hipCenter.depth;

            this.torsoPosition = new DepthPoint((shoulderCenter.x + spine.x + hipCenter.x) / 3,
                                                (shoulderCenter.y + spine.y + hipCenter.y) / 3,
                                                (long) ((shoulderCenter.depth + spine.depth + hipCenter.depth) / 3));

            /*
            this.torsoPosition.x = (shoulderCenter.x + spine.x + hipCenter.x) / 3;
            this.torsoPosition.y = (shoulderCenter.y + spine.y + hipCenter.y) / 3;
            this.torsoPosition.depth = (shoulderCenter.depth + spine.Position.Z + hipCenter.Position.Z) / 3;
             * */

            /*
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
             */
        }
    }
}
