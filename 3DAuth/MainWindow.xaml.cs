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
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Drawing.Imaging;
    using ThreeDAuth;
    using System.Xml;

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


        private XmlDocument data;
        private Joint rightWrist;
        private Joint leftWrist;
        private short[] imadeData;
        private DepthImagePixel[] imagePixelData;
        private int pixelIndex;
        private int maxDepth;
        private int minDepth;
        private DepthImagePixel closestPoint;
        private System.Drawing.Bitmap bmap;
        private User currentUser;
        public static int faceScanCounter = -1;
        private Boolean isUserNew = false;
        public static int faceScanCount = 0;
        private Boolean isfaceTrackerOn = false;
        //End by Siavash

        /// <summary>
        /// Anton
        /// Point Distributor to implement observer pattern
        /// </summary>
        private ThreeDAuth.PointDistributor pDistributor;

        /// <summary>
        /// Anton
        /// Gesture learner
        /// </summary>
        private ThreeDAuth.DiscreteGestureLearner gLearner;

        /// <summary>
        /// Mason
        /// Gesture validator
        /// </summary>
        private ThreeDAuth.GestureValidator gValidator;

        /// <summary>
        /// Mason
        /// Depth cutoff for flood fill in mm
        /// </summary>
        private const int DEPTH_CUTOFF = 50;

        /// <summary>
        /// Mason
        /// Gaussian filter on position and motion
        /// </summary>
        private ThreeDAuth.PositionMotionFilter positionMotionFilter;


        private System.Diagnostics.Stopwatch outOfPlaneTimer;
        private System.Diagnostics.Stopwatch resetButtonTimer;
        private System.Diagnostics.Stopwatch doneButtonTimer;

        private const long BUTTON_HOLD_CUTOFF = 2000; // ms

        private static MainWindow instance; // make it a singleton to allow us to use skeleton projections from the targetBox class


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.WindowState = WindowState.Maximized;
            InitializeComponent();

            // Anton
            //var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensor };

            positionMotionFilter = new ThreeDAuth.PositionMotionFilter();
            outOfPlaneTimer = new System.Diagnostics.Stopwatch();
            resetButtonTimer = new System.Diagnostics.Stopwatch();
            doneButtonTimer = new System.Diagnostics.Stopwatch();
            MainWindow.instance = this;
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

            pDistributor = ThreeDAuth.PointDistributor.GetInstance();

            // create a new gesture learner
            gLearner = new ThreeDAuth.DiscreteGestureLearner(2000, 20);

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

                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();
                // Turn on the depth image stream to receive skeleton frames
                this.sensor.DepthStream.Enable();

                //this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                //End Siavash

                // Add an event handler to be called whenever there is new color frame data
                //this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                this.sensor.AllFramesReady += this.OnAllFramesReady;

                //faceTrackingViewer.setSensor(this.sensor); 

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
                System.Console.WriteLine("The Kinect sensor is not ready");
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            if (faceScanCounter == 0)
            {
                if (this.progressBar1.Value == 100)
                {
                    faceScanCount = 0;
                }
                else
                {
                    this.progressBar1.Value = faceScanCount * 2;
                }


            }
            if (faceScanCounter == 1)
            {
                if (this.progressBar2.Value == 100)
                {
                    faceScanCount = 0;

                }
                else
                {

                    this.progressBar2.Value = faceScanCount * 2;
                }

            }
            if (faceScanCounter == 2)
            {
                if (this.progressBar3.Value == 100)
                {
                    faceScanCount = 0;

                }
                else
                {
                    this.progressBar3.Value = faceScanCount * 2;
                }

            }
           
        }

        private int closestPointCounter = 0;
        private int CLOSEST_POINT_COUNTER_CUTOFF = 1;

        /// <summary>
        /// Start Siavash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    this.maxDepth = depthFrame.MaxDepth;
                    this.minDepth = depthFrame.MinDepth;
                    this.closestPoint.Depth = Convert.ToInt16(this.maxDepth);

                    if (this.imagePixelData == null || this.imadeData == null)
                    {
                        this.imadeData = new short[depthFrame.PixelDataLength];
                        this.imagePixelData = new DepthImagePixel[depthFrame.PixelDataLength];
                    }

                    depthFrame.CopyDepthImagePixelDataTo(imagePixelData);
                    depthFrame.CopyPixelDataTo(imadeData);

                    // This cutoff allows us to control how often the closest point is calculated (it doesn't necessarily need to be calculated every frame)
                    closestPointCounter %= CLOSEST_POINT_COUNTER_CUTOFF;
                    if (closestPointCounter == 0)
                    {
                        findTheClosestPoint(depthFrame.PixelDataLength, depthFrame.Width, depthFrame.Height);
                    }
                    closestPointCounter++;



                    // Flood fill from this point then send a point to the distributor

                    // If the nearest point hasn't broken the plane, then don't bother doing anything with it


                    // The array index is computed as x + y*width,
                    // So x = idx % width
                    // y = (idx - x)/width
                    int xIdx = this.pixelIndex % depthFrame.Width;
                    int yIdx = this.pixelIndex / depthFrame.Width;

                    myPointCluster = ThreeDAuth.Util.FloodFill2(imagePixelData, xIdx, yIdx, depthFrame.Width, depthFrame.Height - 1, DEPTH_CUTOFF);
                    ThreeDAuth.DepthPoint centroid = myPointCluster.Centroid;


                    // send centroid to filter and draw if valid
                    int counter = 0;
                    while (myPointCluster.points.Count < 50 && counter < 20)
                    {
                        imagePixelData[pixelIndex].Depth = short.MaxValue;
                        counter++;

                        findTheClosestPoint(depthFrame.PixelDataLength, depthFrame.Width, depthFrame.Height);
                        xIdx = this.pixelIndex % depthFrame.Width;
                        yIdx = this.pixelIndex / depthFrame.Width;

                        myPointCluster = ThreeDAuth.Util.FloodFill2(imagePixelData, xIdx, yIdx, depthFrame.Width, depthFrame.Height - 1, DEPTH_CUTOFF);
                        centroid = myPointCluster.Centroid;
                    }
                    if (counter < 20)
                    {
                        using (DrawingContext dc = this.liveFeedbackGroup.Open())
                        {
                            drawHands(dc, centroid, positionMotionFilter.IsValidPoint(centroid), depthFrame);
                        }
                    }
                    //showDepthView(depthFrame, depthFrame.Width, depthFrame.Height,centroid);
                    //pDistributor.GivePoint(centroid);

                    //Console.WriteLine("Centroid: " + centroid);
                    //ThreeDAuth.PointDistributor.SGivePoint(centroid);
                }
            }
        }

        private ThreeDAuth.PointCluster myPointCluster;

        /// <summary>
        /// Siavash
        /// </summary>
        /// <param name="depthFrame"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void showDepthView(DepthImageFrame depthFrame, int p1, int p2, ThreeDAuth.DepthPoint hand)
        {
            bmap = new System.Drawing.Bitmap(depthFrame.Width, depthFrame.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            System.Drawing.Imaging.BitmapData bmapdata = bmap.LockBits(new System.Drawing.Rectangle(0, 0, depthFrame.Width
                , depthFrame.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            //Marshal.Copy(imadeData, 0, ptr, depthFrame.Width * depthFrame.Height);
            bmap.UnlockBits(bmapdata);
            /*System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmap);
            this.myImageBox.Source =
            System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight((int)this.myImageBox.Width, (int)this.myImageBox.Height));*/

            ThreeDAuth.BoundingRectangle rect = ThreeDAuth.BoundingRectangle.CreateBoundingRectangle(myPointCluster);
            using (DrawingContext lfdc = this.liveFeedbackGroup.Open())
            {
                lfdc.DrawImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight((int)this.myImageBox.Width, (int)this.myImageBox.Height)),
                new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (userImage != null)
                {
                    lfdc.DrawImage(userImage, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }

                //Console.WriteLine(myPointCluster.points.Count);
                /*
                foreach (ThreeDAuth.DepthPoint point in myPointCluster.points)
                {
                    lfdc.DrawRoundedRectangle(Brushes.Red, null, new Rect(point.x, point.y, 3, 3), null, 1, null, 1, null);
                }
                    
                int xPos = badPoint % depthFrame.Width;
                int yPos = badPoint / depthFrame.Width;
                lfdc.DrawRoundedRectangle(Brushes.Green, null, new Rect(xPos - 15, yPos - 15, 30, 30), null, 14, null, 14, null);
                if (rightWrist.TrackingState == JointTrackingState.Tracked)
                {
                    ThreeDAuth.DepthPoint right = this.SkeletonPointToScreen(rightWrist.Position);
                    lfdc.DrawRoundedRectangle(Brushes.Gold, null, new Rect(right.x - 15, right.y - 15, 30, 30), null, 14, null, 14, null);
                } 
                 */
                //lfdc.DrawRoundedRectangle(Brushes.Blue, null, new Rect(hand.x - 15, hand.y - 15, 30, 30), null, 14, null, 14, null);
                ThreeDAuth.PointDistributor.SGivePoint(hand);
            }

        }

        private int NEIGHBOR_CUTOFF = 50; // mm
        private int badPoint;


        /// <summary>
        /// Siavash
        /// 
        /// </summary>
        /// <param name="pixelDataLenght"></param>
        private void findTheClosestPoint(int pixelDataLenght, int windowWidth, int windowHeight)
        {
            int i = 0;
            int closestBadPoint = -1;
            int closestBadPointDepth = 10000000;
            for (i = 0; i < pixelDataLenght; )
            {
                if (this.imagePixelData[i].IsKnownDepth == true)
                {
                    short currentDepth = this.imagePixelData[i].Depth;
                    if (currentDepth > minDepth && currentDepth < this.closestPoint.Depth)
                    {
                        int leftIdx = i > 0 ? i - 1 : i;
                        int rightIdx = i < pixelDataLenght + 1 ? i + 1 : i;
                        int upIdx = i > windowWidth ? i - windowWidth : i;
                        int downIdx = i < windowHeight * windowWidth - windowWidth ? i + windowWidth : i;
                        int goodCount = 0;
                        if (Math.Abs(this.imagePixelData[leftIdx].Depth - currentDepth) < NEIGHBOR_CUTOFF) goodCount++;
                        if (Math.Abs(this.imagePixelData[leftIdx].Depth - currentDepth) < NEIGHBOR_CUTOFF) goodCount++;
                        if (Math.Abs(this.imagePixelData[leftIdx].Depth - currentDepth) < NEIGHBOR_CUTOFF) goodCount++;
                        if (Math.Abs(this.imagePixelData[leftIdx].Depth - currentDepth) < NEIGHBOR_CUTOFF) goodCount++;

                        if (goodCount >= 3)
                        {
                            this.closestPoint = this.imagePixelData[i];
                            this.pixelIndex = i;
                        }
                        else
                        {
                            if (currentDepth < closestBadPointDepth)
                            {
                                closestBadPoint = i;
                                closestBadPointDepth = currentDepth;
                            }
                        }
                    }
                }
                i = i + 2;
            }
            badPoint = closestBadPoint;

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
            /*using (DrawingContext lfdc = this.liveFeedbackGroup.Open())
            {

                // Draw a transparent background to set the render size
                if (userImage == null)
                {
                    lfdc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
                else
                {
                    lfdc.DrawImage(userImage, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            
                }
            */
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

                            ThreeDAuth.DepthPoint leftWristDepthPoint = this.SkeletonPointToScreen(leftWristTemp.Position);
                            ThreeDAuth.DepthPoint leftShoulderDepthPoint = this.SkeletonPointToScreen(leftShoulder.Position);
                            ThreeDAuth.DepthPoint rightWristDepthPoint = this.SkeletonPointToScreen(rightWristTemp.Position);
                            ThreeDAuth.DepthPoint rightShoulderDepthPoint = this.SkeletonPointToScreen(rightShoulder.Position);
                            myFrame.computeArmLengthPixels(leftWristDepthPoint, leftShoulderDepthPoint, rightWristDepthPoint, rightShoulderDepthPoint);
                            myFrame.computeArmLength(leftWristTemp, leftShoulder, rightWristTemp, rightShoulder);

                            ThreeDAuth.DepthPoint shoulderDepthPoint = this.SkeletonPointToScreen(shoulderCenter.Position);
                            ThreeDAuth.DepthPoint spineDepthPoint = this.SkeletonPointToScreen(spine.Position);
                            ThreeDAuth.DepthPoint hipCenterDepthPoint = this.SkeletonPointToScreen(hipCenter.Position);
                            myFrame.computerTorsoDepth(shoulderDepthPoint, spineDepthPoint, hipCenterDepthPoint);
                        }
                    }

                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        //this.drawHands(lfdc);
                    }
                    else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        //this.drawHands(lfdc);
                    }
                }
                // prevent drawing outside of our render area
                this.liveFeedbackGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
            //}

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
                            this.SkeletonPointToScreen(skel.Position).GetPoint(),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
            }
        }

        private void DrawWedge(DrawingContext drawingContext, int centerX, int centerY, SweepDirection sweepDirection, double percentage)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            StreamGeometry streamGeo = new StreamGeometry();
            
            double sweepRadians = (Math.PI / 2.0) * percentage;

            // Determine start point
            double startX = centerX;
            double startY = centerY;

            // Determine end point
            double endX = Math.Cos(sweepRadians) * 50.0;
            double endY = Math.Sin(sweepRadians) * 50.0;

            if (sweepDirection == SweepDirection.Clockwise)
            {
                endX = centerX - endX;
            }

            using (StreamGeometryContext sgc = streamGeo.Open())
            {
                sgc.BeginFigure(new System.Windows.Point(startX, startY), true, true);
                sgc.ArcTo(new System.Windows.Point(endX, endY), new Size(50.0, 50.0), sweepRadians, false, sweepDirection, false, true); 
            }
            geometryDrawing.Geometry = streamGeo;
            drawingContext.DrawGeometry(Brushes.Red, null, geometryDrawing.Geometry);
        }

        private void drawHands(DrawingContext drawingContext, ThreeDAuth.DepthPoint hand, bool drawHand, DepthImageFrame depthFrame)
        {
            //Start Siavash

            double resetButtonTimePercentage = 0.0;
            double doneButtonTimePercentage = 0.0;
            if (userImage != null)
            {
                drawingContext.DrawImage(userImage, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                // Draw the reset button regardless
                //Pen buttonPen = new Pen(Brushes.Black, 0.1);
                //Brush resetButtonBrush = new SolidColorBrush(Color.FromRgb());
                //drawingContext
                if (resetButtonTimer.IsRunning)
                {
                    resetButtonTimePercentage = (double) resetButtonTimer.ElapsedMilliseconds / (double) BUTTON_HOLD_CUTOFF;
                }
                if (resetButtonTimePercentage > 1.0) resetButtonTimePercentage = 1.0;
                drawingContext.DrawEllipse(Brushes.White, null, new System.Windows.Point(0, 0), 50, 50);
                DrawWedge(drawingContext, 0, 0, SweepDirection.Counterclockwise, resetButtonTimePercentage); 
                drawingContext.DrawText(
                    new FormattedText("Reset", 
                                      System.Globalization.CultureInfo.CurrentCulture, 
                                      FlowDirection.LeftToRight, 
                                      new Typeface("Arial"), 
                                      14.0, 
                                      Brushes.Black), 
                    new System.Windows.Point(3, 12.5));

                // Draw the done button if we're learning
                if (gLearner.isRecording)
                {
                    if (doneButtonTimer.IsRunning)
                    {
                        doneButtonTimePercentage = (double)doneButtonTimer.ElapsedMilliseconds / (double)BUTTON_HOLD_CUTOFF;
                    }
                    if (doneButtonTimePercentage > 1.0) doneButtonTimePercentage = 1.0;
                    drawingContext.DrawEllipse(Brushes.White, null, new System.Windows.Point(depthFrame.Width, 0), 50, 50);
                    DrawWedge(drawingContext, depthFrame.Width, 0, SweepDirection.Clockwise, doneButtonTimePercentage);
                    drawingContext.DrawText(
                        new FormattedText("Done",
                                          System.Globalization.CultureInfo.CurrentCulture,
                                          FlowDirection.LeftToRight,
                                          new Typeface("Arial"),
                                          14.0,
                                          Brushes.Black),
                        new System.Windows.Point(depthFrame.Width - 35.0, 12.5));
                }
            }

            //End Siavash
            drawHand = true;
            if (drawHand)
            {
                if (myFrame.AvgTorsoPosition != null)
                //if (myFrame.torsoPosition != null)
                {
                    // Anton's code
                    // when a new frame is available, we check if the wrists are crossing the plane and we draw an appropriately colored
                    // rectangle over them to give the user feedback
                    double planeDepth = myFrame.armLength * .8;
                    int planeDepthPixels = (int)((planeDepth / myFrame.armLength) * myFrame.AvgArmLengthPixels);
                    //ThreeDAuth.FlatPlane myPlane = new ThreeDAuth.FlatPlane(myFrame.torsoPosition, planeDepth);
                    ThreeDAuth.FlatPlane myPlane = new ThreeDAuth.FlatPlane(myFrame.AvgTorsoPosition, planeDepth);
                    //Console.WriteLine("Torso depth: " + torsoSkeletonPoint.Z);
                    //ThreeDAuth.Point3d wristRight = new ThreeDAuth.Point3d(rightWrist.Position.X, rightWrist.Position.Y, rightWrist.Position.Z);

                    //ThreeDAuth.DepthPoint right = this.SkeletonPointToScreen(rightWrist.Position);

                    //ThreeDAuth.PlanePoint arrived = new ThreeDAuth.PlanePoint(right.x, right.y, myPlane.crossesPlane(right));

                    //pDistributor.GivePoint(arrived);
                    //Console.WriteLine("Depth: " + hand.depth);

                    short planeDepthmm = (short)(planeDepth * 1000); // convert m to mm
                    Tuple<int, int> handTuple = new Tuple<int, int>(hand.x, hand.y);
                    handTuple = ProjectPoint(handTuple,
                                             myFrame.AvgTorsoPosition,
                                             myFrame.AvgArmLengthPixels,
                                             planeDepthPixels,
                                             .9,
                                             depthFrame.Width,
                                             depthFrame.Height);
                    if (handTuple == null)
                    {
                        // encountered a problem and won't be able to project this point, so drop this frame
                        return;
                    }

                    ThreeDAuth.PlanePoint planePoint = new ThreeDAuth.PlanePoint(handTuple.Item1, handTuple.Item2, myPlane.crossesPlane(hand));
                    //ThreeDAuth.PlanePoint planePoint = new ThreeDAuth.PlanePoint(hand.x, hand.y, myPlane.crossesPlane(hand));
                    pDistributor.GivePoint(planePoint);

                    //drawingContext.DrawRoundedRectangle(Brushes.Green, null, new Rect(hand.x, hand.y, 30, 30), null, 14, null, 14, null);

                    // Check if the point is in the buttons

                    // reset button
                    bool inButton = false;
                    if (Math.Sqrt((planePoint.x * planePoint.x) + (planePoint.y * planePoint.y)) < 50.0)
                    {
                        // in reset button
                        inButton = true;
                        if (!resetButtonTimer.IsRunning)
                        {
                            resetButtonTimer.Start();
                        }
                    }
                    else
                    {
                        if (resetButtonTimer.IsRunning)
                        {
                            resetButtonTimer.Reset();
                        }
                    }

                    // done button
                    if (gLearner.isRecording)
                    {
                        if (Math.Sqrt(((planePoint.x - depthFrame.Width) * (planePoint.x - depthFrame.Width)) + 
                                      (planePoint.y * planePoint.y)) < 50.0)
                        {
                            // in done button
                            inButton = true;
                            if (!doneButtonTimer.IsRunning)
                            {
                                doneButtonTimer.Start();
                            }
                        }
                        else
                        {
                            if (doneButtonTimer.IsRunning)
                            {
                                doneButtonTimer.Reset();
                            }
                        }
                    }

                    drawingContext.DrawRoundedRectangle(Brushes.Yellow, null, new Rect(hand.x, hand.y, 30, 30), null, 14, null, 14, null);

                    //if (arrived.inPlane)
                    if (planePoint.inPlane)
                    {
                        //drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(right.x, right.y, 30, 30), null, 14, null, 14, null);
                        //drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(hand.x, hand.y, 30, 30), null, 14, null, 14, null);
                        drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(planePoint.x, planePoint.y, 30, 30), null, 14, null, 14, null);
                    }
                    else
                    {
                        //drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(right.x, right.y, 30, 30), null, 14, null, 14, null);
                        //drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(hand.x, hand.y, 30, 30), null, 14, null, 14, null);
                        drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(planePoint.x, planePoint.y, 30, 30), null, 14, null, 14, null);
                    }


                    ThreeDAuth.Point3d wristLeft = new ThreeDAuth.Point3d(leftWrist.Position.X, leftWrist.Position.Y, leftWrist.Position.Z);

                    ThreeDAuth.DepthPoint left = this.SkeletonPointToScreen(leftWrist.Position);

                    if (myPlane.crossesPlane(left))
                    {
                        //drawingContext.DrawRoundedRectangle(Brushes.Blue, null, new Rect(left.X, left.Y, 30, 30), null, 14, null, 14, null);
                    }
                    else
                    {
                        //drawingContext.DrawRoundedRectangle(Brushes.Red, null, new Rect(left.X, left.Y, 30, 30), null, 14, null, 14, null);
                    }



                    // Mason's code
                    // If we're learning a gesture, draw the learned points
                    if (gLearner != null && gLearner.isRecording)
                    {
                        System.Collections.Generic.Queue<ThreeDAuth.Point2d> currentPoints = gLearner.getGesturePath();
                        foreach (ThreeDAuth.Point2d point in currentPoints)
                        {
                            drawingContext.DrawRoundedRectangle(Brushes.Green, null, new Rect(point.x, point.y, 30, 30), null, 14, null, 14, null);

                        }
                    }

                    if (gLearner != null && gLearner.isRecording && inButton && doneButtonTimePercentage >= 1.0)
                    {
                        // Done recording
                        gLearner.stopRecording();
                        this.myImageBox.Visibility = System.Windows.Visibility.Collapsed;
                        List<ThreeDAuth.Point2d> password = new List<ThreeDAuth.Point2d>(gLearner.getGesturePath());
                        this.currentUser.password = password;
                        this.gestureMassage.Text = "Success. Your account has been created. Welcome to 3DAuth!";
                        SaveUser(currentUser);
                    }

                    if (inButton && resetButtonTimePercentage >= 1.0)
                    {
                        if (gLearner != null && gLearner.isRecording)
                        {
                            // Reset the learner
                            gLearner.restart();
                        }
                        else if (gValidator != null)
                        {
                            // Reset the validator
                            gValidator.restart();
                        }
                    }

                    /*
                    // If we're learning a gesture and we've been out of the plane for 5 seconds, stop learning
                    if (gLearner.isRecording && !planePoint.inPlane)
                    {
                        if (!outOfPlaneTimer.IsRunning)
                        {
                            outOfPlaneTimer.Start();
                        }
                        if (outOfPlaneTimer.ElapsedMilliseconds > BUTTON_HOLD_CUTOFF)
                        {
                            gLearner.stopRecording();
                            this.myImageBox.Visibility = System.Windows.Visibility.Collapsed;
                            List<ThreeDAuth.Point2d> password = new List<ThreeDAuth.Point2d>(gLearner.getGesturePath());
                            this.currentUser.password = password;
                            this.gestureMassage.Text = "Success. Your account has been created. Welcome to 3DAuth!";
                            SaveUser(currentUser);
                        }
                    }
                    else if (gLearner.isRecording)
                    {
                        outOfPlaneTimer.Reset();
                    }
                     * */
                }
            }
        }

        private enum StoredDataType
        {
            REFERENCE,
            USERNAME,
            PASSWORD
        }

        private void CopyClick(User user, int itemIdx, StoredDataType type)
        {
            String textToCopy = "";
            if (user.StoredData.Length > itemIdx)
            {
                if (type == StoredDataType.REFERENCE)
                {
                    textToCopy = user.StoredData[itemIdx].Reference;
                }
                else if (type == StoredDataType.USERNAME)
                {
                    textToCopy = user.StoredData[itemIdx].Username;
                }
                else if (type == StoredDataType.PASSWORD)
                {
                    textToCopy = user.StoredData[itemIdx].Password;
                }
            }
            System.Windows.Clipboard.SetText(textToCopy);
        }

        private void CopyClick(int itemIdx, StoredDataType type)
        {
            CopyClick(currentUser, itemIdx, type);
        }

        private void SaveUser()
        {
            SaveUser(currentUser);
        }

        private void SaveUser(User user)
        {
            String filename = "users.xml";
            System.Xml.XmlDocument userFile = new System.Xml.XmlDocument();
            try
            {
                userFile.Load(filename);
                System.Xml.XmlNodeList users = userFile.GetElementsByTagName("user");

                bool existingUser = false;
                System.Xml.XmlNode node = null;
                foreach (System.Xml.XmlNode userNode in users)
                {
                    if (userNode["name"].InnerText.Equals(user.name))
                    {
                        existingUser = true;
                        node = userNode;
                    }
                }
                if (!existingUser)
                {
                    node = userFile.CreateNode(System.Xml.XmlNodeType.Element, "user", null);

                    System.Xml.XmlNode nameNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "name", null);
                    nameNode.InnerText = user.name;

                    System.Xml.XmlNode imageNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "user-image", null);
                    imageNode.InnerText = user.imgPath;

                    System.Xml.XmlNode faceParamsNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "face-params", null);
                    // Need to get all the face params in here
                    FaceClassifier classifier = CurrentObjectBag.SCurrentFaceClassifier;

                    // add by Anton - write face params to XML
                    int tempIdCounter = 1;
                    foreach (ThreeDAuth.Point2d point in user.faceParams)
                    {
                        System.Xml.XmlNode paramNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "param", null);
                        System.Xml.XmlNode id = userFile.CreateNode(System.Xml.XmlNodeType.Element, "id", null);
                        System.Xml.XmlNode mean = userFile.CreateNode(System.Xml.XmlNodeType.Element, "mean", null);
                        System.Xml.XmlNode sdev = userFile.CreateNode(System.Xml.XmlNodeType.Element, "stdev", null);
                        mean.InnerText = "" + point.x;
                        sdev.InnerText = "" + point.y;
                        id.InnerText = "" + tempIdCounter;
                        tempIdCounter++;
                        paramNode.AppendChild(id);
                        paramNode.AppendChild(mean);
                        paramNode.AppendChild(sdev);

                        faceParamsNode.AppendChild(paramNode);
                    }


                    System.Xml.XmlNode passwordNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "points", null);
                    foreach (ThreeDAuth.Point2d point in user.password)
                    {
                        System.Xml.XmlNode pointNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "point", null);
                        System.Xml.XmlNode xCoord = userFile.CreateNode(System.Xml.XmlNodeType.Element, "x", null);
                        System.Xml.XmlNode yCoord = userFile.CreateNode(System.Xml.XmlNodeType.Element, "y", null);
                        xCoord.InnerText = "" + point.x;
                        yCoord.InnerText = "" + point.y;
                        pointNode.AppendChild(xCoord);
                        pointNode.AppendChild(yCoord);

                        passwordNode.AppendChild(pointNode);
                    }

                    System.Xml.XmlNode storedDataNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "stored-data", null);
                    foreach (UserInfoTuple userInfo in user.StoredData)
                    {
                        System.Xml.XmlNode tupleNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "stored-data-tuple", null);
                        System.Xml.XmlNode refNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "reference", null);
                        System.Xml.XmlNode unNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "username", null);
                        System.Xml.XmlNode pwNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "password", null);

                        refNode.InnerText = userInfo.Reference;
                        unNode.InnerText = userInfo.Username;
                        pwNode.InnerText = userInfo.Password;

                        tupleNode.AppendChild(refNode);
                        tupleNode.AppendChild(unNode);
                        tupleNode.AppendChild(pwNode);

                        storedDataNode.AppendChild(tupleNode);
                    }

                    node.AppendChild(nameNode);
                    node.AppendChild(imageNode);
                    node.AppendChild(faceParamsNode);
                    node.AppendChild(passwordNode);
                    node.AppendChild(storedDataNode);
                    userFile.DocumentElement.AppendChild(node);
                }
                else
                {
                    // Do things or something
                    //XmlNode storedData = node["stored-data"];
                    System.Xml.XmlNode storedDataNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "stored-data", null);
                    foreach (UserInfoTuple userInfo in user.StoredData)
                    {
                        if (userInfo != null)
                        {
                            System.Xml.XmlNode tupleNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "stored-data-tuple", null);
                            System.Xml.XmlNode refNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "reference", null);
                            System.Xml.XmlNode unNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "username", null);
                            System.Xml.XmlNode pwNode = userFile.CreateNode(System.Xml.XmlNodeType.Element, "password", null);

                            refNode.InnerText = userInfo.Reference;
                            unNode.InnerText = userInfo.Username;
                            pwNode.InnerText = userInfo.Password;

                            tupleNode.AppendChild(refNode);
                            tupleNode.AppendChild(unNode);
                            tupleNode.AppendChild(pwNode);

                            storedDataNode.AppendChild(tupleNode);
                        }
                    }
                    //node["stored-data"] = storedDataNode;
                    node.RemoveChild(node["stored-data"]);
                    node.AppendChild(storedDataNode);
                }
                userFile.Save(filename);
            }
            catch (Exception exception)
            {
                Console.WriteLine("file not found");
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
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position).GetPoint(), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private ThreeDAuth.DepthPoint SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            /*
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
             * */
            return new ThreeDAuth.DepthPoint(depthPoint.X, depthPoint.Y, (long)depthPoint.Depth);
        }


        private Tuple<int, int> ProjectPoint(Tuple<int, int> basePoint,
                                            DepthPoint torsoPosition,
                                            int armLengthPixels,
                                            int planeDepthFromTorsoPixels,
                                            double alpha,
                                            int windowWidth,
                                            int windowHeight)
        {

            // return basePoint;

            //armLength *= 1000; // Convert meters to mm
            // Get pixel length of the arm

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

            double tempValue = Math.Pow(alpha * armLengthPixels, 2) - Math.Pow(planeDepthFromTorsoPixels, 2);
            // If this tempValue is less than 0 then holding the arm straight out infront of you 
            // (with the given alpha) will not cross the plane and we have bigger problems
            if (tempValue < 0)
            {
                // Sanity check
                return null;
            }

            double distanceToCornermm = Math.Sqrt(tempValue);

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
            cornerSkeletonPoint.X = (float)(torsoPosition.x + xOffset);
            cornerSkeletonPoint.Y = (float)(torsoPosition.y + yOffset);
            //DepthPoint lowerRightCornerDepthPoint = this.SkeletonPointToScreen(cornerSkeletonPoint);

            // This cornerPoint should represent the bottom right corner more or less
            // So its x value should be our pre-projected target box width,
            // and its y value should be our pre-projected target box height

            double xScale = (double)windowWidth / (double)(torsoPosition.x + xOffset);
            double yScale = (double)windowHeight / (double)(torsoPosition.y + yOffset);


            Tuple<int, int> shiftedPoint = new Tuple<int, int>(basePoint.Item1 - torsoPosition.x, basePoint.Item2 - torsoPosition.y);

            int xProj = (int)Math.Floor(xScale * shiftedPoint.Item1);
            int yProj = (int)Math.Floor(yScale * shiftedPoint.Item2);

            // unshift the point
            xProj = xProj + torsoPosition.x;
            yProj = yProj + torsoPosition.y;


            //bound checking
            if (xProj > windowWidth) xProj = windowWidth - 1;
            if (yProj > windowHeight) yProj = windowHeight - 1;
            if (xProj <= 0) xProj = 1;
            if (yProj <= 0) yProj = 1;

            //Console.WriteLine("(" + basePoint.Item1 + ", " + basePoint.Item2 + ") -> (" + xProj + ", " + yProj + ")" +
                                //"     Arm length: " + armLengthPixels);

            return new Tuple<int, int>(xProj, yProj);
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

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position).GetPoint(), this.SkeletonPointToScreen(joint1.Position).GetPoint());
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


        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {

            this.layoutGrid.Height = this.mainWindow.ActualHeight;
            this.layoutGrid.Width = this.mainWindow.ActualWidth;
            this.myImageBox.Height = this.layoutGrid.Height * .8;
            this.myImageBox.Width = this.layoutGrid.Width * .8;
            this.Image.Height = this.layoutGrid.Height * .1;
            this.Image.Width = this.layoutGrid.Width * .1;
        }

        private void graphPage(object sender, RoutedEventArgs e)
        {
            ThreeDAuth.GraphWindow grap = new ThreeDAuth.GraphWindow(this);
            grap.Show();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            gLearner.startRecording();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            gLearner.stopRecording();
            System.Console.WriteLine(gLearner.getGesturePath().Count);
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (gLearner.isRecording)
            {
                gLearner.stopRecording();
            }
            System.Collections.Generic.Queue<ThreeDAuth.Point2d> path = gLearner.getGesturePath();
            if (path != null)
            {
                gValidator = new ThreeDAuth.GestureValidator(path, 50);
                gValidator.beginPath();
            }
        }

        private void New_Account_Click(object sender, RoutedEventArgs e)
        {
            CurrentObjectBag.SLearningNewUser = true;

            this.currentUser = new User("", "", null, null, null);

            this.isUserNew = true;
            this.InitialPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.userNamestackPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            CurrentObjectBag.SLearningNewUser = false;

            this.currentUser = new User("", "", null, null, null);

            this.isUserNew = false;
            this.InitialPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.userNamestackPanel.Visibility = System.Windows.Visibility.Visible;
            this.AccountButton.Content = "Log in";


        }

        void GiveUser(User current)
        {

            faceTrackingViewer.stopTracking();
            this.isfaceTrackerOn = false;
            if (this.isUserNew)
            {
                if (current.name.Length > 0)
                {

                    faceTrackingViewer.stopTracking();
                    this.isfaceTrackerOn = false;
                    this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                    if (string.Compare(this.currentUser.name, current.name, true) == 0)
                    {
                        this.welcomeMassage.Text = "We have found your scan if thats incorecct click on rescan!";
                        this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;
                        this.rescan.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else
                {
                    // new user registering, start learner
                    faceTrackingViewer.stopTracking();
                    this.isfaceTrackerOn = false;
                    this.currentUser.faceParams = current.faceParams;
                    this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                    this.ImagePanel.Visibility = System.Windows.Visibility.Visible;

                }

            }

            else
            {
                if (current.name.Length > 0)
                {
                    if (String.Compare(current.name, this.currentUser.name, true) == 0)
                    {
                        faceTrackingViewer.stopTracking();
                        this.isfaceTrackerOn = false;
                        Console.WriteLine("The name is " + current.name);
                        this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                        this.InitialPanel.Visibility = System.Windows.Visibility.Visible;
                        this.New_Account.Visibility = System.Windows.Visibility.Collapsed;
                        this.login.Visibility = System.Windows.Visibility.Collapsed;
                        this.rescan.Visibility = System.Windows.Visibility.Collapsed;
                        this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;

                        this.welcomeMassage.Text = "Hello " + current.name + "! " + "Start drawing your pattern when the circle is blue.";

                        userImage = new BitmapImage(new Uri(current.imgPath));
                        myImageBox.Source = handSource;
                        this.myImageBox.Visibility = Visibility.Visible;
                        this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                        this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                        Queue<ThreeDAuth.Point2d> passwordQueue = new Queue<ThreeDAuth.Point2d>(current.password);
                        gValidator = new ThreeDAuth.GestureValidator(passwordQueue, 20);
                        gValidator.OnCompletedValidation += new CompletedValidation(gValidator_OnCompletedValidation);

                    }
                    else
                    {

                        faceTrackingViewer.stopTracking();
                        this.isfaceTrackerOn = false;
                        Console.WriteLine("The name is " + current.name);
                        this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                        this.InitialPanel.Visibility = System.Windows.Visibility.Visible;
                        this.New_Account.Visibility = System.Windows.Visibility.Collapsed;
                        this.login.Visibility = System.Windows.Visibility.Collapsed;
                        this.rescan.Visibility = System.Windows.Visibility.Visible;
                        this.byPass.Visibility = System.Windows.Visibility.Visible;
                        this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;
                        this.welcomeMassage.Text = "Hello " + this.currentUser.name + "! " + "We did not find the right match. Please rescan";
                    }
                }
                else
                {
                    faceTrackingViewer.stopTracking();
                    this.isfaceTrackerOn = false;
                    Console.WriteLine("The name is " + current.name);
                    this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                    this.InitialPanel.Visibility = System.Windows.Visibility.Visible;
                    this.New_Account.Visibility = System.Windows.Visibility.Collapsed;
                    this.login.Visibility = System.Windows.Visibility.Collapsed;
                    this.rescan.Visibility = System.Windows.Visibility.Visible;
                    this.byPass.Visibility = System.Windows.Visibility.Visible;
                    this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;
                    this.welcomeMassage.Text = "Hello " + this.Username.Text + "! " + "We did not find you. Please rescan";

                }

            }
        }

        private void gValidator_OnCompletedValidation(bool successful)
        {
            if (successful)
            {
                if (this.isfaceTrackerOn)
                {
                    faceTrackingViewer.stopTracking();
                }

                this.sensor.DepthFrameReady -= this.SensorDepthFrameReady;
                this.sensor.SkeletonFrameReady -= this.SensorSkeletonFrameReady;

                this.currentUser = LoadUser(this.currentUser.name);
                this.myImageBox.Source = null;
                this.Username.Text = "";
                faceScanCount = 0;
                faceScanCounter = -1;
                this.progressBar1.Value = 0;
                this.progressBar2.Value = 0;
                this.progressBar3.Value = 0;
                this.InitialPanel.Visibility = System.Windows.Visibility.Visible;
                this.New_Account.Visibility = System.Windows.Visibility.Collapsed;
                this.login.Visibility = System.Windows.Visibility.Collapsed;
                this.progressBar1.Width = 55;
                this.progressBar1.Width = 55;
                this.progressBar1.Width = 55;
                this.progressBar1.Visibility = System.Windows.Visibility.Collapsed;
                this.progressBar2.Visibility = System.Windows.Visibility.Collapsed;
                this.progressBar3.Visibility = System.Windows.Visibility.Collapsed;
                this.byPass.Visibility = System.Windows.Visibility.Collapsed;
                this.rescan.Visibility = System.Windows.Visibility.Collapsed;
                this.gestureTracker.Visibility = System.Windows.Visibility.Collapsed;
                this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
                this.RegistrationMassage.Visibility = System.Windows.Visibility.Collapsed;
                this.userNamestackPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.ImagePanel.Visibility = System.Windows.Visibility.Collapsed;


                this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;
                if (currentUser != null)
                {
                    this.welcomeMassage.Text = "Congratulations " + currentUser.name + ". You are now logged in!";
                }
                else
                {
                    this.welcomeMassage.Text = "Congratulations! You are now logged in!";
                }

                this.NewReferencePanel.Visibility = System.Windows.Visibility.Visible;
                this.NewUsernamePanel.Visibility = System.Windows.Visibility.Visible;
                this.NewPasswordPanel.Visibility = System.Windows.Visibility.Visible;
                this.LoggedInUI.Visibility = System.Windows.Visibility.Visible;
                //this.NewStoredDataButtonPanel.Visibility = System.Windows.Visibility.Visible;
                //this.StoredDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                //this.StoredDataGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }


        private User LoadUser(String username)
        {
            XmlDocument data = new XmlDocument();
            try
            {
                data.Load("users.xml");
            }
            catch (Exception)
            {
                Console.WriteLine("file not found");
            }

            SortedDictionary<String, double> matches = new SortedDictionary<String, double>();

            XmlNodeList users = data.GetElementsByTagName("user");


            foreach (XmlNode user in users)
            {
                if (user["name"].InnerText == username)
                {
                    List<Point2d> tempPts = new List<Point2d>();
                    XmlNode points = user["points"];
                    for (int i = 0; i < points.ChildNodes.Count; i++)
                    {
                        XmlNode point = points.ChildNodes[i];
                        double x = Convert.ToDouble(point["x"].InnerText);
                        double y = Convert.ToDouble(point["y"].InnerText);
                        Point2d tmp = new Point2d(x, y);
                        tempPts.Add(tmp);
                    }
                    XmlNode storedDataXml = user["stored-data"];
                    UserInfoTuple[] storedData = new UserInfoTuple[Util.NUM_STORED_DATA];
                    for (int i = 0; i < storedDataXml.ChildNodes.Count; i++)
                    {
                        XmlNode storedDataTuple = storedDataXml.ChildNodes[i];
                        string reference = Convert.ToString(storedDataTuple["reference"].InnerText);
                        string uname = Convert.ToString(storedDataTuple["username"].InnerText);
                        string password = Convert.ToString(storedDataTuple["password"].InnerText);
                        storedData[i] = new UserInfoTuple(reference, uname, password);
                    }
                    return new User(user["name"].InnerText, user["user-image"].InnerText, tempPts, null, storedData);
                }
            }
            return null;
        }

        private void accountButton_Click(object sender, RoutedEventArgs e)
        {
            this.userNamestackPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.currentUser.name = this.Username.Text;
            if (this.isUserNew)
            {
                this.scanpanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.progressBar1.Width = 170;
                this.scanpanel.Visibility = System.Windows.Visibility.Visible;
                this.progressBar2.Visibility = System.Windows.Visibility.Collapsed;
                this.progressBar3.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.isfaceTrackerOn = true;
            faceTrackingViewer.setSensor(this.sensor);
            CurrentObjectBag.SCurrentFaceClassifier.OnUserReceived += new GiveUser(GiveUser);

        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.AccountButton.IsEnabled = true;
        }


        private void browse_Click(object sender, RoutedEventArgs e)
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
                currentUser.imgPath = fileName;
                this.myImageBox.Visibility = Visibility.Visible;
                this.myImageBox.Source = this.handSource;
                this.userNamestackPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.welcomeMassage.Visibility = System.Windows.Visibility.Collapsed;
                this.ImagePanel.Visibility = System.Windows.Visibility.Collapsed;
                this.RegistrationMassage.Visibility = System.Windows.Visibility.Collapsed;
                this.gestureTracker.Visibility = System.Windows.Visibility.Visible;
                this.gestureMassage.Text = "Start drawing your pattern when the circle is blue";
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                gLearner.startRecording();

            }
            catch (Exception)
            {
                MessageBox.Show("Error opening file", "File Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
        }

        private void rescan_Click(object sender, RoutedEventArgs e)
        {
            faceScanCount = 0;
            this.rescan.Visibility = System.Windows.Visibility.Collapsed;
            this.welcomeMassage.Visibility = System.Windows.Visibility.Collapsed;
            this.byPass.Visibility = System.Windows.Visibility.Collapsed;
            this.myImageBox.Visibility = System.Windows.Visibility.Collapsed; ;
            this.scanpanel.Visibility = System.Windows.Visibility.Visible;
            this.isfaceTrackerOn = true;
            faceTrackingViewer.startTracking();
            CurrentObjectBag.SCurrentFaceClassifier.OnUserReceived += new GiveUser(GiveUser);
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            if (this.isfaceTrackerOn)
            {
                faceTrackingViewer.stopTracking();
            }

            //this.sensor.DepthFrameReady += null;

            this.sensor.DepthFrameReady -= this.SensorDepthFrameReady;
            this.sensor.SkeletonFrameReady -= this.SensorSkeletonFrameReady;

            //this.sensor.SkeletonFrameReady += null;
            if (gValidator != null)
            {
                //gValidator.OnCompletedValidation += null;
            }

            this.currentUser = null;
            this.myImageBox.Source = null;
            this.Username.Text = "";
            faceScanCount = 0;
            faceScanCounter = -1;
            this.progressBar1.Value = 0;
            this.progressBar2.Value = 0;
            this.progressBar3.Value = 0;
            this.InitialPanel.Visibility = System.Windows.Visibility.Visible;
            this.New_Account.Visibility = System.Windows.Visibility.Visible;
            this.login.Visibility = System.Windows.Visibility.Visible;
            this.progressBar1.Width = 55;
            this.progressBar1.Width = 55;
            this.progressBar1.Width = 55;
            this.progressBar1.Visibility = System.Windows.Visibility.Visible;
            this.progressBar2.Visibility = System.Windows.Visibility.Visible;
            this.progressBar3.Visibility = System.Windows.Visibility.Visible;
            this.welcomeMassage.Visibility = System.Windows.Visibility.Collapsed;
            this.byPass.Visibility = System.Windows.Visibility.Collapsed;
            this.rescan.Visibility = System.Windows.Visibility.Collapsed;
            this.gestureTracker.Visibility = System.Windows.Visibility.Collapsed;
            this.scanpanel.Visibility = System.Windows.Visibility.Collapsed;
            this.RegistrationMassage.Visibility = System.Windows.Visibility.Collapsed;
            this.userNamestackPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.ImagePanel.Visibility = System.Windows.Visibility.Collapsed;

            this.NewReferencePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.NewUsernamePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.NewPasswordPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.LoggedInUI.Visibility = System.Windows.Visibility.Collapsed;
            //this.NewStoredDataButtonPanel.Visibility = System.Windows.Visibility.Collapsed;
            //this.StoredDataGridPanel.Visibility = System.Windows.Visibility.Collapsed;
            //this.StoredDataGrid.Visibility = System.Windows.Visibility.Collapsed;

        }

        /*
        private void populateGrid(User user, System.Windows.Controls.Grid grid)
        {
            //System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();
            grid.Width = 800;
            grid.Height = 1000;
            grid.Visibility = System.Windows.Visibility.Visible;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.ShowGridLines = true;

            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            ColumnDefinition refCol = new ColumnDefinition();
            ColumnDefinition usernameCol = new ColumnDefinition();
            ColumnDefinition passwordCol = new ColumnDefinition();

            refCol.Width = new GridLength(200);
            usernameCol.Width = new GridLength(200);
            passwordCol.Width = new GridLength(200);

            grid.ColumnDefinitions.Add(refCol);
            grid.ColumnDefinitions.Add(usernameCol);
            grid.ColumnDefinitions.Add(passwordCol);


            // rows
            for (int i = 0; i < user.StoredData.Length + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(40);
                grid.RowDefinitions.Add(row);
            }
            TextBlock refText = new TextBlock();
            refText.Text = "Reference";
            refText.FontSize = 14;
            refText.Foreground = new SolidColorBrush(Colors.Black);
            refText.VerticalAlignment = VerticalAlignment.Top;
            System.Windows.Controls.Grid.SetRow(refText, 0);
            System.Windows.Controls.Grid.SetColumn(refText, 0);

            TextBlock usernameText = new TextBlock();
            refText.Text = "Username";
            refText.FontSize = 14;
            refText.Foreground = new SolidColorBrush(Colors.Black);
            refText.VerticalAlignment = VerticalAlignment.Top;
            System.Windows.Controls.Grid.SetRow(usernameText, 0);
            System.Windows.Controls.Grid.SetColumn(usernameText, 1);

            TextBlock passwordText = new TextBlock();
            refText.Text = "Password";
            refText.FontSize = 14;
            refText.Foreground = new SolidColorBrush(Colors.Black);
            refText.VerticalAlignment = VerticalAlignment.Top;
            System.Windows.Controls.Grid.SetRow(passwordText, 0);
            System.Windows.Controls.Grid.SetColumn(passwordText, 2);
            
            grid.Children.Add(refText);
            grid.Children.Add(usernameText);
            grid.Children.Add(passwordText);

            for (int i = 0; i < user.StoredData.Length; i++)
            {
                TextBlock referenceText = new TextBlock();
                referenceText.Text = user.StoredData[i].Reference;
                referenceText.FontSize = 12;
                referenceText.FontWeight = FontWeights.Bold;
                System.Windows.Controls.Grid.SetRow(referenceText, i + 1);
                System.Windows.Controls.Grid.SetColumn(referenceText, 0);

                Button usernameButton = new Button();
                usernameButton.Width = 20;
                usernameButton.Height = 25;
                usernameButton.Content = "Copy";
                usernameButton.Tag = user.StoredData[i].Username;
                usernameButton.Click += new RoutedEventHandler(copyClick);
                System.Windows.Controls.Grid.SetRow(usernameButton, i + 1);
                System.Windows.Controls.Grid.SetColumn(usernameButton, 1);
                //usernameButton.Click += (s, e) => { CopyClick(i - 1, StoredDataType.USERNAME); };

                Button passwordButton = new Button();
                passwordButton.Width = 20;
                passwordButton.Height = 25;
                passwordButton.Content = "Copy";
                passwordButton.Tag = user.StoredData[i].Password;
                passwordButton.Click += new RoutedEventHandler(copyClick);
                System.Windows.Controls.Grid.SetRow(passwordButton, i + 1);
                System.Windows.Controls.Grid.SetColumn(passwordButton, 1);
                //usernameButton.Click += (s, e) => { CopyClick(i - 1, StoredDataType.PASSWORD); };

                grid.Children.Add(referenceText);
                grid.Children.Add(usernameButton);
                grid.Children.Add(passwordButton);
            }
        }
         * */

        private void copyClick(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            System.Windows.Clipboard.SetText((String) clicked.Tag);
        }

        private void byPass_Click(object sender, RoutedEventArgs e)
        {
            if (data == null)
            {
                data = new XmlDocument();
            }
            this.rescan.Visibility = System.Windows.Visibility.Collapsed;
            this.welcomeMassage.Visibility = System.Windows.Visibility.Collapsed;

            try
            {
                data.Load("users.xml");
            }
            catch (Exception exception)
            {
                Console.WriteLine("file not found");
            }

            SortedDictionary<String, double> matches = new SortedDictionary<String, double>();

            XmlNodeList users = data.GetElementsByTagName("user");

            foreach (XmlNode user in users)
            {
                if (String.Compare(user["name"].InnerText, this.currentUser.name, true) == 0)
                {

                    this.currentUser.imgPath = user["user-image"].InnerText;

                    List<Point2d> tempPts = new List<Point2d>();
                    XmlNode points = user["points"];
                    for (int i = 0; i < points.ChildNodes.Count; i++)
                    {
                        XmlNode point = points.ChildNodes[i];
                        double x = Convert.ToDouble(point["x"].InnerText);
                        double y = Convert.ToDouble(point["y"].InnerText);
                        Point2d tmp = new Point2d(x, y);
                        tempPts.Add(tmp);
                    }

                    this.currentUser.password = tempPts;

                    this.byPass.Visibility = System.Windows.Visibility.Collapsed;
                    this.welcomeMassage.Visibility = System.Windows.Visibility.Visible;
                    this.welcomeMassage.Text = "Hello " + this.currentUser.name + "! " + "Start drawing your pattern when the circle is blue.";
                    userImage = new BitmapImage(new Uri(this.currentUser.imgPath));
                    myImageBox.Source = handSource;
                    this.myImageBox.Visibility = Visibility.Visible;
                    this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                    this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                    Queue<ThreeDAuth.Point2d> passwordQueue = new Queue<ThreeDAuth.Point2d>(this.currentUser.password);
                    gValidator = new ThreeDAuth.GestureValidator(passwordQueue, 20);
                    gValidator.OnCompletedValidation += new CompletedValidation(gValidator_OnCompletedValidation);
                }
            }
        }


        /*
        private void addStoredData_Click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.AddStoredData(reference, username, password);
                SaveUser(currentUser);
                //populateGrid(currentUser, this.StoredDataGrid);
                //this.StoredDataGrid = populateGrid(currentUser);
                //this.StoredDataGrid = populateGrid(currentUser);
                //this.StoredDataGridPanel.
            }
        }
        */


        private void copy1u_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                System.Windows.Clipboard.SetText(currentUser.StoredData[0].Username);
            }
        }

        private void copy1p_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(currentUser.StoredData[0].Password);
        }

        private void setstored1_click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.SetStoredData(0, reference, username, password);
                SaveUser(currentUser);
            }
            this.NewReferenceNameTextBlock1.Text = reference;

            this.NewReferenceText.Text = "";
            this.NewUserNameText.Text = "";
            this.NewPasswordText.Text = "";
        }






        private void copy2u_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                System.Windows.Clipboard.SetText(currentUser.StoredData[1].Username);
            }
        }

        private void copy2p_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(currentUser.StoredData[1].Password);
        }

        private void setstored2_click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.SetStoredData(1, reference, username, password);
                SaveUser(currentUser);
            }
            this.NewReferenceNameTextBlock2.Text = reference;

            this.NewReferenceText.Text = "";
            this.NewUserNameText.Text = "";
            this.NewPasswordText.Text = "";
        }





        /*

        private void copy3u_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                System.Windows.Clipboard.SetText(currentUser.StoredData[2].Username);
            }
        }

        private void copy3p_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(currentUser.StoredData[2].Password);
        }

        private void setstored3_click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.SetStoredData(2, reference, username, password);
                SaveUser(currentUser);
            }
            this.NewReferenceNameTextBlock3.Text = reference;
        }






        private void copy4u_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                System.Windows.Clipboard.SetText(currentUser.StoredData[3].Username);
            }
        }

        private void copy4p_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(currentUser.StoredData[3].Password);
        }

        private void setstored4_click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.SetStoredData(3, reference, username, password);
                SaveUser(currentUser);
            }
            this.NewReferenceNameTextBlock4.Text = reference;
        }





        private void copy5u_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                System.Windows.Clipboard.SetText(currentUser.StoredData[4].Username);
            }
        }

        private void copy5p_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(currentUser.StoredData[4].Password);
        }

        private void setstored5_click(object sender, RoutedEventArgs e)
        {
            String reference = this.NewReferenceText.Text;
            String username = this.NewUserNameText.Text;
            String password = this.NewPasswordText.Text;

            if (currentUser != null)
            {
                currentUser.SetStoredData(4, reference, username, password);
                SaveUser(currentUser);
            }
            this.NewReferenceNameTextBlock5.Text = reference;
        }*/
    }
}