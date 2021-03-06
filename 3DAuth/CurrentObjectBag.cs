﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    delegate void GiveGestureValidatorReference(GestureValidator validator);
    delegate void GiveGestureLearnerReference(GestureLearner learner);
    delegate void GiveFaceClassifierReference(FaceClassifier classifier);

    // singleton that holds references to current objects such as:
    // current gesture validator
    class CurrentObjectBag
    {
        private static CurrentObjectBag instance;


        private GestureValidator _CurrentGestureValidator;
        private GestureLearner _CurrentGestureLearner;
        private FaceClassifier _CurrentFaceClassifier;

        public bool LearningNewUser { get; set; }

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

        public FaceClassifier CurrentFaceClassifier
        {
            get
            {
                return _CurrentFaceClassifier;
            }
            set
            {
                _CurrentFaceClassifier = value;
                if (_onFaceClassifierChanged != null)
                {
                    _onFaceClassifierChanged(value);
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

        // S prefix is for the static reference
        public static FaceClassifier SCurrentFaceClassifier
        {
            get
            {
                return CurrentObjectBag.GetInstance().CurrentFaceClassifier;
            }
            set
            {
                CurrentObjectBag.GetInstance().CurrentFaceClassifier = value;
            }
        }

        // S prefix is for the static reference
        public static bool SLearningNewUser
        {
            get
            {
                return CurrentObjectBag.GetInstance().LearningNewUser;
            }
            set
            {
                CurrentObjectBag.GetInstance().LearningNewUser = value;
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
        private event GiveFaceClassifierReference _onFaceClassifierChanged;

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

        public event GiveFaceClassifierReference onFaceClassifierChanged
        {
            add
            {
                _onFaceClassifierChanged += value;
            }
            remove
            {
                _onFaceClassifierChanged -= value;
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

        public static event GiveFaceClassifierReference SOnFaceClassifierChanged
        {
            add
            {
                CurrentObjectBag.GetInstance().onFaceClassifierChanged += value;
            }
            remove
            {
                CurrentObjectBag.GetInstance().onFaceClassifierChanged -= value;
            }
        }
    }
}
