namespace KianCommons {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// A simple logging class.
    ///
    /// When mod activates, it creates a log file in same location as `output_log.txt`.
    /// Mac users: It will be in the Cities app contents.
    /// </summary>
    public class Log {
        /// <summary>
        /// Set to <c>true</c> to include log level in log entries.
        /// </summary>
        private static readonly bool ShowLevel = true;

        /// <summary>
        /// Set to <c>true</c> to include timestamp in log entries.
        /// </summary>
        private static readonly bool ShowTimestamp = true;

        /// <summary>
        /// File name for log file.
        /// </summary>
        private static readonly string LogFileName
            = Path.Combine(Application.dataPath, Assembly.GetExecutingAssembly().GetName().Name + ".log");

        /// <summary>
        /// Full path and file name of log file.
        /// </summary>
        private static readonly string LogFilePath = Path.Combine(Application.dataPath, LogFileName);

        /// <summary>
        /// Stopwatch used if <see cref="ShowTimestamp"/> is <c>true</c>.
        /// </summary>
        private static readonly Stopwatch Timer;

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// Resets log file on startup.
        /// </summary>
        static Log() {
            try {
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);
                }

                if (ShowTimestamp) {
                    Timer = Stopwatch.StartNew();
                }

                AssemblyName details = typeof(Log).Assembly.GetName();
                Info($"{details.Name} v{details.Version.ToString()}", true);
            }
            catch {
                // ignore
            }
        }

        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        private enum LogLevel {
            Debug,
            Info,
            Error,
        }


        public const int MAX_WAIT_ID = 1000;
        static DateTime[] times_ = new DateTime[MAX_WAIT_ID];

        [Conditional("DEBUG")]
        public static void DebugWait(string message, int id = 0, float seconds = 0.5f, bool copyToGameLog = true) {
            float diff = seconds + 1;
            if (id < 0) id = -id;
            id = System.Math.Abs(id % MAX_WAIT_ID);
            if (times_[id] != null) {
                var diff0 = DateTime.Now - times_[id];
                diff = diff0.Seconds;
            }
            if (diff >= seconds) {
                Log.Debug(message, copyToGameLog);
                times_[id] = DateTime.Now;
            }
        }

        public static void DebugWait(string message, object id, float seconds = 0.5f, bool copyToGameLog = true)
            => DebugWait(message, id.GetHashCode(), seconds, copyToGameLog);

        /// <summary>
        /// Logs debug trace, only in <c>DEBUG</c> builds.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        [Conditional("DEBUG")]
        public static void Debug(string message, bool copyToGameLog = true) {
            LogToFile(message, LogLevel.Debug);
            if (copyToGameLog) {
                UnityEngine.Debug.Log(message);
            }
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Info(string message, bool copyToGameLog = false) {
            LogToFile(message, LogLevel.Info);
            if (copyToGameLog) {
                UnityEngine.Debug.Log(typeof(Log).Assembly.GetName().Name + " : " + message);
            }
        }

        /// <summary>
        /// Logs error message and also outputs a stack trace.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Error(string message, bool copyToGameLog = true) {
            LogToFile(message, LogLevel.Error);
            if (copyToGameLog) {
                UnityEngine.Debug.LogError(message);
            }
        }

        /// <summary>
        /// Write a message to log file.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="level">Logging level. If set to <see cref="LogLevel.Error"/> a stack trace will be appended.</param>
        private static void LogToFile(string message, LogLevel level) {
            try {
                using StreamWriter w = File.AppendText(LogFilePath);
                if (ShowLevel) {
                    w.Write("{0, -8}", $"[{level.ToString()}] ");
                }

                if (ShowTimestamp) {
                    w.Write("{0, 15}", Timer.ElapsedTicks + " | ");
                }

                w.WriteLine(message);

                if (level == LogLevel.Error) {
                    w.WriteLine(new StackTrace(true).ToString());
                    w.WriteLine();
                }
            }
            catch {
                // ignore
            }
        }

        internal static void LogToFileSimple(string file, string message) {
            try {
                using StreamWriter w = File.AppendText(file);
                w.WriteLine(message);
                w.WriteLine(new StackTrace().ToString());
                w.WriteLine();
            }
            catch {
                // ignore
            }
        }

        /// <summary>
        /// useful for easily debuggin inline functions
        /// to be used like this example:
        /// TYPE inlinefunctionname(...) => expression
        /// TYPE inlinefunctionname(...) => expression.LogRet("messege");
        /// </summary>
        internal static T LogRet<T>(this T a, string m) {
            Log.Debug(m + a);
            return a;
        }
    }
}