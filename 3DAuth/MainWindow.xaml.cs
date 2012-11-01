﻿
//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Win32;
    using System;
    using System.Windows.Media.Imaging;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        //Start By Siavash

        private DrawingGroup liveFeedbackGroup;

        /// <summary>
        /// To draw the hands on the selected image
        /// </summary>
        private DrawingImage handSource;

        /// <summary>
        /// user selected image
        /// </summary>
        private BitmapImage userImage;

        private ThreeDAuth.ReferenceFrame myFrame;

        private Joint rightWrist;
        private Joint leftWrist;
        //End by Siavash

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.WindowState = WindowState.Maximized;
            InitializeComponent();

        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));

            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
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

            Image.Source = this.imageSource;

            //start by Siavash
            // Display the drawing using our image control

            this.liveFeedbackGroup = new DrawingGroup();

            this.myFrame = new ThreeDAuth.ReferenceFrame();

            this.handSource = new DrawingImage(this.liveFeedbackGroup);

            myImageBox.Source = this.handSource;


            //End by siavash

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
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

            //Start Siavash
            using (DrawingContext lfdc = this.liveFeedbackGroup.Open())
            {

                // Draw a transparent background to set the render size
                lfdc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {

                        /***
                         * This parts makes sure that the skeleton is being tracked and after it can track all the 
                         * four joints that we need(left and right shoulder, left and right wrist) we send the information of
                         * these joints to compute the length of the arm.
                         * 
                         */

                        if (skel.TrackingState.Equals(SkeletonTrackingState.Tracked))
                        {

                            if (!skel.Joints[JointType.WristLeft].TrackingState.Equals(JointTrackingState.NotTracked))
                            {
                                leftWrist = skel.Joints[JointType.WristLeft];
                            }

                            if (!skel.Joints[JointType.WristRight].TrackingState.Equals(JointTrackingState.NotTracked))
                            {
                                rightWrist = skel.Joints[JointType.WristRight];
                            }

                            if (skel.Joints[JointType.ShoulderLeft].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.ShoulderRight].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.WristLeft].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.WristRight].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.ShoulderCenter].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.Spine].TrackingState.Equals(JointTrackingState.Tracked)
                                && skel.Joints[JointType.HipCenter].TrackingState.Equals(JointTrackingState.Tracked))
                            {
                                Joint leftShoulder = skel.Joints[JointType.ShoulderLeft];
                                Joint leftWristTemp = skel.Joints[JointType.WristLeft];
                                Joint rightShoulder = skel.Joints[JointType.ShoulderRight];
                                Joint rightWristTemp = skel.Joints[JointType.WristRight];
                                Joint shoulderCenter = skel.Joints[JointType.ShoulderCenter];
                                Joint hipCenter = skel.Joints[JointType.HipCenter];
                                Joint spine = skel.Joints[JointType.Spine];
                                myFrame.computeArmLength(leftWristTemp, leftShoulder, rightWristTemp, rightShoulder);
                                myFrame.computerTorsoDepth(shoulderCenter, spine, hipCenter);
                            }
                        }

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.drawHands(lfdc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            this.drawHands(lfdc);
                        }
                    }
                    // prevent drawing outside of our render area
                    this.liveFeedbackGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
            }

            //End by Siavash



            using (DrawingContext dc = this.drawingGroup.Open())
            {

                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
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
                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
            }
        }

        private void drawHands(DrawingContext drawingContext)
        {
            //Start Siavash
            if (userImage != null)
            {
                drawingContext.DrawImage(userImage, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
            //End Siavash

            if (myFrame.torsoPosition != null)
            {
                // Anton's code
                // when a new frame is available, we check if the wrists are crossing the plane and we draw an appropriately colored
                // rectangle over them to give the user feedback

                ThreeDAuth.FlatPlane myPlane = new ThreeDAuth.FlatPlane(myFrame.torsoPosition, myFrame.armLength * .6);
                ThreeDAuth.Point3d wristRight = new ThreeDAuth.Point3d(rightWrist.Position.X, rightWrist.Position.Y, rightWrist.Position.Z);

                Point right = this.SkeletonPointToScreen(rightWrist.Position);

                if (myPlane.crossesPlane(wristRight))
                {
                    drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(right.X, right.Y, 30, 30), null, 14, null, 14, null);
                    System.Console.WriteLine("You crossed the plane");
                }
                else
                {
                    drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(right.X, right.Y, 30, 30), null, 14, null, 14, null);
                }


                ThreeDAuth.Point3d wristLeft = new ThreeDAuth.Point3d(leftWrist.Position.X, leftWrist.Position.Y, leftWrist.Position.Z);

                Point left = this.SkeletonPointToScreen(leftWrist.Position);

                if (myPlane.crossesPlane(wristLeft))
                {
                    drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(left.X, left.Y, 30, 30), null, 14, null, 14, null);
                    System.Console.WriteLine("You crossed the plane");
                }
                else
                {
                    drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(left.X, left.Y, 30, 30), null, 14, null, 14, null);
                }
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {

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

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
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

        /*
        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
        */

        /// <summary>
        /// Opens a file browser to select and image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void New_Account_Click(object sender, RoutedEventArgs e)
        {




            string fileName = "";
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Title = "Select Your Image";
            browseFile.InitialDirectory = @"Libraries\Pictures";
            browseFile.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            browseFile.FilterIndex = 2;
            browseFile.RestoreDirectory = true;
            browseFile.ShowDialog();
            try
            {
                fileName = browseFile.FileName;
                userImage = new BitmapImage(new Uri(fileName));
                //this.myImageBox.Source = userImage;
                this.myImageBox.Visibility = Visibility.Visible;
                this.myImageBox.Source = userImage;
                this.myImageBox.Source = handSource;
                New_Account.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening file", "File Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
        }
    }
}