using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    interface IPoint3f
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
    }

    class Point2f
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2f(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    class PlanePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool inPlane { get; set; }

        public PlanePoint(double X, double Y, bool inPlane)
        {
            this.X = X;
            this.Y = Y;
            this.inPlane = inPlane;
        }
    }


    class GenericPoint : IPoint3f {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public GenericPoint() : this(0, 0, 0) { }

        public GenericPoint(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;

            
        }
         public static GenericPoint operator +(GenericPoint firstPoint, GenericPoint secondPoint)
        {
            GenericPoint sumPoint = new GenericPoint();
            sumPoint.X = firstPoint.X + secondPoint.X;
            sumPoint.Y = firstPoint.Y + secondPoint.Y;
            sumPoint.Z = firstPoint.Z + secondPoint.Z;
            return sumPoint;
        }
    }

    class DepthPoint : GenericPoint { }

    class Vec2f
    {
        public Point2f p1 { get; set; }
        public Point2f p2 { get; set; }
        double X
        {
            get { return Math.Abs(p2.X - p1.X); }
        }

        double Y
        {
            get { return Math.Abs(p2.Y - p1.Y); }
        }

        public Vec2f() { }
        public Vec2f(Point2f p1, Point2f p2) 
        {
            this.p1 = p1;
            this.p2 = p2;
        }
        
        public Vec2f(IPoint3f p1, IPoint3f p2) 
        {
            this.p1 = new Point2f(p1.X, p1.Y);
            this.p2 = new Point2f(p2.X, p2.Y);
        }
    }

    class Vec3f
    {
        IPoint3f p1 { get; set; }
        IPoint3f p2 { get; set; }
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

        public Vec3f() { }

        public Vec3f(IPoint3f p1, IPoint3f p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public double length()
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Y, 2));
        }
    }
}
