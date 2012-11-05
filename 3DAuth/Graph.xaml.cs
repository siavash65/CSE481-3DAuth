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

        private const long WindowTimeWidth = 10 * 1000; // 10 seconds

        private Microsoft.Samples.Kinect.SkeletonBasics.MainWindow owningWindow;

        private const int TargetNumPointsToDisplay = 200;

        private double MaxWindowDistSeen;

        /// <summary>
        /// Initializes a new instance of the Graph class.
        /// </summary>
        public GraphWindow(Microsoft.Samples.Kinect.SkeletonBasics.MainWindow owningWindow)
        {
            InitializeComponent();

            this.owningWindow = owningWindow;

            MaxWindowDistSeen = 0;

            if (CurrentObjectBag.SCurrentGestureValidator != null)
            {
                CurrentObjectBag.SCurrentGestureValidator.OnDistanceUpdated += new UpdateTargetDistance(AddDistance);
            }
            CurrentObjectBag.SOnGestureValidatorChanged += SetGestureValidator;
        }

        private void SetGestureValidator(GestureValidator validator)
        {
            validator.OnDistanceUpdated += new UpdateTargetDistance(AddDistance);
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

            //AddDistance(2, 1); 
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
            while (currentPointBuffer.Count > TargetNumPointsToDisplay)
            {
                // Pull the oldest point off and throw it away
                currentPointBuffer.Dequeue();
            }
            DrawGraph();
        }

        private double getTimePixelPosition(long currentTime, long leftEnd) 
        {
            // WindowTimeWidth is a constant, which should be > 0 (or else you graph nothing since the range of time you can graph has width 0)
            return ((1.0 * currentTime - 1.0 * leftEnd) / 1.0 * WindowTimeWidth) * GraphImageBox.ActualWidth;
        }

        private double getDistPixelPosition(double currentDist, double windowDistHeight) 
        {
            // Note: Window distance height measures starting at 0, so we just divide by windowDistHeight rather than (windowDistHeight - 0)
            return (currentDist / windowDistHeight) * GraphImageBox.ActualHeight;

            // Note: The only way windowDistHeight can be zero is if the diagonal length of the image box was 0, which would mean
            // the image was infinitely thin, which shouldn't really happen
        }


        private void DrawGraph()
        {
            using( DrawingContext dc = this.drawingGroup.Open() ) 
            {
                long maxTime = long.MinValue;
                long minTime = long.MaxValue;


                // The maximum distance between two anchor points is the diagonal distance across the drawable screen
                double windowDistHeight = Math.Sqrt( Math.Pow(owningWindow.myImageBox.ActualHeight, 2) +
                                                     Math.Pow(owningWindow.myImageBox.ActualWidth, 2) );
                // We want our graph size to be bounded by the largest window size ever seen
                MaxWindowDistSeen = Math.Max(windowDistHeight, MaxWindowDistSeen);

                // If we have exactly TargetNumPointsToDisplay many points to display, then they should fill the graph area,
                // and use that point size for every other scenario
                double pointRadius = Math.Min(GraphImageBox.ActualWidth, GraphImageBox.ActualHeight) / TargetNumPointsToDisplay;
                foreach (DistanceTimeTuple point in currentPointBuffer)
                {
                    maxTime = Math.Max(point.Time, maxTime);
                    minTime = Math.Min(point.Time, minTime);
                }

                long totalWidth = maxTime - minTime;

                // since we're scrolling to the right in a sense, we only care about the value at the right edge and back track from there
                // however, if our entire set of points would not fill the graph window horizontally, then we consider from the left
                if (maxTime - minTime >= WindowTimeWidth)
                {
                    minTime = maxTime - WindowTimeWidth;
                }

                if (MaxWindowDistSeen > 0 && WindowTimeWidth > 0)
                {
                    foreach (DistanceTimeTuple point in currentPointBuffer)
                    {
                        if (point.Time >= minTime)
                        {
                            System.Windows.Point centerPoint =
                                     new System.Windows.Point(getTimePixelPosition(point.Time, minTime),
                                     getDistPixelPosition(point.Distance, MaxWindowDistSeen));

                            dc.DrawEllipse(Brushes.Red, null, centerPoint, pointRadius, pointRadius);
                        }

                    }
                }
            }
        }
    }
}
