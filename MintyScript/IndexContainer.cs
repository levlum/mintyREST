using System;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{

	/// <summary>
	/// Index container for sentences (and words) for responses, responseTypes and textMeanings.
	/// </summary>
	public class IndexContainer
	{
		//private MintyTextsContainer ct;

		private Dictionary<string, HashSet<Sentence>> responseTypes = new Dictionary<string, HashSet<Sentence>>();
	

		private Dictionary<string, HashSet<Sentence>> responses = new Dictionary<string, HashSet<Sentence>>();


		private Dictionary<TextMeaning, HashSet<Sentence>> textMeanings = new Dictionary<TextMeaning, HashSet<Sentence>>();


		private Dictionary<string, HashSet<Sentence>> dependentOnVariablesSentences = new Dictionary<string, HashSet<Sentence>>();


		public IndexContainer(MintyTextsContainer ct)
		{
			//this.ct = ct;
			foreach (Sentence s in ct.Sentences)
			{
				AddToIndex(s);
			}

			foreach (Word w in ct.words)
			{
				AddToIndex(w);
			}
		}


		public bool ContainsIndexCondition(TextPattern pattern)
		{
			return pattern.HasCondition(typeof(ResponseCondition))
				|| pattern.HasCondition(typeof(ResponseTypeCondition))
				|| pattern.HasCondition(typeof(VariableDependencyCondition))
				|| pattern.HasCondition(typeof(TextMeaningCondition));
		}

		/// <summary>
		/// Gets all sentences that share all responseTypes, responses and meanings.
		/// </summary>
		/// <returns>The sentences.</returns>
		/// <param name="pattern">Pattern.</param>
		public HashSet<Sentence> GetSentences(TextPattern pattern)
		{
			HashSet<Sentence> result = new HashSet<Sentence>();
			HashSet<Sentence> part;


			//TopicCondition topicCondition;
			//if (pattern.TryGetFirst<TopicCondition>(out topicCondition))
			//{
			//	foreach (ComicTopic topic in topicCondition.conditionTopics.GetHashSet())
			//	{
			//		if (topicSentences.TryGetValue(topic, out part))
			//		{
			//			result.IntersectWith(part);
			//		}
			//	}
			//	pattern.RemovePatternType<TopicCondition>();
			//}



			//positive sentence conditions
			List<SentenceCondition> sentenceConditions;
			if (pattern.TryGetPatternConditions<SentenceCondition>(out sentenceConditions))
			{
				foreach (var sentenceCond in sentenceConditions)
				{
					if (!sentenceCond.not)
					{
						result.Add(sentenceCond.conditionSentence);
					}
				}
			}

			List<ResponseCondition> responseConditions;
			if (pattern.TryGetPatternConditions<ResponseCondition>(out responseConditions))
			{
				HashSet<Sentence> fullPart = new HashSet<Sentence>();
				foreach (var responseCond in responseConditions)
				{
					if (responses.TryGetValue(responseCond.conditionResponse, out part))
					{
						fullPart.UnionWith(part);
					}
				}
				pattern.RemovePatternType<ResponseCondition>();
				if (result != null && result.Count > 0)
				{
					result.IntersectWith(fullPart);
				}
				else
				{
					result = fullPart;
				}
			}

			List<ResponseTypeCondition> responseTypeConditions;
			if (pattern.TryGetPatternConditions<ResponseTypeCondition>(out responseTypeConditions))
			{
				HashSet<Sentence> fullPart = new HashSet<Sentence>();
				foreach (var responseTypeCond in responseTypeConditions)
				{
					if (responseTypes.TryGetValue(responseTypeCond.conditionResponseType, out part))
					{
						fullPart.UnionWith(part);
					}
				}
				pattern.RemovePatternType<ResponseTypeCondition>();
				if (result != null && result.Count > 0)
				{
					result.IntersectWith(fullPart);
				}
				else
				{
					result = fullPart;
				}
			}



			VariableDependencyCondition dependencyCond;
			if (pattern.TryGetFirst<VariableDependencyCondition>(out dependencyCond))
			{
				HashSet<Sentence> fullPart = new HashSet<Sentence>();
				if (dependentOnVariablesSentences.TryGetValue(Constants.NO_VARIABLE, out part))
				{
					fullPart.UnionWith(part);
				}

				if (dependencyCond.availableVariables.Count > 0)
				{
					foreach (string varName in dependencyCond.availableVariables)
					{
						if (dependentOnVariablesSentences.TryGetValue(varName, out part))
						{
							fullPart.UnionWith(part);
						}
					}
				}


				//if (fullPart.Count == 0 || dependencyCond.availableVariables.Count == 0)
				//{
				//	dependentOnVariablesSentences.TryGetValue(Constants.NO_VARIABLE, out fullPart);
				//}


				pattern.RemovePatternType<VariableDependencyCondition>();
				if (result != null && result.Count > 0)
				{
					result.IntersectWith(fullPart);
				}
				else
				{
					result = fullPart;
				}
			}

			TextMeaningCondition meaningCond;
			if (pattern.TryGetFirst<TextMeaningCondition>(out meaningCond))
			{
				HashSet<Sentence> fullPart = new HashSet<Sentence>();
				foreach (var meaning in Enum.GetValues(typeof(TextMeaning)))
				{
					if (((int)meaning & (int)meaningCond.enumValue) != 0)
					{
						if (textMeanings.TryGetValue((TextMeaning)meaning, out part))
						{
							fullPart.UnionWith(part);
						}
					}
				}
				pattern.RemovePatternType<TextMeaningCondition>();
				if (result != null && result.Count > 0)
				{
					result.IntersectWith(fullPart);
				}
				else
				{
					result = fullPart;
				}
			}



			//NOT sentence conditions
			if (pattern.TryGetPatternConditions<SentenceCondition>(out sentenceConditions))
			{
				foreach (var sentenceCond in sentenceConditions)
				{
					if (sentenceCond.not && result.Contains(sentenceCond.conditionSentence))
					{
						result.Remove(sentenceCond.conditionSentence);
					}
				}
				pattern.RemovePatternType<SentenceCondition>();
			}

			return result;
		}


		private void AddToIndex ( MintyText mintyText)
		{

			if (mintyText is Sentence)
			{
				Sentence s = (Sentence)mintyText;
				HashSet<Sentence> hashSet;

				if (mintyText.responseTypes != null)
				{
					foreach (var responseType in s.responseTypes)
					{
						if (!responseTypes.TryGetValue(responseType, out hashSet))
						{
							hashSet = new HashSet<Sentence>();
							responseTypes[responseType] = hashSet;
						}
						hashSet.Add(s);
					}
				}

				if (s.responses != null)
				{
					foreach (var response in s.responses)
					{
						if (!responses.TryGetValue(response, out hashSet))
						{
							hashSet = new HashSet<Sentence>();
							responses[response] = hashSet;
						}
						hashSet.Add(s);
					}
				}

				if (s.dependencyList != null && s.dependencyList.Count>0)
				{
					foreach (var varName in s.dependencyList)
					{
						if (!dependentOnVariablesSentences.TryGetValue(varName, out hashSet))
						{
							hashSet = new HashSet<Sentence>();
							dependentOnVariablesSentences[varName] = hashSet;
						}
						hashSet.Add(s);
					}
				}
				else
				{
					if (!dependentOnVariablesSentences.TryGetValue(Constants.NO_VARIABLE, out hashSet))
					{
						hashSet = new HashSet<Sentence>();
						dependentOnVariablesSentences[Constants.NO_VARIABLE] = hashSet;
					}
					hashSet.Add(s);
				}

				foreach (var meaning in Enum.GetValues(typeof(TextMeaning)))
				{
					if (((int)meaning & (int)mintyText.meaning) != 0)
					{

						if (!textMeanings.TryGetValue((TextMeaning)meaning, out hashSet))
						{
							hashSet = new HashSet<Sentence>();
							textMeanings[(TextMeaning)meaning] = hashSet;
						}
						hashSet.Add(s);
					}
				}

				//foreach (var topic in mintyText.topics.ToArray())
				//{

				//	if (!topicSentences.TryGetValue(topic, out hashSet))
				//	{
				//		hashSet = new HashSet<Sentence>();
				//		topicSentences[topic] = hashSet;
				//	}
				//	hashSet.Add(s);
				//}
			}
			else
			{

				Word w = (Word)mintyText;
				HashSet<Word> hashSet;
				//foreach (var topic in mintyText.topics.ToArray())
				//{

				//	if (!topicWords.TryGetValue(topic, out hashSet))
				//	{
				//		hashSet = new HashSet<Word>();
				//		topicWords[topic] = hashSet;
				//	}
				//	hashSet.Add(w);
				//}
			}

		}
	}
}
