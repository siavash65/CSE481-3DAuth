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
        public static double StandardGaussianCDFPhi(double zValue)
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

        public static double GaussianCDFPhi(double mean, double variance, double value)
        {
            return StandardGaussianCDFPhi((value - mean) / variance);
        }

        public static double GenerateMean(ICollection<double> values)
        {
            double sum = 0;
            foreach (double value in values)
            {
                sum += value;
            }
            return sum / values.Count;
        }

        public static double GenerateVariance(ICollection<double> values, double mean)
        {
            double sumOfSquareDistToMean = 0;
            foreach (double value in values)
            {
                sumOfSquareDistToMean += Math.Pow(value - mean, 2);
            }
            return sumOfSquareDistToMean / values.Count;
        }
    }

    class GaussianHolder
    {
        private double mean;
        private double variance;

        public GaussianHolder(double mean, double variance)
        {
            this.mean = mean;
            this.variance = variance;
        }

        public double GetPhiValue(double value)
        {
            return Statistics.GaussianCDFPhi(mean, variance, value);
        }

        public static GaussianHolder GenerateGaussianFromValueCollection(ICollection<double> values)
        {
            double mean = Statistics.GenerateMean(values);
            return new GaussianHolder(mean, Statistics.GenerateVariance(values, mean));
        }

        public static GaussianHolder GenerateSumOfGaussians(GaussianHolder first, GaussianHolder second)
        {
            return new GaussianHolder(first.mean + second.mean, first.variance + second.variance);
        }

        public static GaussianHolder GenerateScalarProductOfGaussian(GaussianHolder gaussian, double scalar)
        {
            return new GaussianHolder(gaussian.mean * scalar, gaussian.variance * Math.Pow(scalar, 2));
        }

        public static GaussianHolder GenerateScalarSumOfGaussian(GaussianHolder gaussian, double scalar)
        {
            return new GaussianHolder(gaussian.mean + scalar, gaussian.variance);
        }
    }

    // Simple gaussian filter on the position and motion
    // First try: 6 individual gaussians
    class PositionMotionFilter
    {
        private const double PHI_CUTOFF = 0.05; // Probability

        private const int MIN_POINTS = 5;
        private const int HISTORY_COUNT = 20;
        private const int TICKS_PER_MS = 10000;
        private const long ERROR_TIME_CUTOFF_MS = 1000; // ms
        private const long ERROR_TIME_CUTOFF_TICKS = ERROR_TIME_CUTOFF_MS * TICKS_PER_MS; // ticks
        private const int ERROR_POINT_LIMIT = 20;

        private Queue<Tuple<DepthPoint, long>> previousPoints;
        private Queue<Tuple<DepthPoint, long>> rejectedPoints;

        public PositionMotionFilter()
        {
            previousPoints = new Queue<Tuple<DepthPoint, long>>();
            rejectedPoints = new Queue<Tuple<DepthPoint, long>>();
        }

        public bool IsValidPoint(DepthPoint point)
        {
            long now = System.DateTime.UtcNow.Ticks;
            if (previousPoints.Count < MIN_POINTS)
            {
                previousPoints.Enqueue(new Tuple<DepthPoint, long>(point, now));
                return true;
            }
            else
            {
                // Apply the 6-dimensional gaussian
                // If it's 2+ std dev's away in any dimension, reject it
                ICollection<double> xPos = new LinkedList<double>();
                ICollection<double> yPos = new LinkedList<double>();
                ICollection<double> depthPos = new LinkedList<double>();
                ICollection<long> times = new LinkedList<long>();

                ICollection<double> xVel = new LinkedList<double>();
                ICollection<double> yVel = new LinkedList<double>();
                ICollection<double> depthVel = new LinkedList<double>();
                foreach (Tuple<DepthPoint, long> pointTuple in previousPoints)
                {
                    xPos.Add(pointTuple.Item1.x);
                    yPos.Add(pointTuple.Item1.y);
                    depthPos.Add(pointTuple.Item1.depth);
                    times.Add(pointTuple.Item2);
                    if (xPos.Count > 1 && yPos.Count > 1 && depthPos.Count > 1 && times.Count > 1)
                    {
                        int prev2Index = times.Count - 2;
                        int prev1Index = times.Count - 1;
                        long timeInterval = times.ElementAt(prev1Index) - times.ElementAt(prev2Index);
                        double deltaX = xPos.ElementAt(prev1Index) - xPos.ElementAt(prev2Index);
                        double deltaY = yPos.ElementAt(prev1Index) - yPos.ElementAt(prev2Index);
                        double deltaDepth = depthPos.ElementAt(prev1Index) - depthPos.ElementAt(prev2Index);
                        xVel.Add(deltaX / timeInterval);
                        yVel.Add(deltaY / timeInterval);
                        depthVel.Add(deltaDepth / timeInterval);
                    }
                }
                GaussianHolder xPosGaussian = GaussianHolder.GenerateGaussianFromValueCollection(xPos);
                GaussianHolder yPosGaussian = GaussianHolder.GenerateGaussianFromValueCollection(yPos);
                GaussianHolder depthPosGaussian = GaussianHolder.GenerateGaussianFromValueCollection(depthPos);

                GaussianHolder xVelGaussian = GaussianHolder.GenerateGaussianFromValueCollection(xVel);
                GaussianHolder yVelGaussian = GaussianHolder.GenerateGaussianFromValueCollection(yVel);
                GaussianHolder depthVelGaussian = GaussianHolder.GenerateGaussianFromValueCollection(depthVel);

                double newPointTimeDelta = now - times.ElementAt(times.Count - 1);
                double newXVel = (point.x - xPos.ElementAt(xPos.Count - 1)) / newPointTimeDelta;
                double newYVel = (point.y - yPos.ElementAt(yPos.Count - 1)) / newPointTimeDelta;
                double newDepthVel = (point.depth - depthPos.ElementAt(depthPos.Count - 1)) / newPointTimeDelta;

                // approximate the new expected position
                GaussianHolder deltaXPosGaussian = GaussianHolder.GenerateScalarProductOfGaussian(xVelGaussian, newPointTimeDelta);
                GaussianHolder deltaYPosGaussian = GaussianHolder.GenerateScalarProductOfGaussian(yVelGaussian, newPointTimeDelta);
                GaussianHolder deltaDepthPosGaussian = GaussianHolder.GenerateScalarProductOfGaussian(depthVelGaussian, newPointTimeDelta);

                GaussianHolder newXPosGaussian = GaussianHolder.GenerateSumOfGaussians(xPosGaussian, deltaXPosGaussian);
                GaussianHolder newYPosGaussian = GaussianHolder.GenerateSumOfGaussians(yPosGaussian, deltaYPosGaussian);
                GaussianHolder newDepthPosGaussian = GaussianHolder.GenerateSumOfGaussians(depthPosGaussian, deltaDepthPosGaussian);

                double xVelPhi = xVelGaussian.GetPhiValue(newXVel);
                double yVelPhi = yVelGaussian.GetPhiValue(newYVel);
                double depthVelPhi = depthVelGaussian.GetPhiValue(newDepthVel);
                double xPosPhi = newXPosGaussian.GetPhiValue(point.x);
                double yPosPhi = newYPosGaussian.GetPhiValue(point.y);
                double depthPosPhi = newDepthPosGaussian.GetPhiValue(point.depth);

                Console.WriteLine("Standard Deviations: " + xPosPhi + " | " + yPosPhi + " | " + depthPosPhi + " | " +
                                                            xVelPhi + " | " + yVelPhi + " | " + depthVelPhi);


                if ( (xVelPhi > PHI_CUTOFF && xVelPhi < (1 - PHI_CUTOFF)) &&
                     (yVelPhi > PHI_CUTOFF && yVelPhi < (1 - PHI_CUTOFF)) &&
                     (depthVelPhi > PHI_CUTOFF && depthVelPhi < (1 - PHI_CUTOFF)) &&
                     (xPosPhi > PHI_CUTOFF && xPosPhi < (1 - PHI_CUTOFF)) &&
                     (yPosPhi > PHI_CUTOFF && yPosPhi < (1 - PHI_CUTOFF)) &&
                     (depthPosPhi > PHI_CUTOFF && depthPosPhi < (1 - PHI_CUTOFF)) )
                {
                    previousPoints.Enqueue(new Tuple<DepthPoint, long>(point, now));
                    if (previousPoints.Count > HISTORY_COUNT)
                    {
                        previousPoints.Dequeue();
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine("Rejected a point!!!!!!!!!!!!!!!!!!!!!!");
                    rejectedPoints.Enqueue(new Tuple<DepthPoint, long>(point, now));
                    while (now - rejectedPoints.First().Item2 > ERROR_TIME_CUTOFF_TICKS)
                    {
                        Console.WriteLine("dumping rejected points");
                        rejectedPoints.Dequeue();
                    }
                    if (rejectedPoints.Count > ERROR_POINT_LIMIT)
                    {
                        // Received so many error points recently we should probably start over
                        rejectedPoints.Clear();
                        previousPoints.Clear();
                        previousPoints.Enqueue(new Tuple<DepthPoint, long>(point, now));
                        Console.WriteLine("***************** STARTING OVER ************************");
                        return true;
                    }
                    else
                    {
                        // Haven't received that many error points ecently and we rejected the most recent one
                        return false;
                    }
                }
            }
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
