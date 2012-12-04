﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    class User
    {
        public String name { get; set; }
        public String imgPath { get; set; }
        public List<Point> password { get; set; }

        public User(String nm, String iPath, List<Point> psw )
        {
            name = nm;
            imgPath = iPath;
            password = psw;
        }
    }
}