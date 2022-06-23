using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
//	[Serializable]
//#if UNITY_5_3_OR_NEWER 
//	public struct LongFlagsEnum<T> where T : struct, IConvertible
//#else
//	public struct LongFlagsEnum<T> where T : struct, IConvertible
//#endif
//	{

//		public static readonly LongFlagsEnum<T> UNDEFINED = new LongFlagsEnum<T>(0);


//		[NonSerialized]
//		public BigInteger flagsInt;


//		//public LongFlagsEnum()
//		//{
//		//	flagsInt = 0;
//		//}

//		public LongFlagsEnum(BigInteger flagsInt)
//		{
//			this.flagsInt = flagsInt;
//		}

//		public LongFlagsEnum(params T[] enumValues)
//		{
//			if (!typeof(T).IsEnum)
//			{
//				throw new ArgumentException(nameof(T));
//			}

//			foreach (var enumV in enumValues)
//			{
//				int bitNum = (int)(object)enumV;
//				flagsInt = flagsInt | (BigInteger.One << bitNum);
//			}
//		}

//		public bool Contains(T enumV)
//		{
//			return (flagsInt & (BigInteger.One << (int)(object)enumV)) != 0;
//		}

//		public bool ContainsAll(LongFlagsEnum<T> otherFlags)
//		{
//			return (flagsInt & otherFlags.flagsInt) == otherFlags.flagsInt;
//		}

//		public bool ContainsAny(LongFlagsEnum<T> otherFlags)
//		{
//			return (flagsInt & otherFlags.flagsInt) != 0;
//		}

//		public bool IsEmpty()
//		{
//			return flagsInt == 0;
//		}

//		public HashSet<T> GetHashSet()
//		{
//			return flagsInt.ToHashSet<T>();
//			//HashSet<T> result = new HashSet<T>();

//			//int byteNum = 0;
//			//foreach (byte b in flagsInt.ToByteArray())
//			//{
//			//	for (int bitInByteIndex = 0; bitInByteIndex<8; bitInByteIndex++)
//			//	{
//			//		byte mask = (byte)(1 << bitInByteIndex);
//			//		if ((b & mask) != 0)
//			//		{
//			//			int enumInt = (byteNum * 8) + bitInByteIndex;
//			//			result.Add((T)Enum.ToObject(typeof(T), enumInt)); 
//			//		}
//			//	}

//			//	byteNum++;
//			//}

//			//return result;
//		}

//		public T[] ToArray()
//		{
//			var hashset = GetHashSet();
//			var result = new T[hashset.Count];
//			int i = 0;
//			foreach (var enumValue in hashset)
//			{
//				result[i++] = enumValue;
//			}
//			return result;
//		}

//		public override int GetHashCode()
//		{
//			return flagsInt.GetHashCode();
//		}

//		public override bool Equals(object obj)
//		{
//			if (obj is LongFlagsEnum<T>)
//			{
//				return flagsInt == ((LongFlagsEnum<T>)obj).flagsInt;
//			}
//			return false;
//		}

//		override public string ToString()
//		{
//			//return flagsInt.ToBinary();

//			if (flagsInt == 0) return default(T).ToString();
//			StringBuilder result = new StringBuilder();
//			var enumerable = GetHashSet();
//			int i = 0;
//			foreach (var enumValue in enumerable)
//			{
//				result.Append(enumValue);
//				if (i<enumerable.Count-1)
//				{
//					result.Append(" | ");
//				}
//			}

//			return result.ToString();
//		}

//		#region operators

//		public static bool operator ==(LongFlagsEnum<T> longFlagsEnum1, LongFlagsEnum<T> longFlagsEnum2)
//		{
//			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
//			//{
//			//	return true;
//			//}

//			//if (ReferenceEquals(longFlagsEnum1, null))
//			//{
//			//	return false;
//			//}
//			//if (ReferenceEquals(longFlagsEnum2, null))
//			//{
//			//	return false;
//			//}
//			return longFlagsEnum1.Equals(longFlagsEnum2);
//		}


//		public static bool operator !=(LongFlagsEnum<T> longFlagsEnum1, LongFlagsEnum<T> longFlagsEnum2)
//		{
//			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
//			//{
//			//	return false;
//			//}

//			//if (ReferenceEquals(longFlagsEnum1, null))
//			//{
//			//	return true;
//			//}
//			//if (ReferenceEquals(longFlagsEnum2, null))
//			//{
//			//	return true;
//			//}
//			return !longFlagsEnum1.Equals(longFlagsEnum2);
//		}

//		public static LongFlagsEnum<T> operator |(LongFlagsEnum<T> longFlagsEnum1, LongFlagsEnum<T> longFlagsEnum2)
//		{
//			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
//			//{
//			//	return null;
//			//}

//			//if (ReferenceEquals(longFlagsEnum1, null))
//			//{
//			//	return longFlagsEnum2;
//			//}
//			//if (ReferenceEquals(longFlagsEnum2, null))
//			//{
//			//	return longFlagsEnum1;
//			//}
//			return new LongFlagsEnum<T> (longFlagsEnum1.flagsInt | longFlagsEnum2.flagsInt);
//		}

//		public static LongFlagsEnum<T> operator &(LongFlagsEnum<T> longFlagsEnum1, LongFlagsEnum<T> longFlagsEnum2)
//		{

//			if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
//			{
//				return UNDEFINED;
//			}

//			if (ReferenceEquals(longFlagsEnum1, null))
//			{
//				return UNDEFINED;
//			}
//			if (ReferenceEquals(longFlagsEnum2, null))
//			{
//				return UNDEFINED;
//			}
//			return new LongFlagsEnum<T>(longFlagsEnum1.flagsInt & longFlagsEnum2.flagsInt);
//		}

//		public static LongFlagsEnum<T> operator ~(LongFlagsEnum<T> longFlagsEnum1)
//		{
//			if (ReferenceEquals(longFlagsEnum1, null))
//			{
//				return new LongFlagsEnum<T>((T[])Enum.GetValues(typeof(T)));
//			}
//			return new LongFlagsEnum<T>(~longFlagsEnum1.flagsInt);
//		}

//#endregion
	//}

	[Serializable]
	public struct TopicFlags 
	{
		[NonSerialized]
		private BigInteger? _flagsInt;

#if UNITY_5_3_OR_NEWER
		[SerializeField]
#endif
		private byte[] bytes;

		public TopicFlags(BigInteger flagsInt)
		{
			_flagsInt = flagsInt;
			bytes = _flagsInt.Value.ToByteArray();
		}

		public TopicFlags(params ComicTopic[] enumValues)
		{
			_flagsInt = 0;
			foreach (var enumV in enumValues)
			{
				int bitNum = ((int)(object)enumV)-1;
				_flagsInt = _flagsInt.Value | (BigInteger.One << bitNum);
			}
			bytes = _flagsInt.Value.ToByteArray();
		}

		public BigInteger FlagsInt
		{
			get
			{
				if (!_flagsInt.HasValue)
				{
					if (bytes == null)
					{
						bytes = new byte[0];
					}
					_flagsInt = new BigInteger(bytes);
				}
				return _flagsInt.Value;
			}
		}

		public bool Contains(ComicTopic enumV)
		{
			return (FlagsInt & (BigInteger.One << ((int)(object)enumV)-1)) != 0;
		}

		public bool ContainsAll(TopicFlags otherFlags)
		{
			return (FlagsInt & otherFlags.FlagsInt) == otherFlags.FlagsInt;
		}

		public bool ContainsAny(TopicFlags otherFlags)
		{
			return (FlagsInt & otherFlags.FlagsInt) != 0;
		}

		public bool IsEmpty()
		{
			return FlagsInt == 0;
		}

		//public HashSet<T> GetHashSet()
		//{
		//	return flagsInt.ToHashSet<T>();
		//	//HashSet<T> result = new HashSet<T>();

		//	//int byteNum = 0;
		//	//foreach (byte b in flagsInt.ToByteArray())
		//	//{
		//	//	for (int bitInByteIndex = 0; bitInByteIndex<8; bitInByteIndex++)
		//	//	{
		//	//		byte mask = (byte)(1 << bitInByteIndex);
		//	//		if ((b & mask) != 0)
		//	//		{
		//	//			int enumInt = (byteNum * 8) + bitInByteIndex;
		//	//			result.Add((T)Enum.ToObject(typeof(T), enumInt)); 
		//	//		}
		//	//	}

		//	//	byteNum++;
		//	//}

		//	//return result;
		//}

		public ComicTopic[] ToArray()
		{
			return FlagsInt.ToHashSet<ComicTopic>().ToArray();
			//var hashset = GetHashSet();
			//var result = new T[hashset.Count];
			//int i = 0;
			//foreach (var enumValue in hashset)
			//{
			//	result[i++] = enumValue;
			//}
			//return result;
		}

		public override int GetHashCode()
		{
			return FlagsInt.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is TopicFlags)
			{
				return FlagsInt == ((TopicFlags)obj).FlagsInt;
			}
			return false;
		}

		override public string ToString()
		{
			//return flagsInt.ToBinary();

			if (FlagsInt == 0) return ComicTopic.UNDEFINED.ToString();
			StringBuilder result = new StringBuilder();
			var topicsSet = FlagsInt.ToHashSet<ComicTopic>();
			int i = 0;
			foreach (var enumValue in topicsSet)
			{
				result.Append(enumValue);
				if (i++ < (topicsSet.Count - 1))
				{
					result.Append(" | ");
				}
			}

			return result.ToString();
		}

		public static bool operator ==(TopicFlags longFlagsEnum1, TopicFlags longFlagsEnum2)
		{
			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
			//{
			//	return true;
			//}

			//if (ReferenceEquals(longFlagsEnum1, null))
			//{
			//	return false;
			//}
			//if (ReferenceEquals(longFlagsEnum2, null))
			//{
			//	return false;
			//}
			return longFlagsEnum1.Equals(longFlagsEnum2);
		}


		public static bool operator !=(TopicFlags longFlagsEnum1, TopicFlags longFlagsEnum2)
		{
			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
			//{
			//	return false;
			//}

			//if (ReferenceEquals(longFlagsEnum1, null))
			//{
			//	return true;
			//}
			//if (ReferenceEquals(longFlagsEnum2, null))
			//{
			//	return true;
			//}
			return !longFlagsEnum1.Equals(longFlagsEnum2);
		}

		public static TopicFlags operator |(TopicFlags longFlagsEnum1, TopicFlags longFlagsEnum2)
		{
			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
			//{
			//	return null;
			//}

			//if (ReferenceEquals(longFlagsEnum1, null))
			//{
			//	return longFlagsEnum2;
			//}
			//if (ReferenceEquals(longFlagsEnum2, null))
			//{
			//	return longFlagsEnum1;
			//}
			return new TopicFlags(longFlagsEnum1.FlagsInt | longFlagsEnum2.FlagsInt);
		}

		public static TopicFlags operator &(TopicFlags longFlagsEnum1, TopicFlags longFlagsEnum2)
		{

			//if (ReferenceEquals(longFlagsEnum1, longFlagsEnum2))
			//{
			//	return UNDEFINED;
			//}

			//if (ReferenceEquals(longFlagsEnum1, null))
			//{
			//	return UNDEFINED;
			//}
			//if (ReferenceEquals(longFlagsEnum2, null))
			//{
			//	return UNDEFINED;
			//}
			return new TopicFlags(longFlagsEnum1.FlagsInt & longFlagsEnum2.FlagsInt);
		}

		public static TopicFlags operator ~(TopicFlags longFlagsEnum1)
		{
			if (ReferenceEquals(longFlagsEnum1, null))
			{
				return new TopicFlags((ComicTopic[])Enum.GetValues(typeof(ComicTopic)));
			}
			return new TopicFlags(~longFlagsEnum1.FlagsInt);
		}

	}

}
