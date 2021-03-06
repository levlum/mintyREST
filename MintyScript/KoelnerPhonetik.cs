using System;
using System.Collections.Generic;
using System.Text;
//using System.Xml.Linq;

namespace Com.Gamegestalt.MintyScript
{
	/// <summary>
	/// Koelner phonetik.
	/// code von Martin Hey
	/// von: http://www.uniquesoftware.de/Blog/de/post/2010/01/04/Phonetische-Ahnlichkeiten-mit-Hilfe-der-Kolner-Phonetik-erkennen.aspx
	/// 
	/// </summary>
	public class KoelnerPhonetik
	{
		
		    // create an array for all the characters without specialities
		private static char[] value0Chars = new[] { 'A', 'E', 'I', 'J', 'O', 'U', 'Y', 'Ä', 'Ö', 'Ü' };
		private static char[] value1Chars = new[] { 'B' };
		private static char[] value3Chars =  { 'F', 'V', 'W' };
		private static char[] value4Chars =  { 'G', 'K', 'Q' };
		private static char[] value5Chars =  { 'L' };
		private static char[] value6Chars =  { 'M', 'N' };
		private static char[] value7Chars =  { 'R' };
		private static char[] value8Chars =  { 'S', 'Z', 'ß' };
		private static char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y', 'ü', 'ö', 'ä' };
		private static char[] CHAR_CONST1 = new char[] { 'A', 'H', 'K', 'L', 'O', 'Q', 'R', 'U', 'X' };
		private static char[] CHAR_CONST2 = new char[] { 'S', 'Z', 'ß' };
		private static char[] CHAR_CONST3 = new char[] { 'A', 'H', 'K', 'O', 'Q', 'U', 'X' };
		private static char[] CHAR_CONST4 = new char[] { 'C', 'S', 'Z', 'ß' };
		private static char[] CHAR_CONST5 = new char[] { 'C', 'K', 'Q' };
		
		public static int VowelCount(string word){
			int num=0;
			foreach (char c in word){
				if (ArrayUtils.Contains( vowels,c)) {
					num++;
				}
			}
			return num;
		}
		
		public static List<string> Syllables (string word)
	    {
			List<string> syllables = new List<string>();
	        string currentWord = word;
	        bool lastWasVowel = false;
			int pos = 0;
			int lastPos = 0;
	        foreach (char wc in currentWord)
	        {
	            bool foundVowel = ArrayUtils.Contains( vowels,wc);
				
				if (foundVowel && !lastWasVowel && lastPos<(pos)){
					syllables.Add(currentWord.Substring(lastPos, pos-lastPos));
					lastPos = pos;
				}
				
	
	            lastWasVowel = foundVowel;
				pos++;
	        }
			
			if (lastPos<pos) {
				syllables.Add(currentWord.Substring(lastPos, pos-lastPos));
			}
	
	        return syllables;
	    }
		
		public static string ConvertToColognePhoneticCode(string value)
		{
		    // check parameter
		    if (string.IsNullOrEmpty(value))
		    {
		        return string.Empty;
		    }
		 
		    // convert to uppercase and copy to array
		    char[] valueChars = value.ToUpper().ToCharArray();
		 
		 
		    // create a stringbuilder to combine the code
		    StringBuilder cpCode = new StringBuilder();
		 
		    // iterate through the word's characters
		    for (int i = 0; i < valueChars.Length; i++)
		    {
		        // get the current character and it's context
		        char previousChar = i > 0 ? valueChars[i - 1] : ' ';
		        char currentChar = valueChars[i];
		        char nextChar = i < valueChars.Length - 1 ? valueChars[i + 1] : ' ';
		 
		        bool isFirstChar = (i == 0 || !Char.IsLetter(previousChar));
		 
		        // ignore non letters
		        if (!Char.IsLetter(currentChar))
		        {
		            if (Char.IsWhiteSpace(currentChar))
		            {
		                cpCode.Append(' ');
		            }
		             
		            continue;
		        }
		 
		        // if current character is in group with value 0 add value 0
				
		        if (ArrayUtils.Contains(value0Chars,currentChar))
		        {
		            cpCode.Append('0');
		            continue;
		        }
		        // if current character is in group with value 1 add value 1
		        if (ArrayUtils.Contains(value1Chars,currentChar))
		        {
		            cpCode.Append('1');
		            continue;
		        }
		        // if current character is in group with value 3 add value 3
		        if (ArrayUtils.Contains(value3Chars,currentChar))
		        {
		            cpCode.Append('3');
		            continue;
		        }
		        // if current character is in group with value 4 add value 4
		        if (ArrayUtils.Contains(value4Chars,(currentChar)))
		        {
		            cpCode.Append('4');
		            continue;
		        }
		        // if current character is in group with value 5 add value 5
		        if (ArrayUtils.Contains(value5Chars,currentChar))
		        {
		            cpCode.Append('5');
		            continue;
		        }
		        // if current character is in group with value 6 add value 6
		        if (ArrayUtils.Contains(value6Chars,currentChar))
		        {
		            cpCode.Append('6');
		            continue;
		        }
		        // if current character is in group with value 7 add value 7
		        if (ArrayUtils.Contains(value7Chars,currentChar))
		        {
		            cpCode.Append('7');
		            continue;
		        }
		        // if current character is in group with value 8 add value 8
		        if (ArrayUtils.Contains(value8Chars,currentChar))
		        {
		            cpCode.Append('8');
		            continue;
		        }
		 
		        // if we are here it's a special combination of characters
		        switch (currentChar)
		        {
		            case 'C':
		                if (isFirstChar)
		                {
							if (ArrayUtils.Contains(CHAR_CONST1,nextChar))
		                    {
		                        cpCode.Append('4');
		                    }
		                    else
		                    {
		                        cpCode.Append('8');
		                    }
		                }
		                else
		                {
		                    if (ArrayUtils.Contains(CHAR_CONST2,previousChar))
		                    {
		                        cpCode.Append('8');
		                    } else if (ArrayUtils.Contains(CHAR_CONST3,nextChar))
		                    {
		                        cpCode.Append('4');
		                    }
		                    else
		                    {
		                        cpCode.Append('8');
		                    }
		                }
		                break;
		            case 'D':
		            case 'T':
						if (ArrayUtils.Contains(CHAR_CONST4,nextChar))
		                {
		                    cpCode.Append('8');
		                }
		                else
		                {
		                    cpCode.Append('2');
		                }
		                break;
		            case 'P':
		                if (nextChar == 'H')
		                {
		                    cpCode.Append('3');
		                }
		                else
		                {
		                    cpCode.Append('1');
		                }
		                break;
		            case 'X':
		                if (ArrayUtils.Contains(CHAR_CONST5,previousChar))
		                {
		                    cpCode.Append('8');
		                }
		                else
		                {
		                    cpCode.Append('4');
		                    cpCode.Append('8');
		                }
		                break;
		        }
		    }
		 
		    // cleanup the code (remove double characters and remove 0 values)
		    StringBuilder cleanedCpCode = new StringBuilder(cpCode.Length);
		    for (int i = 0; i < cpCode.Length; i++)
		    {
		        char lastAddedChar = cleanedCpCode.Length > 0 ? cleanedCpCode[cleanedCpCode.Length - 1] : ' ';
		        if (lastAddedChar != cpCode[i])
		        {
		            if (cpCode[i] == '0' && lastAddedChar == ' ' || cpCode[i] != '0')
		            {
		                    cleanedCpCode.Append(cpCode[i]);
		            }
		        }
		    }
		    // return trhe result
		    return cleanedCpCode.ToString();
		}
		
	}
}

