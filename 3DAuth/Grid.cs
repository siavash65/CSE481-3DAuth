using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{

    // Class representing a grid square
    // A grid square knows about its position in the grid and what its neighbors are
    class GridSquare
    {
        private int gridSquareX;
        private int gridSquareY;
        private Grid grid;
        private Point2d upperLeftCorner;
        private Point2d upperRightCorner;
        private Point2d lowerRightCorner;
        private Point2d lowerLeftCorner;

        public bool containsPoint(Point2d point)
        {
            return (point.X >= lowerLeftCorner.X &&
                    point.X < upperRightCorner.X &&
                    point.Y >= lowerLeftCorner.Y &&
                    point.Y < upperRightCorner.Y);
        }

        public bool containsPoint(Point3d point)
        {
            return containsPoint(new Point2d(point.X, point.Y));
        }

        public Point2d getCenter()
        {
            return new Point2d((lowerLeftCorner.X + upperRightCorner.X) / 2,
                                (lowerLeftCorner.Y + upperRightCorner.Y) / 2);
        }
    }

    // Grid overlayed on the target box used for tracking hand position
    class Grid
    {
        GridSquare[,] grid;
        TargetBox owningTargetBox;
        int dimX;
        int dimY;

        public Grid(TargetBox box, int dimX, int dimY)
        {
            owningTargetBox = box;
            this.dimX = dimX;
            this.dimY = dimY;
            grid = new GridSquare[dimX, dimY];
        }

        public GridSquare getGridSquareContainingPoint(Point3d point)
        {
            // Initially we just naively search over the entire grid, later we can build in knowledge of closeness to improve performance
            for (int row = 0; row < dimX; row++)
            {
                for (int col = 0; col < dimY; col++)
                {
                    if (grid[row, col].containsPoint(point))
                    {
                        return grid[row, col];
                    }
                }
            }
            return null; // Didn't find a square containing it
        }


    }

    // Determines a path through grid squares that a hand is taking
    class GridPath
    {
        protected LinkedList<GridSquare> currentPath;
        protected Grid grid;

        public GridPath(Grid grid)
        {
            this.grid = grid;
            currentPath = new LinkedList<GridSquare>();
        }

        public void beginPath()
        {
            currentPath = new LinkedList<GridSquare>();
        }

        public void updatePath(Point3d point)
        {
            GridSquare currentSquare = grid.getGridSquareContainingPoint(point);
            if (currentSquare != null)
            {
                currentPath.AddLast(currentSquare);
            }
        }

        public LinkedList<GridSquare> getPath()
        {
            return currentPath;
        }
    }

    // Points in image plane
    // Feedback during training (placing a target point)

    // We set in a series of grid squares that the user is expected to hit and then monitor their path
    // If they ever leave a target square and pursue a non-decreasing (in euclidean distance) path to
    // The next target, they fail the authentication
    class CheckPointPath : GridPath
    {
        private Queue<GridSquare> originalTargetSequence;
        private Queue<GridSquare> manipulatableTargetSequence;
        private HashSet<GridSquare> completedTargets;
        private double previousDistance;
        private GridSquare currentTarget;
        private bool failedAuthentication;

        public CheckPointPath(Queue<GridSquare> targetSquares, Grid grid)
            : base(grid)
        {
            this.originalTargetSequence = targetSquares;
            this.completedTargets = new HashSet<GridSquare>();

            this.manipulatableTargetSequence = new Queue<GridSquare>();
            for (int i = 0; i < originalTargetSequence.Count; i++)
            {
                GridSquare square = originalTargetSequence.Dequeue();
                originalTargetSequence.Enqueue(square);
                manipulatableTargetSequence.Enqueue(square);
            }

            currentTarget = manipulatableTargetSequence.Dequeue();
            this.previousDistance = Double.MaxValue;
            this.failedAuthentication = false;
        }

        public void updatePath(Point3d point)
        {
            GridSquare currentSquare = grid.getGridSquareContainingPoint(point);
            if (currentSquare != null)
            {
                currentPath.AddLast(currentSquare);
            }

            // Advance to the next target
            if (currentSquare == currentTarget)
            {
                currentTarget = manipulatableTargetSequence.Dequeue();
            }

            double currentDistance = euclideanDistance(new Point2d(point.X, point.Y), currentTarget.getCenter());
            if (currentDistance > previousDistance)
            {
                failedAuthentication = true;
            }
            else
            {
                previousDistance = currentDistance;
            }
        }

        private double euclideanDistance(Point2d p1, Point2d p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                              (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public bool passedAuthentication()
        {
            return (!failedAuthentication) && (manipulatableTargetSequence.Count == 0);
        }

        public bool completedPath()
        {
            return (failedAuthentication) || (manipulatableTargetSequence.Count == 0);
        }
    }
}
