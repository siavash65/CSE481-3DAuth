using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // Cube used for bounding spaces
    class Cube
    {
        /* Vertices
         *     4------5
         *    /|     /|
         *   / |    / |
         *  0-(7)--1--6
         *  | /    | /
         *  |/     |/
         *  3------2           
         */
        protected int[,] _vertices;

        public int[,] vertices
        {
            get { return _vertices; }
        }

        public Cube()
        {
            _vertices = new int[8, 3];
            for (int i = 0; i < 8; i++)
            {
                vertices[i, 0] = 0;
                vertices[i, 1] = 0;
                vertices[i, 2] = 0;
            }
        }

        public Cube(int[,] vertices)
        {
            this._vertices = vertices;
        }

        public static Cube CreateBoundingCube(PointCluster points)
        {
            // Start all the vertices out at the first point then grow them accordingly
            Cube boundingCube = new Cube();
            bool initialized = false;
            foreach (DepthPoint point in points.points)
            {
                if (!initialized)
                {
                    initialized = true;
                    for (int i = 0; i < 8; i++)
                    {
                        boundingCube.vertices[i, 0] = point.x;
                        boundingCube.vertices[i, 1] = point.y;
                        boundingCube.vertices[i, 2] = point.depth;
                    }
                }
                else
                {
                    if (point.x < boundingCube.vertices[0, 0])
                    {
                        boundingCube.vertices[0, 0] = point.x;
                        boundingCube.vertices[3, 0] = point.x;
                        boundingCube.vertices[4, 0] = point.x;
                        boundingCube.vertices[7, 0] = point.x;
                    }
                    else if (point.x > boundingCube.vertices[1, 0])
                    {
                        boundingCube.vertices[1, 0] = point.x;
                        boundingCube.vertices[2, 0] = point.x;
                        boundingCube.vertices[5, 0] = point.x;
                        boundingCube.vertices[6, 0] = point.x;
                    }

                    if (point.y < boundingCube.vertices[2, 1])
                    {
                        boundingCube.vertices[2, 1] = point.y;
                        boundingCube.vertices[3, 1] = point.y;
                        boundingCube.vertices[6, 1] = point.y;
                        boundingCube.vertices[7, 1] = point.y;
                    }
                    else if (point.y > boundingCube.vertices[0, 1])
                    {
                        boundingCube.vertices[0, 1] = point.y;
                        boundingCube.vertices[1, 1] = point.y;
                        boundingCube.vertices[4, 1] = point.y;
                        boundingCube.vertices[5, 1] = point.y;
                    }

                    if (point.depth < boundingCube.vertices[0, 2])
                    {
                        boundingCube.vertices[1, 2] = point.depth;
                        boundingCube.vertices[2, 2] = point.depth;
                        boundingCube.vertices[3, 2] = point.depth;
                        boundingCube.vertices[4, 2] = point.depth;
                    }

                    if (point.depth > boundingCube.vertices[4, 2])
                    {
                        boundingCube.vertices[4, 2] = point.depth;
                        boundingCube.vertices[5, 2] = point.depth;
                        boundingCube.vertices[6, 2] = point.depth;
                        boundingCube.vertices[7, 2] = point.depth;
                    }
                }
            }

            return boundingCube;
        }
    }

    class BoundingRectangle : Cube
    {
        public BoundingRectangle()
        {
            this._vertices = new int[4, 2];
        }

        public BoundingRectangle(int[,] vertices)
        {
            this._vertices = vertices;
        }

        public static BoundingRectangle CreateBoundingRectangle(PointCluster points)
        {
            Cube cube = CreateBoundingCube(points);
            BoundingRectangle result = new BoundingRectangle();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    result.vertices[i, j] = cube.vertices[i, j];
                }
            }
            return result;
        }
    }
}
