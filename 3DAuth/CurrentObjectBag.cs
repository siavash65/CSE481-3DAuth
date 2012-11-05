using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    delegate void GiveGestureValidatorReference(GestureValidator validator);

    // singleton that holds references to current objects such as:
    // current gesture validator
    class CurrentObjectBag
    {
        private static CurrentObjectBag instance;

        private GestureValidator _CurrentGestureValidator;

        public GestureValidator CurrentGestureValidator {
            get
            {
                return _CurrentGestureValidator;
            }
            set 
            {
                _CurrentGestureValidator = value;
                _onGestureValidatorChanged(value);
            }
        }

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


        // listeners

        private event GiveGestureValidatorReference _onGestureValidatorChanged;

        public event GiveGestureValidatorReference onGestureValidatorChanged
        {
            add
            {
                _onGestureValidatorChanged += value;
            }
            remove
            {
                _onGestureValidatorChanged -= value;
            }
        }

        public static event GiveGestureValidatorReference SOnGestureValidatorChanged
        {
            add
            {
                CurrentObjectBag.GetInstance().onGestureValidatorChanged += value;
            }
            remove
            {
                CurrentObjectBag.GetInstance().onGestureValidatorChanged -= value;
            }
        }
    }
}
