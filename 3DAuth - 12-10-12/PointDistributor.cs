using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    delegate void GivePoint(Point p);

    // Note: This is a singleton class
    class PointDistributor
    {
        private Point _currentPoint;
        private event GivePoint _onPointReceived;
        private static PointDistributor instance;

        private PointDistributor() { }

        public static PointDistributor GetInstance()
        {
            if (instance == null)
            {
                instance = new PointDistributor();
            }
            return instance;
        }

        public Point CurrentPoint
        {
            get { return _currentPoint; }
            set
            {
                if (_currentPoint != value)
                {
                    _currentPoint = value;
                    Notify();
                }
            }
        }

        public static void SGivePoint(Point p)
        {
            PointDistributor.GetInstance().GivePoint(p);
        }

        public void GivePoint(Point p)
        {
            this.CurrentPoint = p;
        }

        private void Notify()
        {
            if (_onPointReceived != null)
                _onPointReceived(_currentPoint);
        }

        public event GivePoint OnPointReceived
        {
            add
            {
                _onPointReceived += value;
            }
            remove
            {
                _onPointReceived -= value;
            }
        }
    }
}
