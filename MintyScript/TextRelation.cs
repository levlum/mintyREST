using System;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{
	
	[System.Serializable]
	public class TextRelation : IComparable, IWeightable
	{
		public int num = -1;
		public string name;
		public string path;
		public float weight = 1f;

		/// <summary>
		/// this relation needs something in the sentence that it may be used.
		/// example: an OBJECT relation with casus DATIV may need another OBJECT with casus ACCUSATIV, which is dependent on the same word (verb)
		/// </summary>
		public TextPattern needs;

		public string praeposition;
		public List<string> ownerPaths;
		//public List<int> ownerWordIDs;
		public RelationType type;

		public TextRelation(RelationType type, string path)
		{
			this.type = type;
			this.path = path;
		}


		/// <summary>
		/// a context (list of dependent objects or subjects) is ok for a relation, if such a relation exist in that context
		/// and if it fits
		/// </summary>
		/// <returns><c>true</c>, if relation fits in the context, <c>false</c> otherwise.</returns>
		/// <param name="context">List of relational nodes.</param>
		/// <param name="usageInfo">info about nodes with allready set relations</param>
		public bool ContextOK(IEnumerable<ParseTreeNode> context, List<TextRelation> allWordRelations, ref Dictionary<ParseTreeNode, List<TextRelation>> usageInfo)
		{

			if (context == null)
			{
				return false;
			}

			//check if this relation is used before or excluded by any previously chosen relations
			foreach (var chosenRelations in usageInfo.Values)
			{
				foreach (var chosenRelation in chosenRelations)
				{
					if (chosenRelation == this)
					{
						return false;
					}
					if (chosenRelation.HasNeeds)
					{
						if (chosenRelation.needs.HasFlag(TextFlag.SINGLE) && type.baseType == chosenRelation.type.baseType)
						{
							return false;
						}

						List<ExcludeRelationCondition> condList;
						if (chosenRelation.needs.TryGetPatternConditions<ExcludeRelationCondition>(out condList))
						{
							foreach (var cond in condList)
							{
								if (!cond.not && cond.GetRelation == this)
								{
									//!cond.not = exclude
									return false;
								}
							}
						}
					}

					//don't know, why this was here. it prevented different relations (paths) to be used.
					//if (chosenRelation.type.Equals(type))
					//{
					//	return false;
					//}

					//maybe ok, if > chosenRelation.type.Equals(type) then add relation to the isageInfo list?
				 }

			}

			//make copy of usageInfo
			Dictionary<ParseTreeNode, List<TextRelation>> usageInfoTrial = new Dictionary<ParseTreeNode, List<TextRelation>>();
			foreach (var keyValue in usageInfo)
			{
				usageInfoTrial[keyValue.Key] = keyValue.Value;
			}

			//does this relation fit in the context?
			if (!ContainsFittingNode(type, this, context, ref usageInfoTrial))
			{
				return false;
			}



			if (HasNeeds)
			{
				if (needs.HasCondition(typeof(EnumCondition<Casus>)))
				{
					//find and test fitting relations:
					bool found = false;
					foreach (TextRelation fittingRelation in allWordRelations)
					{
						if (fittingRelation != this && fittingRelation.type.casus == needs.Get<Casus>())
						{
							if (fittingRelation.ContextOK(context, allWordRelations, ref usageInfoTrial))
							{
								found = true;
								break;
							}
						}
					}

					if (!found)
					{
						//look, if the needed relation is allready taken?
						foreach (var fittingRelations in usageInfoTrial.Values)
						{
							foreach (var fittingRelation in fittingRelations)
							{
								if (fittingRelation != this && fittingRelation.type.casus == needs.Get<Casus>())
								{
									found = true;
									break;
								}
							}
							if (found)
							{
								break;
							}
						}
						if (!found)
						{
							return false;
						}
					}
				}


				if (needs.HasFlag(TextFlag.SINGLE))
				{
					if (usageInfoTrial.Count > 0)
					{
						foreach (var chosenRelations in usageInfo.Values)
						{
							foreach (var chosenRelation in chosenRelations)
							{
								if (type.baseType == chosenRelation.type.baseType)
								{
									return false;
								}
							}
						}
					}
				}

				List<ExcludeRelationCondition> condList;
				if (needs.TryGetPatternConditions<ExcludeRelationCondition>(out condList))
				{
					foreach (var excludeCondition in condList)
					{
						string relationName = excludeCondition.relationID;
						if (MintyTextsContainer.Instance.NamedRelations.ContainsKey(relationName))
						{
							TextRelation relationFromCondition = MintyTextsContainer.Instance.NamedRelations[relationName];


							if (excludeCondition.not)
							{
								//include
								foreach (var nodeRelations in usageInfoTrial)
								{
									if (nodeRelations.Key != context)
									{
										if (!nodeRelations.Value.Contains(relationFromCondition))
										{
											//would it fit?
											if (!relationFromCondition.ContextOK(context, allWordRelations, ref usageInfoTrial))
											{
												return false;
											}
										}
									}
								}
							}
							else
							{
								//exclude

								foreach (var nodeRelations in usageInfoTrial)
								{
									if (nodeRelations.Key != context
										&& nodeRelations.Value.Contains(relationFromCondition))
									{
										return false;
									}
								}
							}
						}
						else
						{
							Logger.LogError(this, "no relation with name \"" + relationName + "\" found.");
							return false;
						}
					}
				}
			}


			usageInfo = usageInfoTrial;
			return true;

		}

		private bool ContainsFittingNode(RelationType relationType, TextRelation relation, IEnumerable<ParseTreeNode> context, ref Dictionary<ParseTreeNode, List<TextRelation>> usageInfo)
		{
			foreach (ParseTreeNode node in context)
			{	
				if (node.RelationType != null
				    && node.RelationType.Equals(relationType))
				{

					//is at this node a given word to use? and does this word fit with this relation?
					bool wordOK = true;
					if (node.HasDynamicData(Constants.KEY_WORD_TO_TAKE))
					{
						wordOK = false;
						Word wordToTake = (Word)node.GetDynamicData(Constants.KEY_WORD_TO_TAKE);
						foreach (string wordPath in wordToTake.paths)
						{
							if (relationType.casus == wordToTake.casus || relationType.casus == Casus.UNDEFINED)
							{
								if (MintyUtils.BelongsTo(wordPath.Split('/'), relation.path.Split('/')))
								{
									wordOK = true;
									break;
								}
							}
						}
					}

					if (wordOK)
					{
						List<TextRelation> chosenRelations;
						if (!usageInfo.TryGetValue(node, out chosenRelations))
						{
							chosenRelations = new List<TextRelation>();
							usageInfo[node] = chosenRelations;
						}
						chosenRelations.Add (relation);
						return true;
					}
				}
			}

			return false;
		}


		//
		//			private bool HasCorrectCasus (Casus casus, ParseTreeNode sentenceNode, List<ParseTreeNode> excludeNodes, out List<ParseTreeNode> nodesWithCasus) {
		//			nodesWithCasus = null;
		//
		//			if (casus == Casus.UNDEFINED) {
		//				//this is a relation without casus like ADJECTIVE
		//				return true;
		//			}
		//
		//			if (casus == Casus.NOMINATIV && sentenceNode.nodeIndex.ContainsKey (ParseNodeType.SUBJECT)) {
		//				foreach (ParseTreeNode testNode in sentenceNode.nodeIndex[ParseNodeType.SUBJECT]) {
		//					if (excludeNodes == null || !excludeNodes.Contains (testNode)) {
		//						if (nodesWithCasus == null) {
		//							nodesWithCasus = new List<ParseTreeNode> (1);
		//						}
		//						nodesWithCasus.Add (testNode);
		//					}
		//				}
		//			}
		//			if ((casus == Casus.DATIV || casus == Casus.AKKUSATIV || casus == Casus.GENETIV)
		//				&& sentenceNode.nodeIndex.ContainsKey (ParseNodeType.OBJECT)) {
		//
		//				foreach (ParseTreeNode testNode in sentenceNode.nodeIndex[ParseNodeType.OBJECT]) {
		//
		//					if (testNode.pattern == null || testNode.pattern.Casus == casus || testNode.pattern.Casus == Casus.UNDEFINED) {
		//						if (excludeNodes == null || !excludeNodes.Contains (testNode)) {
		//							if (nodesWithCasus == null) {
		//								nodesWithCasus = new List<ParseTreeNode> (1);
		//							}
		//							nodesWithCasus.Add (testNode);
		//						}
		//					}
		//				}
		//			}
		//
		//			if (sentenceNode.nodeIndex.ContainsKey (ParseNodeType.WORDS)) {
		//				foreach (ParseTreeNode testNode in sentenceNode.nodeIndex[ParseNodeType.WORDS]) {
		//
		//					if (testNode.pattern != null && testNode.pattern.Casus == casus) {
		//						if (excludeNodes == null || !excludeNodes.Contains (testNode)) {
		//							if (nodesWithCasus == null) {
		//								nodesWithCasus = new List<ParseTreeNode> (1);
		//							}
		//							nodesWithCasus.Add (testNode);
		//						}
		//					}
		//				}
		//			}
		//
		//			if (sentenceNode.nodeIndex.ContainsKey (ParseNodeType.WORDX)) {
		//				foreach (ParseTreeNode testNode in sentenceNode.nodeIndex[ParseNodeType.WORDX]) {
		//
		//					if (testNode.pattern != null && testNode.pattern.Casus == casus) {
		//						if (excludeNodes == null || !excludeNodes.Contains (testNode)) {
		//							if (nodesWithCasus == null) {
		//								nodesWithCasus = new List<ParseTreeNode> (1);
		//							}
		//							nodesWithCasus.Add (testNode);
		//						}
		//					}
		//				}
		//			}
		//
		//			if (nodesWithCasus!= null && nodesWithCasus.Count > 0) {
		//				return true;
		//			}
		//			return false;
		//		}



		public bool HasNeeds
		{
			get
			{
				return needs != null && needs.NumOfConditionTypes > 0;
			}
		}


		public override bool Equals(object obj)
		{
			if (obj == null
				|| !(obj is TextRelation))
				return false;

			var other = (TextRelation)obj;
			if (num >= 0 && other.num >= 0)
				return num == other.num;
			else
				return 
				other.type.Equals(type)
				&& (other.path == null ? path == null : other.path.Equals(path))
				&& (other.name == null ? name == null : other.name.Equals(name))
				&& (other.needs == null ? needs == null : other.needs.Equals(needs))//TODO:overwrite TextPattern.Equals
				&& (other.praeposition == null ? praeposition == null : other.praeposition.Equals(praeposition))
				&& float.Equals(weight, other.weight);
		}

		//		public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2) {
		//			var cnt = new Dictionary<T, int>();
		//			foreach (T s in list1) {
		//				if (cnt.ContainsKey(s)) {
		//					cnt[s]++;
		//				} else {
		//					cnt.Add(s, 1);
		//				}
		//			}
		//			foreach (T s in list2) {
		//				if (cnt.ContainsKey(s)) {
		//					cnt[s]--;
		//				} else {
		//					return false;
		//				}
		//			}
		//			return cnt.Values.All(c => c == 0);
		//		}

		public int CompareTo(object other)
		{
			if (other == null || !(other is TextRelation))
				return 1;
			if (num < 0 || ((TextRelation)other).num < 0)
			{
				return Equals(other) ? 0 : 1;
			}
			return num.CompareTo(((TextRelation)other).num);
		}

		//unsinn
		public override int GetHashCode()
		{
			if (num >= 0)
			{
				return num;
			}

			int combinedHash = path == null ? 0 : path.GetHashCode();
			combinedHash = ((combinedHash << 5) + combinedHash) ^ (type.GetHashCode());
			combinedHash = ((combinedHash << 5) + combinedHash) ^ (name == null ? 0 : name.GetHashCode());
			combinedHash = ((combinedHash << 5) + combinedHash) ^ (HasNeeds ? needs.GetHashCode() : 0); //TODO: overwrite TextPattern.GetHashCode
			combinedHash = ((combinedHash << 5) + combinedHash) ^ (praeposition == null ? 0 : praeposition.GetHashCode());

			return combinedHash;
		}

		public override string ToString()
		{
			return string.Format("[{0}: {1} needs: {2}{3}]", type, path, (HasNeeds ? needs.ToString() : ""), (praeposition == null ? "" : "praeposition: " + praeposition));
		}

		public float GetWeight()
		{
			return weight;
		}
	}


	[System.Serializable]
	public class RelationType : IComparable
	{
		public RelationBaseType baseType = RelationBaseType.UNDEFINED;
		public Genus genus = Genus.UNDEFINED;
		public Casus casus = Casus.UNDEFINED;
		private static Dictionary<long,RelationType> allTypes;

		protected RelationType()
			: base()
		{
		}

		public static RelationType Get(Casus casus)
		{
			switch (casus)
			{
				case Casus.NOMINATIV:
					return Get(RelationBaseType.SUBJECT, casus);
				case Casus.GENETIV:
				case Casus.DATIV:
				case Casus.AKKUSATIV:
					return Get(RelationBaseType.OBJECT, casus);
				default:
					return Get(RelationBaseType.ADJECTIVE, Casus.UNDEFINED);
			}
		}

		public static RelationType Get(RelationBaseType type, Casus casus = Casus.UNDEFINED, Genus genus = Genus.UNDEFINED)
		{
			if (allTypes == null)
				allTypes = new Dictionary<long,RelationType>();

			if (type == RelationBaseType.SUBJECT)
				casus = Casus.NOMINATIV;

			RelationType result;

			long key = MintyUtils.GetKey(6, (int)type, (int)genus, (int)casus);
			
			if (!allTypes.TryGetValue(key, out result))
			{
				result = new RelationType();
				result.baseType = type;
				result.genus = genus;
				result.casus = casus;

				allTypes.Add(key, result);
			}
			return result;
		}


		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (!(obj is RelationType))
				return false;
			return (((RelationType)obj).baseType == baseType
			&& ((RelationType)obj).genus == genus
			&& ((RelationType)obj).casus == casus);
		}

		public int CompareTo(object obj)
		{
			if (obj == null || this.GetType() != obj.GetType())
				return 1;
			int comp = ((int)baseType).CompareTo((int)(((RelationType)obj).baseType));
			if (comp != 0)
				return comp;
			comp = ((int)genus).CompareTo((int)(((RelationType)obj).genus));
			if (comp != 0)
				return comp;
			comp = ((int)casus).CompareTo((int)(((RelationType)obj).casus));
			
			return comp;
		}

		public override int GetHashCode()
		{
			int combinedHash = (int)baseType;
			combinedHash = ((combinedHash << 2) | (int)genus);
			combinedHash = ((combinedHash << 3) | (int)casus);
//			combinedHash = ((combinedHash << 5) + combinedHash) ^ ((int)genus);
//			combinedHash = ((combinedHash << 5) + combinedHash) ^ ((int)casus);

			return combinedHash;
		}

		public override string ToString()
		{
			return string.Format("[{0}, genus={1}, casus={2}]", baseType, genus, casus);
		}
	}
}

