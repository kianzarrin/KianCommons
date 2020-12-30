namespace KianCommons {
    using ColossalFramework.UI;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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

        private static string assemblyName_ = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// File name for log file.
        /// </summary>
        private static readonly string LogFileName = assemblyName_ + ".log";

        /// <summary>
        /// Full path and file name of log file.
        /// </summary>
        private static readonly string LogFilePath;

        /// <summary>
        /// Stopwatch used if <see cref="ShowTimestamp"/> is <c>true</c>.
        /// </summary>
        private static readonly Stopwatch Timer;

        private static StreamWriter filerWrier_;

        private static object LogLock = new object();

        internal static bool ShowGap = false;

        private static long prev_ms_;

        /// <summary>
        /// buffered logging is much faster but requires extra care for hot-reload/external modifications.
        /// to use Buffered mode with hot-reload: set when mod is enabled and unset when mod is disabled.
        /// Note: buffered mode is 20 times faster but only if you do not copy to game log.
        /// </summary>
        internal static bool Buffered {
            get => filerWrier_ != null;
            set {
                if (value == Buffered) return;
                if (value) {
                    filerWrier_ = new StreamWriter(LogFilePath, true);
                } else {
                    filerWrier_.Flush();
                    filerWrier_.Dispose();
                    filerWrier_ = null;
                }
            }
        }

        /// <summary>
        /// if buffered then lock the file and flush.
        /// otherwise return silently.
        /// </summary>
        internal static void Flush() {
            if (filerWrier_ != null) {
                lock (LogLock)
                    filerWrier_.Flush();
            }
        }

        public static Stopwatch GetSharedTimer() {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(_asm => _asm.GetName().Name == "LoadOrderIPatch");
            var t = asm?.GetType("LoadOrderIPatch.Patches.LoggerPatch", throwOnError: false);
            return t?.GetField("m_Timer")?.GetValue(null) as Stopwatch;
        }

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// Resets log file on startup.
        /// </summary>
        static Log() {
            try {
                var dir = Path.Combine(Application.dataPath, "Logs");
                LogFilePath = Path.Combine(dir, LogFileName);
                var oldFilePath = Path.Combine(Application.dataPath, LogFileName);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (File.Exists(LogFilePath))
                    File.Delete(LogFilePath);
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);


                if (ShowTimestamp) {
                    Timer = GetSharedTimer() ?? Stopwatch.StartNew();
                }

                AssemblyName details = typeof(Log).Assembly.GetName();
                Info($"Log file at " + LogFilePath, true);
                Info($"{details.Name} v{details.Version}", true);
            } catch (Exception ex) {
                Log.LogUnityException(ex);
            }
        }

        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        private enum LogLevel {
            Debug,
            Info,
            Error,
            Exception,
        }


        public const int MAX_WAIT_ID = 1000;
        static DateTime[] times_ = new DateTime[MAX_WAIT_ID];

        [Conditional("DEBUG")]
        public static void DebugWait(string message, int id, float seconds = 0.5f, bool copyToGameLog = true) {
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

        [Conditional("DEBUG")]
        public static void DebugWait(string message, object id = null, float seconds = 0.5f, bool copyToGameLog = true) {
            if (id == null)
                id = Environment.StackTrace + message;
            DebugWait(message, id.GetHashCode(), seconds, copyToGameLog);

        }

        /// <summary>
        /// Logs debug trace, only in <c>DEBUG</c> builds.
        /// </summary>
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        [Conditional("DEBUG")]
        public static void Debug(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Debug, copyToGameLog);
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Info(string message, bool copyToGameLog = false) {
            LogImpl(message, LogLevel.Info, copyToGameLog);
        }

        /// <summary>
        /// Logs error message and also outputs a stack trace.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Error(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Error, copyToGameLog);

        }

        internal static void Exception(Exception ex, string m = "", bool showInPanel = true) {
            if (ex == null)
                Log.Error("null argument e was passed to Log.Exception()");
            try {
                string message = ex.ToString() + $"\n\t-- {assemblyName_}:end of inner stack trace --";
                if (!m.IsNullorEmpty())
                    message = m + " -> \n" + message;
                LogImpl(message, LogLevel.Exception, true);
                if (showInPanel)
                    UIView.ForwardException(ex);
            } catch (Exception ex2) {
                Log.LogUnityException(ex2);
            }
        }

        internal static void LogUnityException(Exception ex, bool showInPanel = true) {
            UnityEngine.Debug.LogException(ex);
            if (showInPanel)
                UIView.ForwardException(ex);
        }

        static string nl = Environment.NewLine;

        /// <summary>
        /// Write a message to log file.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="level">Logging level. If set to <see cref="LogLevel.Error"/> a stack trace will be appended.</param>
        private static void LogImpl(string message, LogLevel level, bool copyToGameLog) {
            try {
                var ticks = Timer.ElapsedTicks;
                string m = "";
                if (ShowLevel) {
                    int maxLen = Enum.GetNames(typeof(LogLevel)).Select(str => str.Length).Max();
                    m += string.Format($"{{0, -{maxLen}}}", $"[{level}] ");
                }

                if (ShowTimestamp) {
                    long ms = Timer.ElapsedMilliseconds;
                    m += $"{ms:#,0}ms | ";
                    if (ShowGap) {
                        long gapms = ms - prev_ms_;
                        prev_ms_ = ms;
                        m += $"gap={gapms:#,0}ms | ";
                    }
                }

                m += message;
                if (level == LogLevel.Error || level == LogLevel.Exception) {
                    m += nl + Environment.StackTrace;
                    m = nl + m + nl; // create line space to draw attention.
                }

                lock (LogLock) {
                    if (filerWrier_ != null) {
                        filerWrier_.WriteLine(m);
                    } else {
                        using (StreamWriter w = File.AppendText(LogFilePath))
                            w.WriteLine(m);
                    }
                }

                if (copyToGameLog) {
                    // copying to game log is slow anyways so
                    // this is a good time to flush if neccessary.
                    Flush();
                    m = assemblyName_ + " | " + m;
                    switch (level) {
                        case LogLevel.Error:
                        case LogLevel.Exception:
                            UnityEngine.Debug.LogError(m);
                            break;
                        default:
                            UnityEngine.Debug.Log(m);
                            break;
                    }
                }
            } catch (Exception ex) {
                Log.LogUnityException(ex);
            }
        }

        internal static void LogToFileSimple(string file, string message) {
            using (StreamWriter w = File.AppendText(file)) {
                w.WriteLine(message);
                w.WriteLine(new StackTrace().ToString());
                w.WriteLine();
            }
        }
    }

    internal static class LogExtensions {
        /// <summary>
        /// useful for easily debuggin inline functions
        /// to be used like this example:
        /// TYPE inlinefunctionname(...) => expression
        /// TYPE inlinefunctionname(...) => expression.LogRet("messege");
        /// </summary>
        internal static T LogRet<T>(this T a, string m) {
            Log.Debug(m + " -> " + a);
            return a;
        }
    }
}