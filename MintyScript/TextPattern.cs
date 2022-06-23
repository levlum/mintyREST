using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
	[System.Serializable]
	#if UNITY_5_3_OR_NEWER
	public class TextPattern : ISerializationCallbackReceiver, IGrammatical
	#else
	public class TextPattern: IGrammatical
	#endif
	{

#if UNITY_5_3_OR_NEWER


		[SerializeField]
		private List<string> _serializedPatternConditions = new List<string>();
#endif

		public bool immutable = false;

		private Dictionary<Type, List<PatternCondition>> conditions;

		public int NumOfConditionTypes { get { return conditions.Count; } }

		public static List<string> QUANTITATIVE_VALUES = new List<string>
		{
			"LIKE",
			"COMPLEXITY",
			"LENGTH",
			"WEIGHT"
		};

		public TextPattern()
		{
			conditions = new Dictionary<Type, List<PatternCondition>>();
		}

		public TextPattern(params PatternCondition[] pList)
			: this()
		{
			AddPatternConditions(pList);
		}


		#if UNITY_5_3_OR_NEWER
		



		



		#region ISerializationCallbackReceiver implementation

		
		
		
		public void OnBeforeSerialize()
		{
			_serializedPatternConditions.Clear();
			foreach (var conditionTypeList in conditions)
			{
				string[] jsonStrings = new string[conditionTypeList.Value.Count + 1];
				jsonStrings[0] = conditionTypeList.Key.ToString();
				int i = 1;
				foreach (var cond in conditionTypeList.Value)
				{
					jsonStrings[i] = cond.ToJson();
					i++;
				}
				_serializedPatternConditions.Add(string.Join("+", jsonStrings));
			}
			if (_serializedPatternConditions.Count > 0)
			{
				string gax;
			}
		}

		public void OnAfterDeserialize()
		{
			conditions = new Dictionary<Type, List<PatternCondition>>();

			foreach (string oneTypeConditions in _serializedPatternConditions)
			{
				string[] typeConditions = oneTypeConditions.SplitNotInBrackets('+');
				Type type = Type.GetType(typeConditions[0]);
				List<PatternCondition> conditionList = new List<PatternCondition>();
				for (int i = 1; i < typeConditions.Length; i++)
				{
					var newCond = (PatternCondition)Activator.CreateInstance(type, true);
					newCond.FromJsonOverwrite(typeConditions[i]);
					conditionList.Add(newCond);
				}
				conditions[type] = conditionList;
			}
		}




		



		#endregion

		
		

		#endif

		public override bool Equals(object obj)
		{
			if (obj is TextPattern)
			{
				if (conditions == null || ((TextPattern)obj).conditions == null)
					return ((TextPattern)obj).conditions == ((TextPattern)obj).conditions;

				if (conditions.Count != ((TextPattern)obj).conditions.Count)
				{
					return false;
				}

				foreach (var conditionsEntry in conditions)
				{
					List<PatternCondition> otherCondList;
					if (!((TextPattern)obj).conditions.TryGetValue(conditionsEntry.Key, out otherCondList))
					{
						return false;
					}
					if (otherCondList == null && conditionsEntry.Value == null)
					{
						continue;
					}
					if (otherCondList == null || otherCondList.Count != conditionsEntry.Value.Count)
					{
						return false;
					}
					for (int i = 0; i < otherCondList.Count; i++)
					{
						if (!conditionsEntry.Value[i].Equals(otherCondList[i]))
						{
							return false;
						}
					}
				}

				return true;
			}
			return false;
		}


		public List<PatternCondition> GetAllConditions()
		{
			List<PatternCondition> result = new List<PatternCondition>();
			foreach (var condList in conditions.Values)
			{
				result.AddRange(condList);
			}
			return result;
		}

		public List<PatternCondition> GetPatternConditions(Type t)
		{
			List<PatternCondition> result;
			if (conditions.TryGetValue(t, out result))
			{
				return result;
			}
			return null;
		}

		public bool TryGetPatternConditions<T>(out List<T> result) where T: PatternCondition
		{
			result = null;
			List<PatternCondition> condList;
			if (conditions.TryGetValue(typeof(T), out condList))
			{
				result = new List<T>();
				foreach (var cond in condList)
				{
					result.Add((T)cond);
				}
				return true;
			}
			return false;
		}

		public List<string> Paths
		{
			get
			{
				List<string> paths = new List<string>();
				List<PathCondition> condList;
				if (TryGetPatternConditions<PathCondition>(out condList))
				{
					foreach (var pv in condList)
					{
						paths.Add(String.Join("/", pv.conditionPath));
					}
				}
				return paths;
			}
		}


		public TopicFlags PossibleTopics
		{
			get
			{
				//List<ComicTopic> topics = new List<ComicTopic>();
				TopicCondition topicCond;
				if (TryGetFirst<TopicCondition>(out topicCond))
				{
					return topicCond.conditionTopics;
					//foreach (var pv in condList)
					//{
					//	topics.Add(pv.conditionTopic);
					//}
				}
				return default(TopicFlags);
			}
		}

		public List<string> PossibleResponseTypes
		{
			get
			{
				List<string> list = new List<string>();
				List<ResponseTypeCondition> condList;
				if (TryGetPatternConditions<ResponseTypeCondition>(out condList))
				{
					foreach (var pv in condList)
					{
						list.Add(pv.conditionResponseType);
					}
				}
				return list;
			}
		}

		public List<Word> WordsToInclude
		{
			get
			{
				List<Word> list = new List<Word>();
				List<WordCondition> condList;
				if (TryGetPatternConditions<WordCondition>(out condList))
				{
					foreach (var pv in condList)
					{
						list.Add(pv.conditionWord);
					}
				}
				return list;
			}
		}

		public ActorType Speaker
		{
			get
			{
				List<PatternCondition> list;
				if (conditions.TryGetValue(typeof(ActorTypeCondition), out list))
				{
					if (list.Count > 0 && !list[0].not)
						return ((ActorTypeCondition)list[0]).enumValue;
				}
				return ActorType.UNDEFINED;
			}
		}

		public Personality Personality
		{
			get
			{
				List<PatternCondition> list;
				if (conditions.TryGetValue(typeof(PersonalityCondition), out list))
				{
					if (list.Count > 0 && !list[0].not)
						return ((PersonalityCondition)list[0]).enumValue;
				}
				return Personality.UNDEFINED;
			}
		}

		public TextMeaning Meaning
		{
			get
			{
				List<PatternCondition> list;
				if (conditions.TryGetValue(typeof(TextMeaningCondition), out list))
				{
					if (list.Count > 0 && !list[0].not)
						return ((TextMeaningCondition)list[0]).enumValue;
				}
				return TextMeaning.UNDEFINED;
			}
		}

		public Form Form
		{
			get
			{
				List<PatternCondition> list;
				if (conditions.TryGetValue(typeof(EnumCondition<Form>), out list))
				{
					if (list.Count > 0 && !list[0].not)
						return ((EnumCondition<Form>)list[0]).enumValue;
				}
				return Form.UNDEFINED;
			}
		}

		#region IGrammatical implementation

		public T Get<T>() where T:struct, IConvertible
		{
			List<PatternCondition> list;
			if (conditions.TryGetValue(typeof(EnumCondition<T>), out list))
			{
				if (list.Count > 0 && !list[0].not)
					return ((EnumCondition<T>)list[0]).enumValue;
			}

			return default(T);
		}


		#endregion


		/// <summary>
		/// Gets the grammatical probability (is used at end of line).
		/// example: a pattern with genetiv is less probable than a pattern with akkusativ
		/// </summary>
		/// <returns>The grammatical probability.</returns>
		public int GetGrammaticalProbability()
		{
			int probSum = 0;
			foreach (var condList in conditions.Values)
			{
				foreach (var cond in condList)
				{
					if (cond is ISingleValue && (GrammaUtil.IsGrammatical(((ISingleValue)cond).SingleValue)))
					{
						probSum += MintyTextsContainer.Instance.GetLastWordGrammaCount(((ISingleValue)cond).SingleValue);
					}
				}
			}

			return probSum;
		}

		public void AddTextFlag(TextFlag newFlag, bool avoid = false)
		{
			List<EnumCondition<TextFlag>> flagConds;
			if (!TryGetPatternConditions<EnumCondition<TextFlag>>(out flagConds)
			    || !flagConds.Exists(fc => fc.not == avoid))
			{
				var flags = new EnumCondition<TextFlag>(newFlag, Constants.MATCH_PERFECT, avoid);
				AddPatternConditions(flags);
			}
			else
			{
				var flags = flagConds.Find(fc => fc.not == avoid);
				if (flags != null)
				{
					flags.enumValue |= newFlag;
				}
			}
		}

		public bool HasFlag(TextFlag flag)
		{
			List<EnumCondition<TextFlag>> conds;
			if (TryGetPatternConditions<EnumCondition<TextFlag>>(out conds))
			{
				foreach (var cond in conds)
				{
					if (!cond.not)
						return (cond.enumValue & flag) != 0;
				}
			}

			return false;
		}

		/// <summary>
		/// flags in this list must not be set in the word or sentence
		/// </summary>
		/// <returns><c>true</c> if this instance has to avoid flag the specified flag; otherwise, <c>false</c>.</returns>
		/// <param name="flag">Flag.</param>
		public bool HasFlagToAvoid(TextFlag flag)
		{
			List<EnumCondition<TextFlag>> conds;
			if (TryGetPatternConditions<EnumCondition<TextFlag>>(out conds))
			{
				foreach (var cond in conds)
				{
					if (cond.not)
						return (cond.enumValue & flag) != 0;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds the pattern conditions.
		/// (does not add a condition, if it is a "single value condition" and there is allready such a conditions and the impact of the new condition is lower than the old one.)
		/// </summary>
		/// <param name="pList">conditions to add</param>
		public void AddPatternConditions(params PatternCondition[] pList)
		{
			
			foreach (PatternCondition cond in pList)
			{
				
				List<PatternCondition> typeConditions; //list of all conditions of one type
				if (!conditions.TryGetValue(cond.GetType(), out typeConditions))
				{
					typeConditions = new List<PatternCondition>();
					conditions.Add(cond.GetType(), typeConditions);
				}
				else if (cond is TopicCondition)
				{
					((TopicCondition)typeConditions[0]).conditionTopics |= ((TopicCondition)cond).conditionTopics;
					continue;
				}
				else if (cond is VariableDependencyCondition)
				{
					((VariableDependencyCondition)typeConditions[0]).availableVariables.UnionWith(((VariableDependencyCondition)cond).availableVariables);
					continue;
				}
				else if (cond is EnumCondition<TextFlag>)
				{
					((EnumCondition<TextFlag>)typeConditions[0]).enumValue |= ((EnumCondition<TextFlag>)cond).enumValue;
					continue;
				}
				else
				{
					switch (cond.valueType)
					{
						case ConditionValueType.SINGLE:
							if (cond.Equals(typeConditions[0]) || cond.impact < typeConditions[0].impact)
							{
								continue;
							}
							break;

						case ConditionValueType.FLAGS:
							var flagCondition = typeConditions.Find(c => (c.not == cond.not));
							if (flagCondition == null)
							{
								typeConditions.Add(cond);
							}
							else
							{
								((EnumCondition<TextFlag>)flagCondition).enumValue |= ((EnumCondition<TextFlag>)cond).enumValue;
							}
							continue;
							break;
					}
				}

				if (cond.valueType == ConditionValueType.SINGLE)
					typeConditions.Clear();

				if (cond is PathCondition)
				{
					//if a parent-path is allready in this pattern, then do not add the subpath

					PathCondition pathCond = (PathCondition)cond;
					bool NewBelongsToOld = false;
					List<PathCondition> conditionsToDelete = new List<PathCondition>();
					foreach (PatternCondition tc in typeConditions)
					{
						PathCondition otherPathCond = (PathCondition)tc;

						if (MintyUtils.BelongsTo(pathCond.conditionPath, otherPathCond.conditionPath))
						{
							//do not add this pathCondition
							NewBelongsToOld = true;
							break;
						}
						else if (MintyUtils.BelongsTo(otherPathCond.conditionPath, pathCond.conditionPath))
						{
							//old belongs to new
							conditionsToDelete.Add(otherPathCond);
						}
					}

					if (conditionsToDelete.Count > 0)
					{
						foreach (PathCondition deleteMe in conditionsToDelete)
						{
							typeConditions.Remove(deleteMe);
						}
					}

					if (!NewBelongsToOld)
					{
						typeConditions.Add(cond);
					}

				}
				else if (!typeConditions.Contains(cond))
				{

					typeConditions.Add(cond);
				}
			}
		}

		public void AddGramatikConditionsFrom(IGrammatical grammatical, bool? overwrite = null)
		{
			if (grammatical == null)
				return;

            if (immutable)
            {
                Logger.LogWarning(this, "can not add GramatikConditions to immutable TextPattern!");
                return;
            }

            float prio = Constants.MATCH_GOOD;
			if (overwrite.HasValue)
			{
				prio = overwrite.Value ? Constants.MATCH_SUPER : Constants.MATCH_MINIMAL;
			}

			if (grammatical.Get<Genus>() != Genus.UNDEFINED)
				AddPatternConditions(new EnumCondition<Genus>(grammatical.Get<Genus>(), prio));
			if (grammatical.Get<Numerus>() != Numerus.UNDEFINED)
				AddPatternConditions(new EnumCondition<Numerus>(grammatical.Get<Numerus>(), prio));
			if (grammatical.Get<Casus>() != Casus.UNDEFINED)
				AddPatternConditions(new EnumCondition<Casus>(grammatical.Get<Casus>(), prio));
			if (grammatical.Get<Person>() != Person.UNDEFINED)
				AddPatternConditions(new EnumCondition<Person>(grammatical.Get<Person>(), prio));
			if (grammatical.Get<Komparation>() != Komparation.UNDEFINED)
				AddPatternConditions(new EnumCondition<Komparation>(grammatical.Get<Komparation>(), prio));
			if (grammatical.Get<Tempus>() != Tempus.UNDEFINED)
				AddPatternConditions(new EnumCondition<Tempus>(grammatical.Get<Tempus>(), prio));
//			if (grammatical.Get<TextFlag>() != TextFlag.UNDEFINED)
//				AddPatternConditions(new EnumCondition<TextFlag>(grammatical.Get<TextFlag>(), prio));
			if (grammatical.Get<VerbCategory>() != VerbCategory.UNDEFINED)
				AddPatternConditions(new VerbCategoryCondition(grammatical.Get<VerbCategory>(), prio));
			if (grammatical.Get<Article_Type>() != Article_Type.UNDEFINED_VALUE)
				AddPatternConditions(new EnumCondition<Article_Type>(grammatical.Get<Article_Type>(), prio));
			if (grammatical.Get<DeclinationType>() != DeclinationType.UNDEFINED)
				AddPatternConditions(new EnumCondition<DeclinationType>(grammatical.Get<DeclinationType>(), prio));
			if (grammatical.Get<WordType>() != WordType.UNDEFINED)
				AddPatternConditions(new EnumCondition<WordType>(grammatical.Get<WordType>(), prio));
		}

		/// <summary>
		/// Determines whether this instance is grammar compatible width the specified otherPattern.
		/// Use it with this Pattern is: pattern in sentence
		/// other pattern: pattern of word
		/// </summary>
		/// <returns><c>true</c> if this instance is grammar compatible the specified otherPattern; otherwise, <c>false</c>.</returns>
		/// <param name="otherPattern">pattern of word.</param>
		public bool IsGrammarCompatible(TextPattern otherPattern)
		{
			List<FixedTextCondition> condList;
			if (TryGetPatternConditions<FixedTextCondition>(out condList))
			{
				foreach (var cond in condList)
				{
					List<FixedTextCondition> otherCondList;
					if (otherPattern.TryGetPatternConditions<FixedTextCondition>(out otherCondList))
					{
						foreach (var otherCond in otherCondList)
						{
							if (cond.text.Equals(otherCond.text, StringComparison.CurrentCultureIgnoreCase))
							{
								return true;
							}
						}
					}
				}

				return false;
			}


			WordCondition wCond;
			if (otherPattern.TryGetFirst<WordCondition>(out wCond))
			{
				otherPattern = TextPattern.CreateFromWord(wCond.conditionWord);
				Logger.LogWarning(this, "use the method IsGrammarCompatible with a set of grammatical conditions! Don't use WordCondition!");
			}


			if (otherPattern.Get<WordType>() == WordType.UNDEFINED
			    && otherPattern.Get<Genus>() == Genus.UNDEFINED
			    && otherPattern.Get<Numerus>() == Numerus.UNDEFINED
			    && otherPattern.Get<Casus>() == Casus.UNDEFINED
			    && otherPattern.Get<Person>() == Person.UNDEFINED
			    && otherPattern.Get<Komparation>() == Komparation.UNDEFINED
			    && otherPattern.Get<Tempus>() == Tempus.UNDEFINED
			    && otherPattern.Get<VerbCategory>() == VerbCategory.UNDEFINED
			    && otherPattern.Get<Article_Type>() == Article_Type.UNDEFINED_VALUE)
			{


				return true;
			}

			if (Get<WordType>() != WordType.UNDEFINED && otherPattern.Get<WordType>() != Get<WordType>())
			{
				return false;
			}


			if (Get<Genus>() != Genus.UNDEFINED && otherPattern.Get<Genus>() != Get<Genus>() && otherPattern.Get<Genus>() != Genus.UNDEFINED)
				return false;
			if (Get<Casus>() != Casus.UNDEFINED && otherPattern.Get<Casus>() != Get<Casus>() && otherPattern.Get<Casus>() != Casus.UNDEFINED)
				return false;
			if (Get<Casus>() == Casus.UNDEFINED && otherPattern.Get<Casus>() != Casus.NOMINATIV && otherPattern.Get<Casus>() != Casus.UNDEFINED)
				return false;

			if (Get<Tempus>() != Tempus.INFINITIV)
			{
				if (Get<Numerus>() != Numerus.UNDEFINED && otherPattern.Get<Numerus>() != Get<Numerus>() && otherPattern.Get<Numerus>() != Numerus.UNDEFINED)
					return false;
				if (Get<Numerus>() == Numerus.UNDEFINED && otherPattern.Get<Numerus>() != Numerus.SINGULAR && otherPattern.Get<Numerus>() != Numerus.UNDEFINED)
					return false;


				if (Get<Person>() != Person.UNDEFINED && otherPattern.Get<Person>() != Get<Person>() && otherPattern.Get<Person>() != Person.UNDEFINED)
					return false;
				if (Get<Person>() == Person.UNDEFINED && otherPattern.Get<Person>() != Person.P1 && otherPattern.Get<Person>() != Person.UNDEFINED)
					return false;
			}

			if (Get<Article_Type>() != Article_Type.UNDEFINED_VALUE && otherPattern.Get<Article_Type>() != Get<Article_Type>())
				return false;

			PathCondition pathCondition;
			bool isAdjective = Get<Komparation>() != Komparation.UNDEFINED
			                   || (TryGetFirst<PathCondition>(out pathCondition) && pathCondition.conditionPath[0] == "adjective");


			if (Get<Komparation>() != Komparation.UNDEFINED)
			{
				if (otherPattern.Get<Komparation>() != Get<Komparation>() && otherPattern.Get<Komparation>() != Komparation.UNDEFINED)
				{
					#if MintyWeb
					if (isAdjective) {
						//System.Console.WriteLine ("NOT COMPATIBEL: "+this+" - "+otherPattern);
					}
					#endif
					return false;
				}
				//if an adjective has a Komparation only, then this is a special form, ...

				//at this point, casus, genus, numerus and articleType are EQUAL or UNDEFINED
//				else if (otherPattern.Komparation == Komparation
//					&& (otherPattern.Casus != Casus.UNDEFINED
//						|| otherPattern.Genus != Genus.UNDEFINED
//						|| otherPattern.Numerus != Numerus.UNDEFINED
//						|| otherPattern.Article_Type != Article_Type.UNDEFINED)) {
//
//					return false;
//				}
			}
			if (Get<Komparation>() == Komparation.UNDEFINED && otherPattern.Get<Komparation>() != Komparation.Positiv && otherPattern.Get<Komparation>() != Komparation.UNDEFINED)
			{
				#if MintyWeb
				if (isAdjective) {
					//System.Console.WriteLine ("NOT COMPATIBEL: "+this+" - "+otherPattern);
				}
				#endif
				return false;
			}


			if (Get<Tempus>() != Tempus.UNDEFINED && otherPattern.Get<Tempus>() != Get<Tempus>())
				return false;
			Tempus defTemp = (Get<Person>() != Person.UNDEFINED) ? Tempus.PRAESENS : Tempus.INFINITIV;
			if (Get<Tempus>() == Tempus.UNDEFINED && otherPattern.Get<Tempus>() != defTemp && otherPattern.Get<Tempus>() != Tempus.UNDEFINED)
				return false;


			if (Get<VerbCategory>() != VerbCategory.UNDEFINED)
			{
//				&& otherPattern.VerbCategory != VerbCategory && otherPattern.VerbCategory != VerbCategory.UNDEFINED) return false;
				if (otherPattern.Get<VerbCategory>() != VerbCategory.REFLEXIV && otherPattern.Get<VerbCategory>() != VerbCategory.REFLEXIV_ONLY)
				{
					return false;
				}
			}

			//if the word is an adjective, the "normal form" is used, if GENUS, CASUS and NUMERUS is undefined
			if (isAdjective)
			{
				bool thisNormalForm = (Get<Genus>() == Genus.UNDEFINED && Get<Casus>() == Casus.UNDEFINED && Get<Numerus>() == Numerus.UNDEFINED);
				bool otherNormalForm = (otherPattern.Get<Genus>() == Genus.UNDEFINED && otherPattern.Get<Casus>() == Casus.UNDEFINED && otherPattern.Get<Numerus>() == Numerus.UNDEFINED);

				if (thisNormalForm == otherNormalForm)
				{
					return true;
				}
				else
				{
					return false;
				}
			}



			#if MintyWeb
			if (isAdjective) {
				//System.Console.WriteLine ("OK ADJECTIVE!: "+this+" - "+otherPattern);
			}
			#endif
			return true;
		}

		public bool HasCondition(System.Type type)
		{
			return (conditions.ContainsKey(type));
		}

		public bool TryGetFirst<T>(out T resultCond) where T: PatternCondition
		{
			resultCond = null;
			List<PatternCondition> conds;
			if (conditions.TryGetValue(typeof(T), out conds)
			    && conds.Count > 0)
			{
				resultCond = (T)conds[0];
				return true;
			}
			return false;
		}

		public void RemovePatternType<T>() where T: PatternCondition
        {
            if (immutable)
            {
                Logger.LogWarning(this, "can not remove PatternConditions from immutable TextPattern!");
                return;
            }
            conditions.Remove(typeof(T));
		}

		public void RemovePatternCondition(PatternCondition pc)
		{
			if (pc == null)
				return;

            if (immutable)
            {
                Logger.LogWarning(this, "can not remove PatternConditions from immutable TextPattern!");
                return;
            }

            List<PatternCondition> list;
			if (conditions.TryGetValue(pc.GetType(), out list))
			{
				if (list.Contains(pc))
					list.Remove(pc);
				if (list.Count == 0)
				{
					conditions.Remove(pc.GetType());
				}
			}
		}

		/// <summary>
		/// if there is a condition like "numerus:plural" then add the TextFlag to avoid condition "textFlag:no_plural"
		/// </summary>
		public void AddFormFlags()
		{
			if (Get<Numerus>()!= Numerus.UNDEFINED)
			{
				if (Get<Numerus>() == Numerus.PLURAL)
				{
					AddTextFlag(TextFlag.NO_PLURAL, true);
				}
				else
				{
					AddTextFlag(TextFlag.NO_SINGULAR, true);
				}
			}

				
		}


		public void RemoveFormConditions()
		{

            if (immutable)
            {
                Logger.LogWarning(this, "can not remove FormConditions from immutable TextPattern!");
                return;
            }

            List<Type> typesToDelete = new List<Type>();
			foreach (var condList in conditions.Values)
			{
				if (condList[0].IsFormCondition)
					typesToDelete.Add(condList[0].GetType());
			}
			foreach (Type type in typesToDelete)
			{
				conditions.Remove(type);
			}
		}

		public virtual object Clone()
		{
			TextPattern clone = new TextPattern();
            clone.Concatenate(this);
            clone.immutable = immutable;
            return clone;
		}

		public virtual TextPattern Concatenate(TextPattern snd)
		{
			return Concatenate(snd, null);
		}

		public virtual TextPattern Concatenate(TextPattern snd, float? impact)
		{
			if (snd == null)
				return this;
			foreach (var condList in snd.conditions.Values)
			{
				AddPatternConditions(condList.ToArray());
//				foreach (PatternCondition cond in condList) {
//					List<PatternCondition> myList;
//					if (!conditions.TryGetValue( cond.GetType(), out myList)){
//						myList = new List<PatternCondition>();
//						conditions.Add( cond.GetType(), myList);
//					}
//					if (!myList.Contains (cond)) {
//						//TODO: cond should be cloned, cond.impact is changed for snd condition as well!
//						if (impact.HasValue) cond.impact = impact.Value;
//						myList.Add (cond);
//					}
//				}
			}
			
			return this;
		}

		private static char[] LEV_GAX_CHARS = new char[] { ' ', '<', '>', '=' };

		public void SetFromString(string pattern, float impact = Constants.MATCH_GOOD)
		{
			string origPattern = pattern;
			if (pattern == null || pattern.Length == 0)
				return;
			
			//cut out path first if there is any:
			int pathStart = pattern.IndexOf("words/");
			int pathEnd = pattern.IndexOf(':');
			if (pathEnd < 0)
				pathEnd = pattern.Length;
			string pathOnly = "";
			if (pathStart >= 0)
			{
				pathStart += 6;
				//Logger.DebugL(this, "SetFromString: "+pattern+", "+pathStart+", "+(pathEnd-pathStart));
				pathOnly = pattern.Substring(pathStart, pathEnd - pathStart);
				
				
				AddPatternConditions(new PathCondition(pathOnly));

				//find out wordType from path
				WordType wordType = WordType.UNDEFINED;
				if (pathOnly.StartsWith("substantive"))
				{
					wordType = WordType.SUBSTANTIV;
				}
				else if (pathOnly.StartsWith("adjective"))
				{
					wordType = WordType.ADJEKTIV;
				}
				else if (pathOnly.StartsWith("verb"))
				{
					wordType = WordType.VERB;
				}
				else if (pathOnly.StartsWith("article"))
				{
					wordType = WordType.ARTICLE;
				}
				if (wordType != WordType.UNDEFINED)
				{
					AddPatternConditions(new EnumCondition<WordType>(wordType));
				}
				
				if (pathEnd < pattern.Length - 1)
				{
					pattern = pattern.Substring(pathEnd + 1);
					//Logger.DebugL(this, "pattern: \""+pattern+"\", path: \""+pathOnly+"\"");
				}
				else
				{
					//Logger.DebugL(this, "pattern: \"\", path: \""+pathOnly+"\"");
					return;
				}
			}
			else if (pattern.IndexOf('/') >= 0)
			{
				throw new Exception("words path must start with \"words/\": " + pattern);
			}
			
			//cut out brackets
			pattern = TextProcessor.GetOutside(pattern, '(', ')');

			
			//find other patterninfos
			int endTokenPos = pattern.IndexOfAny(LEV_GAX_CHARS);
			if (endTokenPos < 0)
			{
				int posStripu = pattern.IndexOf(';');
				string patternPart = (posStripu < 0) ? pattern : pattern.Substring(0, posStripu - 1);
				
				if (patternPart.IndexOf('=') < 0)
				{
					//short form
					
					if (patternPart.Length % 2 == 0)
					{
						bool not = false;

						for (int ci = 0; ci * 2 < patternPart.Length; ci++)
						{
							string pp = patternPart.Substring(ci * 2, 2);
							if (pp == "si")
								AddPatternConditions(new EnumCondition<Numerus>(Numerus.SINGULAR, impact, not));
							else if (pp == "pl")
								AddPatternConditions(new EnumCondition<Numerus>(Numerus.PLURAL, impact, not));
							else if (pp == "ma")
								AddPatternConditions(new EnumCondition<Genus>(Genus.MASCULINUM, impact, not));
							else if (pp == "fe")
								AddPatternConditions(new EnumCondition<Genus>(Genus.FEMININUM, impact, not));
							else if (pp == "ne")
								AddPatternConditions(new EnumCondition<Genus>(Genus.NEUTRUM, impact, not));
							else if (pp == "no")
								AddPatternConditions(new EnumCondition<Casus>(Casus.NOMINATIV, impact, not));
							else if (pp == "ge")
								AddPatternConditions(new EnumCondition<Casus>(Casus.GENETIV, impact, not));
							else if (pp == "da")
								AddPatternConditions(new EnumCondition<Casus>(Casus.DATIV, impact, not));
							else if (pp == "ac")
								AddPatternConditions(new EnumCondition<Casus>(Casus.AKKUSATIV, impact, not));
							else if (pp == "fu")
								AddPatternConditions(new EnumCondition<Tempus>(Tempus.FUTUR, impact, not));
							else if (pp == "pr")
								AddPatternConditions(new EnumCondition<Tempus>(Tempus.PRAESENS, impact, not));
							else if (pp == "pa")
								AddPatternConditions(new EnumCondition<Tempus>(Tempus.PAST, impact, not));
							else if (pp == "in")
							{
								AddPatternConditions(new EnumCondition<Tempus>(Tempus.INFINITIV, impact, not));
								AddPatternConditions(new EnumCondition<Form>(Form.INFINITIV, impact, not));
							}
							else if (pp == "im")
								AddPatternConditions(new EnumCondition<Tempus>(Tempus.IMPERATIV, impact, not));
							else if (pp == "au")
								AddPatternConditions(new EnumCondition<Form>(Form.HELPER, impact, not));
							else if (pp == "ws")
								AddPatternConditions(new EnumCondition<Form>(Form.STEM, impact, not));
							else if (pp == "p1")
								AddPatternConditions(new EnumCondition<Person>(Person.P1, impact, not));
							else if (pp == "p2")
								AddPatternConditions(new EnumCondition<Person>(Person.P2, impact, not));
							else if (pp == "p3")
								AddPatternConditions(new EnumCondition<Person>(Person.P3, impact, not));
							else if (pp == "k1")
								AddPatternConditions(new EnumCondition<Komparation>(Komparation.Positiv, impact, not));
							else if (pp == "k2")
								AddPatternConditions(new EnumCondition<Komparation>(Komparation.Komparativ, impact, not));
							else if (pp == "k3")
								AddPatternConditions(new EnumCondition<Komparation>(Komparation.Superlativ, impact, not));
							else if (pp == "k4")
								AddPatternConditions(new EnumCondition<Komparation>(Komparation.Elativ, impact, not));
							else if (pp == "rx")
								AddPatternConditions(new VerbCategoryCondition(VerbCategory.REFLEXIV, impact, not));
							else if (pp == "de")
								AddPatternConditions(new EnumCondition<Article_Type>(Article_Type.DEFINED_ARTICLE, impact, not));
							else if (pp == "un")
								AddPatternConditions(new EnumCondition<Article_Type>(Article_Type.UNDEFINED_ARTICLE, impact, not));
							else if (pp == "st")
								AddPatternConditions(new EnumCondition<DeclinationType>(DeclinationType.STRONG, impact, not));
							else if (pp == "we")
								AddPatternConditions(new EnumCondition<DeclinationType>(DeclinationType.WEAK, impact, not));
							else if (pp == "mi")
								AddPatternConditions(new EnumCondition<DeclinationType>(DeclinationType.MIXED, impact, not));
							else if (pp != "!!")
							{

								Logger.LogError(this, "wrong pattern definition: " + origPattern + " :" + pattern + " \"" + pp + "\"");
							}
							not = (pp == "!!");
						}
					}
					else
					{
						Logger.LogError(this, "wrong pattern definition: \"" + origPattern + "\" :" + pattern);
					}
					
				}
			}
			else
			{
				string firstToken = pattern.Substring(0, endTokenPos);
				if (QUANTITATIVE_VALUES.Contains(firstToken)
				    && pattern.IndexOfAny(new char[]{ '<', '>', '=' }) >= 0)
				{
					AddPatternConditions(new ValueCondition(pattern, impact));
				}
				else
				{
					//long form
					
//					int posStripu = pattern.IndexOf (';');
//					string patternPart = (posStripu<0)? pattern : pattern.Substring (0, posStripu-1);
					
					foreach (string part in pattern.Split (';'))
					{
						int equalPos = part.IndexOf('=');
						string fieldname = part.Substring(0, equalPos).ToLower();
						string pValue = part.Substring(equalPos + 1).Trim();
						
						switch (fieldname)
						{
							case "speaker":
								AddPatternConditions(new ActorTypeCondition(MintyEnums.Get<ActorType>(pValue.ToUpper()), impact));
								break;
							case "personality":
								AddPatternConditions(new PersonalityCondition(MintyEnums.Get<Personality>(pValue.ToUpper()), impact));
								break;
							case "meaning":
								AddPatternConditions(new TextMeaningCondition(MintyEnums.Get<TextMeaning>(pValue.ToUpper()), impact));
								break;
							case "genus":
								AddPatternConditions(new EnumCondition<Genus>(MintyEnums.Get<Genus>(pValue.ToUpper()), impact));
								break;
							case "numerus":
								AddPatternConditions(new EnumCondition<Numerus>(MintyEnums.Get<Numerus>(pValue.ToUpper()), impact));
								break;
							case "casus":
								AddPatternConditions(new EnumCondition<Casus>(MintyEnums.Get<Casus>(pValue.ToUpper()), impact));
								break;
							case "topic":
								AddPatternConditions(new TopicCondition(new TopicFlags (MintyEnums.Get<ComicTopic>(pValue.ToUpper())), impact, false));
								break;
							case "responsetype":
								AddPatternConditions(new ResponseTypeCondition(pValue, impact, false));
								break;
							case "response":
								AddPatternConditions(new ResponseCondition(pValue, impact, false));
								break;
							case "exclude":
								AddPatternConditions(new ExcludeRelationCondition(pValue, impact, false));
								break;
							case "include":
								AddPatternConditions(new ExcludeRelationCondition(pValue, impact, true));
								break;
							case "flags":
								AddPatternConditions(new EnumCondition<TextFlag>(MintyEnums.Get<TextFlag>(pValue.ToUpper()), impact, false));
								break;
							default:
								Logger.LogWarning(this, "could not add \"" + part + "\" to TextPattern.");
								break;
						}
					}
				}
			}
//			else if (firstToken.StartsWith ("words")) {
//				//AddPatternConditions (new PathCondition (firstToken, impact));
//			}
//			else {
//				if (mainPart.IndexOfAny (new char[]{'<','>','='}) < 0) {
////					if (PossibleResponseTypes == null) {
////						PossibleResponseTypes = new List<string>();
////					}
////					PossibleResponseTypes.Add (mainPart);
//					AddPatternConditions (new ResponseTypeCondition (mainPart, impact));
//				}
//			}
		}




		public static bool TryCreateFromIntersection(List<TextPattern> otherPatterns, out TextPattern resultPattern)
		{
			resultPattern = null;
			if (otherPatterns == null || otherPatterns.Count == 0)
			{
				return false;
			}

			if (otherPatterns.Count == 1)
			{
				resultPattern = otherPatterns[0];
				return true;
			}

			foreach (PatternCondition cond in otherPatterns[0].GetAllConditions())
			{
				bool isInAllPatterns = true;
				bool foundEqualCondition = false;

				for (int i = 1; i < otherPatterns.Count; i++)
				{
					if (otherPatterns[i].HasCondition(cond.GetType()))
					{
						foundEqualCondition = false;
						foreach (PatternCondition otherCond in otherPatterns[i].GetPatternConditions(cond.GetType()))
						{
							if (cond.Equals(otherCond))
							{
								foundEqualCondition = true;
								break;
							}
						}
						if (!foundEqualCondition)
						{
							isInAllPatterns = false;
							break;
						}
					}
					else
					{
						isInAllPatterns = false;
						break;
					}
				}

				if (isInAllPatterns)
				{
					if (resultPattern == null)
					{
						resultPattern = new TextPattern();
					}
					resultPattern.AddPatternConditions(cond);
				}
			}

			return (resultPattern != null && resultPattern.NumOfConditionTypes > 0);
		}

		public static TextPattern CreateFromWord(Word word)
		{
			var pattern = new TextPattern();
			pattern.AddGramatikConditionsFrom(word);
			return pattern;
		}

		public static TextPattern CreateFromString(string patternString)
		{
			var pattern = new TextPattern();
			pattern.SetFromString(patternString, Constants.MATCH_GOOD);
			return pattern;
		}


		public static TextPattern CreateFromPaths(params string[] paths)
		{
			var pattern = new TextPattern();
			if (paths == null || paths.Length == 0)
				return pattern;
			
			foreach (string path in paths)
			{
				pattern.AddPatternConditions(new PathCondition(path));
			}
			WordType wordType = MintyUtils.GetTypeFromPath(paths[0]);
			if (wordType != WordType.UNDEFINED)
			{
				pattern.AddPatternConditions(new EnumCondition<WordType>(wordType));
			}
			return pattern;
		}

		//public void AddConditionString(string p)
		//{
		//	SetFromString(p, Constants.MATCH_GOOD);
		//}

		//public void AddConditionString(string p, float impact)
		//{
		//	SetFromString(p, impact);
		//}

		public void AddConditionsResponseType(IEnumerable<string> p)
		{
			AddConditionsResponseType(p, Constants.MATCH_GOOD);
		}

		public void AddConditionsResponseType(IEnumerable<string> p, float impact)
		{
			if (p == null)
				return;
			
			foreach (string text in p)
			{
				AddPatternConditions(new ResponseTypeCondition(text, impact));
			}
		}

		public void AddConditionsResponse(IEnumerable<string> p)
		{
			AddConditionsResponse(p, Constants.MATCH_GOOD);
		}

		public void AddConditionsResponse(IEnumerable<string> p, float impact)
		{
			if (p == null)
				return;
			
			foreach (string text in p)
			{
				AddPatternConditions(new ResponseCondition(text, impact));
			}
		}

		//public void AddConditionsTopic(TopicFlags p)
		//{
		//	AddConditionsTopic(p, Constants.MATCH_GOOD);
		//}

		//public void AddConditionsTopic(TopicFlags p, float impact)
		//{
		//	if (p.flagsInt == 0)
		//		return;

		//	TopicCondition topicCondition;
		//	if (TryGetFirst<TopicCondition>(out topicCondition))
		//	{
		//		topicCondition.conditionTopics |= p;
		//	}
		//	else
		//	{
		//		//AddPatternConditions(new TopicCondition())
		//	}
		//	//foreach (ComicTopic topic in p)
		//	//{
		//	//	AddPatternConditions(new TopicCondition(topic, impact, false));
		//	//}
		//}

		public void AddConditionsWords(List<Word> p)
		{
			AddConditionsWords(p, Constants.MATCH_GOOD);
		}

		public void AddConditionsWords(List<Word> p, float impact)
		{
			if (p == null || p.Count == 0)
				return;

			foreach (Word word in p)
			{
				AddPatternConditions(new WordCondition(word, impact));
			}
		}
		
		//		protected virtual float quantitativeValuesMatch (SelectableText st) {
		//			float match = 0f;
		//
		//			if (relationStrings == null) return Constants.MATCH_GOOD;
		//
		//			foreach (string relationString in relationStrings) {
		//				bool relationOK = true;
		//
		//				int tokenPos = relationString.IndexOfAny (new char[] {'<','>','='});
		//				if (tokenPos>0) {
		//					string field = relationString.Substring (0, tokenPos).Trim();
		//					float[] fieldValues = null;
		//					switch (field) {
		//					case "LIKE": fieldValues = st.like; break;
		//					case "COMPLEXITY": fieldValues = new float[] {st.complexity, st.complexity}; break;
		//					case "LENGTH": fieldValues = new float[] {st.processedLength, st.processedLength}; break;
		//					}
		//
		//					if (fieldValues != null) {
		//						string valueString = relationString.Substring (tokenPos+1);
		//
		//						try {
		//							float relationValue = float.Parse (valueString);
		//
		//							switch (relationString[tokenPos]) {
		//							case '<': relationOK = ( fieldValues[0] < relationValue || fieldValues[1] < relationValue ); break;
		//							case '>': relationOK = ( fieldValues[0] > relationValue || fieldValues[1] > relationValue ); break;
		//							case '=': relationOK = ( fieldValues[0] >= relationValue && fieldValues[1] <= relationValue ); break;
		//							}
		//
		//							if (!relationOK) return 0;
		//
		//							match += Constants.MATCH_GOOD;
		//							break;
		//						}
		//						catch (Exception e) {
		//							Logger.LogError (e.Message);
		//						}
		//					}
		//				}
		//				Logger.LogError ("wrong relation string: "+relationString);
		//				return 0;
		//			}
		//
		//			return match;
		//		}


		
		//public static Dictionary<Type, float[]> _stats = new Dictionary<Type, float[]>();

		public virtual float MatchValue(MintyText st)
		{
			if (st == null)
				return -Constants.MATCH_GOOD;
			float match = 0f;
			
			//test if one condition type allready matched
			//Dictionary<Type,float> matchedTypes = new Dictionary<Type,float>();
			HashSet<Type> matchedTypes = new HashSet<Type>();

			foreach (var condList in conditions.Values)
			{
				foreach (PatternCondition pc in condList)
				{
					//DateTime start = DateTime.Now;
					if (!pc.useForMatchValue)
						break;

					float m = pc.MatchValue(st);

					if (pc.excluding && m < 0)
					{
						return -pc.impact;
					}

					if (m > 0 && !matchedTypes.Contains(pc.GetType()))
					{ 
						matchedTypes.Add(pc.GetType());

						if (!pc.multipleMatches)
						{
							match += m;
							break;
						}
					}

					//if (m > 0)
					//{
					//	matchedTypes.[pc.GetType()] = matchedTypes.ContainsKey(pc.GetType()) ? matchedTypes[pc.GetType()] + m : m;
					//}

					match += m;

					//float time = (float)(DateTime.Now - start).TotalMilliseconds;
					//float[] stats;
					//if (_stats.TryGetValue(pv.GetType(), out stats))
					//{
					//	stats[0] += 1;
					//	stats[1] += time;
					//}
					//else
					//{
					//	_stats[pv.GetType()] = new float[] { 1.0f, time };
					//}

				}
			}

			if (match > 0 && (st.flags & TextFlag.REUSABLE) == 0)
				match = Math.Max(Constants.MATCH_MINIMAL, match - st.usageCount);
			
			return match;
		}

		public override string ToString()
		{
			string patternstring = "TextPattern: ";
			if (conditions.Count > 0)
			{
				foreach (var condList in conditions.Values)
				{
					foreach (PatternCondition pc in condList)
					{
						patternstring += pc.ToString();
					}
				}
			}
			return patternstring;
		}
	}
}

