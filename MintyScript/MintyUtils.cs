using System;
using System.Linq;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{
	
	
	public static class MintyUtils
	{
		
		public static long GetKey(int keyBitLength, params int[] keys)
		{
			long key = 0;
			for (int i = keys.Length - 1; i >= 0; i--)
			{
				key = key | keys[i];
				if (i > 0)
					key = key << keyBitLength;
			}
			return key;
		}

		public static int GetRandomIndex(float[] probabilities)
		{
			float sum = 0;
			foreach (float fl in probabilities)
				sum += fl;
			if (sum == 0)
				return Utils.RandomRange(0, probabilities.Length);
			float rand = Utils.RandomRange(0f, sum);
			sum = 0;
			for (int i = 0; i < probabilities.Length; i++)
			{
				sum += probabilities[i];
				if (rand < sum)
				{
					return i;
				}
			}
			return 0;
		}


		public static WordType GetTypeFromPath(string path)
		{
			if (path.ToUpper().StartsWith("SUBSTANTIV"))
			{
				return WordType.SUBSTANTIV;
			}
			else if (path.ToUpper().StartsWith("VERB"))
			{
				return WordType.VERB;
			}
			else if (path.ToUpper().StartsWith("ADJECTIVE"))
			{
				return WordType.ADJEKTIV;
			}
			else if (path.ToUpper().StartsWith("ADVERB"))
			{
				return WordType.ADVERB;
			}
			else if (path.ToUpper().StartsWith("ARTICLE"))
			{
				return WordType.ARTICLE;
			}
			else if (path.ToUpper().StartsWith("PRONOMEN/PERSONAL"))
			{
				return WordType.PRONOMEN_PERSONAL;
			}
			else if (path.ToUpper().StartsWith("PRONOMEN/REFLEXIV"))
			{
				return WordType.PRONOMEN_REFLEXIV;
			}

			return WordType.UNDEFINED;
		}
		
		//true if path starts with otherPath (if path is parent of otherPath)
		public static bool BelongsTo(string[] path, string[] otherPath)
		{
			if (otherPath == null || path.Length < otherPath.Length)
				return false;
			if (otherPath.Length == 1 && otherPath[0] == "")
				return true;
			
			for (int i = 0; i < otherPath.Length; i++)
			{
				if (path[i] != otherPath[i])
					return false;
			}
			
			return true;
		}

		public static string GetLastWord(string text)
		{
			if (text == null || text.Length == 0)
				return "";
			text = text.Trim();
			int posLastSpace = text.LastIndexOf(' ');
			if (posLastSpace < 0)
				return text;

			string word;
			if (IsPunctuationMark(text[text.Length - 1]))
			{
				word = text.Substring(posLastSpace + 1, text.Length - posLastSpace - 1);
			}
			else
			{
				word = text.Substring(posLastSpace + 1);
			} 

			return word;
		}

		public static bool IsSentenceEndMark(char c)
		{
			return (c == '.'
			|| c == '!'
			|| c == '?'
			|| c == ':');
		}

		public static bool IsPunctuationMark(char c)
		{
			return (c == '.'
			|| c == '!'
			|| c == '?'
			|| c == ':'
			|| c == ','
			|| c == ';');
		}

		public static string AddDotAndBigStartingLetters(string text)
		{
			if (text.Length > 0)
			{
				if (!IsSentenceEndMark(text[text.Length - 1]))
				{
					text += ".";
				}
				text = text[0].ToString().ToUpper() + text.Substring(1);
			}
			return text;
		}

		public static bool Is_story_topic(ComicTopic topic)
		{
			return topic != ComicTopic.UNDEFINED
			&& topic != ComicTopic.NOITEM
//			&& topic != ComicTopic.ALTERNATIVES
//			&& topic != ComicTopic.DEEP_STATEMENTS
			&& topic != ComicTopic.INDIRECT
//			&& topic != ComicTopic.MONOLOGUE
			&& topic != ComicTopic.MY_TOPIC
			&& topic != ComicTopic.NO_RESPONSE
			&& topic != ComicTopic.GRAMMATICAL
			&& topic != ComicTopic.NO_INFO
			&& topic != ComicTopic.MINTYSCRIPT
			&& topic != ComicTopic.QUOTATIONS
			&& topic != ComicTopic.SPECIAL
			&& topic != ComicTopic.SPECIALSTARTERS
			/*&& topic != ComicTopic.STATEMENTS*/;
		}

		public static ComicTopic[] StoryTopics
		{
			get
			{
				List<ComicTopic> storyTopics = new List<ComicTopic>();
				foreach (ComicTopic topic in Enum.GetValues(typeof (ComicTopic)))
				{
					if (Is_story_topic(topic))
					{
						storyTopics.Add(topic);
					}
				}

				return storyTopics.ToArray();
			}
		}
	}

}