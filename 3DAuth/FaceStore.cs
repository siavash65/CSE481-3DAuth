﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace ThreeDAuth
{

    delegate void GiveCount(int count, int total);

    class FaceStore
    {
         
        int count;

        //private event GiveCount _OnCountReceived;

        List<FeaturePair> features;
        List<double>[] totalsList;
        double[] totals;
        int[] fts = { 20, 53, 23, 56, 0, 10, 90, 91, 88, 89, 1, 34, 15, 48 };

        List<List<double>> learnList;

        private const int NUM_SAMPLES = 50;

        private const int NUM_LEARNING_SCANS = 3;
        private int learnCount;

        FaceClassifier classifier;


        public double getDist(Vector3DF point1, Vector3DF point2)
        {
            double X1 = point1.X;
            double Y1 = point1.Y;
            double Z1 = point1.Z;

            double X2 = point2.X;
            double Y2 = point2.Y;
            double Z2 = point2.Z;

            
            double deltaX = X1 - X2;
            double deltaY = Y1 - Y2;
            double deltaZ = Z1 - Z2;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            return distance;
        }

        public FaceStore(FaceClassifier fc)
        {
            count = 0;
            classifier = fc;

            features = new List<FeaturePair>();
            learnList = new List<List<double>>();

            learnCount = 0;

            initFeatures();

        }

        /*
        private void Notify(int count, int total)
        {
            // could be an issue if no one is listening, but there should be a listener by the time this is called
            if (_OnCountReceived != null)
                _OnCountReceived(count, total);
        }

        public event GiveCount OnCountReceived
        {
            add
            {
                _OnCountReceived += value;
            }
            remove
            {
                _OnCountReceived -= value;
            }
        }
        */

        private void initFeatures()
        {

            for (int i = 0; i < fts.Length - 1; i += 2)
            {
                FeaturePair temp = new FeaturePair();
                temp.P1 = fts[i];
                temp.P2 = fts[i + 1];
                features.Add(temp);
            }

            totalsList = new List<double>[features.Count];
            for (int j = 0; j < totalsList.Length; j++)
            {
                totalsList[j] = new List<double>();
            }

            totals = new double[features.Count];
        }

        public void updateData(EnumIndexableCollection<FeaturePoint, Vector3DF> pts)
        {
            Console.WriteLine(count);
            if (count > NUM_SAMPLES)
            {
                //Environment.Exit(0);
                return;
            }
            else if (count == NUM_SAMPLES)
            {
                //Notify(count, NUM_SAMPLES);
                String s = DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;

                //StreamWriter writer = new StreamWriter("C:\\Users\\Administrator\\Documents\\Facial Testing\\" + s + ".txt");
                //writer.WriteLine("Siavash");

                if (!CurrentObjectBag.SLearningNewUser)
                {
                    int trim = NUM_SAMPLES / 4;
                    for (int i = 0; i < totalsList.Length; i++)
                    {
                        totalsList[i].Sort();
                        totalsList[i].RemoveRange(0, trim);
                        totalsList[i].RemoveRange(totalsList[i].Count - trim, trim);
                        totals[i] = totalsList[i].Sum();
                        totals[i] /= totalsList[i].Count;
                    }
                } else {
                    for (int i = 0; i < totals.Length; i++)
                    {
                        totals[i] /= NUM_SAMPLES;
                        // writer.WriteLine(totals[i]);
                        //writer.WriteLine(totals[i]);
                    }
                }

                // writer.Write("Siavash");
                // writer.Close();

                if (CurrentObjectBag.SLearningNewUser)
                {
                    // new user, so do additional scans
                    List<double> tempList = new List<double>();
                    for (int i = 0; i < totals.Length; i++)
                    {
                        tempList.Add(totals[i]);
                        totals[i] = 0;
                    }
                    learnList.Add(tempList);

                    if (learnCount < NUM_LEARNING_SCANS - 1)
                    {
                        count = 0;
                        learnCount++;
                        //Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.faceScanCounter++;
                        ThreeDAuth.MainWindow.faceScanCounter++;

                    }
                    else
                    {
                        count++;
                        classifier.addUser(learnList);
                    }

                }
                else
                {
                    // existing user, so done scanning and validate it
                    //Notify(count, NUM_SAMPLES);
                    count++;
                    classifier.verifyUser(totals);
                }

            }
            else
            {

                //Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.faceScanCount++;
                //Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.faceScanCounter = learnCount;
                ThreeDAuth.MainWindow.faceScanCount++;
                ThreeDAuth.MainWindow.faceScanCounter = learnCount;
                count++;

                for (int i = 0; i < features.Count; i++)
                {
                    double dist = getDist(pts[features[i].P1], pts[features[i].P2]);
                    totals[i] += dist;
                    totalsList[i].Add(dist);
                }

            }
        }



        private struct FeaturePair
        {
            public int P1;
            public int P2;
        }
    }
}
