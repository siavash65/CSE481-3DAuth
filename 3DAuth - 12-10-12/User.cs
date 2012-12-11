using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class User
    {
        public String name { get; set; }
        public String imgPath { get; set; }
        public List<Point2d> password { get; set; }
        public List<Point2d> faceParams { get; set; }
        public UserInfoTuple[] StoredData;

        public User(String nm, String iPath, List<Point2d> psw, List<Point2d> scan, UserInfoTuple[] storedData)
        {
            name = nm;
            imgPath = iPath;
            password = psw;
            faceParams = scan;
            StoredData = storedData != null ? storedData : new UserInfoTuple[5];
        }

        /*
        public void AddStoredData(String reference, String username, String password)
        {
            UserInfoTuple[] newStoredData = new UserInfoTuple[StoredData.Length + 1];
            newStoredData[0] = new UserInfoTuple(reference, username, password);
            for (int i = 0; i < StoredData.Length; i++)
            {
                newStoredData[i + 1] = StoredData[i];
            }
            StoredData = newStoredData;

        }*/

        public void SetStoredData(int idx, String reference, String username, String password)
        {
            if (idx < StoredData.Length)
            {
                UserInfoTuple newdata = new UserInfoTuple(reference, username, password);
                StoredData[idx] = newdata;
            }
        }
    }

    class UserInfoTuple
    {
        public String Reference { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }

        public UserInfoTuple(String reference, String username, String password)
        {
            Reference = reference;
            Username = username;
            Password = password;
        }
    }
}
