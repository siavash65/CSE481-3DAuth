using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{

    class Util
    {
        public static double euclideanDistance(Point2d p1, Point2d p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                                (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public static double euclideanDistance(PlanePoint p1, PlanePoint p2)
        {
            return euclideanDistance(p1.getPoint2d(), p2.getPoint2d());
        }
        public PointCluster floodFill(short[] depthData, int x, int y, int width, int height, int cutoff)
        {
            // Queue of tuples storing <newX, newY, previousDepth>
            Queue<Tuple<int, int, short>> explorePoints = new Queue<Tuple<int, int, short>>();
            HashSet<Tuple<int, int>> exploredPoints = new HashSet<Tuple<int,int>>();
            HashSet<DepthPoint> resultPoints = new HashSet<DepthPoint>();

            explorePoints.Enqueue(new Tuple<int, int, short>(x, y, depthData[x + y * width]));
            exploredPoints.Add(new Tuple<int, int>(x, y));
            while (explorePoints.Count > 0)
            {
                Tuple<int, int, short> currentPoint = explorePoints.Dequeue();
                short currentDepth = depthData[currentPoint.Item1 + currentPoint.Item2 * width];
                if (Math.Abs(currentDepth - currentPoint.Item3) < cutoff)
                {
                    resultPoints.Add(new DepthPoint(currentPoint.Item1, currentPoint.Item2, currentDepth));

                    // Add the neighboring points to be explored
                    Tuple<int, int, short> leftNeighbor = new Tuple<int, int, short>(currentPoint.Item1 - 1, currentPoint.Item2, currentDepth);
                    Tuple<int, int, short> rightNeighbor = new Tuple<int, int, short>(currentPoint.Item1 + 1, currentPoint.Item2, currentDepth);
                    Tuple<int, int, short> topNeighbor = new Tuple<int, int, short>(currentPoint.Item1, currentPoint.Item2 + 1, currentDepth);
                    Tuple<int, int, short> bottomNeighbor = new Tuple<int, int, short>(currentPoint.Item1, currentPoint.Item2 - 1, currentDepth);

                    Tuple<int, int> leftNeighborPoint = new Tuple<int, int>(currentPoint.Item1 - 1, currentPoint.Item2);
                    Tuple<int, int> rightNeighborPoint = new Tuple<int, int>(currentPoint.Item1 + 1, currentPoint.Item2);
                    Tuple<int, int> topNeighborPoint = new Tuple<int, int>(currentPoint.Item1, currentPoint.Item2 + 1);
                    Tuple<int, int> bottomNeighborPoint = new Tuple<int, int>(currentPoint.Item1, currentPoint.Item2 - 1);

                    if (leftNeighbor.Item1 >= 0 && !exploredPoints.Contains(leftNeighborPoint))
                    {
                        exploredPoints.Add(leftNeighborPoint);
                        explorePoints.Enqueue(leftNeighbor);
                    }
                    if (rightNeighbor.Item1 < width && !exploredPoints.Contains(rightNeighborPoint))
                    {
                        exploredPoints.Add(rightNeighborPoint);
                        explorePoints.Enqueue(rightNeighbor);
                    }
                    if (topNeighbor.Item2 < height && !exploredPoints.Contains(topNeighborPoint))
                    {
                        exploredPoints.Add(topNeighborPoint);
                        explorePoints.Enqueue(topNeighbor);
                    }
                    if (bottomNeighbor.Item2 >= 0 && !exploredPoints.Contains(bottomNeighborPoint))
                    {
                        exploredPoints.Add(bottomNeighborPoint);
                        explorePoints.Enqueue(bottomNeighbor);
                    }
                }
            }
            PointCluster result = new PointCluster(resultPoints);
            return result;
        }

    }
    }
}
