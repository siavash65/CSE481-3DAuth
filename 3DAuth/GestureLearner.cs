using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    abstract class GestureLearner
    {
        protected Queue<Point2f> learnedGesturePath;

        public void givePoint(Point2f point);

        public Queue<Point2f> getGesturePath()
        {
            return learnedGesturePath;
        }
    }

    class ContinuousGestureLearner : GestureLearner
    {
        private System.Diagnostics.Stopwatch stopwatch;
        private long samplingCooldown;

        // sampleCooldown in ms
        public ContinuousGestureLearner(long samplingCooldown)
        {
            this.samplingCooldown = samplingCooldown;
            stopwatch = new System.Diagnostics.Stopwatch();
            startRecording();
        }

        public void startRecording()
        {
            stopwatch.Start();
        }

        public void givePoint(Point2f point)
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
        private System.Diagnostics.Stopwatch stopwatch;
        private Queue<TimePointTuple> pointBuffer;
        private int minNumberOfPoints;
        private long holdTime;

        // holdTime in ms
        public DiscreteGestureLearner(long holdTime, double motionEpsilon, int minNumberOfPoints = 5)
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            pointBuffer = new Queue<TimePointTuple>();
            this.minNumberOfPoints = minNumberOfPoints;
            this.holdTime = holdTime;
        }

        public void givePoint(Point2f point)
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
                    learnedGesturePath.Enqueue(pointBuffer.Peek().point.copy());
                }
            }
        }

        private class TimePointTuple
        {
            public long timeMark { get; set; }
            public Point2f point { get; set; }

            public TimePointTuple(long timeMark, Point2f point)
            {
                this.timeMark = timeMark;
                this.point = point.copy();
            }
        }
    }
}
