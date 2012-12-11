using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{
    enum HandTrackingOptions
    {
        NONE                    = 0x00,
        PROJECT_POINTS          = 0x01,
        ALLOW_TORSO_MOTION      = 0x02,
        SHOW_UNPROJECTED_HAND   = 0x04,
        SHOW_TARGET_POINTS      = 0x08
    }

    class HandTrackingOptionSet
    {
        private HandTrackingOptions options;
        private double _alpha;
        private int _floodFillCutoffmm;

        private static HandTrackingOptionSet _instance;
        private static HandTrackingOptionSet Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HandTrackingOptionSet();
                }
                return _instance;
            }
        }

        public static HandTrackingOptions CurrentOptions
        {
            get
            {
                return Instance.options;
            }
        }


        public static double Alpha
        {
            get
            {
                return Instance._alpha;
            }
            set
            {
                if (value > 1.0)
                {
                    Instance._alpha = 1.0;
                }
                else if (value < 0.0)
                {
                    Instance._alpha = 0.0;
                }
                else
                {
                    Instance._alpha = value;
                }
            }
        }

        public static int FloodFillDepth
        {
            get
            {
                return Instance._floodFillCutoffmm;
            }
            set
            {
                if (value < 1)
                {
                    Instance._floodFillCutoffmm = 1;
                }
                else
                {
                    Instance._floodFillCutoffmm = value;
                }
            }
        }

        public static bool ProjectingPoints
        {
            get
            {
                return Instance.options.HasFlag(HandTrackingOptions.PROJECT_POINTS);
            }
            set
            {
                if (value)
                {
                    Instance.options |= HandTrackingOptions.PROJECT_POINTS;
                }
                else
                {
                    Instance.options &= ~HandTrackingOptions.PROJECT_POINTS;
                }
            }
        }

        public static bool ShowUnprojectedHand
        {
            get
            {
                return Instance.options.HasFlag(HandTrackingOptions.SHOW_UNPROJECTED_HAND);
            }
            set
            {
                if (value)
                {
                    Instance.options |= HandTrackingOptions.SHOW_UNPROJECTED_HAND;
                }
                else
                {
                    Instance.options &= ~HandTrackingOptions.SHOW_UNPROJECTED_HAND;
                }
            }
        }

        public static bool AllowingTorsoMotion
        {
            get
            {
                return Instance.options.HasFlag(HandTrackingOptions.ALLOW_TORSO_MOTION);
            }
            set
            {
                if (value)
                {
                    Instance.options |= HandTrackingOptions.ALLOW_TORSO_MOTION;
                }
                else
                {
                    Instance.options &= ~HandTrackingOptions.ALLOW_TORSO_MOTION;
                }
            }
        }

        public static bool ShowTargetPoints
        {
            get
            {
                return Instance.options.HasFlag(HandTrackingOptions.SHOW_TARGET_POINTS);
            }
            set
            {
                if (value)
                {
                    Instance.options |= HandTrackingOptions.SHOW_TARGET_POINTS;
                }
                else
                {
                    Instance.options &= ~HandTrackingOptions.SHOW_TARGET_POINTS;
                }
            }
        }


            
        private HandTrackingOptionSet()
        {
            options = HandTrackingOptions.NONE;
            _alpha = 0.9;
            _floodFillCutoffmm = 50;
        }
    }
}
