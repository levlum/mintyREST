using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{


#if UNITY_5_3_OR_NEWER
	[System.Serializable]
	public class MintyTextsContainer : MonoBehaviour, ISerializationCallbackReceiver
	{


#region ISerializationCallbackReceiver implementation

				[SerializeField]
				private List<string> _serializedNames = new List<string>();
				[SerializeField]
				private List<GenderType> _serializedNameGenders = new List<GenderType>();

				[SerializeField]
				private List<WordFormContainer> _word_form_values = new List<WordFormContainer>();
				[SerializeField]
				private List<string> _word_form_keys = new List<string>();

				[SerializeField]
				private List<TextRelation> _serializedNamedRelations = new List<TextRelation>();

				[SerializeField]
				private List<Sentence> _serializedSentences = new List<Sentence>();

				[SerializeField]
				private List<GrammaticalEntity> _serializedGrammerCounter = new List<GrammaticalEntity>();

				public void OnBeforeSerialize()
				{
					if (_instance == null)
					{
						_instance = this;
					}

					_serializedNames.Clear();
					_serializedNameGenders.Clear();
					foreach (var nameGender in nameGenders)
					{
						_serializedNames.Add(nameGender.Key);
						_serializedNameGenders.Add(nameGender.Value);
					}

					_serializedNamedRelations.Clear();
					foreach (var relation in NamedRelations.Values)
					{
						_serializedNamedRelations.Add(relation);
					}

					_word_form_keys.Clear();
					_word_form_values.Clear();
					foreach (var wordForms in WordForms)
					{
						_word_form_keys.Add(wordForms.Key);
						_word_form_values.Add(wordForms.Value);
					}

					if (_sentences == null)
					{
						_serializedSentences.Clear();
					}
					else
					{
						_serializedSentences = new List<Sentence>(_sentences.Values);
					}


					_serializedGrammerCounter.Clear();
					foreach (var grammaTypeCounter in lastWordGrammerCounter)
					{
						foreach (var typeValues in grammaTypeCounter.Value)
						{
							GrammaticalEntity entity;
							entity.counter = typeValues.Value;
							entity.grammaticalEnumType = grammaTypeCounter.Key.AssemblyQualifiedName;
							entity.grammaticalValue = typeValues.Key.ToString();
							_serializedGrammerCounter.Add(entity);
						}
					}
				}

				public void OnAfterDeserialize()
				{
					if (_instance == null)
					{
						_instance = this;
					}


					//names
					if (nameGenders == null)
					{
						nameGenders = new Dictionary<string, GenderType>();
					}
					else
					{
						nameGenders.Clear();
					}

					for (int n = 0; n < Math.Min(_serializedNames.Count, _serializedNameGenders.Count); n++)
					{
						nameGenders[_serializedNames[n]] = _serializedNameGenders[n];
					}


					//relations
					NamedRelations.Clear(); 
					foreach (var relation in _serializedNamedRelations)
					{
						NamedRelations[relation.name] = relation;
					}


					//wordforms
					WordForms.Clear();
					for (int i = 0; i < Math.Min(_word_form_keys.Count, _word_form_values.Count); i++)
					{
						WordForms[_word_form_keys[i]] = _word_form_values[i];
					}


					//sentences
					if (_sentences == null)
					{
						_sentences = new SortedList<string, Sentence>();
					}
					else
					{
						_sentences.Clear();
					}
					foreach (Sentence s in _serializedSentences)
					{
						_sentences.Add(s.Text, s);
					}


					//last word grammer counter
					if (lastWordGrammerCounter == null)
					{
						lastWordGrammerCounter = new Dictionary<Type, Dictionary<object, int>>();
					}
					else
					{
						lastWordGrammerCounter.Clear();
					}
					foreach (var entity in _serializedGrammerCounter)
					{
						Dictionary<object, int> dict;
						try
						{
							Type type = Type.GetType(entity.grammaticalEnumType);
							object value = Enum.Parse(type, entity.grammaticalValue);

							if (!lastWordGrammerCounter.TryGetValue(type, out dict))
							{
								dict = new Dictionary<object, int>();
								lastWordGrammerCounter[type] = dict;
							}

							dict[value] = entity.counter;
						}
						catch (System.Exception e)
						{
							Logger.DebugL(this, e.Message, entity);
						}
					}


					initWordsOK = false;
					Initialize();

					index = new IndexContainer(this);
		}




#endregion


#else

		public class MintyTextsContainer
	{


#endif


		public static bool use_LocalizationManager = true;
		private static MintyTextsContainer _instance = null;
		public static float loadAmount = 0f;

		public static MintyTextsContainer Instance
		{
			get
			{
				if (_instance == null)
				{
#if UNITY_5_3_OR_NEWER
						_instance = GameObject.FindObjectOfType<MintyTextsContainer>();

						if (!_instance)
						{
							_instance = new GameObject("MintyTextsContainer", typeof(MintyTextsContainer)).GetComponent<MintyTextsContainer>();
						}
#else
						_instance = new MintyTextsContainer();
#endif

				}
				return _instance;
			}
		}


		[System.NonSerialized]
		public Dictionary<long, string[]> cache_endings = new Dictionary<long, string[]>();

		/// <summary>
		/// this is a dictionary which counts for all imported sentences, how often grammatical types are there at the last word.
		/// how often is used genetiv and how often akkusativ? (normally akkusativ is used more often) ..
		/// this is used for the probability to choose a grammatical form (use akkusativ more often)
		/// </summary>
		[System.NonSerialized]
		private Dictionary<Type, Dictionary<object, int>> lastWordGrammerCounter = new Dictionary<Type, Dictionary<object, int>>();

		//public MintyTextsContainer()
		//{
		//		Initialize();
		//}

		//#if UNITY_5_3_OR_NEWER
		//[HideInInspector]
		//#endif
		public bool rhymesReadyToUse = false;


		//		#if	UNITY_5_3_OR_NEWER && !MintyScriptHelper
		//		[SerializeField]
		//		#endif
		private bool initWordsOK = false;

		/// <summary>
		/// Initialize the MintyTextsContainer after loading sentences, words and relations.
		/// </summary>
		/// <param name="initOverride">If set to <c>true</c> init override.</param>
		public void Initialize(bool initOverride = false)
		{
			if (initOverride || !initWordsOK)
			{
				Logger.DebugL(this, "Initializing Words (WordGroups and Relations)");
				CreateWordGroups();

				initWordsOK = true;

				Dictionary<string,WordGroup> wgs = _wordGroups;

				//set radicals
				foreach (Word w in words)
				{
					string radical = w.Radical;
				}
				
				//put textRelations in words
				foreach (TextRelation rel in textRelations)
				{
					
					if (rel.ownerPaths != null && rel.ownerPaths.Count > 0)
					{
						
						foreach (Word w in GetWords( TextPattern.CreateFromPaths( rel.ownerPaths.ToArray())))
						{
							
							if (w.textRelations == null)
								w.textRelations = new List<int>();
							int i = ListUtils.BinarySearch<int>(w.textRelations, rel.num, ListUtils.IntComperator);
							if (i < 0)
							{
								i = ~i;
								w.textRelations.Insert(i, rel.num);
							}
						}
					}
					//if (rel.ownerWordIDs != null && rel.ownerWordIDs.Count > 0)
					//{
					//	foreach (int wordID in rel.ownerWordIDs)
					//	{
					//		Word w = words[wordID];
					//		if (w.textRelations == null)
					//			w.textRelations = new List<int>();
					//		int ind = ListUtils.BinarySearch<int>(w.textRelations, rel.num, ListUtils.IntComperator);
					//		if (ind < 0)
					//			w.textRelations.Insert(~ind, rel.num);
					//	}
					//}
				}

			}
		}



		public void CreateIndex()
		{
			index = new IndexContainer(this);
		}



		public IList<Sentence> Sentences
		{
			get
			{
				return (_sentences == null ? null : _sentences.Values);
			}
		}


		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		private SortedList<string,Sentence> _sentences;

#if UNITY_5_3_OR_NEWER
		[SerializeField]
#endif
		public List<Word> words;

//#if UNITY_5_3_OR_NEWER
//		[SerializeField]
//#endif
		private IndexContainer index;


		/// <summary>
		/// Method to use in Minty Script Editor. Replaces a sentence. Do not use this in a productive MintyTextsContainer.
		/// </summary>
		/// <param name="oldSentence">Old sentence.</param>
		/// <param name="newSentence">New sentence.</param>
		public void ReplaceSentence(Sentence oldSentence, Sentence newSentence)
		{
			_sentences[oldSentence.Text] = newSentence;
			newSentence.num = oldSentence.num;
		}

		public void AddSentence(Sentence s)
		{
			if (_sentences == null)
			{
				_sentences = new SortedList<string, Sentence>();
			}
//			try
//			{
				
			_sentences[s.Text] = s;

//				int index = ListUtils.BinarySearch<Sentence>(_sentences, s, ListUtils.SentenceComperator);
//				if (index < 0)
//				{
//					_sentences.Insert(~index, s);
//				}
//			}
//			catch (ArgumentException e)
//			{
//#if UNITY_FLASH
//					Logger.LogWarning (this, "\"" + s.Text + "\" " + e.ToString());
//#else
//				Logger.LogWarning(this, "\"" + s.Text + "\" " + e.Message);
//#endif
//			}
		}

		public List<Sentence> GetClosestSentences(string sText)
		{
			List<Sentence> closest = new List<Sentence>();
			if (_sentences == null)
			{
				return closest;
			}

//			Sentence sCompare = new Sentence();
//			sCompare.Text = sText;

			int[] indexi = ListUtils.BinarySearchClosest <string>(_sentences.Keys, sText, StringComparer.CurrentCultureIgnoreCase);

			//find text that is at the beginning of sText and Text of sentence
			string searchText_left = _sentences.Keys[indexi[0]];
			string searchText_right = _sentences.Keys[indexi[1]];
			string searchText = "";
			int pos = 0;
			bool leftIsCloser = true;
			for (pos = 0; pos < Math.Min(Math.Max(searchText_left.Length, searchText_right.Length), sText.Length); pos++)
			{
#if UNITY_FLASH
					bool leftOK = pos < searchText_left.Length && searchText_left [pos].ToString().ToLower() == sText [pos].ToString().ToLower();
					bool rightOK = pos < searchText_left.Length && searchText_right [pos].ToString().ToLower() == sText [pos].ToString().ToLower();
#else
				bool leftOK = pos < searchText_left.Length && char.ToLower(searchText_left[pos]) == char.ToLower(sText[pos]);
				bool rightOK = pos < searchText_right.Length && char.ToLower(searchText_right[pos]) == char.ToLower(sText[pos]);
#endif
				if (!leftOK && !rightOK)
				{

					break;
				}

				leftIsCloser = leftOK && !rightOK;
			}



			if (pos <= 2)
			{
				return closest;
			}

			searchText = (leftIsCloser ? searchText_left : searchText_right).Substring(0, pos);

			int index = leftIsCloser ? indexi[0] : indexi[1];

			//search left:
			for (int i = index; i >= 0; i--)
			{
				if (_sentences.Keys[i].StartsWith(searchText/*, StringComparison.CurrentCultureIgnoreCase*/))
				{
					closest.Add(_sentences.Values[i]);
				}
			}

			//search right:
			for (int i = index + 1; i < _sentences.Count; i++)
			{
				if (_sentences.Keys[i].StartsWith(searchText/*, StringComparison.CurrentCultureIgnoreCase*/))
				{
					closest.Add(_sentences.Values[i]);
				}
			}

			return closest;
		}


		public List<Story> stories = new List<Story>();

		public Story GetStory(string storyName, string storyID = "")
		{
			return stories.Find((story) => story.name == storyName);
		}


		public Dictionary<string,GenderType> nameGenders = new Dictionary<string, GenderType>();

		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		public List<TextRelation> textRelations;


		private Dictionary<string,TextRelation> _namedRelations;


		public Dictionary<string,TextRelation> NamedRelations
		{
			get
			{
				if (_namedRelations == null)
				{
					_namedRelations = new Dictionary<string, TextRelation>();
				}
				return _namedRelations;
			}
		}



		//public bool TryGetWordTypeOfRelationOwner(TextRelation relation, out WordType wordType)
		//{
		//	if (relation.ownerWordIDs != null && relation.ownerWordIDs.Count > 0)
		//	{
		//		wordType = words[relation.ownerWordIDs[0]].wordType;
		//		return true;
		//	}

		//	if (relation.ownerPaths != null && relation.ownerPaths.Count>0)
		//	{
		//		wordType = ComicUtils.GetTypeFromPath(relation.ownerPaths[0]);
		//		return true;
		//	}

		//	wordType = WordType.UNDEFINED;
		//	return false;
		//}

		[System.NonSerialized]
		private Dictionary<ComicTopic,List<Sentence>>
			_topic_index_sentences = null;


		public Dictionary<ComicTopic,List<Sentence>> Topic_index_sentences
		{
			get
			{
				if (_topic_index_sentences == null)
				{
					_topic_index_sentences = new Dictionary<ComicTopic, List<Sentence>>();
					foreach (Sentence s in Sentences)
					{
						var allSentenceTopics = new TopicFlags(s.topics.FlagsInt | s.inlineTopics.FlagsInt);
						foreach (var topic in allSentenceTopics.FlagsInt.ToHashSet<ComicTopic>())
						{

							//if (topicsList != null)
							//{
								//foreach (ComicTopic topic in topicsList)
								//{
									List<Sentence> list;
									if (!_topic_index_sentences.TryGetValue(topic, out list))
									{
										list = new List<Sentence>();
										_topic_index_sentences.Add(topic, list);
									}
									list.Add(s);
							//	}
							//}
						}

					}
				}
				return _topic_index_sentences;
			}
		}

		[System.NonSerialized]
		private Dictionary<ComicTopic,List<Word>>
			_topic_index_words = null;



		public Dictionary<ComicTopic,List<Word>> Topic_index_words
		{
			get
			{
				if (_topic_index_words == null)
				{
					_topic_index_words = new Dictionary<ComicTopic, List<Word>>();
					foreach (Word w in words)
					{
						if (w.topics.FlagsInt != 0)
						{
							foreach (ComicTopic topic in w.topics.FlagsInt.ToHashSet<ComicTopic>())
							{
								List<Word> list;
								if (!_topic_index_words.TryGetValue(topic, out list))
								{
									list = new List<Word>();
									_topic_index_words.Add(topic, list);
								}
								list.Add(w);
							}
						}
						else
						{
							List<Word> list;
							if (!_topic_index_words.TryGetValue(ComicTopic.UNDEFINED, out list))
							{
								list = new List<Word>();
								_topic_index_words.Add(ComicTopic.UNDEFINED, list);
							}
							list.Add(w);
						}
						
					}
				}
				return _topic_index_words;
			}
		}


		public  Dictionary<string, WordFormContainer> WordForms = new Dictionary<string, WordFormContainer>();

		public bool TryFindFittingForms(string wordText, out List<Word> fittingWords, out List<TextPattern> pattern)
		{
			fittingWords = null;
			pattern = null;
			wordText = wordText.ToLower();
			
			WordFormContainer container;
			if (!WordForms.TryGetValue(wordText, out container))
				return false;
            
			formFoundCounter++;
			
			//find right form:
			fittingWords = new List<Word>();
			pattern = new List<TextPattern>();
			foreach (int wNum in container.words)
			{
				Word w = words[wNum];
				foreach (TextPattern formPattern in w.CreateAllFormPatterns())
				{
					Word form = w.CreateForm(formPattern);
					if (form != null && form.Text.Equals(wordText, StringComparison.CurrentCultureIgnoreCase))
					{
						fittingWords.Add(form);
						pattern.Add(formPattern);
					}
				}
			}
			
			return fittingWords.Count > 0;
		}

		public static int formFoundCounter = 0;

		public static List<TextPattern> GetFittingPatterns(string form, Word word)
		{
			List<TextPattern> patterns = new List<TextPattern>();
			
			foreach (TextPattern formPattern in word.CreateAllFormPatterns())
			{
				Word formWord = word.CreateForm(formPattern);
				if (formWord != null && string.Equals(formWord.Text, form, StringComparison.CurrentCultureIgnoreCase))
				{
					patterns.Add(formPattern);
				}
			}

			if (patterns.Count == 0)
			{
//				Logger.DebugL ("", "delete me! 3");
			}

			return patterns;
		}

		public void Clear()
		{
			ClearWords();
			ClearSentences();

			//Logger.DebugL(this, "completely removed ComicTextsContainer");
			//_instance = null;
		}

		public void ClearSentences()
		{
			rhymesReadyToUse = false;
			_sentences = null;
			stories.Clear();
			_lastWordSentences = null;
			_topic_index_sentences = null;
			lastWordGrammerCounter.Clear();

			index = null;
		}

		public void ClearWords()
		{
			rhymesReadyToUse = false;
			words = null;
			textRelations = null;
			_namedRelations = null;
			WordForms.Clear();
			_wordGroups = null;
			initWordsOK = false;
			_topic_index_words = null;
			endingsTable = null;
			cache_endings.Clear();

			Logger.DebugL(this, "Cleared Words in ComicTextContainer.");
		}

		public void AddToWordFormIndex(Word word)
		{
			if (!word.IsLeaf)
				return;
			foreach (TextPattern formPattern in word.CreateAllFormPatterns())
			{
				Word form = word.CreateForm(formPattern);
				if (form != null && !string.IsNullOrEmpty(form.Text))
				{
					string formString = form.Text.ToLower();
					WordFormContainer container;
					if (!WordForms.TryGetValue(formString, out container))
					{
						container = new WordFormContainer(formString);
						WordForms[formString] = container;
					}
					if (!container.words.Contains(word.num))
						container.words.Add(word.num);
				}
			}
		}

		#if	UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		private List<LastWordListContainer> _serializedlastWordSentences;
			
		private Dictionary<string,HashSet<int>> _lastWordSentences;


		public Dictionary<string,HashSet<int>> LastWordSentences
		{
			get
			{
				if (_lastWordSentences == null)
				{
					_lastWordSentences = new Dictionary<string, HashSet<int>>();
						
					if (_serializedlastWordSentences != null && _serializedlastWordSentences.Count > 0)
					{
						foreach (LastWordListContainer lastWordContainer in _serializedlastWordSentences)
						{
							HashSet<int> d = new HashSet<int>();
							foreach (int sNum in lastWordContainer.sentences)
							{
								d.Add(sNum);
							}
							_lastWordSentences[lastWordContainer.lastWord] = d;
						}
							
					}
					else
					{
						
						string parentPath = "";
							
						//put sentences with pattern-words at the last position (possible rhymes) into wordgroups
						int counter = 0;
						foreach (Sentence s in Sentences)
						{
							if (s.lastWordPattern != null)
							{
								foreach (TextPattern lastWordPattern in s.lastWordPattern)
								{
									if (lastWordPattern.HasCondition(typeof(PathCondition)))
									{
										foreach (string sPath in lastWordPattern.Paths)
										{
											parentPath = sPath;
										
											HashSet<int> pSentences;
											if (!_lastWordSentences.TryGetValue(parentPath, out pSentences))
											{
												pSentences = new HashSet<int>();
												_lastWordSentences.Add(parentPath, pSentences);
											}
										
											if (!pSentences.Contains(s.num))
											{
												pSentences.Add(s.num);
												counter++;
											}
										
										}
									}
									List<FixedTextCondition> condList;
									if (lastWordPattern.TryGetPatternConditions<FixedTextCondition>(out condList))
									{
										foreach (var fixedCond in condList)
										{
											parentPath = "_fixed" + fixedCond.text;
										
											HashSet<int> pSentences;
											if (!_lastWordSentences.TryGetValue(parentPath, out pSentences))
											{
												pSentences = new HashSet<int>();
												_lastWordSentences.Add(parentPath, pSentences);
											
											
											}
										
											if (!pSentences.Contains(s.num))
											{
												//Logger.DebugL(this, "Added last word sentence ("+((FixedTextCondition)fixedCond).text+"): "+s.Text);
												pSentences.Add(s.num);
												counter++;
											}
										}
									}
								}
							}
						}
							
						_serializedlastWordSentences = new List<LastWordListContainer>();
						foreach (var wordSentences in _lastWordSentences)
						{
							_serializedlastWordSentences.Add(new LastWordListContainer()
								{
									lastWord = wordSentences.Key,
									sentences = new List<int>(wordSentences.Value)
								});
						}
						Logger.DebugL(this, "Added " + counter + " times a sentences in the dictionary for sentences with a last word pattern.");	
					}
						
						
				}

                if (false && !System.IO.File.Exists("LastWordSentences.txt"))
                {
                    StringBuilder saveResult = new StringBuilder();
                    foreach (var lastWordPath in _lastWordSentences)
                    {
                        saveResult.AppendLine(lastWordPath.Key);
                        foreach (int sNum in lastWordPath.Value)
                        {
                            saveResult.AppendLine("\t" + Sentences[sNum].Text);
                        }
                    }
                    saveResult.AppendLine("\nSentences without last word pattern:\n");
                    foreach (Sentence s in Sentences)
                    {
                        if (s.lastWordPattern == null)
                        {
                            saveResult.AppendLine(s.Text);
                        }
                    }
                    System.IO.File.WriteAllText("LastWordSentences.txt", saveResult.ToString());
                }

                return _lastWordSentences;
			}
		}


		public IEnumerable<Sentence> GetSentencesWithLastWordPattern(LastWordPatternCondition cond)
		{
			string parentPath;
			
			HashSet<int> resultDict;
			List<Sentence> result = new List<Sentence>();

				
			float bestMatch = 0;
			HashSet<Sentence> sDict = new HashSet<Sentence>();
			if (cond.pattern.HasCondition(typeof(PathCondition)))
			{

				foreach (string path in cond.pattern.Paths)
				{
					parentPath = path;


					while (parentPath != null)
					{
						if (LastWordSentences.TryGetValue(parentPath, out resultDict))
						{
							foreach (int sNum in resultDict)
							{
								Sentence s = Sentences[sNum];
								if (!sDict.Contains(s))
								{

                                    var matchValue = cond.MatchValue(s);


                                    if (matchValue > bestMatch)
                                    {
                                    	bestMatch = matchValue;
                                    	sDict.Clear();
                                    	sDict.Add(s);
                                    }
//#if UNITY_5_3_OR_NEWER
//                                    else if (Mathf.Approximately(matchValue, bestMatch))
//#else
                                    else if (float.Equals(matchValue, bestMatch))
//#endif
                                    {
                                        sDict.Add(s);
                                    }

                                }
                            }
						}

						int slashPos = parentPath.LastIndexOf('/');
						if (slashPos > 0)
							parentPath = parentPath.Substring(0, slashPos);
						else
							parentPath = null;
					}
				}

				//???
				//if (sList.Count == 0) return sList;
			}
			List<FixedTextCondition> condList;
			if (cond.pattern.TryGetPatternConditions<FixedTextCondition>(out condList))
			{
				foreach (var fixedCond in condList)
				{
					if (LastWordSentences.TryGetValue("_fixed" + fixedCond.text, out resultDict))
					{
						foreach (int sNum in resultDict)
						{
							Sentence s = Sentences[sNum];
							if (!sDict.Contains(s))
							{

								float matchValue = cond.MatchValue(s);
								if (matchValue > bestMatch)
								{
									bestMatch = matchValue;
									sDict.Clear();
									sDict.Add(s);
                                }
//#if UNITY_5_3_OR_NEWER
//                                    else if (Mathf.Approximately(matchValue, bestMatch))
//#else
                                else if (float.Equals(matchValue, bestMatch))
//#endif
                                {
                                    sDict.Add(s);
								}

							}
						}
					}
				}
			}


			//result = new List<Sentence>(sDict.Count);
			//foreach (int sNum in sDict)
			//{
			//	result.Add(Sentences[sNum]);
			//}

			return sDict;
		}

		[System.NonSerialized]
		private Dictionary<string,WordGroup> _wordGroups;


		public Dictionary<string,WordGroup> WordGroups
		{
			get
			{
				//if (!initWordsOK)
				//{
				//	Initialize();
				//}

				return _wordGroups;
			}
		}

		private void CreateWordGroups()
		{
			_wordGroups = new Dictionary<string, WordGroup>();
//			if (!initWordsOK)
//				Initialize();
			foreach (Word w in words)
			{
				AddWordToWordGroups(w);
			}
		}
			
		//[SerializeField]
		//		[System.NonSerialized]
		//		 public List<WordGroup> _wordsGroups = new List<WordGroup>();
		//		 private DictList<string, WordGroup> words {
		//            get {
		//                if (_wordsDict == null) {
		//                    _wordsDict = new DictList<string, WordGroup>(ref _wordsGroups);
		//                }
		//                return _wordsDict;
		//            }
		//        }
		//        private DictList<string, WordGroup> _wordsDict;


		private void AddWordToWordGroups(Word word)
		{
			if (word == null)
				return;

//			if (word.Text == "stöhn")
//			{
//				Logger.DebugL(this, "(del) adding stöhn to word groups");
//			}

			foreach (string path in word.paths)
			{
				string parentPath = "";
				foreach (string pathPart in ("/"+path).Split('/'))
				{
					parentPath += (parentPath.Length > 0 ? "/" : "") + pathPart;
					WordGroup wg = AddWordGroup(parentPath);
				
					if (parentPath == path)
					{
						if (wg.words == null)
							wg.words = new List<int>();
						wg.words.Add(word.num);
					}
					
					if (wg.allWords == null)
						wg.allWords = new HashSet<int>();
					
					wg.allWords.Add(word.num);
				}
			}
		}

		private WordGroup AddWordGroup(string path)
		{
			WordGroup wg;
			if (_wordGroups.TryGetValue(path, out wg))
			{
				return wg;
			}
			wg = new WordGroup(path);
			//Logger.DebugL(this, "Added \""+path+"\" to WordGroups");
			_wordGroups.Add(path, wg);
			return wg;
		}
		//		public WordGroup GetWordGroup (string path) {
		//			WordGroup wg;
		//			if (WordGroups.TryGetValue(path, out wg)){
		//				return wg;
		//			}
		//			return null;
		//		}
		
		//		public List<Word> words = new List<Word>(1);


		/// <summary>
		/// adds one to the count for all imported sentences, how often grammatical types are there at the last word.
		/// how often is used genetiv and how often akkusativ? (normally akkusativ is used more often) ..
		/// this is used for the probability to choose a grammatical form (use akkusativ more often)
		/// </summary>
		/// <param name="grammaticalValue">value of an grammatical enum</param>
		public void AddLastWordGramma(object grammaticalValue)
		{
			Dictionary<object,int> dict;
			if (!lastWordGrammerCounter.TryGetValue(grammaticalValue.GetType(), out dict))
			{
				dict = new Dictionary<object, int>();
				lastWordGrammerCounter[grammaticalValue.GetType()] = dict;
			}

			if (dict.ContainsKey(grammaticalValue))
			{
				dict[grammaticalValue]++;
			}
			else
			{
				dict[grammaticalValue] = 1;
			}
		}

		/// <summary>
		/// this returns the counts for all imported sentences, how often grammatical types are there at the last word.
		/// how often is used genetiv and how often akkusativ? (normally akkusativ is used more often) ..
		/// this is used for the probability to choose a grammatical form (use akkusativ more often)
		/// </summary>
		/// <returns>The last word gramma.</returns>
		/// <param name="grammaticalValue">value of an grammatical enum</param>
		public int GetLastWordGrammaCount(object grammaticalValue)
		{
			Dictionary<object,int> dict;
			if (lastWordGrammerCounter.TryGetValue(grammaticalValue.GetType(), out dict))
			{
				int count;
				if (dict.TryGetValue(grammaticalValue, out count))
				{
					return count;
				}
			}

			return 0;
		}


		public Sentence GetRandomSentence(ComicTopic topic)
		{
			return GetRandomSentence(new TextPattern(new TopicCondition(new TopicFlags (topic))));
		}

		public Sentence GetRandomSentence(TextPattern pattern, bool setPatternForProcessing = true)
		{
			Sentence s;
			if (TryGetRandomElement<Sentence>(GetSentences(pattern), out s))
			{
				if (setPatternForProcessing)
				{
					//this line was out-commented. I removed the comment, because otherwise: topcis and words are not choosen at processing later.
					s.SetPatternForNextProcessing(pattern);
				}
			}
			else
			{
				s = MintyText.ErrorText<Sentence>("Error getting sentences for pattern: "+pattern);
			}



			//			if (pattern.WordsToInclude.Count > 0) s.WordsToUse = pattern.WordsToInclude;
			//			if (pattern.PossibleTopics.Count > 0) s.currentTopic = pattern.PossibleTopics[0];
			return s;
		}

		public const float textLengthInfluenceFactor = 4f;

		public static bool TryGetRandomElement<T>(IEnumerable<T> weightables, out T result) where T : IWeightable
		{
			if (weightables == null)
			{
				result = default(T);
				return false;
			}
			else
			{
				float sum = 0;
				foreach (T weightable in weightables)
				{
					if (weightable != null)
					{
						sum += weightable.GetWeight();// + textLengthInfluenceFactor / (float)(weightable.Text.Length + 1);
					}
				}
				
				float rand = Utils.RandomRange(0f, sum);
//				if (weightables[0] is Sentence){
//					Logger.DebugL (weightables[0], "rand: "+rand+" ("+weightables.Length+" Sentences, weight Sum:"+sum+")");
//				}

				sum = 0f;
				var completeList = new List<T>();
				foreach (T weightable in weightables)
				{
					if (!weightable.Equals(default(T)))
					{
						sum += weightable.GetWeight();// + textLengthInfluenceFactor / (float)(weightable.Text.Length + 1);
						if (rand < sum)
						{
							result = weightable;
							return true;
						}
						completeList.Add(weightable);
					}
				}

				if (completeList.Count > 0)
				{
					Logger.LogWarning("ComicTextContainer", "Did not use the probability attribute.");
					result = (T)completeList[Utils.RandomRange(0, completeList.Count)];
					return true;
				}
				else
				{
					result = default(T);
					return false;
					//return MintyText.ErrorText<T>("error: can not get random text of a empty list");
				}
			}

		}

		public List<Sentence> GetSentences(TextPattern p)
		{
			var starttime = System.DateTime.Now;
			var pattern = (p == null ? new TextPattern() : (TextPattern)p.Clone());
			if (!pattern.HasCondition(typeof(VariableDependencyCondition)))
			{
				pattern.AddPatternConditions(new VariableDependencyCondition(null, Constants.MATCH_SUPER));
			}

			//remove empty topic Condition
			if (pattern.HasCondition(typeof(TopicCondition))
				&& pattern.PossibleTopics.IsEmpty())
			{
				pattern.RemovePatternType<TopicCondition>();
			}


			float bestMatch = 0;

			//IEnumerable<Sentence> sList = new List<Sentence>();
			HashSet<Sentence> sDict = new HashSet<Sentence>();
			List<LastWordPatternCondition> condList;
			if (pattern.TryGetPatternConditions<LastWordPatternCondition>(out condList))
			{
				foreach (var cond in condList)
				{
					foreach (Sentence s in GetSentencesWithLastWordPattern (cond))
					{
						if (!sDict.Contains(s))
						{
							sDict.Add(s);
						}
					}
				}
					
				if (sDict.Count == 0)
				{
					return new List<Sentence>(0);
				}

				pattern.RemovePatternType<LastWordPatternCondition>();
				if (pattern.NumOfConditionTypes == 0)
				{
					Logger.DebugL(this, "time for GetSentences (1): " + (System.DateTime.Now - starttime).Milliseconds);
					return new List<Sentence>(sDict);
				}
			}
			else
			{
				foreach (Sentence s in Sentences)
				{
                    if (s.num == Constants.ERROR_TEXT)
                    {
                        Logger.LogWarning(this, "can not add sentence with error?");
                    }
                    else if (sDict.Contains(s))
                    {
                        Logger.LogWarning(this, "Sentences contain multiple times the same sentence?");
                    }
                    else
                    {
                        sDict.Add(s);
                    }
				}
			}

			var timePos1 = (System.DateTime.Now - starttime).Milliseconds;
			if (index!= null && index.ContainsIndexCondition(pattern))
			{
				var part = index.GetSentences(pattern);
				if (part.Count == 0)
				{
					Logger.DebugL(this, "time for GetSentences (2): " + (System.DateTime.Now - starttime).Milliseconds);
					return new List<Sentence>(0);
				}

				if (sDict.Count > 0)
				{
					sDict.IntersectWith(part);
				}
				else
				{
					sDict = part;
				}
			}


			//int count = 0;
			//HashSet<int> allTopicSentences = new HashSet<int>();
			//TopicCondition topicCondition;
			//if (pattern.TryGetFirst<TopicCondition>(out topicCondition))
			//{
			//	foreach (ComicTopic topic in topicCondition.conditionTopics.GetHashSet())
			//	{
			//		List<Sentence> topicSentences;
			//		if (Topic_index_sentences.TryGetValue(topic, out topicSentences))
			//		{
			//			foreach (Sentence s in topicSentences)
			//			{
			//				if (sDict.ContainsKey(s.num) && !allTopicSentences.Contains(s.num))
			//				{
			//					allTopicSentences.Add(s.num);
			//					((List<Sentence>)sList).Add(s);
			//					count++;
			//				}
			//			}
			//		}
			//	}
			//}
			//else
			//{

			//sList = sDict;
			if (sDict.Count == 0)
			{
				Logger.DebugL(this, "time for GetSentences (3): " + (System.DateTime.Now - starttime).Milliseconds);
				return new List<Sentence>(0);
			}
			//}

			var timePos2 = (System.DateTime.Now - starttime).Milliseconds;



			bestMatch = 0;
			List<Sentence> list = new List<Sentence>();
			float matchValue;
			foreach (Sentence s in sDict)
			{
				matchValue = TextProcessor.IsParsing(s)? -Constants.MATCH_PERFECT : pattern.MatchValue(s);
				if (matchValue > bestMatch)
				{
					bestMatch = matchValue;
					list.Clear();
					list.Add(s);
				}
				else if (float.Equals( matchValue ,bestMatch))
				{
					list.Add(s);
				}
			}

			Logger.DebugL(this, "time for GetSentences ("+ sDict.Count() + "): pos1: "+timePos1+"ms  pos2: " +timePos2
				+ "ms end: "+ (System.DateTime.Now - starttime).Milliseconds 
				+ "ms\npattern: "+pattern
				+ "ms\nstart pattern: " + p);
			return list;
		}

		public List<Sentence> GetSentences(ComicTopic topic)
		{
			TextPattern pattern = topic == ComicTopic.UNDEFINED? new TextPattern():	new TextPattern(new TopicCondition(new TopicFlags (topic)));
			return GetSentences(pattern);
		}


		/** returns one gramatically processed word fitting the pattern **/
		public Word GetRandomWord(TextPattern pattern)
		{

			Word rawWord;

			if (TryGetRandomElement<Word>(GetWords(pattern), out rawWord))
			{
				if (rawWord.wordType != WordType.NOWORD && rawWord.wordType != WordType.UNDEFINED)
				{
					return rawWord.CreateForm(pattern);
				}
				else
				{
					return rawWord;
				}
			}
			else
			{
				return MintyText.ErrorText<Word>("Error at getting randim word for pattern: " + pattern);
			}

		}

		/** returns raw words (not gramatically processed) **/
		public List<Word> GetWords(TextPattern pattern)
		{
			//are there words in the pattern? then just return one of these words
			if (pattern != null && pattern.HasCondition(typeof(WordCondition)))
			{
				return pattern.WordsToInclude;
			}



			//no words in pattern
			TextPattern groundFormPattern = (TextPattern)pattern.Clone();
			List<string> paths = pattern.Paths;

			//special cases:

			//1. word is an article and article_type == "NO_ARTICE" --> use any article
			EnumCondition<WordType> wordType_condition;
			EnumCondition<Article_Type> article_type_condition;
			if (groundFormPattern.TryGetFirst<EnumCondition<WordType>>(out wordType_condition)
			    && wordType_condition.enumValue == WordType.ARTICLE
			    && groundFormPattern.TryGetFirst<EnumCondition<Article_Type>>(out article_type_condition)
			    && article_type_condition.enumValue == Article_Type.NO_ARTICLE)
			{
				groundFormPattern.RemovePatternType<EnumCondition<Article_Type>>();
			}

			//2. word is an adverb in a comparative form, just take adverbs that can have so.
			if (pattern.Get<WordType>() == WordType.ADVERB
				|| (paths != null && paths.Count>0 && MintyUtils.GetTypeFromPath(paths[0]) == WordType.ADVERB))
			{
				if (pattern.Get<Komparation>()== Komparation.Positiv || pattern.Get<Komparation>() == Komparation.Superlativ)
				{
					groundFormPattern.AddPatternConditions(new EnumCondition<TextFlag>(TextFlag.NO_COMPARATION, Constants.MATCH_PERFECT, true));
				}
			}


			//end special cases

			groundFormPattern.AddFormFlags();
			groundFormPattern.RemoveFormConditions();
			groundFormPattern.RemovePatternType<EnumCondition<WordType>>();
			
			float bestMatch = -Constants.MATCH_PERFECT;
			List<Word> selWords = new List<Word>(1);
			
			
			System.Collections.IEnumerable wordsE;
			var allPathWords = new HashSet<int>();

            EnumCondition<WordType> wordTypeCondition;
            if ((paths == null || paths.Count == 0)
            && pattern.TryGetFirst<EnumCondition<WordType>>(out wordTypeCondition))
            {
                switch (wordTypeCondition.enumValue)
                {
                    case WordType.ADJEKTIV:
                        paths = new List<string>(new string[]{"adjective"});
                        break;
                    case WordType.SUBSTANTIV:
                        paths = new List<string>(new string[] { "substantiv" });
                        break;
                    case WordType.VERB:
                        paths = new List<string>(new string[] { "verb" });
                        break;
                }
            }

            if (paths == null || paths.Count == 0)
			{
				wordsE = words;
				Logger.LogWarning(this, "TextPattern for selection without PathCondition? " + pattern);
				
				foreach (Word w in words)
				{
					allPathWords.Add(w.num);
				}
			}
			else
			{
				foreach (string path in paths)
				{
					WordGroup wordGroup;
					if (MintyTextsContainer.Instance.WordGroups.TryGetValue(path, out wordGroup))
					{
						allPathWords.UnionWith(wordGroup.allWords);
					}
				}
				wordsE = allPathWords;
				groundFormPattern.RemovePatternType<PathCondition>();
			}

			
			int count = 0;
			List<Word> allTopicWords = new List<Word>();
			if (pattern.HasCondition(typeof(TopicCondition)))
			{
				foreach (ComicTopic topic in pattern.PossibleTopics.FlagsInt.ToHashSet<ComicTopic>())
				{
					List<Word> topicWords;
					if (Topic_index_words.TryGetValue(topic, out topicWords))
					{
						foreach (Word w in topicWords)
						{
							if (allPathWords.Contains(w.num))
							{
								allTopicWords.Add(w);
								count++;
							}
						}
					}
				}
			}
			if (count == 0 && allPathWords.Count == 0)
				return new List<Word>(0);
			else if (count == 0)
			{
				//intersection is 0 words:
				wordsE = allPathWords;
			}
			else
				wordsE = allTopicWords;



			//TODO: to add the reflexive_only condition here is not good. rework it in a general way.
			//TODO: rework with FlagsCondition, ...
//			bool not_reflexive = !groundFormPattern.HasCondition (typeof (VerbCategoryCondition));
//			if (!not_reflexive){
//				foreach (PatternCondition c in groundFormPattern.GetPatternConditions(typeof (VerbCategoryCondition))) {
//					if (((VerbCategoryCondition)c).enumValue == VerbCategory.REFLEXIV
//						|| ((VerbCategoryCondition)c).enumValue == VerbCategory.REFLEXIV_ONLY) {
//						not_reflexive = false;
//						break;
//					}
//				}
//			}
//			if (not_reflexive) {
//				//if not reflexive and if context has no objects, then do EXCLUDE all REFLEXIV_ONLY words
//				if (groundFormPattern.HasCondition (typeof (ContextCondition))){
//					if (groundFormPattern.GetFirst<ContextCondition> ().context.nodeIndex.ContainsKey (ParseNodeType.OBJECT)) {
//						not_reflexive = false;
//					}
//				}
//				if (not_reflexive) {
//					groundFormPattern.AddPatternConditions (new VerbCategoryCondition (VerbCategory.REFLEXIV_ONLY, Constants.MATCH_SUPER, true));
//				}
//			}

			foreach (object wO in wordsE)
			{
				Word word = (wO is int) ? words[(int)wO] : (Word)wO;


				if (groundFormPattern.NumOfConditionTypes == 0)
				{
					selWords.Add(word);
					bestMatch = 0;
				}
				else
				{
					float matchValue = groundFormPattern.MatchValue(word);

					if (matchValue > bestMatch)
					{
						bestMatch = matchValue;
						selWords.Clear();
						selWords.Add(word);
					}
					else if (float.Equals( matchValue, bestMatch))
					{
						selWords.Add(word);
					}
				}
			}

			if (bestMatch < 0)
			{
				Logger.LogWarning(this, "Could not find fitting words for " + pattern.ToString());
			}
			//Logger.DebugL (this, "Selected "+selWords.Count+" Words out of "+count+". "+pattern);
			//if (wordPathOnly != "special/gramatik") Logger.DebugL(this, selWords.Count+" words for \""+wordPathOnly+"\", "+pattern);
			return selWords;
		}



		public class RhymingResult
		{
			public Sentence s1;
			public Dictionary<Sentence, Word> rhyming_sentences;
			public Word w1;
			public string form1;
			//			public List <Word> rhyming_words;
			//			public TextPattern p1;
			//			public TextPattern p2;
			public bool found = false;
			public bool findAll = false;
		}

		public void GetRhymingSentence(RhymingResult result)
		{

			string w1_form = "";
			if (!string.IsNullOrEmpty(result.form1))
			{
				w1_form = result.form1;
			}
			else
			{
				if (result.w1 == null)
				{
					if (result.s1.last_processing_result != null && result.s1.last_processing_result.Length > 0)
					{
						w1_form = MintyUtils.GetLastWord(result.s1.last_processing_result);
					}
					else
					{
						Logger.LogWarning(this, "No last word for rhyming.");
						return;
					}
				}
				else
				{
					w1_form = result.w1.Text;
				}
			}


			//int rand = 0;
			Dictionary<Word,string> secondWords = new Dictionary<Word, string>();
			WordFormContainer cont;
			//Word secondWord;

			if (!WordForms.TryGetValue(w1_form, out cont) || cont.rhymingForms == null || cont.rhymingForms.Count == 0)
			{
                return;
				//var rhymes = GetRhymes(w1_form);
				//foreach (var wordPatterns in rhymes)
				//{
				//	foreach (var pattern in wordPatterns.Value)
				//	{
				//		secondFormedWord = wordPatterns.Key.CreateForm(pattern);
				//		secondFormedWord.weight *= pattern.GetGrammaticalProbability();
				//		secondFormedWords.Add(secondFormedWord);
				//	}
				//}

			}
			else
			{
				foreach (var rhymeForm in cont.rhymingForms)
				{
					WordFormContainer rhymeFormContainer;

					if (!WordForms.TryGetValue(rhymeForm, out rhymeFormContainer))
					{
						Logger.DebugL(this, "No word forms for: " + rhymeForm);
					}

					if (rhymeFormContainer != null && rhymeFormContainer.words.Count > 0)
					{


						foreach (int wNum in rhymeFormContainer.words)
						{
                            secondWords.Add (words[wNum], rhymeFormContainer.form);
//							foreach (var pattern in GetFittingPatterns(rhymeForm, w))
//							{
//								secondFormedWord = w.CreateForm(pattern);
//								secondFormedWord.weight *= pattern.GetGrammaticalProbability();

//								secondWords.Add(secondFormedWord);
////								if (!secondFormedWords.Exists((aWord) => aWord.Text.ToLower() == rhymeForm))
////								{
////									Debug.Log("next rhyme :" + rhymeForm);
////								}
							//}
						}
					}
				}
			}

			if (secondWords.Count == 0)
			{
				return;
			}


			var foundForms = new HashSet<string>();

			while (secondWords.Count > 0)
			{
				int rand = Utils.RandomRange(0, secondWords.Count);
//				secondFormedWord = secondFormedWords[rand];
//				secondFormedWords.RemoveAt(rand);

				var secondWord = secondWords.ElementAt(rand);
				secondWords.Remove(secondWord.Key);

				//if (result.w1 != null && result.w1.num == secondWord.Key.num)
				//{
				//	continue;
				//}

				//if (w1_form == secondWord.Key.Text)
				//{
				//	Logger.DebugL(this, "first word and second word is equal? " + w1_form);
				//	continue;
				//}

				//if (foundForms.Contains(secondWord.Text.ToLower()))
				//{
				//	continue;
				//}

				
				Dictionary<Sentence,Word> rhymingSentences = null;
				//TextPattern secondPattern = null;
				TextPattern sentencePattern = new TextPattern();
				if (result.s1 != null)
				{
					sentencePattern = new TextPattern(
						new VariableDependencyCondition(result.s1.variableNames),
						new SentenceCondition(result.s1, Constants.MATCH_PERFECT, true)
						////							new TextMeaningCondition( TextMeaning.ANSWER, Constants.MATCH_MINIMAL),
						////								new TextMeaningCondition( TextMeaning.STATEMENT, Constants.MATCH_MINIMAL),
						////								new TextMeaningCondition( TextMeaning.SUBORDINATE, Constants.MATCH_MINIMAL),
					);
				}

				//exclude all previously taken sentences
				if (result.rhyming_sentences != null)
				{
					foreach (Sentence rs in result.rhyming_sentences.Keys)
					{
						sentencePattern.AddPatternConditions(new SentenceCondition(rs, Constants.MATCH_PERFECT, true));
					}
				}
					
				if ( TryFindLastWordSentence(secondWord.Key, secondWord.Value, sentencePattern, out rhymingSentences, result.findAll))
				{

					//found second sentence!!!

					string rhymeResult = "rhyme found for those words: \"" + w1_form + "\" and \"" + secondWord.Value + "\"\n\t"
					                     + "s1: " + (result.s1 == null ? "no first sentence" : result.s1.Text) + "\n\t";

					int rc = 2;
					foreach (var rs in rhymingSentences)
					{
						rhymeResult += "s" + rc + ": " + rs.Key.Text + "\n\t";
						rc++;
					}
					//Logger.DebugL(this, rhymeResult);

					if (result.rhyming_sentences == null)
					{
						result.rhyming_sentences = new Dictionary<Sentence, Word>();
					}
					else
					{
						//look if there is the same word or sentence allready used in one of the previous rhyming sentences
						foreach (var prevSentenceWord in result.rhyming_sentences)
						{
							if (prevSentenceWord.Value.num == secondWord.Key.num
                            || rhymingSentences.ContainsKey(prevSentenceWord.Key))
							{
								continue;
							}
						}
                    }

                    //add one random rhyming sentence
                    var sPair = rhymingSentences.ElementAt(Utils.RandomRange(0, rhymingSentences.Count));
//					Word w2 = secondFormedWord.CreateForm (s.LastWordPattern);
//					Word w2 = secondFormedWord;
//					w2.textPosition = TextPosition.LINE_END;

                    //do not set pattern for next processing. it has to be reset after processing, ... 
                    //					s.SetPatternForNextProcessing(new TextPattern(new WordCondition(w2)));

                    result.rhyming_sentences[sPair.Key] = sPair.Value;

					foundForms.Add(sPair.Value.Text.ToLower());

					result.found = true;
//					if (!result.findAll)
//						return;
				}
			}

		}

#if UNITY_5_3_OR_NEWER
		float startTime = 0;

		public IEnumerator GetRhymingSentences(RhymingResult result, Dictionary<string,Word> precondition = null, TextPattern prePattern = null, Action<RhymingResult> onComplete = null)
		{
			Logger.DebugL(this, "START GetRhymingSentences ... ");
			//startTime = Time.time;



#else

		public void GetRhymingSentences(RhymingResult result, Dictionary<string,Word> precondition = null, TextPattern prePattern = null)
		{
			//Console.WriteLine ("NEW RHYME:");

#endif
			result.s1 = null;
			result.rhyming_sentences = null;
			Word first = null;
//			Word secondW = null;
			result.found = false;
			int n = 0;
			string log = "";

			var wordsWithRhymes = new List<WordFormContainer>();
			foreach (WordFormContainer cont in WordForms.Values)
			{
				if (cont.rhymingForms != null && cont.rhymingForms.Count > 0)
				{
					wordsWithRhymes.Add(cont);
				}
			}
			

			while (wordsWithRhymes.Count > 0 && !result.found)
			{
				result.found = false;
				int rand = Utils.RandomRange(0, wordsWithRhymes.Count);
				var container = wordsWithRhymes[rand];
				wordsWithRhymes.RemoveAt(rand);

				List<Word> firstWords = new List<Word>();
				foreach (int wNum in container.words)
				{
					Word w = words[wNum];
                    firstWords.Add(w);
				//	foreach (var pattern in GetFittingPatterns(container.form, w))
				//	{
				//		var pWord = w.CreateForm(pattern);
				//		pWord.weight *= pattern.GetGrammaticalProbability();
				//		firstWords.Add(pWord);
				//	}
				}

				while (firstWords.Count > 0)
				{

					if (TryGetRandomElement<Word>(firstWords, out first))
					{
						firstWords.Remove(first);


						//try to find a sentence for the first word
						Dictionary<Sentence, Word> rhymingSentences = null;
						//							TextPattern firstPattern = null;
						TextPattern sentencePattern = new TextPattern(
													  //new TextMeaningCondition( TextMeaning.QUESTION, Constants.MATCH_MINIMAL),
													  //new TextMeaningCondition( TextMeaning.STATEMENT, Constants.MATCH_MINIMAL),
													  );

						if (precondition != null)
						{
							sentencePattern.AddPatternConditions(
								new VariableDependencyCondition(new List<string>(precondition.Keys))
							);
						}

						if (TryFindLastWordSentence(first, container.form, sentencePattern, out rhymingSentences, result.findAll))
						{

							var randSentence = rhymingSentences.ElementAt(Utils.RandomRange(0, rhymingSentences.Count));
							result.s1 = randSentence.Key;
							result.w1 = randSentence.Value;
							result.w1.textPosition = TextPosition.LINE_END;


							GetRhymingSentence(result);

							if (result.found)
							{
#if UNITY_5_3_OR_NEWER
								//Logger.DebugL(this, "END GetRhymingSentences ... " + (Time.time - startTime) + "\n" + log);

								if (onComplete != null)
								{
									onComplete.Invoke(result);
								}

								yield break;
#else
							return;
#endif
							}
							else
							{
								//							log += "not taken: " + container.form + " - " + result.w2 + ":\n\t" + result.s1 + "\n\t" + result.s2 + "\n";
							}
						}
					}
				}
				
				if ((n++) % 1 == 0)
				{
					//Logger.DebugM(this, "yield GetRhymingSentences ... ");
#if UNITY_5_3_OR_NEWER
					yield return null;
#endif
				}
			}

#if UNITY_5_3_OR_NEWER && !MintyScriptHelper

            if (onComplete != null)
            {
                onComplete.Invoke(result);
            }
            //Logger.DebugL(this, "END GetRhymingSentences ... " + (Time.time - startTime) + "\n" + log);
#endif
        }

//		public bool TryFindLastWordSentence(string form, out Dictionary<Word, List<Sentence>> allSentences, bool oneSentenceOnly = false)
//		{
//			Dictionary<Sentence,Word> sentences = null;
////			fittingFormedWords = new List<Word>(1);
//			allSentences = new Dictionary<Word, List<Sentence>>(1);
//			var rhymes = GetRhymes(form); //TODO: this is known allready? ind wordForms?
//			if (rhymes == null || rhymes.Count == 0)
//			{
//				return false;
//			}

//			while (rhymes.Count > 0)
//			{
//				int rand = Utils.RandomRange(0, rhymes.Count);
////				#if UNITY_5_3_OR_NEWER
////
////				KeyValuePair<Word, List<TextPattern>> wordPatterns = new KeyValuePair<Word, List<TextPattern>>();
////				int i = 0;
////				foreach (var wp in rhymes)
////				{
////					if (i == rand)
////					{
////						wordPatterns = wp;
////						break;
////					}
////				}
////
////				#else
//				var wordPatterns = rhymes.ElementAt(rand);
////				#endif

		//		if (wordPatterns.Key != null && rhymes.ContainsKey(wordPatterns.Key))
		//		{
		//			rhymes.Remove(wordPatterns.Key);
		//		}
		//		else
		//		{
		//			break;
		//		}

		//		List<TextPattern> patterns = new List<TextPattern>(wordPatterns.Value);
		//		while (patterns.Count > 0)
		//		{
		//			rand = Utils.RandomRange(0, patterns.Count);
		//			var pattern = patterns[rand];
		//			patterns.RemoveAt(rand);

		//			Word formedWord = wordPatterns.Key.CreateForm(pattern);
		//			if (!allSentences.ContainsKey(formedWord)
		//			    && TryFindLastWordSentence(formedWord, formedWord.Text, new TextPattern(), out sentences, !oneSentenceOnly))
		//			{
		//				allSentences.Add(formedWord, sentences);
		//				if (oneSentenceOnly)
		//				{
		//					return true;
		//				}
		//			}
		//		}
		//	}

		//	return allSentences.Count > 0;
		//}

		public bool TryFindLastWordSentence(Word word, string form, TextPattern sentencePattern, out Dictionary<Sentence, Word> sentences, bool findAll)
		{
			sentences = null;

			if (word == null)
			{
				return false;
			}

			//is there a word-form that is the form?

//			var patternList = ComicTextsContainer.GetFittingPatterns(form, word);
			TextPattern pathPattern = new TextPattern();
			foreach (string path in word.paths)
			{
				pathPattern.AddPatternConditions(new PathCondition(path));
			}
			pathPattern.AddPatternConditions(new FixedTextCondition(form));


			
			sentencePattern.AddPatternConditions(
				new LastWordPatternCondition(pathPattern, Constants.MATCH_SUPER)
			);


            IEnumerable<Sentence> fittingPathSentences = GetSentences(sentencePattern);
            //IEnumerable<Sentence> fittingPathSentences = GetSentencesWithLastWordPattern(new LastWordPatternCondition(pathPattern));
            List<Sentence> fittingSentences = new List<Sentence>();

            //remain only sentences that do fit the grammatical form of the word
            List <LastWordPatternCondition> lastWordConditions = new List<LastWordPatternCondition>();
            foreach (var pattern in GetFittingPatterns(form, word))
            {
                lastWordConditions.Add (new LastWordPatternCondition(pattern));
            }

            foreach (var sToProve in fittingPathSentences)
            {
                foreach (var lastWordCondition in lastWordConditions)
                {
                    if (lastWordCondition.MatchValue(sToProve) > 0)
                    {
                        fittingSentences.Add(sToProve);
                        break;
                    }
                }
            }

            Sentence sentence = null;
			//Word wordForm = (Word)word.Clone ();

			while (fittingSentences.Count > 0)
			{

				if (TryGetRandomElement<Sentence>(fittingSentences, out sentence)
					&& sentence.lastWordPattern != null)
				{
					bool sentenceOK = false;
					foreach (var lastWordPattern in sentence.lastWordPattern)
					{
						List<FixedTextCondition> condList;
						if (lastWordPattern.TryGetPatternConditions<FixedTextCondition>(out condList))
						{
							foreach (var cond in condList)
							{
								if (String.Equals(cond.text, form, StringComparison.CurrentCultureIgnoreCase))
								{
									sentenceOK = true;
									//Logger.DebugL (this, "FIXED END WORD FOUND ("+form+"): " + sentence.Text);
									break;
								}
							}
						}
					}

                    Word sentenceWord = null;
                    if (!sentenceOK)
					{
						foreach (var lastWordPattern in sentence.lastWordPattern)
						{
							if (word.TryCreateForm(lastWordPattern, out sentenceWord)
							    && String.Equals(sentenceWord.Text, form, StringComparison.CurrentCultureIgnoreCase))
							{

								//look if the word fits all criteria (lastWordPattern does not give information about "relations")
								if (sentenceWord != null)
									sentence.AddConditionForNextProcessing(new WordCondition(sentenceWord));
								string testSentence = sentence.Process(null, null, null).Trim('.', '!', '?', ' ', ':', ';');
								sentence.Reset();
								if (testSentence.EndsWith(sentenceWord.ToString(), StringComparison.CurrentCultureIgnoreCase))
								{
									sentenceOK = true;
								}

							}
						}
					}

					if (sentenceOK)
					{
						if (sentences == null)
						{
							sentences = new Dictionary<Sentence, Word>(1);
						}
						sentences.Add(sentence, sentenceWord);

						if (!findAll)
						{
							break;
						}
					}
				}

				fittingSentences.Remove(sentence);

			}

			if (sentences != null)
			{
				return true;
			}

			return false;
		}

		private Dictionary<string, Dictionary<string, WordFormContainer>> endingsTable = null;

		public Dictionary<Word,List<TextPattern>> GetRhymes(string wordForm)
		{
			if (wordForm == null || wordForm.Length <= 3)
				return null;
			wordForm = wordForm.Trim().ToLower();
			
			if (endingsTable == null)
			{
				endingsTable = new Dictionary<string, Dictionary<string, WordFormContainer>>();
				foreach (WordFormContainer cont in WordForms.Values)
				{
					
					if (cont.form.Length > 2)
					{
						//Dictionary<string, WordFormContainer> endCont;
						string key = cont.form.Substring(cont.form.Length - 3).ToLower();
						if (!endingsTable.ContainsKey(key))
						{
							endingsTable[key] = new Dictionary<string, WordFormContainer>();
						}
						if (!endingsTable[key].ContainsKey(cont.form.ToLower()))
							endingsTable[key].Add(cont.form.ToLower(), cont);
					}
				}
			}
			
			Dictionary<Word,List<TextPattern>> rhymes = new Dictionary<Word, List<TextPattern>>();
			Dictionary<string, WordFormContainer> forms;
			var syllables = KoelnerPhonetik.Syllables(wordForm);
			if (endingsTable.TryGetValue(wordForm.Substring(wordForm.Length - 3), out forms))
			{
				foreach (WordFormContainer cont in forms.Values)
				{
					if (IsRhyme(wordForm, cont.form.ToLower(), syllables))
					{
						foreach (int wordNum in cont.words)
						{
							if (!rhymes.ContainsKey(words[wordNum]))
							{
								rhymes.Add(words[wordNum], GetFittingPatterns(cont.form, words[wordNum]));
							}
						}
					}
				}
			}
			
			return rhymes;
		}
		
		//private static string[] rhymeEndExceptionsGer = {"este","erer","gere","ier"};
		private static string[] rhymeEndExceptionsGer = { };
		private static string[] rhymeStartExceptionsGer =
			{
				"un",
				"ge",
				"er",
				"be",
				"zer",
				"ent",
				"ver",
				"in",
				"um"
			};
		private static string[,] rhymeExceptions =
			{
				{ "gefühlen", "befühlen" },
				{ "gefühlt", "befühlt" },
				{ "nerd", "herd" },
				{ "nerds", "herds" },
				{ "bier", "eier" },
				{ "eier", "tier" },
				{ "sprache", "drache" },
				{ "sprache", "mache" },
				{ "apfel", "äpfel" },
				{ "schule", "eule" },
				{ "schulen", "eulen" },
				{ "hase", "angsthase" },
				{ "hasen", "angsthasen" },
				{ "hörner", "einhörner" },
				{ "hörnern", "einhörnern" },
				{ "ernsteres", "schweres" }
			};
		private static Dictionary<string,List<string>> _rhymeExceptionMap;


		private static Dictionary<string,List<string>> RhymeExceptionMap
		{
			get
			{ 
				if (_rhymeExceptionMap == null)
				{
					_rhymeExceptionMap = new Dictionary<string, List<string>>();
					for (int i = 0; i < rhymeExceptions.GetLength(0); i++)
					{
						List<string> noRhymes;
						if (!_rhymeExceptionMap.TryGetValue(rhymeExceptions[i, 0], out noRhymes))
						{
							noRhymes = new List<string>();
							_rhymeExceptionMap.Add(rhymeExceptions[i, 0], noRhymes);
						}

						if (!noRhymes.Contains(rhymeExceptions[i, 1]))
						{
							noRhymes.Add(rhymeExceptions[i, 1]);
						}

						if (!_rhymeExceptionMap.TryGetValue(rhymeExceptions[i, 1], out noRhymes))
						{
							noRhymes = new List<string>();
							_rhymeExceptionMap.Add(rhymeExceptions[i, 1], noRhymes);
						}

						if (!noRhymes.Contains(rhymeExceptions[i, 0]))
						{
							noRhymes.Add(rhymeExceptions[i, 0]);
						}
					}
				}
				return _rhymeExceptionMap;
			}
		}

		/// <summary>
		/// Language dependent Method!!
		/// Determines whether w2 is a rhyme of w1.
		/// </summary>
		/// <returns>
		/// <c>true</c> if 2w is a rhyme of w1; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='w1'>
		/// If set to <c>true</c> w1.
		/// </param>
		/// <param name='w2'>
		/// If set to <c>true</c> w2.
		/// </param>
		public static bool IsRhyme(string w1, string w2, List<string> syllables1 = null)
		{
			
			if (w1.Length < 3 || w2.Length < 3)
				return false;
			if (w1.Substring(w1.Length - 2) != w2.Substring(w2.Length - 2))
				return false;


			//special exceptions
			foreach (string exceptionEnd in rhymeEndExceptionsGer)
			{
				if (w1.EndsWith(exceptionEnd) || w2.EndsWith(exceptionEnd))
					return false;
			}
			foreach (string exceptionStart in rhymeStartExceptionsGer)
			{
				//if w1 or w2 starts with the exception and the following word is equal, it is no rhyme
				if (w1.StartsWith(exceptionStart) && w1.Substring(exceptionStart.Length) == w2)
					return false;
				if (w2.StartsWith(exceptionStart) && w2.Substring(exceptionStart.Length) == w1)
					return false;
			}
//			if (w1.ToLower () == "bier") {
//				Logger.DebugL ("", "test for \"" + w1 + "\"");
//			}
			List<string> exceptions;
			if (RhymeExceptionMap.TryGetValue(w1, out exceptions) && exceptions.Contains(w2))
			{
				return false;
			}

			if (syllables1 == null)
				syllables1 = KoelnerPhonetik.Syllables(w1);
			var syllables2 = KoelnerPhonetik.Syllables(w2);
			
			if (syllables1.Count > 1 && syllables2.Count == 1)
				return false;



			string ending1 = "";
			string ending2 = "";
			if (syllables1.Count == 1)
			{
				ending1 = syllables1[syllables1.Count - 1];
				ending2 = syllables2[syllables2.Count - 1];

				if (w2.Length <= w1.Length + 2 && ending1 == ending2 && !String.Equals(w1, w2, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
				return false;
			}
			else if (syllables1.Count == 2 && syllables2.Count <= 3 && (w1.Length == w2.Length))
			{
				ending1 = w1.Substring(1);
				ending2 = w2.Length <= ending1.Length ? w2 : w2.Substring(w2.Length - ending1.Length);
			}
			else if (syllables1.Count == 2 && syllables2.Count <= 3)
			{
				ending1 = w1;
				ending2 = w2.Length <= ending1.Length ? w2 : w2.Substring(w2.Length - ending1.Length);
			}
			else
			{

				int minSyllables = Math.Min(syllables1.Count, syllables2.Count);
				int diff = Math.Abs(syllables1.Count - syllables2.Count);

				if (diff > 1)
				{
					return false;
				}
				else
				{
					if (diff == 0)
						minSyllables -= 1;
					for (int s = 0; s < minSyllables; s++)
					{ 
						ending1 = syllables1[syllables1.Count - s - 1] + ending1;
						ending2 = syllables2[syllables2.Count - s - 1] + ending2;
					}
				}


//				if (syllables1.Count > 3 && syllables2.Count > 3){
//					ending1 = syllables1[syllables1.Count-3] + syllables1[syllables1.Count-2] + syllables1[syllables1.Count-1];
//					ending2 = syllables2[syllables2.Count-3] + syllables2[syllables2.Count-2] + syllables2[syllables2.Count-1];
//					
//				}
//				else {
//					ending1 = syllables1[syllables1.Count-2] + syllables1[syllables1.Count-1];
//					ending2 = syllables2[syllables2.Count-2] + syllables2[syllables2.Count-1];
//				}
				
			}
			
			if (ending1 == w1 || ending1 == w2 || ending2 == w1 || ending2 == w2)
				return false;
			
			if (ending1 == ending2 && !String.Equals(w1, w2, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
			return false;
			
//			string phon1 = KoelnerPhonetik.ConvertToColognePhoneticCode( ending1);
//			string phon2 = KoelnerPhonetik.ConvertToColognePhoneticCode( ending2);
//			
//			//if (phon1.Length<2 || phon2.Length<2) return false;
//			
//			if (phon1 == phon2 && w1!=w2) return true;
//			
//			return false;
		}

	}

	[System.Serializable]
	public class LastWordListContainer
	{
		public string lastWord;
		public List<int> sentences;
	}

}

