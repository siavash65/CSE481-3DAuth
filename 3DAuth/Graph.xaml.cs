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

namespace ThreeDAuth
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class GraphWindow : Window
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
        private readonly Pen mypen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;
        



        private class DistanceTimeTuple {
            public double Distance { get; set; }
            public long Time { get; set; }

            public DistanceTimeTuple(double dist, long time) {
                this.Distance = dist;
                this.Time = time;
            }
        }
        private Queue<DistanceTimeTuple> currentPointBuffer;

        private static int MAX_NUM_POINTS_TO_DISPLAY = 50;



        /// <summary>
        /// Initializes a new instance of the Graph class.
        /// </summary>
        public GraphWindow()
        {
            InitializeComponent();

          // CurrentObjectBag.SCurrentGestureValidator.OnDistanceUpdated += new UpdateTargetDistance(AddDistance);
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

            currentPointBuffer = new Queue<DistanceTimeTuple>();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            GraphImageBox.Source = this.imageSource;

            AddDistance(2, 1); 
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void AddDistance(double distance, long elapsedTimeMS) 
        {
            DistanceTimeTuple newPoint = new DistanceTimeTuple(distance, elapsedTimeMS);
            currentPointBuffer.Enqueue(newPoint);
            while (currentPointBuffer.Count > MAX_NUM_POINTS_TO_DISPLAY) {
                // Pull the oldest point off and throw it away
                currentPointBuffer.Dequeue();
            }
            DrawGraph();
        }

        private double getTimePixelPosition(long currentTime, long minTime, long maxTime) 
        {
            // returns NAN because maxTime = minTime
            // return (1.0 * currentTime / (1.0 * maxTime - 1.0 * minTime)) * GraphImageBox.ActualWidth;
            return 100.0;
        }

        private double getDistPixelPosition(double currentDist, double minDist, double maxDist) 
        {
            // returns NAN because maxdist = minDist
            // return (currentDist / (maxDist - minDist)) * GraphImageBox.ActualHeight;
            return 100.0;
        }


        private void DrawGraph()
        {
            using( DrawingContext dc = this.drawingGroup.Open() ) 
            {
                double maxDistance = double.MinValue;
                double minDistance = double.MaxValue;
                long maxTime = long.MinValue;
                long minTime = long.MaxValue;
                foreach (DistanceTimeTuple point in currentPointBuffer)
                {
                    maxDistance = Math.Max(point.Distance, maxDistance);
                    minDistance = Math.Min(point.Distance, minDistance);
                    maxTime = Math.Max(point.Time, maxTime);
                    minTime = Math.Min(point.Time, minTime);

                    System.Windows.Point centerPoint =
                             new System.Windows.Point(getTimePixelPosition(point.Time, minTime, maxTime),
                             getDistPixelPosition(point.Distance, minDistance, maxDistance));
                    
                    dc.DrawEllipse(Brushes.Red, null, centerPoint, 2, 2);
                }

                foreach (DistanceTimeTuple point in currentPointBuffer) 
                {
                    //System.Windows.Point centerPoint = 
                    //    new System.Windows.Point(getTimePixelPosition(point.Time, minTime, maxTime),
                  //                               getDistPixelPosition(point.Distance, minDistance, maxDistance));
                  //  dc.DrawEllipse(Brushes.Red, null, centerPoint, 10, 10);
                }
            }
        }
    }
}
