using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using Com.Gamegestalt.MintyScript;

#endif


namespace Com.Gamegestalt.MintyScript
{

	[System.Serializable]
	public class Sentence : MintyText
	{
		public List<string> storyNames;
		public int importCounter = 0;

		public bool IsPartOfStory { get { return storyNames != null && storyNames.Count > 0; } }

		public List<string> responses;
		public TopicFlags inlineTopics;

		//		[System.NonSerialized]
		//		private ProcessedText lastProcessingInfo;
		//		public ProcessedText LastProcessingInfo {
		//			get { return lastProcessingInfo; }
		//			set { lastProcessingInfo = value; }
		//		}


		/// <summary>
		/// parse the sentence if it has not been parsed yet
		/// </summary>
		/// <param name="parseInfo">If set to <c>true</c> parse again.</param>
		override public void DoParse(ParseInfo parseInfo = ParseInfo.IF_NO_PARSETREE_PRESENT)
		{
			base.DoParse(parseInfo);


			//save processing info in Sentence
			//						System.DateTime now = System.DateTime.Now;
			inlineTopics = parseTree.topics;

			complexity = parseTree.Complexity; 
			if (parseTree.words != null && parseTree.words.Count > 0)
			{
				if (_serializedWords == null)
				{
					_serializedWords = new List<int>();
				}
				_serializedWords.Clear();

				foreach (var syntagmaWords in parseTree.words.Values)
				{
					foreach (Word w in syntagmaWords)
					{
						_serializedWords.Add(w.num);
					}
				}
			}

			//						if ((System.DateTime.Now-now).TotalMilliseconds > 2) {
			//							Logger.DebugL (this, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for saving  of \""+Text+"\"\n");
			//						}
		}


		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		private List<int> _serializedWords;




		//		[System.NonSerialized]
		//		private List<Word> wordsToUse; //set this for the next processing
		//		public List<Word> WordsToUse {
		//			get { return wordsToUse; }
		//			set { wordsToUse = value; }
		//		}
		//all used words in this sentence in the last processing

		private HashSet<Word> _words;

		public HashSet<Word> Words
		{
			get
			{
				if (_words == null && _serializedWords != null && _serializedWords.Count > 0)
				{
					_words = new HashSet<Word>();
					foreach (int wNum in _serializedWords)
					{
						if (wNum >= 0 && wNum < MintyTextsContainer.Instance.words.Count)
						{
							Word w = MintyTextsContainer.Instance.words[wNum];
							_words.Add(w);
						}
						else
						{
							//Word[] allWords = ComicTextsContainer.Get.Words;
							throw new Exception("WRONG word number saved: " + wNum + "\nText: " + Text);
						}
					}
				}
				return _words;
			}
		}

		[System.NonSerialized]
		public List<Word> last_processing_words;

		[System.NonSerialized]
		public Dictionary<string,Word> last_processing_namedWords;

		[System.NonSerialized]
		public Dictionary<string,Word> next_processing_namedWords;

		//		[System.NonSerialized]
		//		public Sentence
		//			next_processing_sentence;
			
		/// <summary>
		/// list of all used (right side) variable names, that are not set in the sentence itself
		/// </summary>
		public List<string> dependencyList;
		/// <summary>
		/// list of all variables names (left side), that are set in this sentence
		/// </summary>
		public List<string> variableNames;

		override public void Reset()
		{
			base.Reset();
			//last_processing_words = null;
			//last_processing_namedWords = null;

			next_processing_namedWords = null;
			if (parseTree != null)
			{
				parseTree.Reset();
			}
//			next_processing_sentence = null;
		}

					

		public List<TextPattern> lastWordPattern = null;


		public Sentence()
		{
		}

		public bool HasResponse
		{
			get { return (responses != null && responses.Count > 0); }	
		}

		/**
			 * returns the processed text but does not store the result in the Sentence!
			 * **/
		public string Process(ICharacterData me, ICharacterData you, ICharacterData other, bool allSyntagmas = false)
		{
			TextPattern pattern = null;


			last_processing_words = null;
			last_processing_namedWords = null;

			//adress special words in the processed sentence?
			if (patternForNextProcessing != null)
			{
				bool needsWordTopic = patternForNextProcessing.PossibleTopics != default(TopicFlags) 
					&& (topics & patternForNextProcessing.PossibleTopics).IsEmpty();
				//if (!patternForNextProcessing.PossibleTopics.IsEmpty())
				//{
				//	needsWordTopic = true;
				//	if (!(topics & patternForNextProcessing.PossibleTopics).IsEmpty())
				//	{
				//		needsWordTopic = 
				//	}

				//	if (topics != null)
				//	{
				//		foreach (ComicTopic top in patternForNextProcessing.PossibleTopics)
				//		{
				//			if (topics.Contains(top))
				//			{
				//				needsWordTopic = false;
				//				break;
				//			}
				//		}
				//	}
				//}
					
				if (needsWordTopic || patternForNextProcessing.WordsToInclude.Count > 0)
				{
					pattern = new TextPattern();
					if (needsWordTopic)
						pattern.AddPatternConditions(new TopicCondition( patternForNextProcessing.PossibleTopics));
					if (patternForNextProcessing.WordsToInclude.Count > 0)
						pattern.AddConditionsWords(patternForNextProcessing.WordsToInclude);
				}
			}
				
			string result = TextProcessor.Process(this, pattern, me, you, other, allSyntagmas);
				
			//reset for next processing
			patternLastProcessing = patternForNextProcessing;
			patternForNextProcessing = null;
				
			return result;
		}

		public Sentence Response()
		{
			return Response(null);
		}

		public Sentence Response(TextPattern pattern)
		{
			if (pattern == null)
			{
				pattern = new TextPattern();
			}
			//exclude the sentence itself, ...
			pattern.AddPatternConditions(new SentenceCondition(this, Constants.MATCH_PERFECT, true));
				
			if (responses == null || responses.Count == 0)
			{
				pattern.AddPatternConditions(
					new TextMeaningCondition(TextMeaning.STATEMENT)
				);
			}
			else
			{
				pattern.AddConditionsResponseType(responses, Constants.MATCH_SUPER);
			}
				
			if (patternLastProcessing != null)
			{
				//pattern.Concatenate (patternLastProcessing, Constants.MATCH_MINIMAL);
			}
				
			pattern.AddPatternConditions(new VariableDependencyCondition(variableNames));
			Sentence response = MintyTextsContainer.Instance.GetRandomSentence(pattern);
			response.next_processing_namedWords = last_processing_namedWords;
//			response.next_processing_sentence = this;
			return response;
		}

		public Sentence Question()
		{
			return Question(null);
		}

		public Sentence Question(TextPattern pattern)
		{
			if (pattern == null)
			{
				pattern = new TextPattern(
					new SentenceCondition(this, Constants.MATCH_PERFECT, true)
				);
					
			}
			if (responseTypes == null || responseTypes.Count == 0)
			{
				pattern.AddPatternConditions(
					new TextMeaningCondition(TextMeaning.QUESTION)
				);
			}
			else
			{
				pattern.AddConditionsResponse(responseTypes, Constants.MATCH_SUPER);
			}
				
				
			if (patternLastProcessing != null)
			{
				pattern.Concatenate(patternLastProcessing, Constants.MATCH_MINIMAL);
			}
				
			//if (speaker != null) pattern.personality = speaker.Personality;
			return MintyTextsContainer.Instance.GetRandomSentence(pattern);
		}

		public override void CopyValues(MintyText original)
		{
			base.CopyValues(original);
			if (original is Sentence)
			{
				Sentence origSentence = (Sentence)original;
				storyNames = origSentence.storyNames == null ? null : new List<string>(origSentence.storyNames.ToArray());
				importCounter = origSentence.importCounter;
				responses = origSentence.responses == null ? null : new List<string>(origSentence.responses.ToArray());

				//inlineTopics: parse this sentence!
				//Words: parse this sentence!
			}
		}
	}

}