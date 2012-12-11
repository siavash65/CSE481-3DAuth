using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // For demo, show whats going on under the hood (maybe some graphed points)

    // 
    // Version for closest points to trajectory
    // Cover fraction of points
    //
    // Kalman filter
    // Particle filter

    delegate void UpdateTargetDistance(double distance, long timeMS);
    delegate void CompletedValidation(bool successful);

    // Given a sequence of points (in pixel-space) to hit, validates a continuously updating stream of positions
    class GestureValidator
    {
        private Queue<Point2d> targetPoints;
        private Queue<Point2d> manipulatableTargetPoints;
        private Queue<Point2d> completedTargets;
        private double oldDistance;
        private double epsilon;
        private bool failedAuthentication;
        private System.Diagnostics.Stopwatch timer;

        private long lastReset;

        private bool _startedPath; // Don't fail them before they start (it's rude)

        public bool startedPath
        {
            get
            {
                return _startedPath;
            }
        }

        private event UpdateTargetDistance _onDistanceUpdated;

        public event UpdateTargetDistance OnDistanceUpdated
        {
            add
            {
                _onDistanceUpdated += value;
            }
            remove
            {
                _onDistanceUpdated -= value;
            }
        }

        private event CompletedValidation _onCompletedValidation;

        public event CompletedValidation OnCompletedValidation
        {
            add
            {
                _onCompletedValidation += value;
            }
            remove
            {
                _onCompletedValidation -= value;
            }
        }

        public GestureValidator(Queue<Point2d> targetPoints, double epsilonBoundary)
        {
            this.targetPoints = targetPoints;
            this.epsilon = epsilonBoundary;
            manipulatableTargetPoints = new Queue<Point2d>();
            completedTargets = new Queue<Point2d>();
            PointDistributor.GetInstance().OnPointReceived += new GivePoint(GivePoint);
            CurrentObjectBag.SCurrentGestureValidator = this;
            timer = new System.Diagnostics.Stopwatch();
            lastReset = 0;
            beginPath();

            // Give the current object back a reference to us
            CurrentObjectBag.SCurrentGestureValidator = this;
        }

        public Queue<Point2d> getUnHitPoints()
        {
            return manipulatableTargetPoints;
        }

        public Queue<Point2d> getHitPoints()
        {
            return completedTargets;
        }

        public Point2d getNextPoint()
        {
            if (manipulatableTargetPoints.Count > 0)
            {
                return manipulatableTargetPoints.Peek();
            }
            else
            {
                return null;
            }
        }

        public void beginPath()
        {
            manipulatableTargetPoints.Clear();
            completedTargets.Clear();
            for (int i = 0; i < targetPoints.Count; i++) {
                Point2d point = targetPoints.Dequeue();
                targetPoints.Enqueue(point);
                manipulatableTargetPoints.Enqueue(point);
            }
            oldDistance = double.MaxValue;
            failedAuthentication = false;
            _startedPath = false;
            timer.Start();
        }

        public void restart()
        {
            lastReset = System.DateTime.UtcNow.Ticks;
            beginPath();
        }

        public void GivePoint(Point point)
        {
            if (manipulatableTargetPoints.Count > 0)
            {
                if (point is PlanePoint)
                {
                    PlanePoint planePoint = (PlanePoint)point;
                    if (planePoint.inPlane)
                    {
                        Point2d newPoint = (Point2d)point;
                        Point2d target = manipulatableTargetPoints.Peek();
                        double newDistance = Util.euclideanDistance(newPoint, target);
                        if (!_startedPath)
                        {
                            if (newDistance < epsilon)
                            {
                                _startedPath = true;
                                Console.WriteLine("Started path");
                            }
                        }
                        else
                        {
                            if (target != null && !failedAuthentication)
                            {
                                if ((newDistance - 3 * epsilon) > oldDistance)
                                {
                                    // Made a move away from the target point, so failed authentication
                                    failedAuthentication = true;
                                    Console.WriteLine("Failure :( ");
                                    // Give them some time interval to reset it before marking it as a failure

                                    System.Timers.Timer failureTimer = new System.Timers.Timer();
                                    failureTimer.Elapsed += new System.Timers.ElapsedEventHandler(failureTimer_Elapsed);
                                    failureTimer.Interval = 3000;
                                    failureTimer.AutoReset = false;
                                    failureTimer.Enabled = true;
                                }
                                else
                                {
                                    if (newDistance < epsilon)
                                    {
                                        // Hit the target point, so remove it and target the next one
                                        Console.WriteLine("Hit a target point");
                                        completedTargets.Enqueue(manipulatableTargetPoints.Dequeue());
                                        if (manipulatableTargetPoints.Count > 0)
                                        {
                                            oldDistance = Util.euclideanDistance(newPoint, manipulatableTargetPoints.Peek());
                                        }
                                        else
                                        {
                                            // Validated successfully
                                            timer.Stop();
                                            Console.WriteLine("*** Successfully Validated ***");
                                            ValidationComplete();
                                        }
                                    }
                                    else
                                    {
                                        // Didn't hit the target point, but getting closer
                                        oldDistance = newDistance;
                                    }
                                }
                            }
                            Notify();
                        }
                    }
                }
            }
        }

        private void failureTimer_Elapsed(object souce, System.Timers.ElapsedEventArgs e)
        {
            long now = System.DateTime.UtcNow.Ticks;
            if (now - lastReset > 3000 * (10000)) // 3 seconds => 3000 ms * 10,000 ticks / ms
            {
                ValidationComplete();
            }
        }

        public double GetDistanceToCurrentTarget()
        {
            return oldDistance;
        }

        public long GetTotalElapsedMS()
        {
            return timer.ElapsedMilliseconds;
        }

        public bool successfulAuthentication()
        {
            return (!failedAuthentication) && (manipulatableTargetPoints.Count == 0);
        }

        private void Notify()
        {
            if (_onDistanceUpdated != null)
            {
                _onDistanceUpdated(GetDistanceToCurrentTarget(), GetTotalElapsedMS());
            }
        }

        private void ValidationComplete()
        {
            if (_onCompletedValidation != null)
            {
                _onCompletedValidation(successfulAuthentication());
            }
        }
    }
}
