using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ThreeDAuth
{
    delegate void GiveUser(User u);

    class FaceClassifier
    {
        private XmlDocument data;
        private const double MAX_DIFF = 6;

        private event GiveUser _onUserRecieved;
        
        public FaceClassifier()
        {
            data = new XmlDocument();

            try
            {
                data.Load("users.xml");
            }
            catch (Exception)
            {
                Console.WriteLine("file not found");
            }
            
        }

        private void Notify(User u)
        {
            // could be an issue if no one is listening, but there should be a listener by the time this is called
            if (_onUserRecieved != null)
                _onUserRecieved(u);
        }

        public event GiveUser OnUserReceived
        {
            add
            {
                _onUserRecieved += value;
            }
            remove
            {
                _onUserRecieved -= value;
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
                    User current = new User(user["name"].InnerText, user["user-image"].InnerText, null);
                    Notify(current);
                    return null;
                }
            }

            /*
            XmlElement newUser = data.CreateElement("user");
            XmlElement newUserName = data.CreateElement("name");
            XmlElement faceParams = data.CreateElement("face-params");

            for (int j = 0; j < vals.Length; j++)
            {
                XmlElement param = data.CreateElement("param");
                XmlElement id = data.CreateElement("id");
                XmlElement mean = data.CreateElement("mean");
                XmlElement sdv = data.CreateElement("stdev");

                id.Value = (j + 1).ToString();
                mean.Value = (vals[j]).ToString();
                sdv.Value = "0";
                param.AppendChild(id);
                param.AppendChild(mean);
                param.AppendChild(sdv);

                faceParams.AppendChild(param);
            }

            newUser.AppendChild(newUserName);
            newUser.AppendChild(faceParams);

            data.AppendChild(newUser);
            */
            Console.WriteLine("New User");
            User cur = new User("", "", null);
            Notify(cur);

            return null;
        }

        private float getZScore(float observed, float mean, float stdev)
        {
            return Math.Abs((observed - mean) / stdev);
        }


    }
}
