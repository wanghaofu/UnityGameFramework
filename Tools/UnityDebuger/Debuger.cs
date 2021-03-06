﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnityDebuger
{
    public interface ILogTag
    {
        string LOG_TAG { get; }
    }

    public static class Debuger
    {
        public static bool EnableLog = true;
        public static bool EnableTime = true;
        public static bool EnableSave = true;
        public static bool EnableStack = false;
        public static string LogFileDir = "";
        public static string LogFileName = "";
        public static string Prefix = "> ";
        public static StreamWriter LogFileWriter = null;

        public static void Init(string logFileDir = null)
        {
            LogFileDir = logFileDir;
            if (string.IsNullOrEmpty(LogFileDir))
            {
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                LogFileDir = path + "/DebugerLog/";
            }

            LogLogHead();
        }

        private static void LogLogHead()
        {
            DateTime now = DateTime.Now;
            string timeStr = now.ToString("HH:mm:ss.fff") + " ";

            Internal_Log("================================================================================");
            Internal_Log("                                   SGFDebuger                                   ");
            Internal_Log("--------------------------------------------------------------------------------");
            Internal_Log("Time:\t" + timeStr);
            Internal_Log("Path:\t" + LogFileDir);
            Internal_Log("================================================================================");
        }


        public static void Internal_Log(string msg, object context = null)
        {
            if (Debuger.EnableTime)
            {
                DateTime now = DateTime.Now;
                msg = now.ToString("HH:mm:ss.fff") + " " + msg;
            }

            UnityEngine.Debug.Log(msg);
            LogToFile("[I]" + msg);
        }

        public static void Internal_LogWarning(string msg, object context = null)
        {
            if (Debuger.EnableTime)
            {
                DateTime now = DateTime.Now;
                msg = now.ToString("HH:mm:ss.fff") + " " + msg;
            }

            UnityEngine.Debug.LogWarning(msg);

            LogToFile("[W]" + msg);
        }

        public static void Internal_LogError(string msg, object context = null)
        {
            if (Debuger.EnableTime)
            {
                DateTime now = DateTime.Now;
                msg = now.ToString("HH:mm:ss.fff") + " " + msg;
            }

            UnityEngine.Debug.LogError(msg);
            LogToFile("[E]" + msg, true);
        }

        //----------------------------------------------------------------------
        //Log
        //----------------------------------------------------------------------
        //[Conditional("ENABLE_LOG")]
        public static void Log(object obj)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(GetLogCaller(true), obj);
            Internal_Log(Prefix + message);
        }

        //[Conditional("ENABLE_LOG")]
        public static void Log(string message = "")
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(GetLogCaller(true), message);
            Internal_Log(Prefix + message);
        }

        //[Conditional("ENABLE_LOG")]
        public static void Log(string format, params object[] args)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_Log(Prefix + message);
        }

        //[Conditional("ENABLE_LOG")]
        public static void Log(this ILogTag obj, string message = "")
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_Log(Prefix + message);
            
        }

        //[Conditional("ENABLE_LOG")]
        public static void Log(this ILogTag obj, string format, params object[] args)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_Log(Prefix + message);     
        }

        //----------------------------------------------------------------------
        //LogWarning
        //----------------------------------------------------------------------
        public static void LogWarning(object obj)
        {
            string message = GetLogText(GetLogCaller(true), obj);
            Internal_LogWarning(Prefix + message);
        }

        public static void LogWarning(string message)
        {
            message = GetLogText(GetLogCaller(true), message);
            Internal_LogWarning(Prefix + message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_LogWarning(Prefix + message);          
        }

        public static void LogWarning(this ILogTag obj, string message)
        {
            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_LogWarning(Prefix + message);        
        }

        public static void LogWarning(this ILogTag obj, string format, params object[] args)
        {
            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_LogWarning(Prefix + message);           
        }

        //----------------------------------------------------------------------
        //LogWarning
        //----------------------------------------------------------------------
        public static void LogError(object obj)
        {
            string message = GetLogText(GetLogCaller(true), obj);
            Internal_LogError(Prefix + message);
        }

        public static void LogError(string message)
        {
            message = GetLogText(GetLogCaller(true), message);
            Internal_LogError(Prefix + message);           
        }

        public static void LogError(string format, params object[] args)
        {
            string message = GetLogText(GetLogCaller(true), string.Format(format, args));
            Internal_LogError(Prefix + message);          
        }

        public static void LogError(this ILogTag obj, string message)
        {
            message = GetLogText(GetLogTag(obj), GetLogCaller(), message);
            Internal_LogError(Prefix + message);      
        }

        public static void LogError(this ILogTag obj, string format, params object[] args)
        {
            string message = GetLogText(GetLogTag(obj), GetLogCaller(), string.Format(format, args));
            Internal_LogError(Prefix + message);         
        }

        //----------------------------------------------------------------------
        //工具函数
        //----------------------------------------------------------------------

        private static string GetLogText(string tag, string methodName, string message)
        {
            return tag + "::" + methodName + "() " + message;
        }

        private static string GetLogText(string caller, string message)
        {
            return caller + "() " + message;
        }

        private static string GetLogText(string caller, object message)
        {
            return caller + "() " + (message != null? message.ToListString() :"null");
        }

        #region Object 2 ListString 
        /// <summary>
        /// 将容器序列化成字符串
        /// 格式：{a, b, c}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string ListToString<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return "null";
            }

            if (source.Count() == 0)
            {
                return "[]";
            }

            if (source.Count() == 1)
            {
                return "[" + source.First() + "]";
            }

            var s = "";

            s += source.ButFirst().Aggregate(s, (res, x) => res + ", " + x.ToListString());
            s = "[" + source.First().ToListString() + s + "]";

            return s;
        }

        /// <summary>
        /// 将容器序列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string ToListString(this object obj)
        {
            if (obj is string)
            {
                return obj.ToString();
            }
            else
            {
                var objAsList = obj as IEnumerable;
                return objAsList == null ? obj.ToString() : objAsList.Cast<object>().ListToString();
            }
        }

        private static IEnumerable<T> ButFirst<T>(this IEnumerable<T> source)
        {
            return source.Skip(1);
        }

        #endregion 

        private static string GetLogTag(ILogTag obj)
        {
            return obj.LOG_TAG;
            /*
            ILogTag tag = obj as ILogTag;
            if (tag != null)
            {
                return tag.LOG_TAG;
            }
            return obj.GetType().Name;
            */
        }

        private static Assembly ms_Assembly;
        private static string GetLogCaller(bool bIncludeClassName = false)
        {
            StackTrace st = new StackTrace(2, false);
            if (st != null)
            {
                if (null == ms_Assembly)
                {
                    ms_Assembly = typeof(Debuger).Assembly;
                }

                int currStackFrameIndex = 0;
                while (currStackFrameIndex < st.FrameCount)
                {
                    StackFrame oneSf = st.GetFrame(currStackFrameIndex);
                    MethodBase oneMethod = oneSf.GetMethod();
                    

                    if (oneMethod.Module.Assembly != ms_Assembly)
                    {
                        if (bIncludeClassName)
                        {
                            //oneMethod.ReflectedType
                            return oneMethod.DeclaringType.Name + "::" + oneMethod.Name;
                        }
                        else
                        {
                            return oneMethod.Name;
                        }
                    }

                    currStackFrameIndex++;
                }

            }

            return "";
        }

        //----------------------------------------------------------------------
        internal static string CheckLogFileDir()
        {
            if (string.IsNullOrEmpty(LogFileDir))
            {
                Internal_LogError("Debuger::CheckLogFileDir() LogFileDir is NULL!");
                return "";
            }

            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }
            }
            catch (Exception e)
            {
                Internal_LogError("Debuger::CheckLogFileDir() " + e.Message + e.StackTrace);
                return "";
            }

            return LogFileDir;
        }

        internal static string GenLogFileName()
        {
            DateTime now = DateTime.Now;
            string filename = now.GetDateTimeFormats('s')[0].ToString();//2005-11-05T14:06:25
            filename = filename.Replace("-", "_");
            filename = filename.Replace(":", "_");
            filename = filename.Replace(" ", "");
            filename += ".log";

            return filename;
        }



        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
            {
                return;
            }

            if (LogFileWriter == null)
            {
                LogFileName = GenLogFileName();
                LogFileDir = CheckLogFileDir();
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    return;
                }

                string fullpath = LogFileDir + LogFileName;
                try
                {
                    LogFileWriter = File.AppendText(fullpath);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                    Internal_LogError("Debuger::LogToFile() " + e.Message + e.StackTrace);
                    return;
                }
            }

            if (LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.WriteLine(message);
                    if ((EnableStack || Debuger.EnableStack))
                    {
                        StackTrace st = new StackTrace(2, false);
                        LogFileWriter.WriteLine(st.ToString());
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
