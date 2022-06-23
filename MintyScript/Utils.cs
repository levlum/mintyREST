using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
    public static class Utils
    {
#if UNITY_5_3_OR_NEWER
        
		public static void SetParentWithoutMatrixChange(Transform obj, Transform newParent)
		{
			Vector3 localPos = obj.localPosition;
			Vector3 localScale = obj.localScale;
			Quaternion localRot = obj.localRotation;
			obj.parent = newParent;
			obj.localPosition = localPos;
			obj.localScale = localScale;
			obj.localRotation = localRot;
		}

		public static float RangeToRange(float v, float originalMin, float originalMax, float targetMin, float targetMax, bool clamp = true)
		{
			if (clamp)
			{
				if (v < originalMin)
					v = originalMin;
				if (v > originalMax)
					v = originalMax;
			}
			v = (v - originalMin) / (originalMax - originalMin);
			v = v * (targetMax - targetMin) + targetMin;
			return v;
		}



		public static BodyType GetRandomBodyType()
		{
			BodyType[] types = new BodyType[]
			{
				BodyType.FEMALE_ATHLETIC,
				BodyType.FEMALE_LEPTOSOMIC,
				BodyType.FEMALE_PYKNIC,
				BodyType.MALE_ATHLETIC,
				BodyType.MALE_LEPTOSOMIC,
				BodyType.MALE_PYKNIC,
			};
			return types[UnityEngine.Random.Range(0, 6)];
		}

		public static BodyType GetRandomBodyType(GenderType gender)
		{
			BodyType[] types = new BodyType[]
			{
				BodyType.FEMALE_ATHLETIC,
				BodyType.FEMALE_LEPTOSOMIC,
				BodyType.FEMALE_PYKNIC,
				BodyType.MALE_ATHLETIC,
				BodyType.MALE_LEPTOSOMIC,
				BodyType.MALE_PYKNIC,
			};
			if (gender == GenderType.FEMALE)
			{
				return types[UnityEngine.Random.Range(0, 3)];
			}
			else
			{
				return types[UnityEngine.Random.Range(3, 6)];
			}
		}

		public static GenderType GetGender(BodyType body)
		{
			List<BodyType> types = new List<BodyType>()
			{
				BodyType.FEMALE_ATHLETIC,
				BodyType.FEMALE_LEPTOSOMIC,
				BodyType.FEMALE_PYKNIC,
				BodyType.MALE_ATHLETIC,
				BodyType.MALE_LEPTOSOMIC,
				BodyType.MALE_PYKNIC,
			};
			return (types.IndexOf(body)	< 3) ? GenderType.FEMALE : GenderType.MALE;
		}

		/// <summary>
		/// Formats an int to a string with leading zeroes (if the int has less digits than minNumDecimals).
		/// </summary>
		public static string PadIntWithZeros(int value, int minNumDecimals, string c = "0")
		{
			//char[] c = new char[minNumDecimals];
			//long div = (long)Math.Pow(10, minNumDecimals-1);
			//for (int i = 0; i < minNumDecimals; i++) {
			//    long divVal = (value / div);
			//    c[i] = (char)(((int)'0') + divVal);
			//    value -= divVal * div;
			//    div /= 10;
			//}
			//return new string(c);
			string result = value.ToString();
			while (result.Length < minNumDecimals)
				result = c + result;
			return result;
		}
		


		public static string PadIntWithZeros(string result, int minNumDecimals, string c = "0")
		{
			while (result.Length < minNumDecimals)
				result = c + result;
			return result;
		}

#endif

		public static string ToBinary(this System.Numerics.BigInteger bigInteger)
		{
			string result = "";
			var byteArray = bigInteger.ToByteArray();
			for (int n = byteArray.Length - 1; n >= 0; n--)
			{
				byte b = byteArray[n];
				result += Convert.ToString(b, 2).PadLeft(8, '0');
			}

			return result;
		}

		public static HashSet<T> ToHashSet<T>(this System.Numerics.BigInteger bigInteger) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException(nameof(T));
			}

			HashSet<T> result = new HashSet<T>();

			int byteNum = 0;
			foreach (byte b in bigInteger.ToByteArray())
			{
				for (int bitInByteIndex = 0; bitInByteIndex < 8; bitInByteIndex++)
				{
					byte mask = (byte)(1 << bitInByteIndex);
					if ((b & mask) != 0)
					{
						int enumInt = (byteNum * 8) + bitInByteIndex + 1;
						result.Add((T)Enum.ToObject(typeof(T), enumInt));
					}
				}

				byteNum++;
			}

			return result;
		}


        public static string GetLastWord(this string text)
        {
            int lastWordIndex = text.LastIndexOf(" ");
            string lastWord = text;
            if (lastWordIndex >= 0 && lastWordIndex + 1 < text.Length)
            {
                lastWord = text.Substring(lastWordIndex + 1, text.Length - 1 - lastWordIndex);
            }

            if (!MintyUtils.IsSentenceEndMark(lastWord[lastWord.Length - 1]))
            {
                return lastWord;
            }
            else
            {
                return lastWord.Substring(0, lastWord.Length - 1);
            }
        }


        /// <summary>
        /// Replacement for Path.GetFileNameWithoutExtension(), which doesn't get exported to flash.
        /// Does not remove the path (if given), just everything after (and including) the last "."
        /// </summary>
        public static string RemoveFileExtension(string filename)
		{
			int index = filename.LastIndexOf(".");
			if (index == -1)
				return filename;
			return filename.Substring(0, index);
		}



		/// <summary>
		/// Given a GameObject that is either a Papermint Object (tree, character, furniture,...)
		/// or a child of one (e.g. a submesh in a character), find the root GameObject (that
		/// is the Papermint Object). May return null, if none of the objects parents are a PM Object.
		/// </summary>
		//public static GameObject FindPMObjectInHierarchy(GameObject obj) {
		//    if (Utils.ObjectHasComponentInArray(obj, Constants.PAPERMINT_OBJECTS)) return obj;
		//    if (obj.transform.parent == null) return null;
		//    return FindPMObjectInHierarchy(obj.transform.parent.gameObject); // recurse up the hierarchy
		//}

#if UNITY_5_3_OR_NEWER


		public static void SetTransformToIdentity(Transform transform)
		{
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
		}

		public static Vector3 GetUniformScaleVector(float scale)
		{
			return new Vector3(scale, scale, scale);
		}

		public static Vector2 RotateVector2(Vector2 vector, float degrees)
		{
			float radians = Mathf.Deg2Rad * degrees;
			Vector2 result = new Vector2(
				                 Mathf.Cos(radians) * vector.x - Mathf.Sin(radians) * vector.y,
				                 Mathf.Sin(radians) * vector.x + Mathf.Cos(radians) * vector.y
			                 );
			return result;
		}

		public static Vector2 FlattenAlongZ(Vector3 vec3)
		{
			return new Vector2(vec3.x, vec3.y);
		}

		/// <summary>
		/// Returns an angle between -180 and 180 degrees between the two objects.
		/// </summary>
		public static float SignedAngle(Vector2 a, Vector2 b)
		{
			float angle = Vector2.Angle(a, b);
			if (a.x * b.y - a.y * b.x < 0)
				return -angle;
			return angle;
		}
#endif



#if !UNITY_5_3_OR_NEWER
		public static System.Random fortuna;
        #endif
	
	
		public static int RandomRange(int min, int max)
		{
			#if UNITY_5_3_OR_NEWER

			return UnityEngine.Random.Range(min, max);

			#else
			int x = (int)RandomRange((float)min, (float)max);
			if (x >= max)
			{
				return max - 1;
			}
			return x;
			#endif
		}

    
		public static float RandomRange(float min, float max)
		{
#if UNITY_5_3_OR_NEWER

			return UnityEngine.Random.Range(min, max);

#elif UNITY_FLASH
        return UnityEngine.Random.Range(min, max);
#else
            if (fortuna == null)
            {
                fortuna = new System.Random((int)(DateTime.Now.Ticks));

            }
            return (((float)fortuna.NextDouble()) * (max - min)) + min;
#endif
        }

#if UNITY_5_3_OR_NEWER
		public static Vector2 GetBubbleOffset(Vector2 size, Vector2 pos)
		{
			int X = (int)((pos.x / size.x) * 4) - 2;
			int Y = (int)((pos.y / size.y) * 4) - 2;

			Vector2 result = new Vector2();
			result.x = (X == -2 || X == 0 || X == 1) ? 1f : -1f;
			result.y = (Y == -2 || Y == 0 || Y == 1) ? -1f : 1f;

			return result;
		}
#endif

	}


    public static class ListUtils
    {

        //	public static int BinarySearch<T>(List<T> list, T elem) where T : IComparable {
        //
        //		return BinarySearch<T>(list, elem, ComparableComperator<T>);
        //	}

        public static int BinarySearch<T>(List<T> list, T elem, IComparer<T> comparer)
        {
//#if UNITY_FLASH
//		int found =0;
//		if (list != null && elem != null) {
//			for (; found < list.Count; found++) {
//				int result = list[found].CompareTo(elem);
//				if ( result == 0 ) {
//					return found;
//				}
//				if (result > 0) {
//					break;
//				}
//			}
//		}
//		return ~found;
//#else
		
            int index = 0;
            if (list != null && list.Count > 0 && elem != null)
            {
                int maxI = list.Count;
                int minI = 0;
                index = minI + (int)((float)(maxI - minI) / (float)2);
                int lastIndex = -1;
                bool allSearched = false;
                while (!allSearched)
                {
                    int result;
//				if (comparer == null) {
//					result = list[index].CompareTo(elem);
//				} else {
                    result = comparer.Compare(list[index], elem);
//				}
                    if (result == 0)
                    {
                        return index;
                    }
                    if (result < 0)
                    {
                        minI = index + 1;
                        if (minI >= list.Count)
                            return ~list.Count;
                    }
                    else
                    {
                        maxI = index - 1;
                        if (maxI < 0)
                            return ~0;
                    }
				
                    lastIndex = index;
                    index = minI + (int)((float)(maxI - minI) / (float)2);
                    if (index == lastIndex)
                    {
                        allSearched = true;
                    }
                }
            }
            return ~index;
		
		

            //return list.BinarySearch(elem);


        }

        public class ComperatorInt : IComparer<int>
        {

            public int Compare(int ct1, int ct2)
            {
                int cti1 = (int)ct1;
                int cti2 = (int)ct2;
                return cti1.CompareTo(cti2);
            }

        }

        private static ComperatorInt intComperator;

        public static ComperatorInt IntComperator
        {
            get
            {
                if (intComperator == null)
                {
                    intComperator = new ComperatorInt();
                }
                return intComperator;
            }
        }

        //	public class ComperatorComparable<IComparable>: IComparer<IComparable> {
        //		public int Compare(IComparable ct1, IComparable ct2) {
        //			return ct1.CompareTo(ct2);
        //		}
        //	}
        //	private static ComperatorComparable<IComparable> comparableComperator;
        //	public static ComperatorComparable<IComparable> ComparableComperator<T> where T: IComparable {
        //		get {
        //			if (comparableComperator == null) { comparableComperator = new ComperatorComparable<IComparable>(); }
        //			return comparableComperator;
        //		}
        //	}

        public class ComparatorSentence : IComparer<Sentence>
        {
            public int Compare(Sentence ct1, Sentence ct2)
            {
			
#if UNITY_FLASH
			return ct1.Text.CompareTo(ct2.Text);
#else
                return string.Compare(ct1.Text, ct2.Text, true);
#endif

            }
        }

        private static ComparatorSentence sentenceComparator;

        public static ComparatorSentence SentenceComperator
        {
            get
            {
                if (sentenceComparator == null)
                {
                    sentenceComparator = new ComparatorSentence();
                }
                return sentenceComparator;
            }
        }

        public class ComparatorWord : IComparer<Word>
        {
            public int Compare(Word ct1, Word ct2)
            {
                return ct1.CompareTo(ct2);
            }
        }

        private static ComparatorWord wordComparator;

        public static ComparatorWord WordComperator
        {
            get
            {
                if (wordComparator == null)
                {
                    wordComparator = new ComparatorWord();
                }
                return wordComparator;
            }
        }

        public class ComparatorStringIgnoreCase : IComparer<string>
        {
            public int Compare(string ct1, string ct2)
            {

#if UNITY_FLASH
			return ct1.CompareTo(ct2);
#else
                return string.Compare(ct1, ct2, true);
#endif
            }
        }

        private static ComparatorStringIgnoreCase stringComperator;

        public static ComparatorStringIgnoreCase StringComperator
        {
            get
            {
                if (stringComperator == null)
                {
                    stringComperator = new ComparatorStringIgnoreCase();
                }
                return stringComperator;
            }
        }


        public static int[] BinarySearchClosest<T>(IList<T> list, T value, IComparer<T> comp)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            int lo = 0, hi = list.Count - 1;
            while (lo <= hi)
            {
                int m = lo + (hi - lo) / 2;
                int c = comp.Compare(list[m], value);
                if (c == 0)
                {
                    return new int[]{ m, m };
                }
                else if (c < 0)
                {
                    lo = m + 1;
                }
                else
                {
                    hi = m - 1;
                }

//			loT = list [lo];
//			hiT = list [hi];
//			T min1 = list [(lo<0?0:(lo - 1))];
//			T plus1 = list [hi+1 >= list.Count?list.Count-1:hi + 1];
//			Logger.DebugL ("", "delete me 2");
            }
//		if (comp.Compare(list[lo], value) == 0) hi++;
//		if (comp.Compare(list[lo], value) > 0) hi--;

            lo = Math.Min(list.Count - 1, Math.Max(0, lo));
            hi = Math.Min(list.Count - 1, Math.Max(0, hi));

//		var loT = list [lo];
//		var hiT = list [hi];
			return new int[]{ lo, hi };
		}

	}

	public static class StringUtils
	{

		public static string[] SplitNotInBrackets(this string input, char delimiter)
		{
//			input = "a,[1,2,3,{4,5},6],b,{c,d,[e,f],g},h";

			var delimiterPositions = new List<int>();
			int bracesDepth = 0;
			int bracketsDepth = 0;

			for (int i = 0; i < input.Length; i++)
			{
				switch (input[i])
				{
					case '{':
						bracesDepth++;
						break;
					case '}':
						bracesDepth--;
						break;
					case '[':
						bracketsDepth++;
						break;
					case ']':
						bracketsDepth--;
						break;

					default:
						if (bracesDepth == 0 && bracketsDepth == 0 && input[i] == delimiter)
						{
							delimiterPositions.Add(i);
						}
						break;
				}
			}

			var output = new List<string>();

			for (int i = 0; i < delimiterPositions.Count; i++)
			{
				int index = i == 0 ? 0 : delimiterPositions[i - 1] + 1;
				int length = delimiterPositions[i] - index;
				string s = input.Substring(index, length);
				output.Add(s);
			}

			string lastString = input.Substring(delimiterPositions.Last() + 1);
			output.Add(lastString);

			return output.ToArray();
		}

		//indexofany is unknown in flash
		public static int IndexOfAny(string s, char[] chars, int startIndex)
		{
#if UNITY_FLASH
        if (chars != null) {
			int smallestIndex = -1;
            for (int i = 0; i < chars.Length; i++) {
				/*
                int idx = s.IndexOf(chars[i]);
                if (idx >= 0 && idx <cutoff) {
                    return idx;
                }
                */
				int cIndex = s.IndexOf(chars[i], startIndex);
				if (cIndex >= 0 && (smallestIndex == -1 || cIndex < smallestIndex)) {
					smallestIndex = cIndex;
				}
            }
			return smallestIndex;
        }
        return -1;
#else
            return s.IndexOfAny(chars, startIndex);
#endif
		
        }
			
	

    }

    public static class ArrayUtils
    {

        public static bool Contains(int[] a, int v)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(char[] a, char v)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == v)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(string s, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (s.IndexOf(chars[i]) > 0)
                {
                    return true;
                }
            }
            return false;
        }



    }


    public class DLog
    {

        private static StringBuilder debugstr = new StringBuilder();
        public static int frames = 0;

        public static void Append(String s)
        {
            debugstr.Append(s);
        }

        public static void Debug(String s)
        {
            debugstr.Append(s);
            debugstr.Append("\n");
        }

        public static string Update(int frameToDisplay)
        {
            frames++;
            if ((frameToDisplay < 0 || frames >= frameToDisplay) && debugstr.Length > 0)
            {
                return GetBuffer();
            }
            return null;
        }

        public static string GetBuffer()
        {
            String s = debugstr.ToString();
            debugstr = new StringBuilder();
            frames = 0;
            return s;
        }

        public static void Print()
        {
            Print(-1);
        }

        public static void Print(int frameToDisplay)
        {
            string s = Update(frameToDisplay);
            if (s != null)
            {
                Logger.DebugM("*", s);
            }
        }
    }

    public class CallbackWithData<T>
    {
        public T callback;
        public System.Object data;

        public CallbackWithData(T callback, System.Object data)
        {
            this.callback = callback;
            this.data = data;
        }
    }


    public class CallbackParameter<T>
    {
        public T parameter;
        public System.Object data;

        public CallbackParameter(T parameter, System.Object data)
        {
            this.parameter = parameter;
            this.data = data;
        }
    }

    public class CallbackParameters<F,S>
    {
        public F first;
        public S second;

        public CallbackParameters(F a, S b)
        {
            this.first = a;
            this.second = b;
        }
    }

	public interface IWeightable
	{
		float GetWeight();
	}

}