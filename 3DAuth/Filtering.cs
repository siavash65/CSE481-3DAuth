using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class Statistics
    {
        /// <summary>
        /// Implementation found at http://www.johndcook.com/csharp_phi.html
        /// </summary>
        /// <param name="zValue"></param>
        /// <returns></returns>
        public static double StandardGaussianCDF(double zValue)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (zValue < 0)
                sign = -1;
            zValue = Math.Abs(zValue) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * zValue);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-zValue * zValue);

            return 0.5 * (1.0 + sign * y);
        }

        public static double GaussianCDF(double mean, double variance, double value)
        {
            return StandardGaussianCDF((value - mean) / variance);
        }
    }

    // Simple gaussian filter on the position and motion
    class PositionMotionFilter
    {
        private DepthPoint previous2;
        private DepthPoint previous1;
        private long time1;
        private long time2;

        public PositionMotionFilter()
        {
        }

        public bool IsValidPoint(DepthPoint point)
        {
            if (previous2 == null && previous1 == null)
            {
                previous1 = point;
                time1 = System.DateTime.UtcNow.Ticks;
                return true;
            }
            else if (previous2 == null)
            {
                previous2 = previous1;
                time2 = time1;
                previous1 = point;
                time1 = System.DateTime.UtcNow.Ticks;
                // TODO: finish with this case
            }
            // TODO: Fill out with general case
            return false;
        }

        private double GetPreviousVelocity()
        {
            return Util.EuclideanDistance2d(previous1, previous2) / (double) ( (time1 / 1000.0) - (time2 / 1000.0));
        }
    }


    // ***Incomplete***
    class ParticleFilter
    {
        private class Particle
        {
            public Tuple<int, int> position;
            
            public int x 
            {
                get
                {
                    return position.Item1;
                }
            }
            public int y 
            {
                get
                {
                    return position.Item2;
                }
            }

            public Particle(int x, int y)
            {
                position = new Tuple<int, int>(x, y);
            }
        }


        private const int NUM_PARTICLES = 1000;
        private Dictionary<Particle, double> particleMap;

        public ParticleFilter()
        {
            particleMap = new Dictionary<Particle, double>();
        }

        public void RandomlyDistribute(int width, int height)
        {
            Random random = new Random();
            for (int i = 0; i < NUM_PARTICLES; i++)
            {
                particleMap.Add(new Particle(random.Next(0, width), random.Next(0, height)), 1.0 / (double) NUM_PARTICLES);
            }
        }

        private void NormalizeWeights()
        {
            double nu = 0;
            foreach (KeyValuePair<Particle, double> particleWeight in particleMap)
            {
                nu += particleWeight.Value;
            }
            foreach (Particle particle in particleMap.Keys)
            {
                particleMap.Add(particle, particleMap[particle] / nu);
            }
        }

        public void Update()
        {
        }

        public void Resample()
        {
            Dictionary<Particle, double> newParticleMap = new Dictionary<Particle, double>();



            particleMap = newParticleMap;
        }
    }
}
