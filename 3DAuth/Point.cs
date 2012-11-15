using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    interface Point { }

    class Point3d : Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3d() : this(0, 0, 0) { }

        public Point3d(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static Point3d operator +(Point3d firstPoint, Point3d secondPoint)
        {
            Point3d sumPoint = new Point3d();
            sumPoint.X = firstPoint.X + secondPoint.X;
            sumPoint.Y = firstPoint.Y + secondPoint.Y;
            sumPoint.Z = firstPoint.Z + secondPoint.Z;
            return sumPoint;
        }

        public static Point3d operator *(Point3d firstPoint, double constant)
        {
            Point3d multPoint = new Point3d();
            multPoint.X = firstPoint.X * constant;
            multPoint.Y = firstPoint.Y * constant;
            multPoint.Z = firstPoint.Z * constant;
            return multPoint;
        }

        public static Point3d operator /(Point3d firstPoint, double constant)
        {
            return firstPoint * (1.0 / constant);
        }
    }

    class Point2d : Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        protected Point2d() : this(0, 0) { }

        public Point2d(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Point2d copy()
        {
            return new Point2d(X, Y);
        }

        public static Point2d operator +(Point2d firstPoint, Point2d secondPoint)
        {
            Point2d sumPoint = new Point2d();
            sumPoint.X = firstPoint.X + secondPoint.X;
            sumPoint.Y = firstPoint.Y + secondPoint.Y;
            return sumPoint;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }

    class PlanePoint : Point2d
    {
        public bool inPlane { get; set; }

        public PlanePoint(double X, double Y, bool inPlane)
        {
            this.X = X;
            this.Y = Y;
            this.inPlane = inPlane;
        }

        public Point2d getPoint2d()
        {
            return new Point2d(X, Y);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + inPlane + ")";
        }
    }

    class Vec2d
    {
        public Point2d p1 { get; set; }
        public Point2d p2 { get; set; }
        double X
        {
            get { return Math.Abs(p2.X - p1.X); }
        }

        double Y
        {
            get { return Math.Abs(p2.Y - p1.Y); }
        }

        public Vec2d() { }
        public Vec2d(Point2d p1, Point2d p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public Vec2d(Point3d p1, Point3d p2)
        {
            this.p1 = new Point2d(p1.X, p1.Y);
            this.p2 = new Point2d(p2.X, p2.Y);
        }
    }

    class Vec3d
    {
        Point3d p1 { get; set; }
        Point3d p2 { get; set; }
        public double X
        {
            get { return Math.Abs(p2.X - p1.X); }
        }
        public double Y
        {
            get { return Math.Abs(p2.Y - p1.Y); }
        }
        public double Z
        {
            get { return Math.Abs(p2.Z - p1.Z); }
        }

        public Vec3d() { }

        public Vec3d(Point3d p1, Point3d p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public double length()
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Y, 2));
        }
    }

    class DepthPoint : Point
    {
        private Tuple<int, int, short> point;
        public int x
        {
            get { return point.Item1; }
        }
        public int y
        {
            get { return point.Item2; }
        }
        public short depth
        {
            get { return point.Item3; }
        }

        public DepthPoint(Tuple<int, int, short> point)
        {
            this.point = point;
        }

        public DepthPoint(int x, int y, short depth)
        {
            this.point = new Tuple<int, int, short>(x, y, depth);
        }

        public static DepthPoint operator +(DepthPoint firstPoint, DepthPoint secondPoint)
        {
            return new DepthPoint(firstPoint.x + secondPoint.x,
                                  firstPoint.y + secondPoint.y,
                                  (short)(firstPoint.depth + secondPoint.depth));
        }

        public static DepthPoint operator *(DepthPoint firstPoint, double constant)
        {
            DepthPoint multPoint = new DepthPoint((int)(firstPoint.x * constant),
                                                  (int)(firstPoint.y * constant),
                                                  (short)(firstPoint.depth * constant));
            return multPoint;
        }

        public static DepthPoint operator /(DepthPoint firstPoint, double constant)
        {
            return firstPoint * (1.0 / constant);
        }
    }
}
