using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;


        /// <summary>
        /// Initializes a new instance of the Graph class.
        /// </summary>
        public Graph()
        {
            //InitializeComponent();
        }


        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            GraphImageBox.Source = this.imageSource;
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }



        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //**************
                        /***
                         * This parts makes sure that the skeleton is being tracked and after it can track all the 
                         * four joints that we need(left and right shoulder, left and right wrist) we send the information of
                         * these joints to compute the length of the arm.
                         * 
                         */
                        
                        if (skel.TrackingState.Equals(SkeletonTrackingState.Tracked))
                        {
                            ThreeDAuth.ReferenceFrame myFrame = new ThreeDAuth.ReferenceFrame();
                            if (skel.Joints[JointType.ShoulderLeft].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.ShoulderRight].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.WristLeft].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.WristRight].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.ShoulderCenter].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.Spine].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.HipCenter].TrackingState.Equals(JointTrackingState.Tracked))
                            {
                                Joint leftShoulder = skel.Joints[JointType.ShoulderLeft];
                                Joint leftWrist = skel.Joints[JointType.WristLeft];
                                Joint rightShoulder = skel.Joints[JointType.ShoulderRight];
                                Joint rightWrist = skel.Joints[JointType.WristRight];
                                Joint shoulderCenter = skel.Joints[JointType.ShoulderCenter];
                                Joint hipCenter = skel.Joints[JointType.HipCenter];
                                Joint spine = skel.Joints[JointType.Spine];
                                myFrame.computeArmLength(leftWrist, leftShoulder, rightWrist, rightShoulder);
                                myFrame.computerTorsoDepth(shoulderCenter, spine, hipCenter);


                                if (myFrame.torsoPosition != null)
                                {
                                    ThreeDAuth.FlatPlane myPlane = new ThreeDAuth.FlatPlane(myFrame.torsoPosition, myFrame.armLength * .8);
                                    ThreeDAuth.Point3d wristRight = new ThreeDAuth.Point3d(rightWrist.Position.X, rightWrist.Position.Y, rightWrist.Position.Z);
                                    if (myPlane.crossesPlane(wristRight))
                                    {
                                        System.Console.WriteLine("You crossed the plane");
                                    }
                                }
                            }                           
                        }
                       
                        
                        //****************
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
        /*
        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {


            /** Begin Add By Mason **/

            // Draw targetbox with some dummy values
            ThreeDAuth.ITargetBoxScheme scheme = new ThreeDAuth.RigidTargetBoxScheme(1.0f, 1.0f);
            ThreeDAuth.Point3d torsoCenter = new ThreeDAuth.Point3d(0, 0, 0);

            double armLength = 0.5f;
            double torsoDepth = 0.5f;
            ThreeDAuth.TargetBox box = new ThreeDAuth.TargetBox(scheme, torsoCenter, armLength, torsoDepth);
            Pen redPen = new Pen(Brushes.Red, 3);
            ThreeDAuth.Vec2d[] lines = box.getBoxLines();
            for (int i = 0; i < 4; i++)
            {
                drawingContext.DrawLine(redPen, new Point(lines[i].p1.X, lines[i].p1.Y), new Point(lines[i].p2.X, lines[i].p2.Y));
            }

            /** End Add By Mason **/

            if (userImage != null)
            {
                drawingContext.DrawImage(userImage, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
            

            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }

            
            
        }
        */

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        /*
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
        */
    }
}
