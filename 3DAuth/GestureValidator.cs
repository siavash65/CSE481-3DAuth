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
        private Queue<Point2f> targetPoints;
        private Queue<Point2f> manipulatableTargetPoints;
        private HashSet<Point2f> completedTargets;
        private double oldDistance;
        private double epsilon;
        private bool failedAuthentication;

        public GestureValidator(Queue<Point2f> targetPoints, double epsilonBoundary)
        {
            this.targetPoints = targetPoints;
            this.epsilon = epsilonBoundary;
            beginPath();
        }

        public void beginPath()
        {
            for (int i = 0; i < targetPoints.Count; i++) {
                Point2f point = targetPoints.Dequeue();
                targetPoints.Enqueue(point);
                manipulatableTargetPoints.Enqueue(point);
            }
            oldDistance = double.MaxValue;
            failedAuthentication = false;
        }

        public void updatePath(Point2f newPoint)
        {
            Point2f target = manipulatableTargetPoints.Peek();
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

                    //TODO check to see if have accumulated enough points to make a new target
                }
            }
        }

        public bool successfulAuthentication()
        {
            return (!failedAuthentication) && (manipulatableTargetPoints.Count == 0);
        }
    }
}
