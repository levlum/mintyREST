using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

#if MintyWeb
using System.Linq;
#endif

namespace Com.Gamegestalt.MintyScript
{
	
	public enum ParseNodeType
	{
		UNKNOWN,
		TEXT,
		WORDS,
		TOPIC,
		SENTENCES,
		WORDX,
		SUBJECT,
		OBJECT,
		ADJECTIVE,
		SEX_ME,
		SEX_YOU,
		SEX_OTHER,
		NAME_ME,
		NAME_YOU,
		NAME_OTHER,
		ALTERNATIVES,
		SWITCH_GENUS,
		SWITCH_NUMERUS,
		SWITCH_CASUS,
		SWITCH_REFLEXIVE,
		SWITCH_LAST_OF_X,
		PARTS,
		UP,
		LOW,
		EMPTY,
		INVISIBLE,

		/// <summary>
		/// PREDEFINED is a fixed list of predefined but known words.
		/// </summary>
		PREDEFINED,
		ADVERB,
		VERB
	}
	
	
	//every treenode has a list of all words and topics of itself and all descendants
	public class ParseTreeNode : IEnumerable<ParseTreeNode>
	{
		#region static fields

		private static int lastID = 10000;

		private static int UniqueID
		{
			get
			{
				lastID++;
				return lastID - 1;
			}
		}

		static Random _random = new Random();

		#endregion

		#region IEnumerable


		public IEnumerator<ParseTreeNode> GetEnumerator()
		{
			if (parts != null && parts.Length > 0)
			{
				foreach (var node in parts)
				{
					foreach (var childNode in node)
					{
						yield return childNode;
					}
				}
			}

			if (alternativeParts != null && alternativeParts.Count > 0)
			{
				foreach (var node in alternativeParts)
				{
					foreach (var childNode in node)
					{
						yield return childNode;
					}
				}
			}

			yield return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		////if this is the root node, this is the index of all nodes in the tree
		//public Dictionary<ParseNodeType, List<ParseTreeNode>> nodeIndex;

		private string unprocessedText;

		public string UnprocessedText
		{
			get { return this.unprocessedText; }
			set
			{
				this.unprocessedText = value; 
				if (type == ParseNodeType.EMPTY)
					staticSolution = "";

				if (IsStatic)
				{
					staticSolution = value;

					//define last word Pattern
					string trimed = value.Trim().ToLower();
					string[] words = trimed.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (words.Length > 0 && MintyTextsContainer.Instance.WordForms.ContainsKey(words[words.Length - 1]))
					{
						pattern = new TextPattern(new FixedTextCondition(words[words.Length - 1]));
					}

					//set as last node
					SetNodeAs(TextPosition.LINE_END);
				}
			}
		}

		public bool IsStatic
		{
			get
			{
				return (type == ParseNodeType.TEXT
				|| type == ParseNodeType.PREDEFINED);
			}
		}


		public bool IsLeaf
		{
			get
			{
				return Constants.NODE_TYPE_INFO[type].isLeaf;
//				return (type != ParseNodeType.ALTERNATIVES
//				&& type != ParseNodeType.PARTS
//				&& type != ParseNodeType.SEX
//				&& type != ParseNodeType.SWITCH);
			}
		}

		private bool _processed = false;

		public bool Processed
		{
			get
			{
				return IsStatic || _processed;
			}
		}

		private Dictionary<int,object> dynamicData;


		public bool HasDynamicData(int key)
		{
			if (dynamicData == null)
				return false;
			return dynamicData.ContainsKey(key);
		}

		public object GetDynamicData(int key)
		{
			if (dynamicData == null)
				return null;
			object data;
			if (dynamicData.TryGetValue(key, out data))
			{
				return data;
			}
			return null;
		}

		private string staticSolution;
		private string dynamicSolutionText;
		//solution for one processing only. will be reset after processing.
		private MintyText dynamicSolutionComicText;

		public string SolutionString
		{
			get
			{
				if (staticSolution != null)
					return staticSolution;
				return dynamicSolutionText;
			}
		}

		public MintyText SolutionComicText
		{
			get
			{
				if (staticSolution != null)
				{
					if (cText != null)
						return cText;
					if (alternativeComicTexts != null && alternativeComicTexts.Count > 0)
					{
						MintyTextsContainer.TryGetRandomElement<MintyText>(alternativeComicTexts.Values, out MintyText result);
						return result;
					}
				}
				return dynamicSolutionComicText;
			}
		}

		public void SetDynamicSolution(string text, MintyText cText)
		{
			dynamicSolutionText = text;
			dynamicSolutionComicText = cText;
			_processed = true;
		}

		public string name = null;
		public ParseTreeNode root;
		public ParseTreeNode parent = null;

		//this has only a value, if this node is a root-node of (this-cText) Sentence or Word
		public MintyText cText = null;

		public ParseTreeNode(ParseNodeType type, string text, string name = null)
		{
			UnprocessedText = text;
			
			this.type = type;
			this.name = name;

		}

		//		public ParseTreeNode (ParseNodeType type, ComicText cText, string variableName = null) : this (type, cText.Text, variableName)
		//		{
		//			this.cText = cText;
		//		}

		//if this node stores words, it stores the pattern for the selection of the words here.
		//TODO: separate dynamic and static parts of the pattern!
		private TextPattern _pattern = null;

		public TextPattern pattern
		{
			get { return _pattern; }
			set { _pattern = value; }
		}

		public ParseNodeType type;

		private WordType _wordType = WordType.UNDEFINED;

		public void CalculateWordType()
		{
			_wordType = WordType;
		}

		public WordType WordType
		{
			get
			{
				if (_wordType != WordType.UNDEFINED)
				{
					return _wordType;
				}

				if (pattern != null)
				{
					_wordType = pattern.Get<WordType>();


					if (_wordType == WordType.UNDEFINED
					    && pattern.HasCondition(typeof(PathCondition)))
					{
						List<WordType> wordTypes = new List<WordType>();
						List<PathCondition> condList;
						if (pattern.TryGetPatternConditions<PathCondition>(out condList))
						{
							foreach (var pCond in condList)
							{
								WordType wt = MintyUtils.GetTypeFromPath(pCond.conditionPath[0]);
								if (!wordTypes.Contains(wt))
								{
									wordTypes.Add(wt);
								}
							}
						}
						if (wordTypes.Count == 1)
						{
							_wordType = wordTypes[0];
						}

					}
				}



				if (_wordType == WordType.UNDEFINED)
				{
					switch (type)
					{
						case ParseNodeType.ADJECTIVE:
							_wordType = WordType.ADJEKTIV;
							break;
						case ParseNodeType.OBJECT:
						case ParseNodeType.SUBJECT:
							_wordType = WordType.SUBSTANTIV;
							break;
					}
				}

				if (_wordType != WordType.UNDEFINED)
				{
					if (pattern == null)
					{
						pattern = new TextPattern();
					}
					if (!pattern.HasCondition(typeof(EnumCondition<WordType>)))
					{
						pattern.AddPatternConditions(new EnumCondition<WordType>(_wordType));
					}
				}

				return _wordType;
			}
			set
			{
				_wordType = value;
			}
		}

		//if this node is OBJECT(v), SUBJECT(v), ..
		public RelationType RelationType
		{
			get
			{
				switch (type)
				{
					case ParseNodeType.ADJECTIVE: 
						return RelationType.Get(RelationBaseType.ADJECTIVE);
					case ParseNodeType.ADVERB:
						return RelationType.Get(RelationBaseType.ADVERB);
					case ParseNodeType.OBJECT:
						Casus casus = pattern == null ? Casus.UNDEFINED : pattern.Get<Casus>();
						return RelationType.Get(RelationBaseType.OBJECT, casus);
					case ParseNodeType.SUBJECT:
						return RelationType.Get(RelationBaseType.SUBJECT, Casus.NOMINATIV);
					case ParseNodeType.VERB:
						return RelationType.Get(RelationBaseType.VERB);
					default:
						return null;
				}
			}
		}
		
		//the text result of this node depends on other nodes.
		//example 1: if this is a "SUBJECT" node, it depends on the named (or first) verb in the same tree. dependency points to the "VERB" (=WORDS) node
		//example 2: if this is some kind of "WORD" node, its "form" may depend on another word.
		public Dictionary<string,ParseTreeNode> dependencies;

		public void AddDependency(string varName, ParseTreeNode otherNode = null)
		{
			if (dependencies == null)
			{
				dependencies = new Dictionary<string, ParseTreeNode>(1);
			}

			dependencies[varName] = otherNode;

			if (otherNode != null && RelationType != null)
			{
				otherNode.AddRelation(RelationType, this);
			}
		}

		public string BasicDependencyName
		{
			get
			{
				if (dependencies != null && dependencies.Count > 0)
				{
					foreach (var key in dependencies.Keys)
					{
						return key;
					}
				}
				return null;
			}
		}

		public ParseTreeNode BasicDependencyNode
		{
			get
			{
				if (dependencies != null && dependencies.Count > 0)
				{
					foreach (var val in dependencies.Values)
					{
						return val;
					}
				}
				return null;
			}
		}

		public Dictionary<string, ParseTreeNode> GetAdditionalDependencyNodes()
		{
			if (dependencies != null
			    && dependencies.Count > 1)
			{

				Dictionary<string, ParseTreeNode> additionalNameNodes = null;

				int i = 0;
				foreach (var nameNode in dependencies)
				{
					if ((i++) > 0)
					{

						if (nameNode.Key != Constants.VAR_ARTICLE)
						{
							if (additionalNameNodes == null)
							{
								additionalNameNodes = new Dictionary<string, ParseTreeNode>(1);
							}

							additionalNameNodes[nameNode.Key] = nameNode.Value;
						}
					}
				}

				return additionalNameNodes;
			}

			return null;
		}

		//example: if this is a verb, it has relationships with objects or subjects
		private void AddRelation(RelationType relType, ParseTreeNode relationNode)
		{
			if (relationships == null)
				relationships = new Dictionary<RelationType, ParseTreeNode>();
			if (!relationships.ContainsKey(relType))
				relationships.Add(relType, relationNode);
		}

		private Dictionary<RelationType, ParseTreeNode> relationships;

		public bool HasRelations
		{
			get
			{
				return (relationships != null && relationships.Count > 0);
			}
		}

        public bool HasRelationsInSyntagma (Syntagma syntagma)
        {
            return (syntagma!=null && relationships != null && relationships.Count > 0 && relationships.Values.Intersect(syntagma.nodes).Any());
        }

        public ParseTreeNode GetRelationship(RelationType relType)
		{
			ParseTreeNode relNode;
			if (relationships != null && relationships.TryGetValue(relType, out relNode))
			{
				return relNode;
			}
			return null;
		}

		//		public RelationType[] RelationTypes {
		//			get {
		//				if (relationships != null) {
		//					RelationType[] result = new RelationType[relationships.Keys.Count];
		//					int i = 0;
		//					foreach (RelationType rt in relationships.Keys) {
		//						result [(i++)] = rt;
		//					}
		//					return result;
		//				}
		//				return null;
		//			}
		//		}

		//		public List<WordRelation> GetWordRelations (Word word)
		//		{
		//			if (relationships != null) {
		//				List<WordRelation> wrs = new List<WordRelation> ();
		//				foreach (RelationType relType in relationships.Keys) {
		//					WordRelation wr = new WordRelation (word, relType);
		//					foreach (ComicText act in alternativeComicTexts.Values) {
		//						if (act != null && act is Word) {
		//
		//							//possible optimization: compare path of act with relation-paths, if there is no fit, ...
		//							foreach (TextRelation textRelation in ((Word)act).GetRelationPaths (relType)) {
		////								if ((word.casus == Casus.UNDEFINED || word.casus == relType.casus)
		//								if (textRelation.ContextOK (root)
		//									&& word.BelongsTo (textRelation.path.Split ('/'))
		//								    && !wr.sources.ContainsKey (act.num)) {
		//
		//									wr.sources.Add (act.num, (Word)act);
		//									wr.probabilitySum += act.weight;
		//									break;
		//								}
		//							}
		//
		//						}
		//					}
		//					if (wr.sources.Count > 0) {
		//						wrs.Add (wr);
		//					}
		//				}
		//				if (wrs.Count > 0)
		//					return wrs;
		//			}
		//			return null;
		//		}

		//		public List<WordRelation> GetTopicRelations (ComicTopic topic)
		//		{
		//			if (relationships != null) {
		//				List<WordRelation> wrs = new List<WordRelation> ();
		//				foreach (RelationType relType in relationships.Keys) {
		//					WordRelation wr = new WordRelation (null, relType);
		//					foreach (ComicText act in alternativeComicTexts.Values) {
		//						if (act != null && act is Word) {
		//
		//							var relatedWords = ((Word)act).GetAllRelatedWords (relType, root);
		//							if (relatedWords != null && relatedWords.Count > 0) {
		//								foreach (var relWordsPair in relatedWords) {
		//									foreach (Word relWord in relWordsPair.Value) {
		//										if (relWord.topics != null
		//										    && relWord.topics.Contains (topic)
		//										    && !wr.sources.ContainsKey (act.num)) {
		//
		//											wr.sources.Add (act.num, (Word)act);
		//											wr.probabilitySum += act.weight;
		//											break;
		//										}
		//									}
		//								}
		//							}
		//						}
		//					}
		//					if (wr.sources.Count > 0) {
		//						wrs.Add (wr);
		//					}
		//				}
		//				if (wrs.Count > 0)
		//					return wrs;
		//			}
		//			return null;
		//		}

		private ParseTreeNode[] parts = null;

		public ParseTreeNode[] Parts
		{
			get { return parts; }
			set
			{
				parts = value;
//				if (parts != null && parts.Length==3){
//					for (int i=0; i<3; i++) {
//						if (parts[i]!=null) parts[i].AddParent (this);
//					}
//				}
			}
		}

		[System.NonSerialized]
		public float[] alternativePartsWeights = null;
		[System.NonSerialized]
		public List<ParseTreeNode> alternativeParts = null;

		public void AddAlternativePart(ParseTreeNode part)
		{
			if (part == null)
				return;
			if (alternativeParts == null)
				alternativeParts = new List<ParseTreeNode>();
			alternativeParts.Add(part);
//			part.AddParent (this);
		}

		[System.NonSerialized]
		//list of words (if this is a WORDS node
		//list of sentences (if this is a SENTENCES node)
		public Dictionary<int,MintyText> alternativeComicTexts = null;

		public void AddAlternativeComicText(MintyText comicText)
		{
			if (comicText == null)
				return;
			if (alternativeComicTexts == null)
				alternativeComicTexts = new Dictionary<int,MintyText>();
			if (!alternativeComicTexts.ContainsKey(comicText.num))
				alternativeComicTexts.Add(comicText.num, comicText);
		}

		private HashSet<Syntagma> subtreesCalculated = new HashSet<Syntagma>();
		private bool structureCalculated = false;

		/// <summary>
		/// a syntagma is a combination of relationType-nodes (object, subject, ..) 
		/// with rules that say how to get this combination of nodes 
		/// </summary>
		private List<Syntagma> _syntagmas;

		public List<Syntagma> Syntagmas
		{
			get
			{
				if (!structureCalculated)
				{
//					CalculateStructure ();
					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
				}

				return _syntagmas;
			}
		}

		public List<string> AncestorVariables
		{
			get
			{
				List<string> ancestorVariables = new List<string>();
				if (!string.IsNullOrEmpty(name))
				{
					ancestorVariables.Add(name);
				}
				if (parent != null && parent.type != ParseNodeType.PARTS)
				{
					ancestorVariables.AddRange(parent.AncestorVariables);
				}
				return ancestorVariables;
			}
		}

		public Dictionary<string, ParseTreeNode> AncestorVariableNodes
		{
			get
			{
				Dictionary<string, ParseTreeNode> ancestorVariableNodes = new Dictionary<string, ParseTreeNode>();
				if (!string.IsNullOrEmpty(name))
				{
					ancestorVariableNodes.Add(name, this);
				}
				if (parent != null && parent.type != ParseNodeType.PARTS)
				{
					var parentNamedNodes = parent.AncestorVariableNodes;
					if (parentNamedNodes != null && parentNamedNodes.Count > 0)
					{
						foreach (var nameNode in parentNamedNodes)
						{
							ancestorVariableNodes.Add(nameNode.Key, nameNode.Value);
						}
					}
				}
				return ancestorVariableNodes;
			}
		}

		private float _complexity = -1f;

		public float Complexity
		{
			get
			{
//				if (!subtreesCalculated.Count == root.Syntagmas.Count)
//				{
////					CalculateStructure ();
//					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
//				}
				return _complexity;
			}
		}

		public Dictionary<ParseTreeNode, TextPattern> lastWordPattern = null;

		public void AddLastWordPattern(ParseTreeNode node, TextPattern pattern)
		{
			if (pattern == null)
			{
				return;
			}

			if (lastWordPattern == null)
			{
				lastWordPattern = new Dictionary<ParseTreeNode, TextPattern>();
			}

			lastWordPattern[node] = pattern;
		}

		/** dynamic node-info, this is deleted after processing **/
		private Dictionary<TextPosition, bool> textPositionDict;

		/** TODO: only LINE_END is working so far, ... **/
		//		public ParseTreeNode GetNodeByPosition (Syntagma syntagma)
		//		{
		//			if (!subtreesCalculated) {
		//				CalculateSubtrees ();
		//			}
		//			return syntagmaSubtreeInfo [syntagma];
		//		}

		/** TODO: only LINE_END and NOT_LINE_END are working so far, ... **/
		public bool NodeIs(TextPosition textPosition)
		{

			switch (textPosition)
			{
				case TextPosition.UNDEFINED:
					return true;
			//return syntagmaDict == null || syntagmaDict.Count==0 || syntagmaDict.ContainsKey(Syntagma.UNDEFINED);
				case TextPosition.NOT_LINE_END:
					return !NodeIs(TextPosition.LINE_END);
				case TextPosition.NOT_LINE_START:
					return !NodeIs(TextPosition.LINE_START);
				default:
					return (textPositionDict != null
					&& textPositionDict.ContainsKey(textPosition)
					&& textPositionDict[textPosition]);
			}
		}


		/// <summary>
		/// The words for each syntagma.
		/// </summary>
		public Dictionary<Syntagma, HashSet<Word>> words = null;

		/// <summary>
		/// Gets all the words of this and all ancestor nodes.
		/// </summary>
		/// <value>
		/// The words.
		/// </value>
		//		public HashSet<Word> Words
		//		{
		//			get
		//			{
		////				if (!subtreesCalculated)
		////				{
		////					//					CalculateStructure ();
		////					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
		////				}
		////				if (_words == null)
		////				{
		////					_words = new HashSet<Word>();
		////				}
		//				return _words;
		//			}
		//		}

		private List<Sentence> _sentences = null;

		/// <summary>
		/// Gets all sentences of this and all ancestor nodes.
		/// </summary>
		/// <value>
		/// The sentences.
		/// </value>
		public List<Sentence> Sentences
		{
			get
			{
//				if (!subtreesCalculated)
//				{
//					//					CalculateStructure ();
//					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
//				}
				return _sentences;
			}
		}

		public TopicFlags topics;

//		public Dictionary<int,ComicTopic> Topics
//		{
//			get
//			{
////				if (!subtreesCalculated)
////				{
////					//					CalculateStructure ();
////					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
////				}
		//		return topics;
		//	}
		//}

		private float _WordsProbabilitySum = 0;

		public float WordsProbabilitySum
		{
			get
			{
//				if (!subtreesCalculated)
//				{
//					//					CalculateStructure ();
//					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
//				}
				return _WordsProbabilitySum;
			}
		}

		private float _ProbabilitySum = 0;

		/// <summary>
		/// Gets the probability sum of all sentences and words in this part of the tree.
		/// </summary>
		/// <value>
		/// The probability sum.
		/// </value>
		public float ProbabilitySum
		{
			get
			{
//				if (!subtreesCalculated)
//				{
//					//					CalculateStructure ();
//					Logger.LogWarning(this, "value not valid, subtrees not calculated yet!");
//				}
				return _ProbabilitySum;
			}
		}

		#region public methods

		public void SetNodeAs(TextPosition textPosition, bool val = true)
		{
			if (textPositionDict == null)
			{
				textPositionDict = new Dictionary<TextPosition, bool>();
			}
			textPositionDict[textPosition] = val;
		}


		/// <summary>
		///  storage for different dynamic data. will be deleted after processing
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="data">Data.</param>
		public void StoreDynamicData(int key, object data)
		{
			if (dynamicData == null)
				dynamicData = new Dictionary<int, object>();
//			if (!dynamicData.ContainsKey(key))
//			{
			dynamicData[key] = data;
//			}
		}


		public void SetRoot()
		{
			root = this;
//			if (root._syntagmas == null)
//			{
//				root._syntagmas = new List<Syntagma>();
//			}
//			if (root._syntagmas.Count == 0)
//			{
//				root._syntagmas.Add(new Syntagma());
//				//root._syntagmas [0].nodes.Add (this);
//			}
			root.CalculateStructure();
		}

		public List<string> ToSyntagmaStrings()
		{
			List<string> results = new List<string>();

			if (parts != null && parts.Length > 0)
			{
				foreach (ParseTreeNode pn in parts)
				{
					if (results.Count == 0)
					{
						results = pn.ToSyntagmaStrings();
					}
					else
					{
						List<string> newResults = new List<string>();
						foreach (string partString in pn.ToSyntagmaStrings())
						{
							foreach (string oneResult in results)
							{
								newResults.Add(oneResult + partString);
							}
						}

						results = newResults;
					}
				}
			}
			else if (alternativeParts != null && alternativeParts.Count > 0)
			{
				if (type == ParseNodeType.ALTERNATIVES)
				{
					for (int i = 0; i < alternativeParts.Count; i++)
					{
						var aParts = alternativeParts[i].ToSyntagmaStrings();
						results.AddRange(aParts);
					}
				}
				else
				{
					string typeNodeString = "?";
					switch (type)
					{
						case ParseNodeType.SEX_ME:
						case ParseNodeType.SEX_OTHER:
						case ParseNodeType.SEX_YOU:
						case ParseNodeType.SWITCH_CASUS:
						case ParseNodeType.SWITCH_GENUS:
						case ParseNodeType.SWITCH_NUMERUS:
						case ParseNodeType.SWITCH_REFLEXIVE:
							typeNodeString = "";
							break;
					}

					//					string[] alternatives = new string[alternativeParts.Count];
					//					for (int a = 0; a < alternativeParts.Count; a++)
					//					{
					//						alternatives[a] = alternativeParts
					//					}
					//
					//					results.Add(string.Format("[{0}{1}]", typeNodeString, string.Join(":", alternatives)));
					results.Add(unprocessedText);
				}
			}

			//leafNode
			else
			{
				results.Add(unprocessedText);
			}

			if (!string.IsNullOrEmpty(name))
			{
				for (int i = 0; i < results.Count; i++)
				{
					results[i] = string.Format("[{0}={1}]", name, results[i]);
				}
			}

			return results;
		}

		public override string ToString()
		{
			string result = "";
			bool leafNode = true;
			if (parts != null && parts.Length > 0)
			{
				leafNode = false;
				foreach (ParseTreeNode pn in parts)
				{
					result += "{" + pn.ToString() + "} ";
				}
			}
			if (alternativeParts != null && alternativeParts.Count > 0)
			{
				leafNode = false;
				result += "[";
				for (int i = 0; i < alternativeParts.Count; i++)
				{
					result += alternativeParts[i].ToString();
					if (i < alternativeParts.Count - 1)
					{
						result += " | ";
					}
				}
				result += "]";
			}

			if (leafNode)
			{
				if (type == ParseNodeType.TEXT)
					result = unprocessedText;
				else
					result = type.ToString() + ":" + unprocessedText;
			}

			if (!string.IsNullOrEmpty(name))
			{
				result = name + "=" + result;
			}

			return result;
			//return unprocessedText;
			//return string.Format ("[ParseTreeNode: UnprocessedText={0}, Parts={1}, AlternativeParts={2}, Complexity={3}, Words={4}, Topics={5}, WordsProbabilitySum={6}]", UnprocessedText, Parts, AlternativeParts, Complexity, Words, Topics, WordsProbabilitySum);
		}


		/// <summary>
		/// Allways reset this instance after processing!
		/// </summary>
		public void Reset()
		{
			_processed = false;
			dynamicSolutionText = null;
			dynamicSolutionComicText = null;

			dynamicData = null;
			
			if (parts != null)
			{
				foreach (ParseTreeNode node in parts)
				{
					if (node != null)
						node.Reset();
				}
			}
			if (alternativeParts != null)
			{
				foreach (ParseTreeNode node in alternativeParts)
				{
					if (node != null)
						node.Reset();
				}
			}
//			if (alternativeComicTexts != null) {
//				foreach (ComicText act in alternativeComicTexts.Values) {
//					if (act != null)
//						act.ParseTree.Reset ();
//				}
//			}
		}


		#endregion

		#region private methods

		private void ShuffleRelations()
		{
			#if MintyWeb
			if (relationships == null) {
				return;
			}

			relationships = relationships.OrderBy(x => _random.Next()).ToDictionary(item => item.Key, item => item.Value);
			#endif
		}

		/// <summary>
		/// Combines the syntagmas.
		/// </summary>
		/// <returns>combined syntagmas.</returns>
		/// <param name="sy1">first List of Syntagmas.</param>
		/// <param name="sy2">second List of Syntagmas.</param>
		private List <Syntagma> CombineSyntagmas(List<Syntagma> sys1, List<Syntagma> sys2)
		{
			List <Syntagma> result = null;

			if ((sys1 != null && sys1.Count > 0) || (sys2 != null && sys2.Count > 0))
			{
				result = new List<Syntagma>();

				if (sys1 == null)
				{
					sys1 = new List<Syntagma>();
				}
				if (sys1.Count == 0)
				{
					Syntagma emptySyntagma = new Syntagma();
					sys1.Add(emptySyntagma);
				}
				if (sys2 == null)
				{
					sys2 = new List<Syntagma>();
				}
				if (sys2.Count == 0)
				{
					Syntagma emptySyntagma = new Syntagma();
					sys2.Add(emptySyntagma);
				}


				foreach (Syntagma s1 in sys1)
				{
					foreach (Syntagma s2 in sys2)
					{
						Syntagma newSyntagma = new Syntagma();
						newSyntagma.nodes.UnionWith(s1.nodes);
						newSyntagma.nodes.UnionWith(s2.nodes);
						newSyntagma.weight = s1.weight * s2.weight;

						foreach (var keyValue in s1.decissionValues)
						{
							newSyntagma.decissionValues[keyValue.Key] = keyValue.Value;
						}
						foreach (var keyValue in s2.decissionValues)
						{
							newSyntagma.decissionValues[keyValue.Key] = keyValue.Value;
						}

						foreach (var keyValue in s1.variables)
						{
							newSyntagma.variables[keyValue.Key] = keyValue.Value;
						}
						foreach (var keyValue in s2.variables)
						{
							newSyntagma.variables[keyValue.Key] = keyValue.Value;
						}

						//newSyntagma.decissionValues.
						result.Add(newSyntagma);
					}
				}
			}

			return result;
		}

		private void StoreWeights()
		{

			if (alternativePartsWeights != null)
			{
				return;
			}

			bool probabilitySumNeverZero = true;
			float[] sameRandomWeights = new float[alternativeParts.Count];
			float[] wordWeights = new float[alternativeParts.Count];

			for (int i = 0; i < alternativeParts.Count; i++)
			{
				sameRandomWeights[i] = 1f;
				wordWeights[i] = alternativeParts[i].ProbabilitySum;
				if (wordWeights[i] <= 0f)
				{
					probabilitySumNeverZero = false;
				}
			}

			if (probabilitySumNeverZero)
			{
				alternativePartsWeights = wordWeights;
			}
			else
			{
				alternativePartsWeights = sameRandomWeights;
			}
		}

		/// <summary>
		/// Calculates the structure: set root, parent, lineEnds, Syntagmas
		/// 1. Parse node one by one (treestructure is built) (ComicTextProcessor.CreateParseTree())
		/// 2. set links between nodes and its dependent nodes (by variable names)
		/// 3. SetRoot()
		/// 4. CalculateStructure()
		/// 5. CalculateRelationalWords()
		/// 6. CalculateWordsAndPattern()
		/// </summary>
		private void CalculateStructure()
		{
			if (structureCalculated)
			{
				return;
			}

			if (this == root)
			{
//				Logger.DebugL (this, "CalculateStructure(), delete me!");
			}

			_complexity = 0;

			AddLastWordPattern(this, pattern);

			//root.AddNodeIfNotExistent(this);

			if (this == root)
			{
				ShuffleRelations();
			}

			var vars = AncestorVariables;
			//TODO: add STATIC words! (be carteful. last time i tried, i got 50.700 Syntagmas and more. algorythm hang)
			if (IsLeaf
			    && root.cText is Sentence
			    && ((vars != null && vars.Count > 0)
			    || RelationType != null
			    || type == ParseNodeType.WORDS
                || type == ParseNodeType.SENTENCES
			    //|| pattern != null
                ))
			{
				_syntagmas = new List<Syntagma>(){ new Syntagma(this) };
//				Syntagma syntagma = new Syntagma();
//				_syntagmas.Add(syntagma);
//				syntagma.nodes.Add(this);
			}


			//add words by path
			if (type == ParseNodeType.WORDS)
			{
				if (pattern == null)
				{
					pattern = new TextPattern();
				}


				//moved this to "after parsing" when syntagmas are known
//				if (relationships != null && relationships.Count>0) {
//					pattern.AddPatternConditions (new ContextCondition (relationships.Values, this));
//				}


				//moved down to general "add words for pattern"
//				List<Word> words = ComicTextsContainer.Get.GetWords (pattern); //find correct form in actual processing
//								
//				//is the path incorrect?
//				if (words == null || words.Count == 0 || (words.Count == 1 && words [0].Text.IndexOf ("Eeep") >= 0)) {
//					Logger.LogError (this, "No words for \"" + pattern + "\" found." + (root.cText == null ? "" : "\nSentence: " + root.cText.Text));
//				} else {
//					foreach (Word w in words) {
//						if (w != null) {
//							AddAlternativeComicText (w);
//						}
//					}
//				}
			}


	
			if (parts != null)
			{
				_complexity += 1f;
				bool lastNodeIsEndMark = 
					parts.Length >= 2
					&& parts[parts.Length - 1].IsStatic
					&& parts[parts.Length - 1].UnprocessedText.Trim().Length == 1;

				for (int n = 0; n < parts.Length; n++)
				{

					//is this the "LINE_END" node?
					bool lineEnd = (n == parts.Length - 1 && !lastNodeIsEndMark)
					               || (n == parts.Length - 2 && lastNodeIsEndMark);
					if (lineEnd)
					{

						parts[n].SetNodeAs(TextPosition.LINE_END);
					}
					else
					{
						parts[n].SetNodeAs(TextPosition.LINE_END, false);
					}

					//set parent
					parts[n].root = root;
					parts[n].parent = this;
					parts[n].CalculateStructure();

					//build Syntagmas for this parseTree
					_syntagmas = CombineSyntagmas(_syntagmas, parts[n]._syntagmas);

					if (lineEnd)
					{
						lastWordPattern = parts[n].lastWordPattern;
					}

				}



			}
			else if (alternativeParts != null)
			{


				_complexity += 1f;
				bool nodeIsFreeToChoose = type == ParseNodeType.ALTERNATIVES;

//				if (nodeIsFreeToChoose && _lastWordPattern == null)
//				{
//					_lastWordPattern = new TextPattern();
//				}


				int i = 0;
                bool oneNodeHasASyntagma = false;
                foreach (ParseTreeNode node in alternativeParts)
                {

                    //set parent
                    node.root = root;
                    node.parent = this;
                    node.CalculateStructure();
                    oneNodeHasASyntagma = oneNodeHasASyntagma || node._syntagmas != null && node._syntagmas.Count > 0;
                }

                foreach (ParseTreeNode node in alternativeParts)
                { 
                //Create new Syntagma for every alternative with a syntagma
                    if (oneNodeHasASyntagma)
					{
						if (_syntagmas == null)
						{
							_syntagmas = new List<Syntagma>();
						}

                        if (node._syntagmas != null && node._syntagmas.Count > 0)
                        {
                            foreach (Syntagma ns in node._syntagmas)
                            {
                                Syntagma newSyntagma = new Syntagma();
                                _syntagmas.Add(newSyntagma);
                                newSyntagma.nodes.UnionWith(ns.nodes);
                                newSyntagma.decissionValues[this] = i;

                                foreach (var decissionKeyVal in ns.decissionValues)
                                {
                                    newSyntagma.decissionValues[decissionKeyVal.Key] = decissionKeyVal.Value;
                                }

                                foreach (var keyVal in ns.variables)
                                {
                                    newSyntagma.variables[keyVal.Key] = keyVal.Value;
                                }
                            }
                        }
                        else
                        {
                            Syntagma newSyntagma = new Syntagma(node);
                            _syntagmas.Add(newSyntagma);
                            newSyntagma.decissionValues[this] = i;
                        }
                    }

					if (nodeIsFreeToChoose && node.lastWordPattern != null)
					{
						foreach (var lastWordPatternKeyVal in node.lastWordPattern)
						{
							AddLastWordPattern(lastWordPatternKeyVal.Key, lastWordPatternKeyVal.Value);
						}
					}


					node.SetNodeAs(TextPosition.LINE_END, NodeIs(TextPosition.LINE_END) || NodeIs(TextPosition.UNDEFINED));
					i++;
				}

				//if all alternative nodes do not have any relational-nodes, remove the syntagmes of that subtree!
				//bool hasRelationalNode = false;
				//if (_syntagmas != null)
				//{
				//	foreach (Syntagma sy in _syntagmas)
				//	{
				//		if (sy.nodes != null && sy.nodes.Count > 0)
				//		{
				//			hasRelationalNode = true;
				//			break;
				//		}
				//	}
				//}
//				if (!hasRelationalNode)
//				{
//					_syntagmas = new List<Syntagma>(new Syntagma[]{ new Syntagma() });
//				}

			}



			structureCalculated = true;

			//end of parsing
			if (root == this)
			{
				if (_syntagmas != null)
                {
                    foreach (Syntagma syntagma in _syntagmas)
                    {
                        CalculateRelationalWords(syntagma);
                    }
                    //_syntagmas = new List<Syntagma>(){ new Syntagma(this) };
				}
                else
                {
                    _syntagmas = new List<Syntagma>() { new Syntagma(this) }; 
                }

            }
		}


		/// <summary>
		/// Calculates specific words and pattern for one syntagma.
		///
		/// 1. Parse node one by one (treestructure is built) (ComicTextProcessor.CreateParseTree())
		/// 2. set links between nodes and its dependent nodes (by variable names)
		/// 3. SetRoot()
		/// 4. CalculateStructure()
		/// 5. CalculateRelationalWords()
		/// 6. CalculateWordsAndPattern()
		/// </summary>
		/// <param name="syntagma">Syntagma (root-syntagma, including all nodes).</param>
		private void CalculateWordsAndPattern(Syntagma syntagma)
		{
			if (subtreesCalculated.Contains(syntagma))
			{
				return;
			}


			if (this == root)
			{
//				Logger.DebugL (this, "CalculateWordsAndPattern(), delete me!");
			}

			_complexity = 0;


			if (parts != null)
			{
				_complexity += 1f;

				foreach (ParseTreeNode node in  parts)
				{

					node.CalculateWordsAndPattern(syntagma);
					AddFromAncestorNode(syntagma, node);

				}
			}



			if (alternativeParts != null)
			{

				_complexity += 1f;

				foreach (ParseTreeNode node in alternativeParts)
				{
					
					node.CalculateWordsAndPattern(syntagma);
					AddFromAncestorNode(syntagma, node);

				}
			}






			//add all words for pattern
			if (pattern != null && syntagma.nodes.Contains(this))
			{

				//if this is an adjective-node, add gramma from substantiv to the pattern
				if (type == ParseNodeType.ADJECTIVE)
				{
					var substantiv = BasicDependencyNode;
					if (substantiv != null)
					{
						pattern.AddGramatikConditionsFrom(BasicDependencyNode.pattern);
						ParseTreeNode articleNode;
						if (dependencies.TryGetValue(Constants.VAR_ARTICLE, out articleNode))
						{
							var article = MintyTextsContainer.Instance.GetRandomWord(articleNode.pattern);
							if (article != null && article.article_type != Article_Type.UNDEFINED_VALUE)
							{
								pattern.AddPatternConditions(new EnumCondition<Article_Type>(article.article_type));
							}
						}
					}
				}


				if (pattern.HasCondition(typeof(PathCondition)))
				{
					List<Word> patternWords = MintyTextsContainer.Instance.GetWords(pattern);
					if (patternWords != null && patternWords.Count > 0)
					{
						foreach (Word w in patternWords)
						{
							AddAlternativeComicText(w);
							AddWord(syntagma, w);

							topics |= w.topics;
						}
					}
				}
				//else
				//{
				//	Logger.DebugL(this, "delete me gax.");
				//}
			}



			if (alternativeComicTexts != null)
			{

//				if (_lastWordPattern == null && alternativeComicTexts.Count == 1) {
//
//					foreach (ComicText oneAct in alternativeComicTexts.Values) {
//						if (oneAct is Sentence) {
//							_lastWordPattern = ((Sentence)oneAct).LastWordPattern;
//						}
//						break;
//					}
//				}


				//TODO: add syntagmas of inline-sentences?

				_complexity += 1f;
//				foreach (ComicText act in alternativeComicTexts.Values)
//				{
				//TODO exclude act just, if act does not include subnodes
//					if (!(act is Word)) {
//						AddFromAncestorNode (act.ParseTree);
//					}
//				}
			}




			//TODO: lastWord pattern for inline sentences
//			if (cText != null && cText is Sentence && lastWordPattern == null)
//			{
//				lastWordPattern = ((Sentence)cText).lastWordPattern; 
//			}

			float sum = 0f;
			if (words != null)
			{
				foreach (HashSet<Word> wordSet in words.Values)
				{
					foreach (Word word in wordSet)
					{
						sum += word.weight;
					}
				}
			}
			_WordsProbabilitySum = sum;
			if (_sentences != null)
			{
				foreach (MintyText s in _sentences)
				{
					sum += s.weight;
				}
			}
			_ProbabilitySum = sum;



			if (type == ParseNodeType.ALTERNATIVES)
			{ 
				StoreWeights();


				if (_syntagmas != null && alternativePartsWeights != null && _syntagmas.Count == alternativePartsWeights.Length)
				{ 
					for (int i = 0; i < _syntagmas.Count; i++)
					{
						_syntagmas[i].weight *= alternativePartsWeights[i];
					}
				}
			}

			subtreesCalculated.Add(syntagma);
		}


		/// <summary>
		/// calculates relations between words for syntagmas.
		///
		/// 1. Parse node one by one (treestructure is built) (ComicTextProcessor.CreateParseTree())
		/// 2. set links between nodes and its dependent nodes (by variable names)
		/// 3. SetRoot()
		/// 4. CalculateStructure()
		/// 5. CalculateRelationalWords()
		/// 6. CalculateWordsAndPattern()
		/// </summary>
		private void CalculateRelationalWords(Syntagma rootSyntagma)
		{
			var ct = MintyTextsContainer.Instance;


			if (this == root)
			{
//				Logger.DebugL (this, "CalculateRelationalWords(), delete me!");
			}

			//1. set nodes that are responsible to resolve variables for every syntagma
			if (Syntagmas != null)
			{
				foreach (Syntagma syntagma in Syntagmas)
				{
					foreach (ParseTreeNode node in syntagma.nodes)
					{

						if (node.IsLeaf)
						{

							var variableNodes = node.AncestorVariableNodes;
							if (variableNodes != null && variableNodes.Count > 0)
							{
								foreach (var varNode in variableNodes)
								{
									syntagma.variables[varNode.Key] = node;
									
									//copy Relationships from that node
									if (varNode.Value.HasRelations)
									{
										foreach (var relationNode in varNode.Value.relationships)
										{
											node.AddRelation(relationNode.Key, relationNode.Value);
										}
									}
								}
							}
						}
					}
				}
			}


			//2. set all possible words for relational nodes
			// (this is a time consuming preparation process, ...)
			if (Syntagmas != null)
			{
				bool allNodesSolved = false;
				int i = 0;
				while (!allNodesSolved && (i++) < 10)
				{
					allNodesSolved = true;

					foreach (Syntagma syntagma in Syntagmas)
					{
						foreach (ParseTreeNode node in syntagma.nodes)
						{
							if (!node.IsLeaf)
							{
								continue;
							}
							if (node.RelationType != null
							    && (node.pattern == null
							    || !node.pattern.HasCondition(typeof(PathCondition))))
							{
								allNodesSolved = false;
								continue;
							}
							if (node.pattern == null)
							{
								continue;
							}

//							bool hasRelations = false;
//							foreach (string varName in node.AncestorVariables) {
//								if (syntagma.variables.ContainsKey (varName)
//								   && syntagma.variables [varName] == node) {
//
//									hasRelations = true;
//									break;
//								}
//							}

							//has relations
							if (node.HasRelations)
							{
								//List<Word> words = ct.GetWords(node.pattern);
								//foreach (Word word in words)
                                foreach (Word word in ct.GetWords(node.pattern))
                                    {

									bool contextOK = true;
									var variableUsageInfos = new Dictionary<string, Dictionary<ParseTreeNode, List<TextRelation>>>();
									foreach (string varName in node.AncestorVariables)
									{
										Dictionary<ParseTreeNode, List<TextRelation>> usageInfo;
										if (!TextProcessor.TestRelations(word, varName, syntagma.nodes, out usageInfo))
										{
											contextOK = false;
											break;
										}
										else
										{
											if (usageInfo != null)
											{
												variableUsageInfos[varName] = usageInfo;
											}
										}
									}

									if (contextOK)
									{
										foreach (var nodeRelations in variableUsageInfos.Values)
										{
											foreach (var nodeRelationPair in nodeRelations)
											{
												if (nodeRelationPair.Key.pattern == null)
												{
													nodeRelationPair.Key.pattern = new TextPattern();
												}
												//TODO don't set pattern for the node but for the node in syntagma-context!
												foreach (var relation in nodeRelationPair.Value)
												{
													nodeRelationPair.Key.pattern.AddPatternConditions(new PathCondition(relation.path));
													if (relation.type.casus != Casus.UNDEFINED)
													{
														nodeRelationPair.Key.pattern.AddPatternConditions(new EnumCondition<Casus>(relation.type.casus));
													}
												}
											}
										}

									}
								}
							}




//						if (!string.IsNullOrWhiteSpace (node.variableDependencyName)
//							&& syntagma.variables.ContainsKey(node.variableDependencyName)) {
//
//							ParseTreeNode testNode = syntagma.variables[node.variableDependencyName];
//							if (testNode != null && testNode.alternativeComicTexts != null && testNode.alternativeComicTexts.Count > 0) {
//								if (!allPossibleWords.ContainsKey (testNode)) {
//									allPossibleWords [testNode] = new Dictionary<int, ComicText> ();
//								}
//								foreach (Word testWord in testNode.alternativeComicTexts.Values) {
//									if (ComicTextProcessor.TestRelations (testWord, testNode.name, syntagma.nodes)) {
//
//										allPossibleWords [testNode] [testWord.num] = testWord;
//									}
//								}
//							}
//						}
						}
					}
				}
				//end of while
				if (i >= 9)
				{
					Logger.DebugL(this, "could not find all relational words (ok, if words depend on another sentence): " + cText.Text);
				}

//				foreach (var node_allWords in allPossibleWords) {
//					node_allWords.Key.alternativeComicTexts = node_allWords.Value;
//				}
			}
//			if (Syntagmas != null) {
//				foreach (Syntagma syntagma in Syntagmas) {
//					foreach (ParseTreeNode node in syntagma.nodes) {
//
//						if (node.IsLeaf && node.pattern != null && node.alternativeComicTexts == null) {
//							foreach (Word word in ct.GetWords (node.pattern)) {
//								node.AddAlternativeComicText (word);
//							}
//						}
//					}
//				}
//			}



			CalculateWordsAndPattern(rootSyntagma);
		}

		private void AddFromAncestorNode(Syntagma syntagma, ParseTreeNode ancestor)
		{
			if (ancestor != null)
			{

				
				//is ancestor a Sentence or Word node?
				if (ancestor.cText != null)
				{
					
					if (ancestor.cText is Sentence)
					{
						if (_sentences == null)
							_sentences = new List<Sentence>();
						if (!_sentences.Contains((Sentence)ancestor.cText))
							_sentences.Add((Sentence)ancestor.cText);
					}
					else if (ancestor.cText is Word
					         && syntagma.nodes.Contains(ancestor))
					{
						AddWord(syntagma, (Word)ancestor.cText);
					}

					topics |= ancestor.cText.topics;
					//if (ancestor.cText.topics != null && ancestor.cText.topics.Count > 0)
					//{
					//	foreach (ComicTopic topic in ancestor.cText.topics)
					//	{
					//		AddTopicIfNotExistent(topic);
					//	}
					//}
				}
				
				_complexity += ancestor.Complexity;
				
				
				if (ancestor.words != null)
				{
					foreach (var syntagmaWords in ancestor.words)
					{
						foreach (Word word in syntagmaWords.Value)
						{

							AddWord(syntagmaWords.Key, word);
						}
					}
				}
				
				if (ancestor.Sentences != null)
				{
					if (_sentences == null)
						_sentences = new List<Sentence>();
					foreach (Sentence sentence in ancestor.Sentences)
					{
						if (!_sentences.Contains(sentence))
							_sentences.Add(sentence);
					}
				}

				topics |= ancestor.topics;
				//if (ancestor.topics != null)
				//{
				//	foreach (ComicTopic topic in ancestor.topics.Values)
				//	{
				//		AddTopicIfNotExistent(topic);
				//	}
				//}
			}
		}

		//private void AddNodeIfNotExistent(ParseTreeNode node)
		//{
		//	//if (nodeIndex == null)
		//	//{
		//	//	nodeIndex = new Dictionary<ParseNodeType, List<ParseTreeNode>>();
		//	//}
		//	//List<ParseTreeNode> nodeList;
		//	//if (!nodeIndex.TryGetValue(node.type, out nodeList))
		//	//{
		//	//	nodeList = new List<ParseTreeNode>();
		//	//	nodeIndex[node.type] = nodeList;
		//	//}
		//	//if (!nodeList.Contains(node))
		//	//{
		//	//	nodeList.Add(node);
		//	//}
		//}

		private void AddWord(Syntagma syntagma, Word word)
		{
			if (words == null)
			{
				words = new Dictionary<Syntagma, HashSet<Word>>();
			}
			HashSet<Word> wordsSet;
			if (!words.TryGetValue(syntagma, out wordsSet))
			{
				wordsSet = new HashSet<Word>();
				words[syntagma] = wordsSet;
			}
			wordsSet.Add(word);
		}

		//		private void AddWordIfNotExistent(Word word)
		//		{
		//			if (_words == null)
		//			{
		//				_words = new Dictionary<int, Word>(3);
		//				_words.Add(word.num, word);
		//				return;
		//			}
		//			if (!_words.ContainsKey(word.num))
		//				_words.Add(word.num, word);
		//		}

		//private void AddTopicIfNotExistent(ComicTopic topic)
		//{
		//	if (topics == null)
		//	{
		//		topics = new Dictionary<int, ComicTopic>(3);
		//		topics.Add((int)topic, topic);
		//		return;
		//	}
		//	if (!topics.ContainsKey((int)topic))
		//		topics.Add((int)topic, topic);
		//}

		#endregion



	}

	public class NodeTypeInfo
	{
		public string header = "";
		public string format = "";
		public string delimiter = null;
		public bool isLeaf = false;
	}

	public class WordRelation:IComparable
	{
		public Word word;
		//key
		public RelationType relationType;
		public float probabilitySum;
		public Dictionary<int,Word> sources = new Dictionary<int,Word>();

		
		public WordRelation(Word word, RelationType relationType)
		{
			this.word = word;
			this.relationType = relationType;
		}

		public int CompareTo(object other)
		{
			if (other == null || !(other is WordRelation))
				return 1;
			return ((WordRelation)other).word.CompareTo(word);
		}
	}



	/// <summary>
	/// ein syntagma ist eine liste aller relations-typen-nodes (object, subject, adjective), die in einem satz 
	/// zugleich vorkommen können und die nicht gleich sind, wie in einem anderen syntagma. 
	/// zu jedem syntagma sind voraussetzungen gespeichert
	/// </summary>
	public class Syntagma
	{
		public static Syntagma GetRandom(List<Syntagma> syntagmas)
		{
			float[] weights = new float[syntagmas.Count];
			int i = 0;
			foreach (Syntagma s in syntagmas)
			{
				weights[i] = s.weight;
				i++;
			}

			return syntagmas[MintyUtils.GetRandomIndex(weights)];
		}

		public Syntagma()
		{
			
		}

		public Syntagma(params ParseTreeNode[] nodes)
		{
			this.nodes = new HashSet<ParseTreeNode>(nodes);
		}

		public HashSet<ParseTreeNode> nodes = new HashSet<ParseTreeNode>();

		public Dictionary<ParseTreeNode, Object> decissionValues = new Dictionary<ParseTreeNode, object>();
		public float weight = 1;
		public Dictionary<string, ParseTreeNode> variables = new Dictionary<string, ParseTreeNode>();

		public override string ToString()
		{
			string nodeStrings = "";
			int i = 0;
			foreach (var node in nodes)
			{
				nodeStrings += (i > 0 ? " " : "") + i + "." + node;
				i++;
			}
			return string.Format("[Syntagma: {0}]", nodeStrings);
		}
	}
}

