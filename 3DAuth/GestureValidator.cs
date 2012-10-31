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


    // Given a sequence of points (in pixel-space) to hit, validates a continuously updating stream of positions
    class GestureValidator
    {
        private Queue<Point2d> targetPoints;
        private Queue<Point2d> manipulatableTargetPoints;
        private HashSet<Point2d> completedTargets;
        private double oldDistance;
        private double epsilon;
        private bool failedAuthentication;

        public GestureValidator(Queue<Point2d> targetPoints, double epsilonBoundary)
        {
            this.targetPoints = targetPoints;
            this.epsilon = epsilonBoundary;
            beginPath();
            manipulatableTargetPoints = new Queue<Point2d>();
            completedTargets = new HashSet<Point2d>();
            PointDistributor.GetInstance().OnPointReceived += new GivePoint(GivePoint);
        }

        public void beginPath()
        {
            for (int i = 0; i < targetPoints.Count; i++) {
                Point2d point = targetPoints.Dequeue();
                targetPoints.Enqueue(point);
                manipulatableTargetPoints.Enqueue(point);
            }
            oldDistance = double.MaxValue;
            failedAuthentication = false;
        }

        public void GivePoint(Point point)
        {
            if (point is Point2d) 
            {
                Point2d newPoint = (Point2d) point;
                Point2d target = manipulatableTargetPoints.Peek();
                if (target != null && !failedAuthentication)
                {
                    double newDistance = Util.euclideanDistance(newPoint, target);
                    if (newDistance > oldDistance)
                    {
                        // Made a move away from the target point, so failed authentication
                        failedAuthentication = true;
                    }
                    else
                    {
                        if (newDistance < epsilon)
                        {
                            // Hit the target point, so remove it and target the next one
                            manipulatableTargetPoints.Dequeue();
                            if (manipulatableTargetPoints.Count > 0)
                            {
                                oldDistance = Util.euclideanDistance(newPoint, manipulatableTargetPoints.Peek());
                            }
                        }
                        else
                        {
                            // Didn't hit the target point, but getting closer
                            oldDistance = newDistance;
                        }
                    }
                }
            }
        }

        public bool successfulAuthentication()
        {
            return (!failedAuthentication) && (manipulatableTargetPoints.Count == 0);
        }
    }
}
