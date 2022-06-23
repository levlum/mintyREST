using System;
using System.Linq;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{
	

	public class MintyEnums
	{

	#if UNITY_5_3_OR_NEWER
		public static BiDictionary<string, TokenType> Tokens = new BiDictionary<string, TokenType>
#else
		public static Dictionary<string, TokenType> Tokens = new Dictionary<string, TokenType>
#endif
		 {
			{"[sex", TokenType.SEX},
			{"[nameMe]", TokenType.NAME_ME},
			{"[nameYou]", TokenType.NAME_YOU},
			{"[nameOther]", TokenType.NAME_OTHER},
			{"[words/", TokenType.WORDS},
			{"[word(", TokenType.WORDX},
			{"[subject", TokenType.SUBJECT},
			{"[object", TokenType.OBJECT},
			{"[adjective", TokenType.ADJECTIVE},
			{"[adverb", TokenType.ADVERB},
			{"[verb", TokenType.VERB},
			{"[topic", TokenType.TOPIC},
			{"[sentence", TokenType.SENTENCE},
			{"[?", TokenType.RANDOM},
			{"[Genus(", TokenType.SWITCH_GENUS},
			{"[Numerus(", TokenType.SWITCH_NUMERUS},
			{"[Casus(", TokenType.SWITCH_CASUS},
			{"[Reflexive(", TokenType.SWITCH_REFLEXIVE},
			{"[lastOf(", TokenType.SWITCH_LAST_OF_X},
			{"[Up", TokenType.UPPER},
			{"[Low", TokenType.LOWER},
			{"[invisible", TokenType.INVISIBLE},
			{"[PREDEFINED]", TokenType.PREDEFINED }
		};

		private static Dictionary<TokenType, string> tokensReverse = null;
		public static Dictionary<TokenType, string> TokensReverse 
		{
			get {
				if (tokensReverse == null)
				{
					tokensReverse = new Dictionary<TokenType, string>();
					foreach (var strToken in Tokens)
					{
						tokensReverse.Add(strToken.Value, strToken.Key);
					}
				}
				return tokensReverse;
			}

		}

		/// <summary>
		/// Tries to find token in teh beginning of text
		/// </summary>
		/// <returns><c>true</c>, if get token was tryed, <c>false</c> otherwise.</returns>
		/// <param name="longTokenText">Long token text.</param>
		/// <param name="token">Token.</param>
		public static bool TryGetToken(string longTokenText, out TokenType token)
		{
			token = Tokens.FirstOrDefault((p) => longTokenText.StartsWith(p.Key, StringComparison.CurrentCulture)).Value;
			return token != default(TokenType);
		}

		public static T Get<T>(string stringValue) where T:struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				Logger.DebugL(typeof(T), "Type must be a grammatical Enum");
			}

			try
			{
				return (T)Enum.Parse(typeof(T), stringValue);
			}
			catch
			{
				return default(T);
			}
		}


		//
		//		[Obsolete("use generic Get<T>(string stringVal)")]
		//		public static Genus GetGenus(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "FEMININUM":
		//					return Genus.FEMININUM;
		//				case "MASCULINUM":
		//					return Genus.MASCULINUM;
		//				case "NEUTRUM":
		//					return Genus.NEUTRUM;
		//			}
		//			return Genus.UNDEFINED;
		//		}
		//
		//		public static Casus GetCasus(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "NOMINATIV":
		//					return Casus.NOMINATIV;
		//				case "GENETIV":
		//					return Casus.GENETIV;
		//				case "DATIV":
		//					return Casus.DATIV;
		//				case "AKKUSATIV":
		//					return Casus.AKKUSATIV;
		//			}
		//			return Casus.UNDEFINED;
		//		}
		//
		//		public static Numerus GetNumerus(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "PLURAL":
		//					return Numerus.PLURAL;
		//				case "SINGULAR":
		//					return Numerus.SINGULAR;
		//			}
		//			return Numerus.UNDEFINED;
		//		}
		//
		//		public static WordType GetWordType(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "ADJEKTIV":
		//					return WordType.ADJEKTIV;
		//				case "ADVERB":
		//					return WordType.ADVERB;
		//				case "NOWORD":
		//					return WordType.NOWORD;
		//				case "SUBSTANTIV":
		//					return WordType.SUBSTANTIV;
		//				case "VERB":
		//					return WordType.VERB;
		//			}
		//			return WordType.UNDEFINED;
		//		}
		//
		//		public static TextMeaning GetTextMeaning(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "ANSWER":
		//					return TextMeaning.ANSWER;
		//				case "HEADLINE":
		//					return TextMeaning.HEADLINE;
		//				case "QUESTION":
		//					return TextMeaning.QUESTION;
		//				case "STATEMENT":
		//					return TextMeaning.STATEMENT;
		//				case "SUBORDINATE":
		//					return TextMeaning.SUBORDINATE;
		//			}
		//			return TextMeaning.UNDEFINED;
		//		}
		//
		//		public static ActorType GetActorType(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "CAMERA":
		//					return ActorType.CAMERA;
		//				case "OTHER":
		//					return ActorType.OTHER;
		//				case "PROTAGONIST":
		//					return ActorType.PROTAGONIST;
		//				case "SECOND":
		//					return ActorType.SECOND;
		//			}
		//			return ActorType.UNDEFINED;
		//		}
		//
		//		public static Personality GetPersonality(string stringValue)
		//		{
		//			switch (stringValue)
		//			{
		//				case "NERD":
		//					return Personality.NERD;
		//				case "PRINCESS":
		//					return Personality.PRINCESS;
		//				case "PROLETARIAN":
		//					return Personality.PROLETARIAN;
		//			}
		//			return Personality.UNDEFINED;
		//		}
		//
		//		public static ComicTopic GetTopic(string stringValue)
		//		{
		//
		//			foreach (object topic in PMEnum.GetValues (typeof (ComicTopic)))
		//			{
		//				if (topic.ToString().Equals(stringValue.ToUpper()))
		//					return (ComicTopic)topic;
		//			}
		//			return ComicTopic.UNDEFINED;
		//		}
		//
		//		public static TextFlag GetFlag(string stringValue)
		//		{
		//
		//			switch (stringValue)
		//			{
		//				case "NO_ARTICLE":
		//					return TextFlag.NO_ARTICLE;
		//				case "SINGLE":
		//					return TextFlag.SINGLE;
		//			}
		//			return TextFlag.UNDEFINED;
		//		}
		//
		
		public static string EmergencyRelation(RelationBaseType baseType)
		{
			switch (baseType)
			{
				case RelationBaseType.ADJECTIVE:
					return "adjective";
				case RelationBaseType.OBJECT:
					return "substantiv/thing";
				case RelationBaseType.SUBJECT:
					return "substantiv/person";
				case RelationBaseType.VERB:
					return "verb";
				case RelationBaseType.ADVERB:
					return "adverb";
				default:
					return "";
			}
		}
	}
	
#region enums
	
#region RelationType
	public enum RelationBaseType
	{
		UNDEFINED,
		SUBJECT,
		OBJECT,
		ADJECTIVE,
		VERB,
		ADVERB
	}

#endregion

	public enum DeclinationType
	{
		UNDEFINED,
		STRONG,
		WEAK,
		MIXED
	}

#region ARTICLE_TYPE
	public enum Article_Type
	{
		UNDEFINED_VALUE,
		/** the girls*/
		DEFINED_ARTICLE,
		/** [a] girl */
		UNDEFINED_ARTICLE,

		/// <summary>
		/// like for the word "god".
		/// </summary>
		NO_ARTICLE,
	}
#endregion

#region Genus
	public enum Genus
	{
		UNDEFINED = 0,
		MASCULINUM = 1,
		FEMININUM = 2,
		NEUTRUM = 3
	}
#endregion

#region Casus
	public enum Casus
	{
		UNDEFINED = 0,
		NOMINATIV = 1,
		GENETIV = 2,
		DATIV = 3,
		AKKUSATIV = 4
	}
#endregion

#region Numerus
	public enum Numerus
	{
		UNDEFINED = 0,
		SINGULAR = 1,
		PLURAL = 2
	}
#endregion

#region Person
	public enum Person
	{
		UNDEFINED = 0,
		P1 = 1,
		P2 = 2,
		P3 = 3
	}
#endregion
	
#region Tempus
	public enum Tempus
	{
		UNDEFINED,
		PAST,
		PRAESENS,
		FUTUR,
		INFINITIV,
		IMPERATIV,
		HELPER
	}
#endregion
	
	
#region Form
	public enum Form
	{
		UNDEFINED,
		HELPER,
		INFINITIV,
		/// <summary>
		/// word stem, radical (wortstamm)
		/// </summary>
		STEM
	}
#endregion
	
#region Komparation (Steigerungsform)
	public enum Komparation
	{
		UNDEFINED,
		Positiv,
		Komparativ,
		Superlativ,
		Elativ
	}
#endregion

#region WordType
	public enum WordType
	{
		UNDEFINED,
		SUBSTANTIV,
		VERB,
		ADJEKTIV,
		ADVERB,
		ARTICLE,
		PRONOMEN_PERSONAL,
		PRONOMEN_REFLEXIV,
		NOWORD
	}
#endregion

#region VerbCategory
	public enum VerbCategory
	{
		UNDEFINED,
		REFLEXIV,
		REFLEXIV_ONLY
	}
#endregion


	public enum TextPosition
	{
		LINE_START,
		NOT_LINE_START,
		LINE_END,
		NOT_LINE_END,
		UNDEFINED
	}

	[Flags]
	public enum TextFlag
	{
		UNDEFINED = 0,
		/// <summary>
		/// no other relation of the same type (object, subject, ..) is allowd in a sentence.
		/// </summary>
		SINGLE = 1,
		/// <summary>
		/// usage count of a sentence with this flag will not increase
		/// </summary>
		REUSABLE = 2,
		/// <summary>
		/// no singular for this word. like "die Leute"
		/// </summary>
		NO_SINGULAR = 4,
		/// <summary>
		/// no plural for this word. like "vernunft"
		/// </summary>
		NO_PLURAL = 8,
		/// <summary>
		/// no comparative for this (adverb). like "deshalb", "gestern", ...
		/// </summary>
		NO_COMPARATION = 16
		//NEXT2_FLAG = 32,
	}

	[Flags]
	public enum TextMeaning
	{
		UNDEFINED = 0,
		QUESTION = 1,
		ANSWER = 2,
		STATEMENT = 4,
		HEADLINE = 8,
		SUBORDINATE = 16,
		NARRATION = 32
	}

#region ActorType
	public enum ActorType
	{
		UNDEFINED,
		PROTAGONIST,
		SECOND,
		OTHER,
		CAMERA
	}
#endregion

#region Personality
	public enum Personality
	{
		UNDEFINED,
		PRINCESS,
		NERD,
		PROLETARIAN
	}
#endregion



	public enum TokenType
	{
		undefined,
		SEX,
		NAME_ME,
		NAME_YOU,
		NAME_OTHER,
		WORDS,
		WORDX,
		SUBJECT,
		OBJECT,
		VERB,
		ADJECTIVE,
		ADVERB,
		TOPIC,
		SENTENCE,
		RANDOM,
		SWITCH_GENUS,
		SWITCH_NUMERUS,
		SWITCH_CASUS,
		SWITCH_REFLEXIVE,
		SWITCH_LAST_OF_X,
		UPPER,
		LOWER,
		INVISIBLE,
		PREDEFINED
	};
}

#endregion