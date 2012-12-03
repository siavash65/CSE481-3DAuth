using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ThreeDAuth
{
    class FaceClassifier
    {
        private XmlDocument data;
        private const double MAX_DIFF = 6;
        
        public FaceClassifier()
        {
            data = new XmlDocument();

            try
            {
                data.Load("users.xml");
            }
            catch (Exception)
            {
                Console.WriteLine("fuck");
            }
            
        }

        public String verifyUser(float[] vals)
        {

            XmlNodeList users = data.GetElementsByTagName("user");

            foreach (XmlNode user in users)
            {
                double total = 0;
                //Console.WriteLine(user["name"].InnerText);
                XmlNode mets = user["face-params"];

                for (int i = 0; i < mets.ChildNodes.Count; i++)
                {
                    XmlNode met = mets.ChildNodes[i];
                    float mean = (float)Convert.ToDouble(met["mean"].InnerText);
                    float stdev = (float)Convert.ToDouble(met["stdev"].InnerText);

                    float score = getZScore(vals[i], mean, stdev);
                    total += score;
                    //Console.WriteLine(score);
                }

                if (total <= MAX_DIFF)
                {
                    Console.WriteLine(user["name"].InnerText);
                    return null;
                }
            }


            Console.WriteLine("New User");
            return null;
        }

        private float getZScore(float observed, float mean, float stdev)
        {
            return Math.Abs((observed - mean) / stdev);
        }


    }
}
