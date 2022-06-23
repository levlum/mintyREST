using System;
using System.Collections.Generic;

//using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
using Com.Gamegestalt.MintyScript;

namespace Com.Gamegestalt.MintyScript.Import
{

	public class ResourceSource : DirectoryWalk
	{

		public List<string> filenames = new List<string>();
		public string extention;

		public ResourceSource(string path, string extention = ".xml")
			: base(path)
		{
			this.extention = extention;
		}

		public override void onFile(string fileName)
		{
			string ext = Path.GetExtension(fileName);
			if (ext.ToLower().Equals(this.extention))
			{
				filenames.Add(fileName);
			}
		}

		public override bool onDirectory(string Directory)
		{
			return true;
		}
	}

	public abstract class DirectoryWalk
	{

		private int HowDeepToScan;
		private string path;

		public DirectoryWalk(string path, int howDeep = -1)
		{
			this.HowDeepToScan = howDeep;
			this.path = path;
		}

		public void Walk()
		{
			ProcessDir(this.path, 0);
		}

		private void ProcessDir(string sourceDir, int recursionLvl)
		{
			if (HowDeepToScan == -1 || recursionLvl <= HowDeepToScan)
			{
				// Process the list of files found in the directory. 
				string[] fileEntries = Directory.GetFiles(sourceDir);
				foreach (string fileName in fileEntries)
				{
					onFile(fileName);
				}


				// Recurse into subdirectories of this directory.
				string[] subdirEntries = Directory.GetDirectories(sourceDir);
				foreach (string subdir in subdirEntries)
                    // Do not iterate through reparse points
                    if ((File.GetAttributes(subdir) &
					                   FileAttributes.ReparsePoint) !=
					                   FileAttributes.ReparsePoint)
					{
						if (onDirectory(subdir))
						{
							ProcessDir(subdir, recursionLvl + 1);
						}
					}
			}
		}

		public abstract void onFile(string fileName);

		public abstract bool onDirectory(string Directory);
	}

	public static class XmlUtils
	{
		public static string GetFirstElementText(XmlNode aNode, string p)
		{
			List<XmlNode> elems = GetElementsNonRecursive(aNode, p);
			if (elems == null || elems.Count <= 0)
			{
				return null;
			}
			return ((XmlElement)elems[0]).InnerText;

		}

		public static string GetAllElementText(XmlNode aNode, string p, char seperator)
		{
			List<XmlNode> elems = GetElementsNonRecursive(aNode, p);
			if (elems == null || elems.Count <= 0)
			{
				return null;
			}
			StringBuilder s = new StringBuilder();
			foreach (XmlNode n in elems)
			{
				if (s.Length > 0)
				{
					s.Append(seperator);
				}
				s.Append(((XmlElement)n).InnerText);
			}
			return s.ToString();

		}

		public static List<XmlNode> GetElementsNonRecursive(XmlNode aNode, string elemName)
		{
			List<XmlNode> result = new List<XmlNode>();
			foreach (XmlNode node in aNode.ChildNodes)
			{
				if (node.Name.Equals(elemName))
				{
					result.Add(node);
				}
			}
			return result;

		}

		public static int GetAttributeInteger(XmlNode n, string a, int defaultValue)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				return int.Parse(at.InnerText);
			}
			return defaultValue;
		}

		public static long GetAttributeLong(XmlNode n, string a, long defaultValue)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				return long.Parse(at.InnerText);
			}
			return defaultValue;
		}

		public static bool GetAttributeBool(XmlNode n, string a, bool defaultValue)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				return bool.Parse(at.InnerText);
			}
			return defaultValue;
		}

		public static float GetAttributeFloat(XmlNode n, string a, float defaultValue)
		{
			XmlAttribute at = null;
			try
			{
				at = n.Attributes[a];
			}
			catch (Exception e)
			{
				Logger.DebugL(e, "*", "XmlUtils.GetAttributeFloat( " + n + ", " + a + ", " + ", " + defaultValue + ")\n" + e.Message);
			}
			//XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				//if (at.InnerText == "50")
				//{
				//	Logger.DebugL("gax", "gax");
				//}
				try
				{
					CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
					ci.NumberFormat.NumberDecimalSeparator= ".";
					return float.Parse(at.Value, ci);
				}
				catch (Exception e)
				{
					Logger.LogWarning(n, e.Message, e);
				}
			}
			return defaultValue;
		}


		public static string GetAttribute(XmlNode n, string a)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				return at.InnerText;
			}
			return null;
		}

		public static TopicFlags GetAttributeTopicFlags(XmlNode n, string a)
		{
			TopicFlags result = new TopicFlags();

			string enumsString = GetAttribute(n, a);
			if (!string.IsNullOrEmpty(enumsString))
			{
				foreach (string oneEnum in enumsString.Split(new char[] { ',' }))
				{
					string enumString = oneEnum.Trim().ToUpper();
					try
					{
						System.Object enumObject = Enum.Parse(typeof(ComicTopic), enumString);
						result |= new TopicFlags((ComicTopic)enumObject);
					}
					catch
					{
						//throw (new Exception("Could not find Enum value: "+enumString));
					}
				}
			}

			return result;
		}

		//public static LongFlagsEnum<T> GetAttributeLongFlagsEnum<T>(XmlNode n, string a) where T : struct, IConvertible
		//{
		//	if (!typeof(T).IsEnum)
		//	{
		//		throw new ArgumentException(nameof(T));
		//	}

		//	LongFlagsEnum<T> result = new LongFlagsEnum<T>();

		//	string enumsString = GetAttribute(n, a);
		//	if (!string.IsNullOrEmpty(enumsString))
		//	{
		//		foreach (string oneEnum in enumsString.Split(new char[] { ',' }))
		//		{
		//			string enumString = oneEnum.Trim().ToUpper();
		//			try
		//			{
		//				System.Object enumObject = Enum.Parse(typeof(T), enumString);
		//				result |= new LongFlagsEnum<T>((T)enumObject);
		//			}
		//			catch
		//			{
		//				//throw (new Exception("Could not find Enum value: "+enumString));
		//			}
		//		}
		//	}

		//	return result;
		//}

		public static List<T> GetAttributeEnumList<T>(XmlNode n, string a) where T : struct, IConvertible
		{
			string enumsString = GetAttribute(n, a);
			if (enumsString != null && enumsString.Length > 0)
			{
				List<T> enumList = new List<T>();
				foreach (string oneEnum in enumsString.Split (new char[] {','}))
				{
					string enumString = oneEnum.Trim().ToUpper();
					try
					{
						System.Object enumObject = Enum.Parse(typeof(T), enumString);
						enumList.Add((T)enumObject);
					}
					catch
					{
						//throw (new Exception("Could not find Enum value: "+enumString));
					}
				}
				if (enumList.Count > 0)
					return enumList;
			}
			return null;
		}

		public static T GetAttributeEnum<T>(XmlNode n, string a) where T : struct
		{
			Object e = GetAttributeEnum(typeof(T), n, a);
			if (e == null)
				return default (T);
			return (T)e;
		}

		public static Object GetAttributeEnum(System.Type ty, XmlNode n, string a)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				string s = at.InnerText;
				s = s.Trim().ToUpper();
				return Enum.Parse(ty, s);
			}
			return null;
		}


		public static long GetAttributeTime(XmlNode n, string a)
		{
			XmlAttribute at = n.Attributes[a];
			if (at != null)
			{
				DateTime t = DateTime.ParseExact(at.InnerText, "yyyy.M.d H:m", CultureInfo.InvariantCulture);
				return t.Ticks;
			}
			return long.MinValue;


		}

		public static bool TryGetAttributeList<T>(this XmlNode node, string attributeName, char seperator, out List<T> result)
		{
			result = null;
			if (node == null
			    || node.Attributes == null
			    || string.IsNullOrEmpty(attributeName))
			{
				return false;
			}

			XmlAttribute at = node.Attributes[attributeName];
			if (at != null)
			{
				string[] stringParts = at.InnerText.Split(seperator);
				if (stringParts != null && stringParts.Length > 0)
				{
					result = new List<T>();
					foreach (string part in stringParts)
					{
						result.Add(part.Parse<T>());
					}
					return true;
				}
			}
			return false;
		}


		public static T Parse<T>(this string target)
		{
			Type type = typeof(T);

			//In case of a nullable type, use the underlaying type:
			var ReturnType = Nullable.GetUnderlyingType(type) ?? type;

			try
			{
				//in case of a nullable type and the input text is null, return the default value (null)
				if (ReturnType != type && target == null)
					return default(T);

				return (T)Convert.ChangeType(target, ReturnType);
			}
			catch
			{
				return default(T);
			}
		}
	}

}
