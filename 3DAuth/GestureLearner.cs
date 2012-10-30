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

        abstract public void givePoint(Point2d point);

        public Queue<Point2d> getGesturePath()
        {
            return learnedGesturePath;
        }

        public void startRecording()
        {
            stopwatch.Start();
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
        }

        override public void givePoint(Point2d point)
        {
            if (stopwatch.ElapsedMilliseconds >= samplingCooldown)
            {
                learnedGesturePath.Enqueue(point.copy());
                stopwatch.Stop();
                stopwatch.Start();
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
        }

        override public void givePoint(Point2d point)
        {
            long elapsedMS = stopwatch.ElapsedMilliseconds;
            if (pointBuffer.Count == 0)
            {
                // Enqueue the first point
                pointBuffer.Enqueue(new TimePointTuple(elapsedMS, point));
            }
            else
            {
                if ( (elapsedMS - pointBuffer.Peek().timeMark > holdTime) &&
                     (pointBuffer.Count >= minNumberOfPoints) ) 
                {
                    // Have a target point
                    learnedGesturePath.Enqueue(pointBuffer.Dequeue().point.copy());
                    pointBuffer.Clear();
                }
                else if (Util.euclideanDistance(pointBuffer.Peek().point, point) < motionEpsilon)
                {
                    // Person is staying still
                    pointBuffer.Enqueue(new TimePointTuple(elapsedMS, point));
                }
                else
                {
                    // Not a target point and person has moved too much, so dump the queue
                    pointBuffer.Clear();
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
