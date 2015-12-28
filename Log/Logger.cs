// <copyright file="Logger.cs" company="Mobilityware">
//     Copyright Mobilityware, 2015
// </copyright>
namespace Mobilityware.Logging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    #region Delegates

    /// <summary>
    /// Delegate to be called when the Logging system is initialized.
    /// </summary>
    public delegate void LogConfigDelegate();

    #endregion  

    /// <summary>
    /// <para>Logger for Unity. </para>
    /// <para>This is a static class that can be called anywhere from within a Unity project.</para>
    /// <para>Provides :</para>
    /// Several different logging levels.
    /// Output in the Unity console that links to the source of the message.
    /// File and console logging.
    /// </summary>
    public static class Logger
    { 
        #region Public constants

        /// <summary>
        /// Default the library to logging all priority types
        /// </summary>
        public const string AllTypes = "ALL";

        #endregion

        #region Configurable Variables

        /// <summary>
        /// Delegate to be called upon initialization
        /// </summary>
        public static LogConfigDelegate ConfigDelegate = null;

        /// <summary>
        /// Should write to the Unity console?
        /// </summary>
        public static bool WriteToUnityConsole = true; 

        /// <summary>
        /// Should write to a file?
        /// </summary>
        public static bool WriteToFile = false;

        /// <summary>
        /// Should include a timestamp in the log?
        /// </summary>
        public static bool ShowTimeStamp = true;

        /// <summary>
        /// Should show names of log priorities?
        /// </summary>
        public static bool ShowPriorityNames = true;

        /// <summary>
        /// Should show the name of the object where the message was logged?
        /// </summary>
        public static bool ShowObjectName = true;

        /// <summary>
        /// Should show the object from which the message was logged as a string?
        /// </summary>
        public static bool ShowObjectAsString = false;

        /// <summary>
        /// What file should the logs be placed in?
        /// </summary>
        public static string LogFile = "/Logs/Log.txt"; // log file name

        /// <summary>
        /// What timestamp format to use?
        /// see http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.85).aspx for formatting options
        /// </summary>
        public static string TimeStampFormat = "{0:HH:mm:ss tt}";

        /// <summary>
        /// Is the logger initialized?
        /// </summary>
        public static bool Initialized = false;

        #endregion

        #region Static variables

        /// <summary>
        /// The priorities to log. Default the library to using all priorities
        /// </summary>
        private static int PrioritiesToLog = (int)PriorityFlags.ALL;

        /// <summary>
        /// The types to log at which levels. Default to empty.
        /// </summary>
        private static Dictionary<Type, PriorityFlags> TypesToLogMap = new Dictionary<Type, PriorityFlags>();

        /// <summary>
        /// Whether to log all types or only those configured. Default to all.
        /// </summary>
        private static bool LogAllTypes = true;

        /// <summary>
        /// Whether to suppress logging. Default to 0;
        /// </summary>
        private static int SuppressLogging = 0;

        #endregion

        #region Enum

        /// <summary>
        /// Enumeration, stored as a bit field, for logging levels.
        /// </summary>
        [Flags]
        public enum PriorityFlags : int
        {
            /// <summary>
            /// Do not log anything
            /// </summary>
            NONE = 0,

            /// <summary>
            /// Log debug or higher
            /// </summary>
            DEBUG = 1,

            /// <summary>
            /// Log info or higher
            /// </summary>
            INFO = 2,

            /// <summary>
            /// Log warning or higher
            /// </summary>
            WARNING = 4,

            /// <summary>
            /// Log error or higher
            /// </summary>
            ERROR = 8,

            /// <summary>
            /// Log everything
            /// </summary>
            ALL = ~0,
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set the minimum priority to log. The library will log all messages with this priority or higher.
        /// </summary>
        /// <param name="priorities">A string matching the priorities enum.</param>
        public static void SetPrioritiesToLog(string priorities)
        {
            SetPrioritiesToLog((PriorityFlags)Enum.Parse(typeof(PriorityFlags), priorities));
        }

        /// <summary>
        /// Set the minimum priority to log. The library will log all messages with this priority or higher.
        /// </summary>
        /// <param name="priorities">Priorities to log</param>
        public static void SetPrioritiesToLog(PriorityFlags priorities)
        {
            PrioritiesToLog = (int)priorities;
            Debug.Log("Loggin flags = " + PrioritiesToLog);
        }

        /// <summary>
        /// Delete the log file if it exists.
        /// </summary>
        public static void DestroyLogFile()
        {
            // Get rid of the old log file
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

        /// <summary>
        /// Add one to the count of messages to suppress.
        /// </summary>
        public static void PushLoggingSupression()
        {
            ++SuppressLogging;
        }

        /// <summary>
        /// Remove one from the count of messages to suppress.
        /// </summary>
        public static void PopLoggingSupression()
        {
            --SuppressLogging;
        }

        /// <summary>
        /// Log a message with DEBUG level.
        /// </summary>
        /// <param name="message">The message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            LogMessage(null, PriorityFlags.DEBUG, message);
        }

        /// <summary>
        /// Log a message with INFO level
        /// </summary>
        /// <param name="message">The message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogInfo(string message)
        {
            LogMessage(null, PriorityFlags.INFO, message);
        }
        
        /// <summary>
        /// Log a WARNING message.
        /// </summary>
        /// <param name="message">The message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarning(string message)
        {
            LogMessage(null, PriorityFlags.WARNING, message);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="message">The message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogError(string message)
        {
            LogMessage(null, PriorityFlags.ERROR, message);
        }

        /// <summary>
        /// Log a DEBUG message.
        /// </summary>
        /// <param name="message">The message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string message)
        {
            LogMessage(null, PriorityFlags.DEBUG, message);
        }

        /// <summary>
        /// Log a DEBUG message coming from an object.
        /// </summary>
        /// <param name="objectToLog">The object where the message occurs.</param>
        /// <param name="message">The message.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(object objectToLog, string message)
        {
            LogMessage(objectToLog, PriorityFlags.DEBUG, message);
        }

        /// <summary>
        /// Log a INFO message coming from an object.
        /// </summary>
        /// <param name="objectToLog">The object where the message occurs.</param>
        /// <param name="message">The message.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogInfo(object objectToLog, string message)
        {
            LogMessage(objectToLog, PriorityFlags.INFO, message);
        }

        /// <summary>
        /// Log a WARNING message coming from an object.
        /// </summary>
        /// <param name="objectToLog">The object where the message occurs.</param>
        /// <param name="message">The message.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarning(object objectToLog, string message)
        {
            LogMessage(objectToLog, PriorityFlags.WARNING, message);
        }

        /// <summary>
        /// Log an ERROR message coming from an object.
        /// </summary>
        /// <param name="objectToLog">The object where the message occurs.</param>
        /// <param name="message">The message.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogError(object objectToLog, string message)
        {
            LogMessage(objectToLog, PriorityFlags.ERROR, message);
        }

        /// <summary>
        /// Log a DEBUG message coming from an object.
        /// </summary>
        /// <param name="objectToLog">The object where the message occurs.</param>
        /// <param name="priorities">Override the configured priorities</param>
        /// <param name="message">The message.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object objectToLog, PriorityFlags priorities, string message)
        {
            LogMessage(objectToLog, priorities, message);
        }

        /// <summary>
        /// Set the types to log
        /// </summary>
        /// <param name="csvList">An array list of types that will be logged.</param>
        public static void SetTypesToLog(ArrayList csvList)
        {
            if (csvList != null)
            {
                foreach (object obj in csvList)
                {
                    SetTypesToLogSingle((string)obj);
                }
            }
            else
            {
                Debug.LogWarning("Type CVS arraylist missimng");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Create the directory to contain the logs if it doesn't exist
        /// </summary>
        /// <param name="filepath">The directory to create the log file in</param>
        private static void CreateLogFilesDirectory(string filepath)
        {
            // Take a file path like "/Logs/Log.txt" and create the "/Logs" directory
            int lastSlashPosition = filepath.LastIndexOf("/");
            if (lastSlashPosition > -1)
            {
                // Remove the file name from the file path
                string directoryPath = filepath.Substring(0, lastSlashPosition);
                
                // Create the directory
                Directory.CreateDirectory(directoryPath);
            }
            else
            {
                Debug.LogWarning("[Logging directory couldn't be created]");
            }
        }

        /// <summary>
        /// Create a timestamp according to the configured format
        /// </summary>
        /// <returns>The timestamp as a string</returns>
        private static string GenerateTimeStamp()
        {
            if (ShowTimeStamp)
            {
                StringBuilder sb = new StringBuilder("[");
                sb.AppendFormat(TimeStampFormat, DateTime.Now);
                sb.Append("]");
                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Create a string of the priorities enum
        /// </summary>
        /// <param name="priorities"></param>
        /// <returns>A parseable string of priorities</returns>
        private static string GeneratePriorities(PriorityFlags priorities)
        {
            if (ShowPriorityNames)
            {
                return "[" + priorities.ToString() + "]";
            }

            return string.Empty;
        }

        /// <summary>
        /// Generate the name of the object where the logging event occurred
        /// </summary>
        /// <param name="objectToLog">The object</param>
        /// <returns>a string for the object name</returns>
        private static string GenerateObjectName(object objectToLog)
        {
            if (objectToLog != null)
            {
                if (ShowObjectName)
                {
                    if (objectToLog is Type)
                    {
                        return "[" + objectToLog.ToString() + "]";
                    }
                    else if (objectToLog is string)
                    {
                        return "[" + (objectToLog as string) + "]";
                    }
                    else
                    {
                        return "[" + objectToLog.GetType().ToString() + "]";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Serialize the object where the log originated
        /// </summary>
        /// <param name="objectToLog">The object</param>
        /// <returns>The object serialized as a string</returns>
        private static string SerializeObject(object objectToLog)
        {
            if (objectToLog != null)
            {
                if (ShowObjectAsString)
                {
                    return objectToLog.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the type of the object where the log originated.
        /// </summary>
        /// <param name="objectToLog">The object</param>
        /// <returns>The type of the object</returns>
        private static Type GetActualType(object objectToLog)
        {
            if (objectToLog is string)
            {
                return Type.GetType(objectToLog as string);
            }
            else if (objectToLog is Type)
            {
                return objectToLog as Type;
            }
            else if (objectToLog != null)
            {
                return objectToLog.GetType();
            }

            return null;
        }

        /// <summary>
        /// Based on the priority of the message and the priority of the logging system, should the 
        /// message be logged?
        /// </summary>
        /// <param name="priorities">All priorities</param>
        /// <param name="objectToLog">The object to log</param>
        /// <returns>true if the message should be logged</returns>
        private static bool ShouldLogMessage(PriorityFlags priorities, object objectToLog)
        {
            // Nothing is enabled for logging.
            if (!WriteToFile && !WriteToUnityConsole)
            {
                return false;
            }

            if (LogAllTypes)
            {
                return (int)priorities >= PrioritiesToLog;
            }

            Type actualType = GetActualType(objectToLog);

            if (!ReferenceEquals(actualType, null))
            {
                PriorityFlags logThisPriority;
                if (TypesToLogMap.TryGetValue(actualType, out logThisPriority))
                {
                    if (logThisPriority == PriorityFlags.NONE)
                    {
                        return false;
                    }

                    if (logThisPriority == PriorityFlags.ALL)
                    {
                        return true;
                    }

                    // if config is set to debug for this type, and this is a warning message, then print it
                    return priorities >= logThisPriority;
                }
            }

            return false;
        }

        /// <summary>
        /// Format the logging message
        /// </summary>
        /// <param name="objectToLog">The object to log</param>
        /// <param name="priorities">All priorities</param>
        /// <param name="message">The message</param>
        /// <returns>The formatted message</returns>
        private static string FormatMessage(object objectToLog, PriorityFlags priorities, string message)
        {
            string timeStamp = GenerateTimeStamp();
            string prioritiesAsString = GeneratePriorities(priorities);

            // Build the object string and serialize the object if configured to do so
            string objectName = GenerateObjectName(objectToLog);
            string objectAsString = SerializeObject(objectToLog);

            // Using StringBuilder for better performance
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}{2} {3} {4}", timeStamp, prioritiesAsString, objectName, message, objectAsString);
            return sb.ToString();
        }

        /// <summary>
        /// Default catch all logger
        /// </summary>
        /// <param name="objectToLog">The object to log</param>
        /// <param name="priorities">All priorities</param>
        /// <param name="message">The message</param>
        private static void LogMessage(object objectToLog, PriorityFlags priorities, string message)
        {
            Init();

            if (SuppressLogging == 0 && ShouldLogMessage(priorities, objectToLog))
            {
                string logMessage = FormatMessage(objectToLog, priorities, message);
                if (WriteToFile)
                {
                    using (StreamWriter writer = new StreamWriter(LogFile, true))
                    {
                        writer.Write(message + Environment.NewLine);
                    }
                }

                UnityEngine.Object context = objectToLog as UnityEngine.Object;

                if (WriteToUnityConsole)
                {
                    if ((priorities & PriorityFlags.ERROR) == PriorityFlags.ERROR)
                    {
                        UnityEngine.Debug.LogError(logMessage, context);
                    }
                    else if ((priorities & PriorityFlags.WARNING) == PriorityFlags.WARNING)
                    {
                        UnityEngine.Debug.LogWarning(logMessage, context);
                    }
                    else
                    {
                        UnityEngine.Debug.Log(logMessage, context);
                    }
                }
            }
        }

        /// <summary>
        /// Set the types to log
        /// </summary>
        /// <param name="typeName">A colon separated list of types</param>
        private static void SetTypesToLogSingle(string typeName)
        {
            string[] colonSplits = typeName.Split(':');
            string trimmedTypeName = colonSplits[0].Trim();
            Type type = Type.GetType(trimmedTypeName);

            if (string.Equals(trimmedTypeName, AllTypes, StringComparison.OrdinalIgnoreCase))
            {
                LogAllTypes = true;
            }
            else if (ReferenceEquals(type, null))
            {
                Debug.LogError("Attempting to do per type logging on a type that does not exist. Verify the namespace and type name!");
                throw new InvalidCastException();
            }
            else
            {
                LogAllTypes = false;
                if (colonSplits.Length > 1)
                {
                    try
                    {
                        PriorityFlags onePriorityFlag = (PriorityFlags)Enum.Parse(typeof(PriorityFlags), colonSplits[1]);
                        TypesToLogMap[type] = onePriorityFlag;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e + " parsing " + colonSplits[1] + " for type " + trimmedTypeName + " defaulting to ALL");
                        TypesToLogMap[type] = PriorityFlags.ALL;
                    }
                }
                else
                {
                    TypesToLogMap[type] = PriorityFlags.ALL;
                }
            }
        }

        /// <summary>
        /// Initialize the logging library if it isn't already.
        /// </summary>
        private static void Init()
        {
            if (Initialized == false)
            {
                if (ConfigDelegate != null)
                {
                    ConfigDelegate();
                }

                Initialized = true;
            }
        }

        #endregion
    }
}
