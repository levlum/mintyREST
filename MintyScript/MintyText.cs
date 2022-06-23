using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

#if UNITY_5_3_OR_NEWER
using UnityEngine;

#endif


namespace Com.Gamegestalt.MintyScript
{

	#region Selectable Text
	[System.Serializable]
	public class MintyText : IComparable, IEquatable<MintyText>, IGrammatical, IWeightable
	{
		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		
		protected string text;

		[System.NonSerialized]
		public string
			last_processing_result;
		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		public int
			num = -1;
		//unique number for every word or sentence given by import
		public int processedLength = -1;
		

		//		public ComicTopicEnumList topics;
		//[System.NonSerialized]
		public TopicFlags topics;
		public List<string> responseTypes;
		public TextMeaning meaning;
		public int sequence = -1;
		public string id;
		public ActorType speaker = ActorType.UNDEFINED;
		public Personality personality = Personality.UNDEFINED;
		public Genus genus = Genus.UNDEFINED;
		public Numerus numerus = Numerus.UNDEFINED;
		public Casus casus = Casus.UNDEFINED;
		public Tempus tempus = Tempus.UNDEFINED;
		public Person person = Person.UNDEFINED;
		public Komparation komparation = Komparation.UNDEFINED;
		public DeclinationType declinationType = DeclinationType.UNDEFINED;
		public TextFlag flags = TextFlag.UNDEFINED;

		public WordType wordType = WordType.UNDEFINED;
		public VerbCategory verbCategory = VerbCategory.UNDEFINED;
		public Article_Type article_type = Article_Type.UNDEFINED_VALUE;
		public TextPosition textPosition = TextPosition.UNDEFINED;

		public BubbleType bubbleType;
		public float weight = 1f;
		public Interval like;
		public List<SymbolType> symbols = null;
		public float complexity;

		public bool has_saved_some_parseInfos = false;

		//how many variables does the sentence or word have, ...
		public string GetKey()
		{
			return Text;
		}

		#region IGrammatical implementation

		public T Get<T>() where T:struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				Logger.DebugL(this, "Type must be a grammatical Enum");
			}
			if (typeof(T) == typeof(Genus))
				return (T)((System.Object)genus);
			if (typeof(T) == typeof(Numerus))
				return (T)((System.Object)numerus);
			if (typeof(T) == typeof(Casus))
				return (T)((System.Object)casus);
			if (typeof(T) == typeof(Person))
				return (T)((System.Object)person);
			if (typeof(T) == typeof(Komparation))
				return (T)((System.Object)komparation);
			if (typeof(T) == typeof(Tempus))
				return (T)((System.Object)tempus);
			if (typeof(T) == typeof(TextFlag))
				return (T)((System.Object)flags);
			if (typeof(T) == typeof(WordType))
				return (T)((System.Object)wordType);
			if (typeof(T) == typeof(Article_Type))
				return (T)((System.Object)article_type);
			if (typeof(T) == typeof(VerbCategory))
				return (T)((System.Object)verbCategory);
			if (typeof(T) == typeof(TextPosition))
				return (T)((System.Object)textPosition);
			if (typeof(T) == typeof(DeclinationType))
				return (T)((System.Object)declinationType);

			return default(T);
		}

		//		public Genus GetGenus()
		//		{
		//			return genus;
		//		}
		//
		//		public Numerus GetNumerus()
		//		{
		//			return numerus;
		//		}
		//
		//		public Casus GetCasus()
		//		{
		//			return casus;
		//		}
		//
		//		public Person GetPerson()
		//		{
		//			return person;
		//		}
		//
		//		public Komparation GetKomparation()
		//		{
		//			return komparation;
		//		}
		//
		//		public Tempus GetTempus()
		//		{
		//			return tempus;
		//		}
		//
		//		public TextFlag GetFlags()
		//		{
		//			return flags;
		//		}
		//
		//		public WordType GetWordType()
		//		{
		//			return wordType;
		//		}
		//
		//		public Article_Type GetArticle_Type()
		//		{
		//			return article_type;
		//		}
		//
		//		public VerbCategory GetVerbCategory()
		//		{
		//			return verbCategory;
		//		}
		//
		//		public TextPosition GetTextPosition()
		//		{
		//			return textPosition;
		//		}

		#endregion

		public TextPattern patternForNextProcessing = null;
		public TextPattern patternLastProcessing = null;

		public void AddConditionForNextProcessing(params PatternCondition[] conditions)
		{
			if (conditions != null && conditions.Length > 0)
			{
				if (patternForNextProcessing == null)
					patternForNextProcessing = new TextPattern(conditions);
				else
					patternForNextProcessing.AddPatternConditions(conditions);
			}
		}

		public void SetPatternForNextProcessing(TextPattern pattern)
		{
			patternForNextProcessing = pattern;
		}

		public virtual void Reset()
		{
			patternForNextProcessing = null;
		}
			
		//		[System.NonSerialized]
		//		public ComicTopic currentTopic = ComicTopic.UNDEFINED;
		[System.NonSerialized]
		public int usageCount = 0;

		[System.NonSerialized]
		protected ParseTreeNode
			parseTree;

		/// <summary>
		/// parse the sentence if it has not been parsed yet
		/// </summary>
		/// <param name="parseAgain">If set to <c>true</c> parse again.</param>
		public virtual void DoParse(ParseInfo parseInfo = ParseInfo.IF_NO_PARSETREE_PRESENT)
		{
			bool doParse = parseTree == null
				|| parseInfo == ParseInfo.ALLWAYS
				|| (parseInfo == ParseInfo.ONLY_IF_NO_SENTENCE_INFO && !has_saved_some_parseInfos);

			
			if (doParse)
			{
				Logger.DebugL(this, "create new parse tree for: " + Text);
				parseTree = TextProcessor.CreateParseTree(this);
				if (parseTree == null)
				{
					parseTree = new ParseTreeNode(ParseNodeType.TEXT, Text, null);
					parseTree.cText = this;
				}
				has_saved_some_parseInfos = true;
			}
		}


		public virtual ParseTreeNode ParseTree
		{
			get
			{ 

				return parseTree; 
			}
			set { parseTree = value; }
		}

		//public MintyText()
		//{
		//}



		public override string ToString()
		{
			return Text;
		}

		public virtual int CompareTo(object other)
		{
			if (other == null || this.GetType() != other.GetType())
				return 1;
			//return other.GetHashCode().CompareTo (GetHashCode());
			return num.CompareTo(((MintyText)other).num);
		}

		public override int GetHashCode()
		{
			//todo: make distinction between grammatically different but text equal words
			int hash = 23;

			hash = hash * 31 + num;
			if (num >= 0)
				return hash;


			hash = hash * 31 + topics.GetHashCode();
			//hash = hash * 31 + sameResponseTypes
			//hash = hash * 31 + sym
			hash = hash * 31 + (text==null? 0: text.GetHashCode());
			hash = hash * 31 + (int)meaning;
			hash = hash * 31 + sequence;
			hash = hash * 31 + (id == null ? 0 : id.GetHashCode());
			hash = hash * 31 + (int)speaker;
			hash = hash * 31 + (int)personality;
			hash = hash * 31 + (int)genus;
			hash = hash * 31 + (int)numerus;
			hash = hash * 31 + (int)casus;
			hash = hash * 31 + (int)declinationType;
			hash = hash * 31 + (int)komparation;
			hash = hash * 31 + (int)verbCategory;
			hash = hash * 31 + (int)person;
			hash = hash * 31 + (int)tempus;
			hash = hash * 31 + (int)bubbleType;
			hash = hash * 31 + weight.GetHashCode();
			return hash;
		}

		public override bool Equals(object other)
		{
			if (other == null || this.GetType() != other.GetType())
			{
				return false;
			}
			else
			{
				return Equals((MintyText)other);
			}
		}

		public virtual bool Equals(MintyText other)
		{

			if (other == null || this.GetType() != other.GetType())
			{
				return false;
			}
			else
			{
				if (num >= 0 && ((MintyText)other).num >= 0)
					return num == ((MintyText)other).num;


				//MintyText st = (MintyText)other;
				bool sameTopics = other.topics == topics;

				bool sameResponseTypes = responseTypes != null && other.responseTypes != null && responseTypes.Count == other.responseTypes.Count;
				if (sameResponseTypes)
					for (int i = 0; i < responseTypes.Count; i++)
					{
						if (responseTypes[i] != other.responseTypes[i])
						{
							sameResponseTypes = false;
							break;
						}
					}
				sameResponseTypes = sameResponseTypes || (responseTypes == null && other.responseTypes == null);
				bool sameSymbols = symbols != null && other.symbols != null && symbols.Count == other.symbols.Count;
				if (sameSymbols)
					for (int i = 0; i < symbols.Count; i++)
					{
						if (symbols[i] != other.symbols[i])
						{
							sameSymbols = false;
							break;
						}
					}
				sameSymbols = sameSymbols || (symbols == null && other.symbols == null);
					
				return sameTopics
				&& sameResponseTypes
				&& sameSymbols
				&& text == other.text
				&& meaning == other.meaning
				&& sequence == other.sequence
				&& id == other.id
				&& speaker == other.speaker
				&& personality == other.personality
				&& genus == other.genus
				&& numerus == other.numerus
				&& casus == other.casus
				&& declinationType == other.declinationType
				&& komparation == other.komparation
				&& verbCategory == other.verbCategory
				&& person == other.person
				&& tempus == other.tempus
				&& bubbleType == other.bubbleType
				&& float.Equals (weight, other.weight);
					
			}
		}

		public virtual string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}

		public virtual void CopyValues(MintyText original)
		{
			text = original.text == null ? null : String.Copy(original.text);
			num = original.num;
				
			if (original.patternForNextProcessing != null)
			{
				patternForNextProcessing = (TextPattern)original.patternForNextProcessing.Clone();
			}

			topics = new TopicFlags(original.topics.FlagsInt);
			//if (original.topics != null)
			//{
			//	//				topics = new ComicTopicEnumList();
			//	topics = new List<ComicTopic>();
			//	topics.AddRange(original.topics);
			//}
			if (original.responseTypes != null)
			{
				responseTypes = new List<string>();
				responseTypes.AddRange(original.responseTypes);
			}
			if (original.symbols != null)
			{
				symbols = new List<SymbolType>();
				symbols.AddRange(original.symbols);
			}

			flags = original.flags;
			meaning = original.meaning;
			sequence = original.sequence;
			id = original.id == null ? null : String.Copy(original.id);
			speaker = original.speaker;
			personality = original.personality;
			genus = original.genus;
			numerus = original.numerus;
			casus = original.casus;
			bubbleType = original.bubbleType;
			weight = original.weight;
			komparation = original.komparation;

			wordType = original.wordType;
			verbCategory = original.verbCategory;
			article_type = original.article_type;
			declinationType = original.declinationType;
			textPosition = original.textPosition;
				
			//do not delete the following lines. they are just commented out because of a flash-exception (destination array length not fitting)
			//			if (like == null && original.like != null && original.like.Length > 0) {
			//				like = new float[original.like.Length];
			//				Array.Copy(original.like, like, original.like.Length);
			//			}
			complexity = original.complexity;
		}
		//	 public void AddTopic (ComicTopic topic) {
		//	 if (topics == null) topics = new List<ComicTopic>();
		//	 topics.Add (topic);
		//	 currentTopic = topic;
		//	 }
		public void AddResponseType(string responseType)
		{
			if (responseTypes == null)
				responseTypes = new List<string>();
			responseTypes.Add(responseType);
		}

		public static T ErrorText<T>(string hint) where T : MintyText
		{
			MintyText st = null;
			if (typeof(T) == typeof(Word))
				st = new Word("");
			else if (typeof(T) == typeof(Sentence))
				st = new Sentence();
			else
				st = new MintyText();
			st.Text = "Eeep. (" + hint + ")";
			st.num = Constants.ERROR_TEXT;
			return (T)st;
		}

		public float GetWeight()
		{
			return weight;
		}
	}
	#endregion




	public enum ParseInfo
	{
		IF_NO_PARSETREE_PRESENT,
		ALLWAYS,
		ONLY_IF_NO_SENTENCE_INFO
	}

}

