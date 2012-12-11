using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDAuth
{

    enum State
    {
        MAIN_MENU   = 0x01, // 00000001
        CREATE      = 0x02, // 00000010
        LOG_IN      = 0x04, // 00000100
        NAME        = 0x08, // 00001000
        FACE        = 0x10, // 00010000
        DRAW        = 0x20, // 00100000
        LOGGED_IN   = 0x40,  // 01000000

        CREATE_USER_NAME    = CREATE | NAME,
        CREATE_USER_FACE    = CREATE | FACE,
        CREATE_USER_DRAW    = CREATE | DRAW,
        LOG_IN_NAME         = LOG_IN | NAME,
        LOG_IN_FACE         = LOG_IN | FACE,
        LOG_IN_DRAW         = LOG_IN | DRAW,
    }

    // Singleton
    class StateMachine
    {
        private static StateMachine _instance;
        private static StateMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StateMachine();
                }
                return _instance;
            }
        }


        private State _currentState;

        public static State CurrentState
        {
            get
            {
                return Instance._currentState;
            }
        }

        private StateMachine()
        {
            _currentState = State.MAIN_MENU;
        }

        public static void AdvanceState()
        {
            switch (CurrentState)
            {
                case State.MAIN_MENU:
                    return;
                case State.LOGGED_IN:
                    return;

                case State.CREATE_USER_NAME:
                    Instance._currentState = State.CREATE_USER_FACE;
                    return;
                case State.CREATE_USER_FACE:
                    Instance._currentState = State.CREATE_USER_DRAW;
                    return;
                case State.CREATE_USER_DRAW:
                    Instance._currentState = State.MAIN_MENU;
                    return;

                case State.LOG_IN_NAME:
                    Instance._currentState = State.LOG_IN_FACE;
                    return;
                case State.LOG_IN_FACE:
                    Instance._currentState = State.LOG_IN_DRAW;
                    return;
                case State.LOG_IN_DRAW:
                    Instance._currentState = State.LOGGED_IN;
                    return;
            }
        }

        public static void CreateUserState()
        {
            Instance._currentState = State.CREATE_USER_NAME;
        }

        public static void LogInUserState()
        {
            Instance._currentState = State.LOG_IN_NAME;
        }

        public static void MainMenuState()
        {
            Instance._currentState = State.MAIN_MENU;
        }

        public static bool IsHandTracking()
        {
            return Instance._currentState.HasFlag(State.DRAW);
        }

        public static bool IsFaceScanning()
        {
            return Instance._currentState.HasFlag(State.FACE);
        }

        public static bool IsNameEntering()
        {
            return Instance._currentState.HasFlag(State.NAME);
        }
    }
}
