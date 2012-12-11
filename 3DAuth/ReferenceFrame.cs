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


        private Queue<DepthPoint> leftWristPoints;
        private Queue<DepthPoint> rightWristPoints;
        private Queue<DepthPoint> leftShoulderPoints;
        private Queue<DepthPoint> rightShoulderPoints;

        private Queue<DepthPoint> shoulderCenterPoints;
        private Queue<DepthPoint> spinePoints;
        private Queue<DepthPoint> hipCenterPoints;

        private Queue<DepthPoint> shoulderCenterPointsSupplemental;
        private Queue<DepthPoint> spinePointsSupplemental;
        private Queue<DepthPoint> hipCenterPointsSupplemental;

        private int totalArmPoints;
        private int totalTorsoPoints;

        private const int MIN_POINTS_BEFORE_READING = 10;

        private const int MAX_POINTS_IN_READING = 50;

        private const double Z_SCORE_SUM_BOUND = 3.0;

        private const double TORSO_LEARNING_ALPHA = 0.1; // torso learning alpha for after we have MAX_POINTS_IN_READING


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
            leftWristPoints = new Queue<DepthPoint>();
            rightWristPoints = new Queue<DepthPoint>();
            leftShoulderPoints = new Queue<DepthPoint>();
            rightShoulderPoints = new Queue<DepthPoint>();
            totalArmPoints = 0;
            totalTorsoPoints = 0;

            shoulderCenterPoints = new Queue<DepthPoint>();
            spinePoints = new Queue<DepthPoint>();
            hipCenterPoints = new Queue<DepthPoint>();

            shoulderCenterPointsSupplemental = new Queue<DepthPoint>();
            spinePointsSupplemental = new Queue<DepthPoint>();
            hipCenterPointsSupplemental = new Queue<DepthPoint>();
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

        private DepthPoint _avgTorsoPosition;
        public DepthPoint AvgTorsoPosition
        {
            get
            {
                if (_avgTorsoPosition != null)
                {
                    return _avgTorsoPosition;
                }
                double shoulderCenterSumX = 0;
                double shoulderCenterSumY = 0;
                double shoulderCenterSumZ = 0;

                double hipCenterSumX = 0;
                double hipCenterSumY = 0;
                double hipCenterSumZ = 0;

                double spineSumX = 0;
                double spineSumY = 0;
                double spineSumZ = 0;

                List<double> shoulderCenterX = new List<double>();
                List<double> shoulderCenterY = new List<double>();
                List<double> shoulderCenterZ = new List<double>();

                List<double> hipCenterX = new List<double>();
                List<double> hipCenterY = new List<double>();
                List<double> hipCenterZ = new List<double>();

                List<double> spineX = new List<double>();
                List<double> spineY = new List<double>();
                List<double> spineZ = new List<double>();

                if (shoulderCenterPoints.Count > 30)
                {
                    for (int i = 0; i < shoulderCenterPoints.Count; i++)
                    {
                        shoulderCenterX.Add(shoulderCenterPoints.ElementAt(i).x);
                        shoulderCenterY.Add(shoulderCenterPoints.ElementAt(i).y);
                        shoulderCenterZ.Add(shoulderCenterPoints.ElementAt(i).depth);

                        hipCenterX.Add(hipCenterPoints.ElementAt(i).x);
                        hipCenterY.Add(hipCenterPoints.ElementAt(i).y);
                        hipCenterZ.Add(hipCenterPoints.ElementAt(i).depth);

                        spineX.Add(spinePoints.ElementAt(i).x);
                        spineY.Add(spinePoints.ElementAt(i).y);
                        spineZ.Add(spinePoints.ElementAt(i).depth);
                    }
                    shoulderCenterX.Sort();
                    shoulderCenterY.Sort();
                    shoulderCenterZ.Sort();

                    hipCenterX.Sort();
                    hipCenterY.Sort();
                    hipCenterZ.Sort();

                    spineX.Sort();
                    spineY.Sort();
                    spineZ.Sort();

                    int numToRemove = shoulderCenterPoints.Count / 4;
                    for (int i = 0; i < numToRemove; i++)
                    {
                        shoulderCenterX.RemoveAt(0);
                        shoulderCenterY.RemoveAt(0);
                        shoulderCenterZ.RemoveAt(0);

                        hipCenterX.RemoveAt(0);
                        hipCenterY.RemoveAt(0);
                        hipCenterZ.RemoveAt(0);

                        spineX.RemoveAt(0);
                        spineY.RemoveAt(0);
                        spineZ.RemoveAt(0);


                        shoulderCenterX.RemoveAt(shoulderCenterX.Count - 1);
                        shoulderCenterY.RemoveAt(shoulderCenterY.Count - 1);
                        shoulderCenterZ.RemoveAt(shoulderCenterZ.Count - 1);

                        hipCenterX.RemoveAt(hipCenterX.Count - 1);
                        hipCenterY.RemoveAt(hipCenterZ.Count - 1);
                        hipCenterZ.RemoveAt(hipCenterZ.Count - 1);

                        spineX.RemoveAt(spineX.Count - 1);
                        spineY.RemoveAt(spineY.Count - 1);
                        spineZ.RemoveAt(spineZ.Count - 1);
                    }

                    shoulderCenterSumX = shoulderCenterX.Sum();
                    shoulderCenterSumY = shoulderCenterY.Sum();
                    shoulderCenterSumZ = shoulderCenterZ.Sum();

                    hipCenterSumX = hipCenterX.Sum();
                    hipCenterSumY = hipCenterY.Sum();
                    hipCenterSumZ = hipCenterZ.Sum();

                    spineSumX = spineX.Sum();
                    spineSumY = spineX.Sum();
                    spineSumZ = spineX.Sum();

                    /*
                    for (int i = 0; i < shoulderCenterX.Count; i++)
                    {
                        shoulderCenterSumX += shoulderCenterX.ElementAt(i).x;
                        shoulderCenterSumY += shoulderCenterY.ElementAt(i).y;
                        shoulderCenterSumZ += shoulderCenterZ.ElementAt(i).depth;

                        hipCenterSumX += hipCenterPoints.ElementAt(i).x;
                        hipCenterSumY += hipCenterPoints.ElementAt(i).y;
                        hipCenterSumZ += hipCenterPoints.ElementAt(i).depth;

                        spineSumX += spinePoints.ElementAt(i).x;
                        spineSumY += spinePoints.ElementAt(i).y;
                        spineSumZ += spinePoints.ElementAt(i).depth;
                    }
                     * */
                    /*
                    double avgShoulderX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                    double avgShoulderY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                    double avgShoulderZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;

                    double avgSpineX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                    double avgSpineY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                    double avgSpineZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;

                    double avgHipX = shoulderCenterSumX / (double)shoulderCenterPoints.Count;
                    double avgHipY = shoulderCenterSumY / (double)shoulderCenterPoints.Count;
                    double avgHipZ = shoulderCenterSumZ / (double)shoulderCenterPoints.Count;
                    */
                    double avgShoulderX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                    double avgShoulderY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                    double avgShoulderZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                    double avgSpineX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                    double avgSpineY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                    double avgSpineZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                    double avgHipX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                    double avgHipY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                    double avgHipZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                    double avgTorsoX = (avgShoulderX + avgSpineX + avgHipX) / 3.0;
                    double avgTorsoY = (avgShoulderY + avgSpineY + avgHipY) / 3.0;
                    double avgTorsoZ = (avgShoulderZ + avgSpineZ + avgHipZ) / 3.0;

                    DepthPoint torsoAvg = new DepthPoint((int)avgTorsoX, (int)avgTorsoY, (long)avgTorsoZ);

                    if (shoulderCenterPoints.Count == MAX_POINTS_IN_READING)
                    {
                        _avgTorsoPosition = torsoAvg;
                    }
                    return torsoAvg;
                }
                else
                {
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
                leftWristPoints.Enqueue(leftWristPoint);
                leftShoulderPoints.Enqueue(leftShoulderPoint);
                rightWristPoints.Enqueue(rightWristPoint);
                rightShoulderPoints.Enqueue(rightShoulderPoint);
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
            
            /*
            if (shoulderCenterPoints.Count == MAX_POINTS_IN_READING)
            {
                shoulderCenterPointsSupplemental.Enqueue(shoulderCenter);
                hipCenterPointsSupplemental.Enqueue(hipCenter);
                spinePointsSupplemental.Enqueue(spine);
            }
            */

            while (shoulderCenterPoints.Count > MAX_POINTS_IN_READING)
            {
                shoulderCenterPoints.Dequeue();
                hipCenterPoints.Dequeue();
                spinePoints.Dequeue();
            }

            while (shoulderCenterPointsSupplemental.Count > MAX_POINTS_IN_READING)
            {
                shoulderCenterPointsSupplemental.Dequeue();
                hipCenterPointsSupplemental.Dequeue();
                spinePointsSupplemental.Dequeue();
            }


            if (totalTorsoPoints > MIN_POINTS_BEFORE_READING && shoulderCenterPoints.Count < MAX_POINTS_IN_READING)
            {
                shoulderCenterPoints.Enqueue(shoulderCenter);
                hipCenterPoints.Enqueue(hipCenter);
                spinePoints.Enqueue(spine);
            }
            else
            {
                shoulderCenterPointsSupplemental.Enqueue(shoulderCenter);
                hipCenterPointsSupplemental.Enqueue(hipCenter);
                spinePointsSupplemental.Enqueue(spine);
            }

            // Alpha average between the supplemental points and our stored average torso pos
            
            if (HandTrackingOptionSet.AllowingTorsoMotion && shoulderCenterPointsSupplemental.Count > 30 && _avgTorsoPosition != null)
            {
                List<double> shoulderCenterX = new List<double>();
                List<double> shoulderCenterY = new List<double>();
                List<double> shoulderCenterZ = new List<double>();

                List<double> hipCenterX = new List<double>();
                List<double> hipCenterY = new List<double>();
                List<double> hipCenterZ = new List<double>();

                List<double> spineX = new List<double>();
                List<double> spineY = new List<double>();
                List<double> spineZ = new List<double>();

                for (int i = 0; i < shoulderCenterPoints.Count; i++)
                {
                    shoulderCenterX.Add(shoulderCenterPoints.ElementAt(i).x);
                    shoulderCenterY.Add(shoulderCenterPoints.ElementAt(i).y);
                    shoulderCenterZ.Add(shoulderCenterPoints.ElementAt(i).depth);

                    hipCenterX.Add(hipCenterPoints.ElementAt(i).x);
                    hipCenterY.Add(hipCenterPoints.ElementAt(i).y);
                    hipCenterZ.Add(hipCenterPoints.ElementAt(i).depth);

                    spineX.Add(spinePoints.ElementAt(i).x);
                    spineY.Add(spinePoints.ElementAt(i).y);
                    spineZ.Add(spinePoints.ElementAt(i).depth);
                }
                shoulderCenterX.Sort();
                shoulderCenterY.Sort();
                shoulderCenterZ.Sort();

                hipCenterX.Sort();
                hipCenterY.Sort();
                hipCenterZ.Sort();

                spineX.Sort();
                spineY.Sort();
                spineZ.Sort();

                int numToRemove = shoulderCenterPoints.Count / 4;
                for (int i = 0; i < numToRemove; i++)
                {
                    shoulderCenterX.RemoveAt(0);
                    shoulderCenterY.RemoveAt(0);
                    shoulderCenterZ.RemoveAt(0);

                    hipCenterX.RemoveAt(0);
                    hipCenterY.RemoveAt(0);
                    hipCenterZ.RemoveAt(0);

                    spineX.RemoveAt(0);
                    spineY.RemoveAt(0);
                    spineZ.RemoveAt(0);


                    shoulderCenterX.RemoveAt(shoulderCenterX.Count - 1);
                    shoulderCenterY.RemoveAt(shoulderCenterY.Count - 1);
                    shoulderCenterZ.RemoveAt(shoulderCenterZ.Count - 1);

                    hipCenterX.RemoveAt(hipCenterX.Count - 1);
                    hipCenterY.RemoveAt(hipCenterZ.Count - 1);
                    hipCenterZ.RemoveAt(hipCenterZ.Count - 1);

                    spineX.RemoveAt(spineX.Count - 1);
                    spineY.RemoveAt(spineY.Count - 1);
                    spineZ.RemoveAt(spineZ.Count - 1);
                }

                double shoulderCenterSumX = shoulderCenterX.Sum();
                double shoulderCenterSumY = shoulderCenterY.Sum();
                double shoulderCenterSumZ = shoulderCenterZ.Sum();

                double hipCenterSumX = hipCenterX.Sum();
                double hipCenterSumY = hipCenterY.Sum();
                double hipCenterSumZ = hipCenterZ.Sum();

                double spineSumX = spineX.Sum();
                double spineSumY = spineX.Sum();
                double spineSumZ = spineX.Sum();

                double avgShoulderX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                double avgShoulderY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                double avgShoulderZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                double avgSpineX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                double avgSpineY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                double avgSpineZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                double avgHipX = shoulderCenterSumX / (double)shoulderCenterX.Count;
                double avgHipY = shoulderCenterSumY / (double)shoulderCenterX.Count;
                double avgHipZ = shoulderCenterSumZ / (double)shoulderCenterX.Count;

                double avgTorsoX = (avgShoulderX + avgSpineX + avgHipX) / 3.0;
                double avgTorsoY = (avgShoulderY + avgSpineY + avgHipY) / 3.0;
                double avgTorsoZ = (avgShoulderZ + avgSpineZ + avgHipZ) / 3.0;

                _avgTorsoPosition = new DepthPoint((int) (_avgTorsoPosition.x * (1 - TORSO_LEARNING_ALPHA) + avgTorsoX * TORSO_LEARNING_ALPHA),
                                                   (int) (_avgTorsoPosition.y * (1 - TORSO_LEARNING_ALPHA) + avgTorsoY * TORSO_LEARNING_ALPHA),
                                                   (long) (_avgTorsoPosition.depth * (1 - TORSO_LEARNING_ALPHA) + avgTorsoZ * TORSO_LEARNING_ALPHA));
            }

            
            if (shoulderCenterPointsSupplemental.Count == MAX_POINTS_IN_READING)
            {
                // Determine if we should switch to the supplemental queue because the user has moved
                double torsoSumX = 0.0;
                double torsoSumY = 0.0;
                double torsoSumZ = 0.0;

                double torsoSDX = 0.0;
                double torsoSDY = 0.0;
                double torsoSDZ = 0.0;

                for (int i = 0; i < shoulderCenterPointsSupplemental.Count; i++)
                {
                    torsoSumX += (shoulderCenterPointsSupplemental.ElementAt(i).x +
                        hipCenterPointsSupplemental.ElementAt(i).x +
                        spinePointsSupplemental.ElementAt(i).x) / 3.0;
                    torsoSumY += (shoulderCenterPointsSupplemental.ElementAt(i).y +
                        hipCenterPointsSupplemental.ElementAt(i).y +
                        spinePointsSupplemental.ElementAt(i).y) / 3.0;
                    torsoSumZ += (shoulderCenterPointsSupplemental.ElementAt(i).depth +
                        hipCenterPointsSupplemental.ElementAt(i).depth +
                        spinePointsSupplemental.ElementAt(i).depth) / 3.0;
                }
                double avgTorsoX = torsoSumX / shoulderCenterPointsSupplemental.Count;
                double avgTorsoY = torsoSumY / shoulderCenterPointsSupplemental.Count;
                double avgTorsoZ = torsoSumZ / shoulderCenterPointsSupplemental.Count;
                for (int i = 0; i < shoulderCenterPointsSupplemental.Count; i++)
                {
                    torsoSDX += Math.Pow(((shoulderCenterPointsSupplemental.ElementAt(i).x +
                        hipCenterPointsSupplemental.ElementAt(i).x +
                        spinePointsSupplemental.ElementAt(i).x) / 3.0) - avgTorsoX, 2);
                    torsoSDY += Math.Pow(((shoulderCenterPointsSupplemental.ElementAt(i).y +
                        hipCenterPointsSupplemental.ElementAt(i).y +
                        spinePointsSupplemental.ElementAt(i).y) / 3.0) - avgTorsoY, 2);
                    torsoSDZ += Math.Pow(((shoulderCenterPointsSupplemental.ElementAt(i).depth +
                        hipCenterPointsSupplemental.ElementAt(i).depth +
                        spinePointsSupplemental.ElementAt(i).depth) / 3.0) - avgTorsoZ, 2);
                }
                torsoSDX /= (shoulderCenterPointsSupplemental.Count - 1);
                torsoSDY /= (shoulderCenterPointsSupplemental.Count - 1);
                torsoSDZ /= (shoulderCenterPointsSupplemental.Count - 1);
                DepthPoint current = AvgTorsoPosition;
                double xZScore = Math.Abs((current.x - avgTorsoX) / torsoSDX);
                double yZScore = Math.Abs((current.y - avgTorsoY) / torsoSDY);
                double zZScore = Math.Abs((current.depth - avgTorsoZ) / torsoSDZ);
                if (xZScore + yZScore + zZScore > Z_SCORE_SUM_BOUND)
                {
                    //Console.WriteLine("Adjusting torso center, z score: " + (xZScore + yZScore + zZScore));

                    shoulderCenterPoints = shoulderCenterPointsSupplemental;
                    spinePoints = spinePointsSupplemental;
                    hipCenterPoints = hipCenterPointsSupplemental;

                    shoulderCenterPointsSupplemental.Clear();
                    spinePointsSupplemental.Clear();
                    hipCenterPointsSupplemental.Clear();
                }
                else
                {
                    while (shoulderCenterPointsSupplemental.Count > MAX_POINTS_IN_READING)
                    {
                        shoulderCenterPointsSupplemental.Dequeue();
                        spinePointsSupplemental.Dequeue();
                        hipCenterPointsSupplemental.Dequeue();
                    }
                }
            }

            //long shoulderCenterZ = shoulderCenter.depth;
            //long spineZ = spine.depth;
            //long hipCenterZ = hipCenter.depth;

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
