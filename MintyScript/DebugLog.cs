using System;
using System.Collections.Generic;
using System.Text;

#if UNITY_5_3_OR_NEWER
//using System.Linq;
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{

	public enum LoggerLevel
	{
		ERROR,
		WARNING,
		INFO,
		DEBUGM,
		DEBUGO,
		DEBUGP,
		DEBUGL,
	}

	public enum LogEnvironment
	{
		UNITY_FLASH,
		UNITY_EDITOR,
		EXTERNAL,
		UNITY_FLASH_DEBUG,
	}

	public interface LogAppender
	{

		void Log(LoggerLevel level, string s);

		string GetLoggBuffer();
	}

	public interface LogFormat
	{

		string Log(string module, LoggerLevel level, string msg, params object[] plist);

	}


	public static class  Logger
	{
		#if UNITY_5_3_OR_NEWER
		public static LogAppender LogAppender = new Unity3DLog();


		#else
		public static LogAppender LogAppender = new StringLogger(16000);
		#endif
		public static LogFormat LogFormater = new Unity3DFormat();
		private static LogEnvironment environment = LogEnvironment.UNITY_EDITOR;

		public static LogEnvironment ENVIRONMENT
		{
			
			get
			{
				return environment;
			}
			set
			{
				environment = value;
				switch (environment)
				{
#if UNITY_5_3_OR_NEWER
					case LogEnvironment.UNITY_EDITOR:
						LogAppender = new MultiLogger(
							new LogAppender[]
							{
								new Unity3DLog(),
								new StringLogger(16000)
							});
						break;
					case LogEnvironment.UNITY_FLASH_DEBUG:
						LogAppender = new MultiLogger(
							new LogAppender[]
							{
								new Unity3DLog(),
								new StringLogger(16000)
							});
						break;
					case LogEnvironment.UNITY_FLASH:
						LogAppender = new StringLogger(16000);
						break;
#endif
					case LogEnvironment.EXTERNAL:
						LogAppender = null;
						break;

				}

			}
			
		}

		public const bool DISABLE_ALL = false;

		[Obsolete("Use Log(module, exception)")]
		public static void Log(Exception e)
		{
			Log("*", LoggerLevel.ERROR, e.StackTrace);
		}

		[Obsolete("Use Log(module, msg)")]
		public static void Log(string msg)
		{
			Log("*", LoggerLevel.DEBUGO, msg);
		}

		public static void Log(System.Object o, string msg, params object[] plist)
		{
			Log("*", LoggerLevel.DEBUGO, msg, plist);
		}


		//public static void Log(System.Object o, string msg) {
		//    Log(o.GetType().Name, LoggerLevel.DEBUGO, msg);
		//}
		public static void LogError(System.Object o, string msg, params object[] plist)
		{
			if (o == null)
			{
				o = msg;
			}
			Log(o.GetType().Name, LoggerLevel.ERROR, msg, plist);
		}

		public static void LogError(System.Object o, string msg, Exception e)
		{
			if (o == null)
			{
				o = msg;
			}
			string es = e.ToString();
			if (e != null)
			{
				es += "\n  " + e.ToString();
			}
			Log(o.GetType().Name, LoggerLevel.ERROR, msg, es);
		}

		public static void LogWarning(System.Object o, string msg, params object[] plist)
		{
			if (o == null)
			{
				o = msg;
			}
			Log(o.GetType().Name, LoggerLevel.WARNING, msg, plist);
		}

		public static void Info(System.Object o, string msg, params object[] plist)
		{
			Log(o.GetType().Name, LoggerLevel.INFO, msg, plist);
		}

		public static void DebugM(System.Object o, string msg, params object[] plist)
		{
			if (o == null)
			{
				o = msg;
			}
			Log(o.GetType().Name, LoggerLevel.DEBUGM, msg, plist);
		}

		public static void DebugL(System.Object o, string msg, params object[] plist)
		{
			if (o == null)
			{
				o = msg;
			}
			Log(o.GetType().Name, LoggerLevel.DEBUGL, msg, plist);
		}

		public static void DebugP(System.Object o, string msg, params object[] plist)
		{
			Log(o.GetType().Name, LoggerLevel.DEBUGP, msg, plist);
		}

		public static void DebugO(System.Object o, string msg, params object[] plist)
		{
			Log(o.GetType().Name, LoggerLevel.DEBUGO, msg, plist);
		}


		public static void Log(string module, LoggerLevel level, string msg, params object[] plist)
		{
			if (DISABLE_ALL)
			{
				return;
			}

			if (LogAppender != null && LogFormater != null)
			{
				string fmtStr = LogFormater.Log(module, level, msg, plist);
				LogAppender.Log(level, fmtStr);
			} 
		}


		internal static string GetLogBuffer()
		{
			if (LogAppender != null)
			{
				return LogAppender.GetLoggBuffer();
			}
			return null;
		}
	}

	#if UNITY_5_3_OR_NEWER
	public class Unity3DLog : LogAppender
	{

		public virtual void Log(LoggerLevel level, string s)
		{
			switch (level)
			{
				case LoggerLevel.ERROR:
					Debug.LogError(s);
					break;
				case LoggerLevel.WARNING:
					Debug.LogWarning(s);
					break;
				default:
					Debug.Log(s);
					break;
			}
		}


		public string GetLoggBuffer()
		{
			return null;
		}
	}
	#endif


	public class MultiLogger : LogAppender
	{
		LogAppender[] loggers;

		public MultiLogger(LogAppender[] loggers)
		{
			this.loggers = loggers;
		}

		public virtual void Log(LoggerLevel level, string s)
		{
			for (int i = 0; i < loggers.Length; i++)
			{
				loggers[i].Log(level, s);
			}
		}


		public string GetLoggBuffer()
		{
			for (int i = 0; i < loggers.Length; i++)
			{
				string s = loggers[i].GetLoggBuffer();
				if (s != null)
				{
					return s;
				}
			}
			return null;
		}
	}

	public class StringLogger : LogAppender
	{

		private List<string> logBuffer = new List<string>();
		private int bufferSize = 0;
		private int maxSize;

		public StringLogger(int maxSize)
		{
			this.maxSize = maxSize;
		}

		public virtual void Log(LoggerLevel level, string s)
		{
			if (s == null)
			{
				return;
			}
			logBuffer.Add(s);
			bufferSize += s.Length;
			while (bufferSize > maxSize && logBuffer.Count > 1)
			{
				string remove = logBuffer[0];
				bufferSize -= remove.Length;
				logBuffer.RemoveAt(0);
			}
		}

		public string GetLoggBuffer()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < logBuffer.Count; i++)
			{
				sb.Append(logBuffer[i]);
				sb.Append("\n");
			}
			return sb.ToString();
		}


	}

	public class LoggerStringFormat : LogFormat
	{

		public virtual string  Log(string module, LoggerLevel level, string msg, params object[] plist)
		{
			if (plist != null && plist.Length >= 1)
			{
				StringBuilder sb = new StringBuilder(msg);
				foreach (object o in plist)
				{
					if (o != null)
					{
						sb.Append(o.ToString());
					}
				}
				msg = sb.ToString();
			} 
			return msg;
		}
	}


	public class Unity3DFormat : LoggerStringFormat
	{

		public const string UNFLASH_STRING = "{0}:{1}:{2} {3} {4} {5}";
		public const string FORMAT_STRING = "{0,2}:{1,2}:{2,2} {3,-15}  {4,-7} {5}";

		public override string  Log(string module, LoggerLevel level, string msg, params object[] plist)
		{
			msg = base.Log(module, level, msg, plist);
			DateTime t = new DateTime();
			#if UNITY_EDITOR
			string fmt = FORMAT_STRING;
			#elif UNITY_FLASH
				string fmt = UNFLASH_STRING;
			#else
            string fmt = FORMAT_STRING;
			#endif		
			string s = string.Format(fmt, t.Hour, t.Minute, t.Second, module, level.ToString(), msg);
			return s;
		}
	}

}
