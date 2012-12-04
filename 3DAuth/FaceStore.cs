using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace ThreeDAuth
{
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

        List<FeaturePair> features;
        float[] totals;
        int[] fts = { 20, 53, 23, 56, 0, 10, 90, 91, 88, 89, 1, 34 };

        private const int NUM_SAMPLES = 50;

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
            initFeatures();

        }

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

                String s = DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;


                StreamWriter writer = new StreamWriter("C:\\Users\\Administrator\\Documents\\Facial Testing\\" + s + ".txt");
                writer.WriteLine("Anton");


                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] /= NUM_SAMPLES;
                    writer.Write(totals[i] + ",");
                    //writer.WriteLine(totals[i]);
                }
                writer.Write("Siavash");
                writer.Close();

                count++;
                classifier.verifyUser(totals);
            }
            else
            {
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
