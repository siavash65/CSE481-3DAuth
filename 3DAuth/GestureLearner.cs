using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    abstract class GestureLearner
    {
        protected System.Diagnostics.Stopwatch stopwatch;
        protected Queue<Point2d> learnedGesturePath;

        abstract public void GivePoint(Point point);

        public Queue<Point2d> getGesturePath()
        {
            return learnedGesturePath;
        }

        public void startRecording()
        {
            stopwatch.Start();
            learnedGesturePath = new Queue<Point2d>();
        }

        public void stopRecording()
        {
            stopwatch.Stop();
        }
    }

    class ContinuousGestureLearner : GestureLearner
    {
        private long samplingCooldown;

        // sampleCooldown in ms
        public ContinuousGestureLearner(long samplingCooldown)
        {
            this.samplingCooldown = samplingCooldown;
            stopwatch = new System.Diagnostics.Stopwatch();
            startRecording();
            PointDistributor.GetInstance().OnPointReceived += new GivePoint(GivePoint);
        }

        override public void GivePoint(Point point)
        {
            if (point is Point2d)
            {
                Point2d newPoint = (Point2d) point;
                if (stopwatch.ElapsedMilliseconds >= samplingCooldown)
                {
                    learnedGesturePath.Enqueue(newPoint.copy());
                    stopwatch.Stop();
                    stopwatch.Start();
                }
            }
        }
    }

    class DiscreteGestureLearner : GestureLearner
    {
        private Queue<TimePointTuple> pointBuffer;
        private int minNumberOfPoints;
        private long holdTime;
        private double motionEpsilon;

        // holdTime in ms
        public DiscreteGestureLearner(long holdTime, double motionEpsilon, int minNumberOfPoints = 5)
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            pointBuffer = new Queue<TimePointTuple>();
            this.minNumberOfPoints = minNumberOfPoints;
            this.holdTime = holdTime;
            this.motionEpsilon = motionEpsilon;
            PointDistributor.GetInstance().OnPointReceived += new GivePoint(GivePoint);
        }

        override public void GivePoint(Point point)
        {
            if (point is Point2d)
            {
                Point2d newPoint = (Point2d) point;
                long elapsedMS = stopwatch.ElapsedMilliseconds;
                if (pointBuffer.Count == 0)
                {
                    // Enqueue the first point
                    pointBuffer.Enqueue(new TimePointTuple(elapsedMS, newPoint));
                }
                else
                {
                    if ((elapsedMS - pointBuffer.Peek().timeMark > holdTime) &&
                         (pointBuffer.Count >= minNumberOfPoints))
                    {
                        // Have a target point
                        learnedGesturePath.Enqueue(pointBuffer.Dequeue().point.copy());
                        pointBuffer.Clear();
                    }
                    else if (Util.euclideanDistance(pointBuffer.Peek().point, newPoint) < motionEpsilon)
                    {
                        // Person is staying still
                        pointBuffer.Enqueue(new TimePointTuple(elapsedMS, newPoint));
                    }
                    else
                    {
                        // Not a target point and person has moved too much, so dump the queue
                        pointBuffer.Clear();
                    }
                }
            }
        }

        private class TimePointTuple
        {
            public long timeMark { get; set; }
            public Point2d point { get; set; }

            public TimePointTuple(long timeMark, Point2d point)
            {
                this.timeMark = timeMark;
                this.point = point.copy();
            }
        }
    }
}
