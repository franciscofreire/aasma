using System;
using UnityEngine;


public class Logger {
    public enum VERBOSITY {DEBUG, AGENTS, LAYERS};

    private static readonly bool DEBUG  = false;
    private static readonly bool AGENTS = false;
    private static readonly bool LAYERS = true;

    public Logger() {
    }

    public static void Log(string s, VERBOSITY verbosity) {
        switch(verbosity) {
            case VERBOSITY.DEBUG:
                if (DEBUG)
                    Debug.Log(s);
                break;
            case VERBOSITY.AGENTS:
                if (AGENTS)
                    Debug.Log(s);
                break;
            case VERBOSITY.LAYERS:
                if (LAYERS)
                    Debug.Log(s);
                break;
        }
    }
}