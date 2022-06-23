using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
	[System.Serializable]
	public abstract class PatternCondition : IEquatable<PatternCondition>
	{

		public float impact = Constants.MATCH_GOOD;
		public bool not = false;

		/// <summary>
		/// if this is false, match values will not increase if there is allready one match of a type
		/// if true, match value will be increased for every positive match condition
		/// example: ResponseTypeCondition is no better match if two responseTypes match. one is ok.
		/// </summary>
		public bool multipleMatches = false;

		/// <summary>
		///  if false, a condition will not be matched with a text (it just holds some special information)
		/// </summary>
		public bool useForMatchValue = true;



		public ConditionValueType valueType = ConditionValueType.MULTI;

		/// <summary>
		/// if a condition is "excluding", it excludes sentences or words that does not match. 
		/// if it is not excluding, it just changes the probability value
		/// </summary>
		public bool excluding = true;

		public PatternCondition()
		{
		}

		public PatternCondition(float impact, bool not)
		{
			this.impact = impact;
			this.not = not;
		}

		
		public abstract float MatchValue(MintyText selText);
		//		{
		//			Logger.LogError(this, "Use just ableitungen von PatternCondition!");
		//			return 0f;
		//		}

		public virtual bool Equals(PatternCondition otherCondition)
		{
			if (otherCondition == null || this.GetType() != otherCondition.GetType())
			{
				return false;
			}
			else
			{
//				bool impactOK = this.impact == (otherCondition).impact;
				bool notOK = not == otherCondition.not;
				bool multiMatchOK = multipleMatches == otherCondition.multipleMatches;
				bool matchValOK = useForMatchValue == otherCondition.useForMatchValue;
				bool formOK = IsFormCondition == otherCondition.IsFormCondition;
				bool singleOK = valueType == otherCondition.valueType;
				bool allOK = /*impactOK &&*/ notOK && multiMatchOK && matchValOK && formOK && singleOK;
				return allOK;
			}
		}

		#if UNITY_5_3_OR_NEWER
		public virtual string ToJson()
		{
			return JsonUtility.ToJson(this);
		}

		public virtual void FromJsonOverwrite(string jsonString)
		{
			try
			{
				JsonUtility.FromJsonOverwrite(jsonString, this);
			}
			catch (ArgumentException e)
			{
				Logger.LogError(this, jsonString, e);
			}
		}
		#endif

		public abstract string StringValue();

		public override string ToString()
		{
			return string.Format("[{0} = {1}{2}]", this.GetType().Name, not ? "!" : "", StringValue());
		}

		/// <summary>
		/// condition is not used for selection but for creating a special form of a word
		/// the condition does not change the "type" of word but its form
		/// </summary>
		public virtual bool IsFormCondition
		{
			get
			{
				return 
				//	this is EnumCondition<TextFlag>
				this is EnumCondition<Komparation>
				|| this is EnumCondition<Casus>
				|| this is EnumCondition<Numerus>
				|| this is EnumCondition<Tempus>
				|| this is EnumCondition<Form>
				|| this is EnumCondition<Person>;
			}
		}
	}



	public class EnumCondition<T> : PatternCondition, ISingleValue
		where T:struct, IConvertible
	{
		public T enumValue;

		protected EnumCondition()
		{
		}

		public EnumCondition(T enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public EnumCondition(T enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public EnumCondition(T enumCondition, float impact, bool not)
			: base(impact, not)
		{
			if (!typeof(T).IsEnum)
			{
				Logger.LogError(this, "generic parameter of EnumCondition must be of type Enum.");
			}
			enumValue = enumCondition;
			if (typeof(T).IsDefined(typeof(FlagsAttribute), false))
			{
				valueType = ConditionValueType.FLAGS;
			}
			else
			{
				valueType = ConditionValueType.SINGLE;
			}
		}


		public override float MatchValue(MintyText selText)
		{
			if (valueType == ConditionValueType.FLAGS)
			{
				if (selText == null || enumValue.Equals(default(T)))
					return 0;

				int textIntVal = (int)Convert.ChangeType(selText.Get<T>(), Enum.GetUnderlyingType(typeof(T)));
				int intVal = (int)Convert.ChangeType(enumValue, Enum.GetUnderlyingType(typeof(T)));
				return (((textIntVal & intVal) == intVal) != not) ? impact : -impact;
			}
			else if (!GrammaUtil.IsGrammatical(enumValue))
			{
				Logger.LogError(this, "MatchValue works with grammatical Enums only.");
				return 0;
			}

			if (selText == null || enumValue.Equals(default(T)) || selText.Get<T>().Equals(default(T)))
				return 0;
			return ((selText.Get<T>().Equals(enumValue)) != not) ? impact : -impact;
		}

		public object SingleValue
		{
			get{ return enumValue; }
			set{ enumValue = (T)value; }
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			
			return base.Equals(otherCondition)
			&& enumValue.Equals(((EnumCondition<T>)otherCondition).enumValue);
		}

		public override string ToString()
		{
			return string.Format("[{0} = {1}{2}]", typeof(T).Name, not ? "!" : "", StringValue());
		}

		public override string StringValue()
		{
			return enumValue.ToString();
		}
	}



	[System.Serializable]
	public class ExcludeRelationCondition : PatternCondition
	{
		public string relationID;

		protected ExcludeRelationCondition()
		{
		}

		public ExcludeRelationCondition(string relationID)
			: this(relationID, Constants.MATCH_SUPER, false)
		{
		}

		public ExcludeRelationCondition(string relationID, float impact)
			: this(relationID, impact, false)
		{
		}

		public ExcludeRelationCondition(string relationID, float impact, bool not)
			: base(impact, not)
		{
			this.relationID = relationID;
			useForMatchValue = false;
		}


		public TextRelation GetRelation
		{
			get
			{ 
				if (MintyTextsContainer.Instance.NamedRelations.ContainsKey(relationID))
				{
					return MintyTextsContainer.Instance.NamedRelations[relationID]; 
				}
				else
				{
					Logger.LogError(this, "No relation with name=\"" + relationID + "\".");
					return null;
				}
			}
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& relationID.Equals(((ExcludeRelationCondition)otherCondition).relationID);
		}

		public override string StringValue()
		{
			return relationID;
		}

		public override float MatchValue(MintyText selText)
		{
			return 0;
		}
	}

	//	[System.Serializable]
	//	public class FlagsCondition : EnumCondition<TextFlag>
	//	{
	//		public TextFlag flags;
	//
	//		protected FlagsCondition()
	//		{
	//		}
	//
	//		public FlagsCondition(TextFlag flags)
	//			: this(Constants.MATCH_SUPER, false, flags)
	//		{
	//		}
	//
	//		public FlagsCondition(float impact, TextFlag flags)
	//			: this(impact, false, flags)
	//		{
	//		}
	//
	//		public FlagsCondition(float impact, bool not, TextFlag flags)
	//			: base(flags, impact, not)
	//		{
	//			useForMatchValue = false;
	//			isFormCondition = true;
	//		}
	//
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& flags == ((FlagsCondition)otherCondition).flags;
	//		//		}
	//
	//
	//
	//		public override float MatchValue(ComicText selText)
	//		{
	//			return 0;
	//		}
	//
	//
	//
	//		//		public override string StringValue()
	//		//		{
	//		//			return flags.ToString();
	//		//		}
	//	}


	//
	//	[System.Serializable]
	//	public class ContextCondition : PatternCondition {
	//		public IEnumerable<ParseTreeNode> context;
	//		public ParseTreeNode centerNode;
	//		public ContextCondition (IEnumerable<ParseTreeNode> context, ParseTreeNode centerNode):this (context, centerNode, Constants.MATCH_SUPER, false) {}
	//		public ContextCondition (IEnumerable<ParseTreeNode> context, ParseTreeNode centerNode, float impact):this (context, centerNode, impact, false) {}
	//		public ContextCondition (IEnumerable<ParseTreeNode> context, ParseTreeNode centerNode, float impact, bool not):base (impact, not) {
	//			this.context = context;
	//			this.centerNode = centerNode;
	//			multipleMatches = false;
	//			singleValue = true;
	//		}
	//
	//		#if UNITY_5_3_OR_NEWER
	//			public ContextCondition (SerializerInStream stream)
	//		{
	//		Deserialize (stream);
	//		}
	//		override public void Serialize(SerializerOutStream stream) {
	//		stream.WriteStringArray( availableVariables.ToArray());
	//		base.Serialize(stream);
	//		}
	//		override public void Deserialize(SerializerInStream stream) {
	//		availableVariables = new List<string>( stream.ReadStringArray());
	//		base.Deserialize(stream);
	//		}
	//		#endif
	//
	//		/// <summary>
	//		/// returns 0, if at least one relation for a word matches the context.
	//		/// returns -impact, if not.
	//		/// </summary>
	//		/// <returns>
	//		/// The match.
	//		/// </returns>
	//		/// <param name='selText'>
	//		/// Sel text.
	//		/// </param>
	//		public override float MatchValue (ComicText selText)
	//		{
	//			if (selText == null || selText is Sentence) return 0;
	//
	//			if (selText is Word){
	//				bool ok = ComicTextProcessor.TestRelations ((Word)selText, centerNode.name, context);
	//
	//
	////				Word w = (Word)selText;
	////				if (w.textRelations != null) {
	////					List<TextRelation> relations = new List<TextRelation> (w.textRelations.Count);
	////					foreach (int relID in w.textRelations) {
	////						relations.Add (ComicTextsContainer.Get.textRelations [relID]);
	////					}
	////					Dictionary<ParseTreeNode, TextRelation> usageInfo = new Dictionary<ParseTreeNode, TextRelation> ();
	////					foreach (TextRelation tr in relations) {
	////						if (tr.ContextOK (context, relations, ref usageInfo)) {
	////							return (not ? -impact : 0);
	////						}
	////					}
	//				return (!ok || not? -impact : impact);
	////				}
	//			}
	//			return (not ? -impact : 0);
	//		}
	//
	//		public override bool Equals (PatternCondition otherCondition)
	//		{
	//			return base.Equals (otherCondition)
	//				&& context.Equals( ((ContextCondition)otherCondition).context);
	//		}
	//
	//		public override string ToString ()
	//		{
	//			string s = "no context";
	//			if (context != null) {
	//				s = "(";
	//				foreach (ParseTreeNode node in context) {
	//					s += node.ToString ();
	//				}
	//				s += ")";
	//			}
	//			return string.Format ("[{0} = {1}, {2}] ",GetType().Name, centerNode.ToString(), s);
	//		}
	//	}
	//	
	
	[System.Serializable]
	/// <summary>
	/// if a sentence (tested with match) has variables in sentence.dependencyList (all additional necessary variables outside of the sentence)
	/// and those variables are in the condition's availableVariables list: ok match, otherwise not ok.
	/// </summary>
	public class VariableDependencyCondition : PatternCondition
	{
		public HashSet<string> availableVariables;

		protected VariableDependencyCondition()
		{
		}

		public VariableDependencyCondition(IEnumerable<string> availableVariables)
			: this(availableVariables, Constants.MATCH_SUPER, false)
		{
		}

		public VariableDependencyCondition(IEnumerable<string> availableVariables, float impact)
			: this(availableVariables, impact, false)
		{
		}

		public VariableDependencyCondition(IEnumerable<string> availableVariables, float impact, bool not)
			: base(impact, not)
		{
			this.availableVariables = (availableVariables==null || availableVariables.Count()==0)?new HashSet<string>() : new HashSet<string> (availableVariables);
			multipleMatches = true;
			this.valueType = ConditionValueType.SINGLE;
		}

		
		/// <summary>
		/// returns 0 (old: high match), if all variables of sentence.dependencyList are in availableVariables. 
		/// minus match, if there is a variable in sentence.dependencyList but not in availableVariables.
		/// </summary>
		/// <returns>
		/// The match.
		/// </returns>
		/// <param name='selText'>
		/// Sel text.
		/// </param>
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || selText is Word)
				return 0;
			
			if (selText is Sentence)
			{
				Sentence s = (Sentence)selText;
				s.DoParse(ParseInfo.ONLY_IF_NO_SENTENCE_INFO);
				if (s.dependencyList == null || s.dependencyList.Count == 0)
				{
					return 0;
				}
				else
				{
					foreach (string varName in s.dependencyList)
					{
						if (availableVariables == null || !availableVariables.Contains(varName))
							return (not ? impact : -impact);
					}
				}
			}
			
			return (not ? -impact : 0);
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& availableVariables.Equals(((VariableDependencyCondition)otherCondition).availableVariables);
		}

		public override string StringValue()
		{
			return availableVariables == null ? "no variables" : String.Join(",", availableVariables.ToArray());
		}

		#if UNITY_5_3_OR_NEWER
		public override string ToJson()
		{
			string jsonString = base.ToJson();
			string listString = string.Join(",", availableVariables.ToArray());
			return jsonString;
		}
		#endif
	}

	
	[System.Serializable]
	public class DefinesVariableCondition : PatternCondition
	{
		public List<string> variables;

		protected DefinesVariableCondition()
		{
		}

		public DefinesVariableCondition(List<string> variables)
			: this(variables, Constants.MATCH_SUPER, false)
		{
		}

		public DefinesVariableCondition(List<string> variables, float impact)
			: this(variables, impact, false)
		{
		}

		public DefinesVariableCondition(List<string> variables, float impact, bool not)
			: base(impact, not)
		{
			this.variables = variables;
			multipleMatches = true;
		}

		
		/// <summary>
		/// high match if all variables are in the sentence.variableNames list.
		/// </summary>
		/// <returns>
		/// The value.
		/// </returns>
		/// <param name='selText'>
		/// Sel text.
		/// </param>
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || variables == null || variables.Count == 0 || selText is Word)
				return 0;
			
			if (selText is Sentence)
			{
				Sentence s = (Sentence)selText;
				s.DoParse(ParseInfo.ONLY_IF_NO_SENTENCE_INFO);
				if (s.variableNames == null || s.variableNames.Count == 0)
				{
					return (not ? impact : -impact);
				}
				else
				{
					foreach (string varName in variables)
					{
						if (!s.variableNames.Contains(varName))
							return (not ? impact : -impact);
					}
				}
			}
			
			return (not ? -impact : impact);
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& variables.Equals(((DefinesVariableCondition)otherCondition).variables);
		}

		public override string StringValue()
		{
			return variables == null ? "no variables" : String.Join(",", variables.ToArray());
		}
	}


	[System.Serializable]
	public class LastWordPatternCondition : PatternCondition
	{
		public TextPattern pattern;

		protected LastWordPatternCondition()
		{
		}

		public LastWordPatternCondition(TextPattern lastWordPattern)
			: this(lastWordPattern, Constants.MATCH_SUPER, false)
		{
		}

		public LastWordPatternCondition(TextPattern lastWordPattern, float impact)
			: this(lastWordPattern, impact, false)
		{
		}

		public LastWordPatternCondition(TextPattern lastWordPattern, float impact, bool not)
			: base(impact, not)
		{
			pattern = lastWordPattern;
			multipleMatches = true;
		}

		public override float MatchValue(MintyText selText)
		{
			if (selText == null || pattern == null || selText is Word)
				return 0;
			
			if (selText is Sentence)
			{
				Sentence s = (Sentence)selText;
				if (s.lastWordPattern != null)
				{
					foreach (var lastWordPattern in s.lastWordPattern)
					{
						if (lastWordPattern.IsGrammarCompatible(pattern))
						{
							return (not ? -impact : impact);
						}
					}
				}
				return (not ? impact : -impact);

			}
			
			return 0;
		}

		
		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& pattern.Equals(((LastWordPatternCondition)otherCondition).pattern);
		}

		public override string StringValue()
		{
			return pattern.ToString();
		}
	}
	

	//	[System.Serializable]
	//	public class RelationTypeCondition : PatternCondition {
	//		public RelationType[] types;
	//		public RelationTypeCondition (RelationType[] relationTypes):this (relationTypes, Constants.MATCH_SUPER, false) {}
	//		public RelationTypeCondition (RelationType[] relationTypes, float impact):this (relationTypes, impact, false) {}
	//		public RelationTypeCondition (RelationType[] relationTypes, float impact, bool not):base (impact, not) {
	//			types = relationTypes;
	//			multipleMatches = false;
	//		}
	//
	//#if UNITY_5_3_OR_NEWER
	//		public RelationTypeCondition (SerializerInStream stream)
	//		{
	//			Deserialize (stream);
	//		}
	//		override public void Serialize(SerializerOutStream stream) {
	//			type.Serialize(stream);
	//			base.Serialize(stream);
	//		}
	//		override public void Deserialize(SerializerInStream stream) {
	//			type = (RelationType) RelationType.SerializerCreate (stream);
	//			base.Deserialize(stream);
	//		}
	//#endif
	//
	//		public override float MatchValue (ComicText selText)
	//		{
	//			if (selText == null || types == null || selText is Sentence) return 0;
	//
	//			if (selText is Word){
	//				Word w = (Word)selText;
	//				if (w.textRelations != null && w.textRelations.Count>0){
	//					bool hasAllRelations = true;
	//					foreach (RelationType relationType in types) {
	//						bool hasThisRelation = false;
	//						foreach (int relNum in w.textRelations) {
	//							TextRelation tr = ComicTextsContainer.Get.textRelations [relNum];
	//							bool genusOK = (relationType.genus == Genus.UNDEFINED || relationType.genus == tr.type.genus);
	//							bool casusOK = (relationType.casus == Casus.UNDEFINED || relationType.casus == tr.type.casus);
	//							bool baseTypeOK = relationType.baseType == tr.type.baseType;
	//							if (genusOK && casusOK && baseTypeOK) {
	//								hasThisRelation = true;
	//								break;
	//							}
	//						}
	//						if (!hasThisRelation) {
	//							hasAllRelations = false;
	//							break;
	//						}
	//					}
	//					return (hasAllRelations != not ? impact : -impact);
	//				}
	//			}
	//
	//			return not? impact : -impact;
	//		}
	//
	//		public override bool Equals (PatternCondition otherCondition)
	//		{
	//			return base.Equals (otherCondition)
	//				&& types.Equals( ((RelationTypeCondition)otherCondition).types);
	//		}
	//
	//		public override string ToString ()
	//		{
	//			return string.Format ("[{0} = {1}] ",GetType().Name, types);
	//		}
	//	}

	//	[System.Serializable]
	//	public class EnumCondition<Article_Type> : EnumCondition<Article_Type>
	//	{
	//		public Article_Type enumValue;
	//
	//		protected EnumCondition<Article_Type>()
	//		{
	//		}
	//
	//		public EnumCondition<Article_Type>(Article_Type enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public EnumCondition<Article_Type>(Article_Type enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public EnumCondition<Article_Type>(Article_Type enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//		}
	//
	//
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || !(selText is Word))
	//				return 0;
	//			if (enumValue == Article_Type.UNDEFINED || ((Word)selText).article_type == Article_Type.UNDEFINED)
	//				return 0;
	//			return ((((Word)selText).article_type == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((EnumCondition<Article_Type>)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}

	[System.Serializable]
	public class VerbCategoryCondition : PatternCondition
	{
		public VerbCategory enumValue;

		protected VerbCategoryCondition()
		{
		}

		public VerbCategoryCondition(VerbCategory enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public VerbCategoryCondition(VerbCategory enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public VerbCategoryCondition(VerbCategory enumCondition, float impact, bool not)
			: base(impact, not)
		{
			enumValue = enumCondition;
			multipleMatches = true;
			excluding = true;
			valueType = ConditionValueType.SINGLE;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || !(selText is Word) || ((Word)selText).wordType != WordType.VERB || enumValue == VerbCategory.UNDEFINED)
				return 0;
			bool ok;
			switch (enumValue)
			{
				case VerbCategory.UNDEFINED:
					return 0;
				case VerbCategory.REFLEXIV:
					ok = ((Word)selText).verbCategory == VerbCategory.REFLEXIV || ((Word)selText).verbCategory == VerbCategory.REFLEXIV_ONLY;
					return (ok != not) ? 0 : -impact;
				case VerbCategory.REFLEXIV_ONLY:
					ok = ((Word)selText).verbCategory == VerbCategory.REFLEXIV_ONLY;
					return (ok != not) ? 0 : -impact;
				default:
					return 0;
			}
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& enumValue == ((VerbCategoryCondition)otherCondition).enumValue;
		}

		public override string StringValue()
		{
			return enumValue.ToString();
		}
	}

	[System.Serializable]
	public class ActorTypeCondition : PatternCondition
	{
		public ActorType enumValue;

		protected ActorTypeCondition()
		{
		}

		public ActorTypeCondition(ActorType enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public ActorTypeCondition(ActorType enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public ActorTypeCondition(ActorType enumCondition, float impact, bool not)
			: base(impact, not)
		{
			enumValue = enumCondition;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || enumValue == ActorType.UNDEFINED)
				return 0;
			return ((selText.speaker == enumValue) != not) ? impact : -impact;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& enumValue == ((ActorTypeCondition)otherCondition).enumValue;
		}

		public override string StringValue()
		{
			return enumValue.ToString();
		}
	}

	[System.Serializable]
	public class TextMeaningCondition : PatternCondition
	{
		public TextMeaning enumValue;

		protected TextMeaningCondition()
		{
		}

		public TextMeaningCondition(TextMeaning enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public TextMeaningCondition(TextMeaning enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public TextMeaningCondition(TextMeaning enumCondition, float impact, bool not)
			: base(impact, not)
		{
			enumValue = enumCondition;
		}

			
		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || enumValue == TextMeaning.UNDEFINED)
				return 0;

			bool basicFit = (selText.meaning & enumValue) != 0;
			bool answer = ((enumValue & TextMeaning.ANSWER) != 0 && selText.responseTypes != null && selText.responseTypes.Count > 0);
			bool question = ((enumValue & TextMeaning.QUESTION) != 0 && selText is Sentence && ((Sentence)selText).responses != null && ((Sentence)selText).responses.Count > 0);

			if (!basicFit && (answer || question))
			{
				int i = 0;
			}
			else if (basicFit)
			{
				int i = 0;
			}
			return ((basicFit
			|| answer
			|| question
			) != not) ? impact : -impact;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& enumValue == ((TextMeaningCondition)otherCondition).enumValue;
		}

		public override string StringValue()
		{
			return enumValue.ToString();
		}
	}

	//	[System.Serializable]
	//	public class GenusCondition : EnumCondition<Genus>
	//	{
	//		//		public Genus enumValue;
	//
	//		protected GenusCondition()
	//		{
	//		}
	//
	//		public GenusCondition(Genus enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public GenusCondition(Genus enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public GenusCondition(Genus enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	////			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Genus.UNDEFINED || selText.genus == Genus.UNDEFINED)
	//				return 0;
	//			return ((selText.genus == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((GenusCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class CasusCondition : EnumCondition<Casus>
	//	{
	//		protected CasusCondition()
	//		{
	//		}
	//
	//		public CasusCondition(Casus enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public CasusCondition(Casus enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public CasusCondition(Casus enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Casus.UNDEFINED || selText.casus == Casus.UNDEFINED)
	//				return 0;
	//			return ((selText.casus == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((CasusCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class NumerusCondition : EnumCondition<Numerus>
	//	{
	//		protected NumerusCondition()
	//		{
	//		}
	//
	//		public NumerusCondition(Numerus enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public NumerusCondition(Numerus enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public NumerusCondition(Numerus enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Numerus.UNDEFINED)
	//				return 0;
	//			return ((selText.numerus == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((NumerusCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class TempusCondition : EnumCondition<Tempus>
	//	{
	//		protected TempusCondition()
	//		{
	//		}
	//
	//		public TempusCondition(Tempus enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public TempusCondition(Tempus enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public TempusCondition(Tempus enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Tempus.UNDEFINED)
	//				return 0;
	//			return ((selText.tempus == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((TempusCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class FormCondition : EnumCondition<Form>
	//	{
	//		protected FormCondition()
	//		{
	//		}
	//
	//		public FormCondition(Form enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public FormCondition(Form enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public FormCondition(Form enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			return 0;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((FormCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class PersonCondition : EnumCondition<Person>
	//	{
	//		protected PersonCondition()
	//		{
	//		}
	//
	//		public PersonCondition(Person enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public PersonCondition(Person enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public PersonCondition(Person enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Person.UNDEFINED)
	//				return 0;
	//			return ((selText.person == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((PersonCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}
	//
	//	[System.Serializable]
	//	public class KomparationCondition : EnumCondition<Komparation>
	//	{
	//		protected KomparationCondition()
	//		{
	//		}
	//
	//		public KomparationCondition(Komparation enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public KomparationCondition(Komparation enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public KomparationCondition(Komparation enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	//			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == Komparation.UNDEFINED)
	//				return 0;
	//			return ((selText.komparation == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((KomparationCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}


	[System.Serializable]
	public class PersonalityCondition : EnumCondition<Personality>
	{
		protected PersonalityCondition()
		{
		}

		public PersonalityCondition(Personality enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public PersonalityCondition(Personality enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public PersonalityCondition(Personality enumCondition, float impact, bool not)
			: base(enumCondition, impact, not)
		{
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || enumValue == Personality.UNDEFINED)
				return 0;
			return ((selText.personality == enumValue) != not) ? impact : -impact;
		}

		//		public override bool Equals(PatternCondition otherCondition)
		//		{
		//			return base.Equals(otherCondition)
		//			&& enumValue == ((PersonalityCondition)otherCondition).enumValue;
		//		}
		//
		//		public override string StringValue()
		//		{
		//			return enumValue.ToString();
		//		}
	}

	[System.Serializable]
	public class SpeakerCondition : EnumCondition<ActorType>
	{
		protected SpeakerCondition()
		{
		}

		public SpeakerCondition(ActorType enumCondition)
			: this(enumCondition, Constants.MATCH_SUPER, false)
		{
		}

		public SpeakerCondition(ActorType enumCondition, float impact)
			: this(enumCondition, impact, false)
		{
		}

		public SpeakerCondition(ActorType enumCondition, float impact, bool not)
			: base(enumCondition, impact, not)
		{
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || enumValue == ActorType.UNDEFINED)
				return 0;
			return ((selText.speaker == enumValue) != not) ? impact : -impact;
		}

		//		public override bool Equals(PatternCondition otherCondition)
		//		{
		//			return base.Equals(otherCondition)
		//			&& enumValue == ((SpeakerCondition)otherCondition).enumValue;
		//		}
		//
		//		public override string StringValue()
		//		{
		//			return enumValue.ToString();
		//		}
	}

	//	[System.Serializable]
	//	public class WordTypeCondition : EnumCondition<WordType>
	//	{
	//		protected WordTypeCondition()
	//		{
	//		}
	//
	//		public WordTypeCondition(WordType enumCondition)
	//			: this(enumCondition, Constants.MATCH_SUPER, false)
	//		{
	//		}
	//
	//		public WordTypeCondition(WordType enumCondition, float impact)
	//			: this(enumCondition, impact, false)
	//		{
	//		}
	//
	//		public WordTypeCondition(WordType enumCondition, float impact, bool not)
	//			: base(enumCondition, impact, not)
	//		{
	////			isFormCondition = true;
	//		}
	//
	//		
	//		public override float MatchValue(ComicText selText)
	//		{
	//			if (selText == null || enumValue == WordType.UNDEFINED || selText.GetType() == typeof(Word))
	//				return 0;
	//			return ((((Word)selText).wordType == enumValue) != not) ? impact : -impact;
	//		}
	//
	//		//		public override bool Equals(PatternCondition otherCondition)
	//		//		{
	//		//			return base.Equals(otherCondition)
	//		//			&& enumValue == ((WordTypeCondition)otherCondition).enumValue;
	//		//		}
	//		//
	//		//		public override string StringValue()
	//		//		{
	//		//			return enumValue.ToString();
	//		//		}
	//	}


	[System.Serializable]
	public class TopicCondition : PatternCondition
	{
		public TopicFlags conditionTopics;
		public string id;
		public int sequence = -1;

		protected TopicCondition()
		{
		}

		public TopicCondition(TopicFlags topics)
			: this(topics, -1, null, Constants.MATCH_GOOD, false)
		{
		}

		public TopicCondition(TopicFlags topics, int sequence)
			: this(topics, sequence, null, Constants.MATCH_GOOD, false)
		{
		}

		public TopicCondition(TopicFlags topics, int sequence, string id)
			: this(topics, sequence, id, Constants.MATCH_GOOD, false)
		{
		}

		public TopicCondition(TopicFlags topics, int sequence, string id, float impact)
			: this(topics, sequence, id, impact, false)
		{
		}

		public TopicCondition(TopicFlags topics, float impact, bool not)
			: this(topics, -1, null, impact, not)
		{
		}

		public TopicCondition(TopicFlags topics, int sequence, string id, float impact, bool not)
			: base(impact, not)
		{
			if (topics == default(TopicFlags))
			{
				Logger.LogWarning(this, "undefined topic condition???");
			}
			conditionTopics = topics;
			this.id = id;
			this.sequence = sequence;
			this.multipleMatches = false;
			valueType = ConditionValueType.FLAGS;
		}

		
		public override float MatchValue (MintyText mintyText)
		{
			if (mintyText == null || conditionTopics.FlagsInt == 0)
				return 0;

			float match = 0;
			bool topicOK = false;
			if (mintyText.topics != null)
			{
				if (mintyText.topics.ContainsAny(conditionTopics))
				{
					match = (not ? -impact : impact);
					topicOK = true;
				}
			}
			
			if (!topicOK && mintyText is Sentence)
			{
				Sentence s = (Sentence)mintyText;
				if (!s.inlineTopics.IsEmpty() 
					&& s.inlineTopics.ContainsAny(conditionTopics))
				{
					match = (not ? -impact : impact);
					topicOK = true;
				}
			}
			
			if (topicOK && id != null && id.Length > 0 && mintyText.id != id)
			{
				match = 0;
			}
			
			if (topicOK && sequence >= 0 && mintyText.sequence != sequence)
			{
				match = 0;
			}
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionTopics.FlagsInt == ((TopicCondition)otherCondition).conditionTopics.FlagsInt
			&& id == ((TopicCondition)otherCondition).id
			&& sequence == ((TopicCondition)otherCondition).sequence;
		}

		public override string StringValue()
		{
			return string.Format("{0}, {1}, {2}", conditionTopics.ToString(), sequence, id);
		}
	}

	[System.Serializable]
	public class ResponseTypeCondition : PatternCondition
	{
		public string conditionResponseType;

		protected ResponseTypeCondition()
		{
		}

		public ResponseTypeCondition(string responseType)
			: this(responseType, Constants.MATCH_GOOD, false)
		{
		}

		public ResponseTypeCondition(string responseType, float impact)
			: this(responseType, impact, false)
		{
		}

		public ResponseTypeCondition(string responseType, float impact, bool not)
			: base(impact, not)
		{
			conditionResponseType = responseType;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || conditionResponseType == null)
				return 0;
			float match = 0;
			if (selText.responseTypes != null)
			{
				foreach (string responseType in selText.responseTypes)
				{
					if (conditionResponseType == responseType)
					{
						match = (not ? -impact : impact);
						break;
					}
				}
			}
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionResponseType == ((ResponseTypeCondition)otherCondition).conditionResponseType;
		}

		public override string StringValue()
		{
			return conditionResponseType;
		}
	}

	[System.Serializable]
	public class ResponseCondition : PatternCondition
	{
		public string conditionResponse;

		protected ResponseCondition()
		{
		}

		public ResponseCondition(string response)
			: this(response, Constants.MATCH_GOOD, false)
		{
		}

		public ResponseCondition(string response, float impact)
			: this(response, impact, false)
		{
		}

		public ResponseCondition(string response, float impact, bool not)
			: base(impact, not)
		{
			conditionResponse = response;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || conditionResponse == null)
				return 0;
			float match = 0;
			if (selText is Sentence && ((Sentence)selText).responses != null)
			{
				foreach (string response in ((Sentence)selText).responses)
				{
					if (conditionResponse == response)
					{
						match = (not ? -impact : impact);
						break;
					}
				}
			}
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionResponse == ((ResponseCondition)otherCondition).conditionResponse;
		}

		public override string StringValue()
		{
			return conditionResponse;
		}
	}


	[System.Serializable]
	public class ValueCondition : PatternCondition
	{
		public string conditionRelation;

		protected ValueCondition()
		{
		}

		public ValueCondition(string relation)
			: this(relation, Constants.MATCH_GOOD, false)
		{
		}

		public ValueCondition(string relation, float impact)
			: this(relation, impact, false)
		{
		}

		public ValueCondition(string relation, float impact, bool not)
			: base(impact, not)
		{
			conditionRelation = relation;
			multipleMatches = true;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || conditionRelation == null)
				return 0;
			float match = 0;
			bool relationOK = true;
			string relationString = conditionRelation;
			
			int tokenPos = relationString.IndexOfAny(new char[] { '<', '>', '=' });
			if (tokenPos > 0)
			{
				string field = relationString.Substring(0, tokenPos).Trim();
				Interval fieldValue = default(Interval);
				switch (field)
				{
					case "LIKE":
						fieldValue = selText.like;
						break;
					case "COMPLEXITY":
						fieldValue = new Interval() { from = selText.complexity, to = selText.complexity };
						break;
					case "LENGTH":
						fieldValue = new Interval() { from = selText.processedLength, to = selText.processedLength };
						break;
					case "WEIGHT":
						fieldValue = new Interval() { from = selText.weight, to = selText.weight };
						break;
				}
				
				if (fieldValue != default(Interval))
				{
					string valueString = relationString.Substring(tokenPos + 1);
					
					try
					{
						float relationValue = float.Parse(valueString);
						
						switch (relationString[tokenPos])
						{
							case '<':
								relationOK = (fieldValue.from < relationValue || fieldValue.to < relationValue);
								break;
							case '>':
								relationOK = (fieldValue.from > relationValue || fieldValue.to > relationValue);
								break;
							case '=':
								relationOK = (fieldValue.from >= relationValue && fieldValue.to <= relationValue);
								break;
						}
						
						if (!relationOK)
							return 0;
						
						match += (not ? -impact : impact);
					}
					catch (Exception e)
					{
						Logger.LogError(this, e.Message);
					}
				}
			}
		
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionRelation == ((ValueCondition)otherCondition).conditionRelation;
		}

		public override string StringValue()
		{
			return conditionRelation;
		}
	}

	[System.Serializable]
	public class WordCondition : PatternCondition
	{
		public Word conditionWord;

		protected WordCondition()
		{
		}

		public WordCondition(Word word)
			: this(word, Constants.MATCH_SUPER, false)
		{
		}

		public WordCondition(Word word, float impact)
			: this(word, impact, false)
		{
		}

		public WordCondition(Word word, float impact, bool not)
			: base(impact, not)
		{
			conditionWord = word;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || conditionWord == null)
				return 0;
			float match = 0;
			
			if (selText is Word)
			{
				if (selText.Equals(conditionWord))
				{
					match += (not ? -impact : impact);
				}
			}
			else if (selText is Sentence)
			{
				Sentence s = (Sentence)selText;
				if (s.Words != null && s.Words.Count > 0)
				{
					if (s.Words.Contains(conditionWord))
					{
						match += (not ? -impact : impact);
					}
				}
			}
			
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionWord.Equals(((WordCondition)otherCondition).conditionWord);
		}

		public override string StringValue()
		{
			return conditionWord.ToString();
		}
	}

	
	/** Path Condition should be checked before finding best matchValue **/
	
	[System.Serializable]
	public class PathCondition : PatternCondition
	{
		public string[] conditionPath;

		protected PathCondition()
		{
		}

		public PathCondition(string path)
			: this(path, Constants.MATCH_PERFECT, false)
		{
		}

		public PathCondition(string path, float impact)
			: this(path, impact, false)
		{
		}

		public PathCondition(string path, float impact, bool not)
			: base(impact, not)
		{
			conditionPath = path.Split('/');
			multipleMatches = false;
//			useForMatchValue = false;
		}

		
		/// <summary>
		/// becareful, this method allways returns 0! select path-fitting words prior to checking MatchValue
		/// </summary>
		/// <returns>
		/// 0.
		/// </returns>
		/// <param name='selText'>
		/// Sel text.
		/// </param>
		public override float MatchValue(MintyText selText)
		{
//			return 0f;
			if (selText == null || !(selText is Word))
				return 0;
			float match = 0;
			Word word = (Word)selText;
			if (word.BelongsTo(conditionPath))
			{
				match += (not ? -impact : impact);
			}
			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			if (!base.Equals(otherCondition))
				return false;

			string[] otherPath = ((PathCondition)otherCondition).conditionPath;
			if (conditionPath == null || otherPath == null)
				return (conditionPath == null && otherPath == null);
			if (conditionPath.Length != otherPath.Length)
				return false;
			for (int i = 0; i < otherPath.Length; i++)
			{
				if (!conditionPath[i].Equals(otherPath[i]))
					return false;
			}
			return true;
		}

		public override string StringValue()
		{
			return String.Join("/", conditionPath);
		}
	}

	[System.Serializable]
	public class SentenceCondition : PatternCondition
	{
		public Sentence conditionSentence;

		protected SentenceCondition()
		{
		}

		public SentenceCondition(Sentence sentence)
			: this(sentence, Constants.MATCH_GOOD, false)
		{
		}

		public SentenceCondition(Sentence sentence, float impact)
			: this(sentence, impact, false)
		{
		}

		public SentenceCondition(Sentence sentence, float impact, bool not)
			: base(impact, not)
		{
			conditionSentence = sentence;
		}

		
		public override float MatchValue(MintyText selText)
		{
			if (selText == null || selText == null || selText.GetType() != typeof(Sentence))
				return 0;
			
			if (selText.Equals(conditionSentence))
			{
				return (not ? -impact : impact);
			}
			
			return 0f;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& conditionSentence.Equals(((SentenceCondition)otherCondition).conditionSentence);
		}

		public override string StringValue()
		{
			return conditionSentence.ToString();
		}
	}

	[System.Serializable]
	public class FixedTextCondition : PatternCondition
	{
		public string text;

		protected FixedTextCondition()
		{
		}

		public FixedTextCondition(string text)
			: this(text, Constants.MATCH_SUPER, false)
		{
		}

		public FixedTextCondition(string text, float impact)
			: this(text, impact, false)
		{
		}

		public FixedTextCondition(string text, float impact, bool not)
			: base(impact, not)
		{
			this.text = text.ToLower();
		}


		public override float MatchValue(MintyText selText)
		{
			if (selText == null || text == null)
				return 0;
			float match = 0;

			if (selText is Word)
			{
				if (((Word)selText).IsFormed)
				{
					if (String.Equals(selText.Text, text, StringComparison.CurrentCultureIgnoreCase))
					{
						match += (not ? -impact : impact);
					}
				}
				else
				{
					WordFormContainer cont;
					if (MintyTextsContainer.Instance.WordForms.TryGetValue(text, out cont)
					    && cont.words.Contains(selText.num))
					{
						match += (not ? -impact : impact);
					}
				}
			}
			else if (String.Equals(selText.Text, text, StringComparison.CurrentCultureIgnoreCase))
			{
				match += (not ? -impact : impact);
			}

			return match;
		}

		public override bool Equals(PatternCondition otherCondition)
		{
			return base.Equals(otherCondition)
			&& text.Equals(((FixedTextCondition)otherCondition).text);
		}

		public override string StringValue()
		{
			return text;
		}
	}

	public interface ISingleValue
	{
		object SingleValue { get; set; }
	}

	public enum ConditionValueType
	{
		/// <summary>
		/// there is no meaning in having two "GenusConditions" or similar, ...
		/// </summary>
		SINGLE,
		/// <summary>
		/// there can be many path conditions
		/// </summary>
		MULTI,
		/// <summary>
		/// there can be exact one flags-condition and one AVOID-flags condition
		/// </summary>
		FLAGS
	}
}

