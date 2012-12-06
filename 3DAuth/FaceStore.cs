using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace ThreeDAuth
{

    delegate void GiveCount(int count, int total);

   
    public enum FeatureLength
    {
        OuterEyes = 0,
        InnerEyes = 1,
        DownCenter = 2,
        CheekWidth = 3,
        MouthWidth = 4
    }


    class FaceStore
    {
         
        int count;

        //private event GiveCount _OnCountReceived;

        List<FeaturePair> features;
        float[] totals;
        int[] fts = { 20, 53, 23, 56, 0, 10, 90, 91, 88, 89, 1, 34 };

        List<List<float>> learnList;

        private const int NUM_SAMPLES = 50;

        private const int NUM_LEARNING_SCANS = 3;
        private int learnCount;

        FaceClassifier classifier;


        public float getDist(Vector3DF point1, Vector3DF point2)
        {
            float deltaX = point1.X - point2.X;
            float deltaY = point1.Y - point2.Y;
            float deltaZ = point1.Z - point2.Z;

            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            return distance;
        }

        public FaceStore(FaceClassifier fc)
        {
            count = 0;
            classifier = fc;

            features = new List<FeaturePair>();
            learnList = new List<List<float>>();

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

            totals = new float[features.Count];
        }

        public void updateData(EnumIndexableCollection<FeaturePoint, Vector3DF> pts)
        {
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
               // writer.WriteLine("Siavash");


                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] /= NUM_SAMPLES;
                   // writer.WriteLine(totals[i]);
                    //writer.WriteLine(totals[i]);
                }
               // writer.Write("Siavash");
               // writer.Close();

                if (CurrentObjectBag.SLearningNewUser)
                {
                    // new user, so do additional scans
                    List<float> tempList = new List<float>();
                    for (int i = 0; i < totals.Length; i++)
                    {
                        tempList.Add(totals[i]);
                        totals[i] = 0;
                    }
                    learnList.Add(tempList);

                    if (learnCount < NUM_LEARNING_SCANS)
                    {
                        count = 0;
                        learnCount++;
                    }
                    else
                    {
                        count++;
                        classifier.addUser(learnList);
                    }

                }
                else
                {
                    // existing user, so done sanding and validate it
                    //Notify(count, NUM_SAMPLES);
                    count++;
                    classifier.verifyUser(totals);
                }

            }
            else
            {
                Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.faceScanCounter++;
                Microsoft.Samples.Kinect.SkeletonBasics.MainWindow.faceScanCount++;
                Console.WriteLine(count);
                count++;

                for (int i = 0; i < features.Count; i++)
                {
                    totals[i] += getDist(pts[features[i].P1], pts[features[i].P2]);
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
