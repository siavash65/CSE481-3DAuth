using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    // singleton that holds references to current objects such as:
    // 
    class CurrentObjectBag
    {
        private static CurrentObjectBag instance;
        public GestureValidator CurrentGestureValidator { get; set; }

        // S prefix is for the static reference
        public static GestureValidator SCurrentGestureValidator
        {
            get
            {
                return CurrentObjectBag.GetInstance().CurrentGestureValidator;
            }
            set
            {
                CurrentObjectBag.GetInstance().CurrentGestureValidator = value;
            }
        }

        private CurrentObjectBag() { }

        public static CurrentObjectBag GetInstance()
        {
            if (instance == null)
            {
                instance = new CurrentObjectBag();
            }
            return instance;
        }
    }
}
