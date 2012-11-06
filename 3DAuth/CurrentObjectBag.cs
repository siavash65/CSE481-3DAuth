using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    delegate void GiveGestureValidatorReference(GestureValidator validator);
    delegate void GiveGestureLearnerReference(GestureLearner learner);

    // singleton that holds references to current objects such as:
    // current gesture validator
    class CurrentObjectBag
    {
        private static CurrentObjectBag instance;


        private GestureValidator _CurrentGestureValidator;
        private GestureLearner _CurrentGestureLearner;

        public GestureValidator CurrentGestureValidator
        {
            get
            {
                return _CurrentGestureValidator;
            }
            set
            {
                _CurrentGestureValidator = value;
                if (_onGestureValidatorChanged != null)
                {
                    _onGestureValidatorChanged(value);
                }
            }
        }

        public GestureLearner CurrentGestureLearner
        {
            get
            {
                return _CurrentGestureLearner;
            }
            set
            {
                _CurrentGestureLearner = value;
                if (_onGestureLearnerChanged != null)
                {
                    _onGestureLearnerChanged(value);
                }
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

        // S prefix is for the static reference
        public static GestureLearner SCurrentGestureLearner
        {
            get
            {
                return CurrentObjectBag.GetInstance().CurrentGestureLearner;
            }
            set
            {
                CurrentObjectBag.GetInstance().CurrentGestureLearner = value;
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
        private event GiveGestureLearnerReference _onGestureLearnerChanged;

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

        public event GiveGestureLearnerReference onGestureLearnerChanged
        {
            add
            {
                _onGestureLearnerChanged += value;
            }
            remove
            {
                _onGestureLearnerChanged -= value;
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

        public static event GiveGestureLearnerReference SOnGestureLearnerChanged
        {
            add
            {
                CurrentObjectBag.GetInstance().onGestureLearnerChanged += value;
            }
            remove
            {
                CurrentObjectBag.GetInstance().onGestureLearnerChanged -= value;
            }
        }
    }
}
