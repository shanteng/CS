using System;
using System.Collections;
using System.Diagnostics;
using UnityEditor;

namespace UnityEngine.PlayerIdentity.Utils
{
    /// <summary>
    /// A helper class for logging debug messages.
    /// It is turned off by default, and can be turned on by setting the log levels.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// traceLevel is the trace level to be enabled. Defaults to Off.
        /// </summary>
        public static TraceLevel traceLevel { get; set; } = TraceLevel.Warning;

        /// <summary>
        /// Logs an info message to the Unity Console.
        /// </summary>
        public static void Info(object message)
        {
            if (traceLevel < TraceLevel.Info)
            {
              return;
            }
            Debug.Log(message);
        }

        /// <summary>
        /// Logs an info message to the Unity Console.
        /// </summary>
        public static void Info(object message, Object context)
        {
            if (traceLevel < TraceLevel.Info)
            {
              return;
            }
            Debug.Log(message, context);
        }

        /// <summary>
        /// Logs an info message to the Unity Console.
        /// </summary>
        public static void InfoFormat(string format, params object[] args)
        {
            if (traceLevel < TraceLevel.Info)
            {
                return;
            }
            Debug.LogFormat(format, args);
        }


        /// <summary>
        /// Logs a warning message to the Unity Console.
        /// </summary>
        public static void Warning(object message)
        {
            if (traceLevel < TraceLevel.Warning)
            {
                return;
            }
            Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs a warning message to the Unity Console.
        /// </summary>
        public static void Warning(object message, Object context)
        {
            if (traceLevel < TraceLevel.Warning)
            {
                return;
            }
            Debug.LogWarning(message, context);
        }


        /// <summary>
        /// Logs an error message to the Unity Console.
        /// </summary>
        public static void Error(object message)
        {
            if (traceLevel < TraceLevel.Error)
            {
                return;
            }
            Debug.LogError(message);
        }

        /// <summary>
        /// Logs an error message to the Unity Console.
        /// </summary>
        public static void Error(object message, Object context)
        {
            if (traceLevel < TraceLevel.Error)
            {
                return;
            }
            Debug.LogError(message, context);
        }

        /// <summary>
        /// Logs an exception to the Unity Console.
        /// </summary>
        public static void Exception(Exception ex)
        {
            if (traceLevel < TraceLevel.Error)
            {
                return;
            }
            Debug.LogException(ex);
        }
    }
}