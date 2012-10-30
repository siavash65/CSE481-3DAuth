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
    }
}
