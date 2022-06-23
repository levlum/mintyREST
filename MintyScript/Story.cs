using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{
	[Serializable]
	#if UNITY_5_3_OR_NEWER
	public class Story:UnityEngine.ISerializationCallbackReceiver, IEquatable<Story>
	#else
	public class Story: IEquatable<Story>
	#endif
	{
		public string name;
		public TopicFlags topics;

		public List<string> _serializedParts = new List<string>();

		/// <summary>
		/// in the story is one sentence from every part, except a story has an id, this id is not used
		/// </summary>
		public List<Dictionary<string,List<int>>> parts = new List<Dictionary<string, List<int>>>();

		HashSet<string> allIDs = new HashSet<string>();

		#region IEquatable implementation

		public bool Equals(Story other)
		{
			return (other != null && name == other.name);
		}

		#endregion

		#if UNITY_5_3_OR_NEWER
		
		

		

		#region ISerializationCallbackReceiver implementation

		
		

		
		public void OnBeforeSerialize()
		{
			_serializedParts.Clear();
			foreach (var dict in parts)
			{
				string[] dictStrings = new string[dict.Count];
				int j = 0;
				foreach (var dictKeyVal in dict)
				{
					string[] sNums = new string[dictKeyVal.Value.Count + 1];
					sNums[0] = dictKeyVal.Key;
					int i = 1;
					foreach (int num in dictKeyVal.Value)
					{
						sNums[i] = num.ToString();
						i++;
					}
					dictStrings[j] = string.Join(",", sNums);
					j++;
				}

				_serializedParts.Add(string.Join(";", dictStrings));
			}
		}

		public void OnAfterDeserialize()
		{
			parts = new List<Dictionary<string, List<int>>>();

			foreach (string dictString in _serializedParts)
			{
				var dict = new Dictionary<string, List<int>>();
				string[] keyValStrings = dictString.Split(';');
				foreach (string keyValString in keyValStrings)
				{
					string[] sNums = keyValString.Split(',');
					List<int> sentenceList = new List<int>();
					for (int i = 1; i < sNums.Length; i++)
					{
						sentenceList.Add(int.Parse(sNums[i]));
					}
					dict[sNums[0]] = sentenceList;
				}

				allIDs.UnionWith(dict.Keys);

				parts.Add(dict);
			}
		}


		

		

		#endregion

		
		
		#endif


		public override string ToString()
		{
			string constructs;
			return ToString(out constructs);
		}


		public string ToString(out string constructs, string id = null, ICharacterData me = null, ICharacterData you = null, ICharacterData other = null)
		{
			
			StringBuilder text = new StringBuilder();
			StringBuilder constructsBuilder = new StringBuilder();
			Dictionary<String, Word> variables = new Dictionary<string, Word>();
			List<Sentence> sentences = new List<Sentence>();
			List<int> sentencesForOnePosition;
			var ct = MintyTextsContainer.Instance;

			if (string.IsNullOrEmpty(id))
			{
				if (allIDs.Count == 0 && parts != null && parts.Count > 0)
				{
					foreach (Dictionary<string, List<int>> part in parts)
					{
						allIDs.UnionWith(part.Keys);
					}
				}

				if (allIDs.Count > 0)
				{
					id = allIDs.ElementAt(Utils.RandomRange(0, allIDs.Count));
				}
				else
				{
					id = "";
				}
			}

			if (parts != null && parts.Count > 0)
			{
				foreach (Dictionary<string,List<int>> part in parts)
				{
					if (!part.TryGetValue(id, out sentencesForOnePosition)
					    && !part.TryGetValue("", out sentencesForOnePosition))
					{
						continue;
					}

					if (sentencesForOnePosition != null && sentencesForOnePosition.Count > 0)
					{
						sentences.Add(ct.Sentences[sentencesForOnePosition[Utils.RandomRange(0, sentencesForOnePosition.Count)]]);
					}
				}

				if (sentences.Count > 0)
				{
					int counter = 0;
					foreach (var s in sentences)
					{
						s.next_processing_namedWords = variables;
						text.Append(MintyUtils.AddDotAndBigStartingLetters(s.Process(me, you, other)) + " ");
						constructsBuilder.Append(s.Text);
						if (counter++ < sentences.Count - 1)
						{
							constructsBuilder.AppendLine();
						}
						//Logger.DebugL (this, "satz " + partNum + ", key: " + storyKey +", num possible keys is "+ sentences.Count + ": " + s.last_processing_result);
						foreach (string key in s.last_processing_namedWords.Keys)
						{
							Word w = null;
							if (s.last_processing_namedWords.TryGetValue(key, out w))
							{
								variables[key] = w;
							}
						}

						s.Reset();
					}
				}
			}


			constructs = constructsBuilder.ToString();
			return text.ToString();
		}



		public string ToString_deprecated(out string constructs, string id = null, ICharacterData me = null, ICharacterData you = null, ICharacterData other = null)
		{
			List<string> textLines = new List<string>();
			List<string> cTextLines = new List<string>();
			Dictionary<String, Word> variables = new Dictionary<string, Word>();
			List<string> usedStoryIDs = new List<string>();
			List<string> notUsedStoryIDs = new List<string>();
			var ct = MintyTextsContainer.Instance;

			//Logger.DebugL (this, "Building Story out of " + parts.Count + " sentences.");
			int partNum = 0;
			foreach (Dictionary<string,List<int>> sentences in parts)
			{
				partNum++;
				Sentence s = null;
				string storyKey = "";
				if (usedStoryIDs.Count == 0)
				{
					if (!TryGetRandomSentence(sentences, notUsedStoryIDs, out storyKey, out s))
					{

						continue;
					}

					if (storyKey != "")
					{
						usedStoryIDs.Add(storyKey);
					}
				}

				if (s == null)
				{
					foreach (string storyID in usedStoryIDs)
					{
						List<int> sss = null;
						if (sentences.TryGetValue(storyID, out sss))
						{
							s = ct.Sentences[sss[Utils.RandomRange(0, sss.Count)]];
							storyKey = storyID;
							break;
						}
					}

					if (s != null)
					{
						foreach (string notUsedKey in sentences.Keys)
						{
							if (notUsedKey != storyKey && notUsedKey != "" && !notUsedStoryIDs.Contains(notUsedKey))
							{
								notUsedStoryIDs.Add(notUsedKey);
								//Logger.DebugL (this, "ompf-Added " + notUsedKey + " to notUsabeleKeys.");
							}
						}
					}
				}

				if (s == null)
				{
					if (!TryGetRandomSentence(sentences, notUsedStoryIDs, out storyKey, out s))
					{
						continue;
					}
					if (storyKey != "" && !usedStoryIDs.Contains(storyKey))
					{
						usedStoryIDs.Add(storyKey);
					}
				}


				s.next_processing_namedWords = variables;
				textLines.Add(MintyUtils.AddDotAndBigStartingLetters(s.Process(me, you, other)));
				cTextLines.Add(s.Text);
				//Logger.DebugL (this, "satz " + partNum + ", key: " + storyKey +", num possible keys is "+ sentences.Count + ": " + s.last_processing_result);
				foreach (string key in s.last_processing_namedWords.Keys)
				{
					Word w = null;
					if (s.last_processing_namedWords.TryGetValue(key, out w))
					{
						variables[key] = w;
					}
				}

				s.Reset();
			}

			constructs = string.Join("\n", cTextLines.ToArray());
			string result = string.Join("\n", textLines.ToArray());
			return result;
		}

		public static bool TryGetRandomSentence(Dictionary<string,List<int>> dict, List<string> notUsabeleKeys, out string key, out Sentence value)
		{
			key = null;
			value = null;

			var ct = MintyTextsContainer.Instance;

			if (dict == null || dict.Count <= 0)
			{
				//Logger.DebugL ("TryGetRandomSentence", "story part has no sentences!");
				return false;
			}

			List<string> keys = new List<string>(dict.Keys);

			while (keys.Count > 0 && key == null)
			{
				key = keys[Utils.RandomRange(0, keys.Count)];
				if (notUsabeleKeys.Contains(key))
				{
					keys.Remove(key);
					key = null;
				}
			}

			if (key == null)
			{
				//Logger.DebugL ("TryGetRandomSentence", "could not find a random sentence in part. ");
				return false;
			}

			//Logger.DebugL("TryGetRandomSentence", "used key: \""+key+"\" out of "+dict.Keys.Count+" possible keys.");
			foreach (string notUsedKey in dict.Keys)
			{
				if (notUsedKey != key && notUsedKey != "")
				{
					notUsabeleKeys.Add(notUsedKey);
					//Logger.DebugL ("TryGetRandomSentence", "Added "+notUsedKey+" to notUsabeleKeys.");
				}
			}

			List<int> sss = null;
			if (dict.TryGetValue(key, out sss))
			{
				value = ct.Sentences[sss[Utils.RandomRange(0, sss.Count)]];
				return true;
			}
			return false;
		}
	}

}

