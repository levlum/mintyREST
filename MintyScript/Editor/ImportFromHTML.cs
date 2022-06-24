	
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;
using System.Text;

#if UNITY_5_3_OR_NEWER
//using UnityEngine;
using UnityEditor;
#endif

using Com.Gamegestalt.MintyScript;

namespace Com.Gamegestalt.MintyScript.Import
{
	public class ImportFromHTML
	{
		
		public static int uniqueNum = 0;

		// #if UNITY_5_3_OR_NEWER
		// [MenuItem("Assets/Run Minty Test %#t")]
		private static void RunTest()
		{
			TextProcessor.ResetParsingData();
			MintyScriptTest.Run();
		}

		// [MenuItem("Assets/Find Minty Rhymes")]
		private static void FindRhymes()
		{
			TextProcessor.ResetParsingData();
			FindRhymes(MintyTextsContainer.Instance);

			// EditorUtility.ClearProgressBar();
		}

		// [MenuItem("Assets/Import Minty Texts %#i")]
		public static void ImportAll()
		{
			TextProcessor.ResetParsingData();
			//			string wordsPath = "Assets/Editor/de_words.xml";
			//			string sentencesPath = "Assets/Editor/de_sentences.xml";
			//string wordsPath = "Assets/Editor/de_words.xml";
			//string sentencesPath = "Assets/Editor/de_sentences.xml";
			//string namesPath = "Assets/Editor/names.txt";

			string wordsPath = "language/de_words.xml";
			string sentencesPath = "language/de_sentences.xml";
			string namesPath = "language/names.txt";

			var ct = MintyTextsContainer.Instance;
			ct.Clear();
			bool importOK = true;

			ImportFromHTML.ImportNames(namesPath);

			if (wordsPath != null && (ct.words == null || ct.words.Count == 0))
			{
				try
				{
					XmlDocument xDoc = new XmlDocument();
					//Logger.DebugL(this, "try to load: "+wordsPath);
					xDoc.Load(wordsPath);
					if (ImportFromHTML.ReadComicTexts(xDoc, false))
					{
						Logger.DebugL(ct, "loaded words successfully: " + wordsPath);
					}
					else
					{
						importOK = false;
					}
				}
				catch (System.Exception e)
				{
					Logger.LogError(e, e.Message, e);
					importOK = false;
				}

			}

			if (sentencesPath != null && (ct.Sentences == null || ct.Sentences.Count == 0))
			{
				XmlDocument xDoc = new XmlDocument();
				try
				{
					xDoc.Load(sentencesPath);
					if (ImportFromHTML.ReadComicTexts(xDoc))
					{
						Logger.DebugL(ct, "loaded sentences successfully: " + sentencesPath);
					}
					else
					{ 
						importOK = false; 
					}
				}
				catch (System.Exception e)
				{
					Logger.LogError(e, e.Message, e);
					importOK = false;
				}
			}

		}
		// #endif

		public static bool ReadComicTexts(XmlDocument localizationDoc, 
		                                  bool doCreateWordFormIndex = true, 
		                                  bool doParseSentences = true, 
		                                  bool doImportStories = true, 
		                                  bool doFindRhymes = true,
										  bool doTestForSelfReferencingLoops = true)
		{
//			if (ComicTextsContainer.Get == null){
//				ComicTextsContainer.SetInstance (new ComicTextsContainer());
//			}

			bool importOK = true;

			XmlNode root = localizationDoc.LastChild;

			if (root.Name != "LocalizedText")
			{
				return false;
			}

			MintyTextsContainer ct = MintyTextsContainer.Instance;

			uniqueNum = 0;
			
			//import words
			XmlNode node = root.SelectSingleNode("words");			
			if (node != null)
			{
				ct.ClearWords();
				//Logger.DebugL(ct,"importing "+node.ChildNodes.Count+" main wordgroups in "+ct.GetHashCode());
				uniqueNum = 0;
				foreach (XmlNode groupNode in node.ChildNodes)
				{
					ImportWords(ct, groupNode, "");
				}

				CreateOppositeRelations(ct);
				
				//create textrelations and wordgroups and put sentences for rhyming into wordgroups
				ct.Initialize();

				CalculateRelationWeights(ct);
			}
			
			
			//import sentences
			uniqueNum = 1000000;
			XmlNode sentencesNode = root.SelectSingleNode("sentences");
			if (sentencesNode != null)
			{
				ct.ClearSentences();
//				List<Sentence> storySentences = new List<Sentence>();
				int importCounter = 0;
				foreach (XmlNode sentenceNode in sentencesNode.SelectNodes("sentence"))
				{
					Sentence sentence = ImportSentence(sentenceNode);
					sentence.importCounter = importCounter;
					importCounter++;

					ct.AddSentence(sentence);
				}

			}



			try
			{
				if (ct.words != null && ct.words.Count > 0
				    && ct.Sentences != null && ct.Sentences.Count > 0)
				{
					if (doCreateWordFormIndex)
					{
						if (!CreateWordFormIndex(ct))
						{
							return false;
						}
					}

					//set sentence.num
					for (int i = 0; i < ct.Sentences.Count; i++)
					{
						ct.Sentences[i].num = i;
					}



					if (doParseSentences)
					{
						if (!ParseSentences(ct))
						{
							return false;
						}

						ct.CreateIndex();
					}



					//import stories
					if (doImportStories)
					{
						var importSortedSentences = ct.Sentences.OrderBy(x => x.importCounter);
						foreach (Sentence s in importSortedSentences)
						{
							if (s.IsPartOfStory)
							{
								AddNextStorySentence(ct, s);
							}
						}
					}

					if (doFindRhymes)
					{
						if (FindRhymes(ct))
						{
							ct.rhymesReadyToUse = true;
						}
						else
						{
							importOK = false;
						}
					}

					if (doTestForSelfReferencingLoops)
					{
						//1. TextReferences
						//foreach (var tReference in ct.textRelations)

					}


				}
			}
			catch (System.Exception e)
			{
				Logger.LogError(e, e.Message + e);
				importOK = false;
			}


			//test

//			if (ct.words != null && ct.words.Count > 0
//			    && ct.Sentences != null && ct.Sentences.Count > 0)
//			{
//
//				MintyScriptTest.Run();
//
//			}
				

			#if UNITY_5_3_OR_NEWER
			EditorUtility.ClearProgressBar();
			#endif

			return importOK;
		}

		public static void AddNextStorySentence(MintyTextsContainer ct, Sentence sentence)
		{
			if (ct.stories == null)
			{
				ct.stories = new List<Story>();
			}
//
//			foreach (Sentence sentence in sentences)
//			{
			if (sentence.IsPartOfStory)
			{
				foreach (string storyName in sentence.storyNames)
				{
					Story story = ct.stories.Find((aStory) => aStory.name == storyName);
					if (story == null)
					{
						story = new Story(){ name = storyName };
						ct.stories.Add(story);
					}

					//set topic
					story.topics |= sentence.topics;
					//if (sentence.topics != null && sentence.topics.Count > 0)
					//{
					//	story.topics.AddRange(sentence.topics);
					//}


					Dictionary<string, List<int>> storyPart = null;
					if (sentence.sequence < 1 || sentence.sequence > story.parts.Count)
					{
						storyPart = new Dictionary<string, List<int>>();
						story.parts.Add(storyPart);
					}
					else
					{
						storyPart = story.parts[sentence.sequence - 1];
						//Logger.DebugL ("Import Text", "found same story sequence number!");
					}

					List<int> sss = null;
					if (!storyPart.TryGetValue(sentence.id == null ? "" : sentence.id, out sss))
					{
						sss = new List<int>();
						storyPart.Add(sentence.id == null ? "" : sentence.id, sss);
					}
					sss.Add(sentence.num);
				}
			}
//			}
		}

		public static bool CreateWordFormIndex(MintyTextsContainer ct)
		{
			bool ok = false;
			//Logger.DebugL ("start","Start Parsing.");
			int i = 0;

			foreach (Word word in ct.words)
			{
				ct.AddToWordFormIndex(word);

				#if UNITY_5_3_OR_NEWER
				if (UpdateProgress(i, ct.words.Count, "Building word-form-index ... "))
				{
					break;
				}
				#endif

				i++;
			}

			if (i == ct.words.Count)
			{
				ok = true;
			}


			#if UNITY_5_3_OR_NEWER
			EditorUtility.ClearProgressBar();
			#endif

			return ok;
		}

		public static bool ParseSentences(MintyTextsContainer ct)
		{
			bool ok = false;
//			int endPatternCount = 0;
//			int sentenceCounter = 0;
			int i = 0;

			foreach (Sentence s in ct.Sentences)
			{

#if UNITY_5_3_OR_NEWER
				if (UpdateProgress(i, ct.Sentences.Count, i+" Parse " + s.Text.Substring(0, System.Math.Min(40, s.Text.Length))))
				{
					break;
				}
#endif

				s.DoParse();


				//count grammatical forms of last word
				if (s.lastWordPattern != null)
				{
					//					endPatternCount++;
					foreach (var lwPattern in s.lastWordPattern)
					{
						
						foreach (var cond in lwPattern.GetAllConditions())
						{
							if (cond is ISingleValue && GrammaUtil.IsGrammatical(((ISingleValue)cond).SingleValue))
							{
								ct.AddLastWordGramma(((ISingleValue)cond).SingleValue);
							}
						}
					}
				}


				i++;
			}

			if (i == ct.Sentences.Count)
			{
				ok = true;


				//test sentences for missing responses or responseTypes
				//get responses and responseTypes to show counts
				var responses = new Dictionary<string,int>();
				var responseTypes = new Dictionary<string, int>();
				//Logger.DebugL(this, "1");
				int count;
				foreach (Sentence s in ct.Sentences)
				{
					if (s.responses != null && s.responses.Count > 0)
						foreach (string response in s.responses)
						{
							if (responses.TryGetValue(response, out count))
							{
								responses[response] = count + 1;
							}
							else
							{
								responses[response] = 1;
							}
						}

					if (s.responseTypes != null && s.responseTypes.Count > 0)
						foreach (string responseType in s.responseTypes)
						{
							if (responseTypes.TryGetValue(responseType, out count))
							{
								responseTypes[responseType] = count + 1;
							}
							else
							{
								responseTypes[responseType] = 1;
							}
						}
				}

				StringBuilder missingResponseTypes = new StringBuilder();
				missingResponseTypes.Append("missing responseTypes: ");

				StringBuilder missingResponses = new StringBuilder();
				missingResponses.Append("missing responses: ");
				foreach (Sentence s in ct.Sentences)
				{
					foreach (var node in s.ParseTree)
					{
						if (node.type == ParseNodeType.SENTENCES)
						{
							List<ResponseCondition> responseConditions;
							if (node.pattern.TryGetPatternConditions<ResponseCondition>(out responseConditions))
							{ 
								foreach (var rC in responseConditions)
								{
									if (!responses.ContainsKey(rC.conditionResponse))
									{
										missingResponses.Append(rC.conditionResponse + ", ");
									}
								}
							}
							List<ResponseTypeCondition> responseTypeConditions;
							if (node.pattern.TryGetPatternConditions<ResponseTypeCondition>(out responseTypeConditions))
							{
								foreach (var rC in responseTypeConditions)
								{
									if (!responseTypes.ContainsKey(rC.conditionResponseType))
									{
										missingResponseTypes.Append(rC.conditionResponseType + ", ");
									}
								}
							}
						}
					}
				}

					
				foreach (var response in responses.Keys)
				{
					if (!responseTypes.ContainsKey(response))
					{
						missingResponseTypes.Append(response + ", ");
					}
				}

				foreach (var responseType in responseTypes.Keys)
				{
					if (!responses.ContainsKey(responseType))
					{
						missingResponses.Append(responseType + ", ");
					}
				}

				Logger.DebugL(ct, "responses: " + responses.Count + " responseTypes: " + responseTypes.Count);
				Logger.DebugL(ct, missingResponses + "\n" + missingResponseTypes);
			}

			#if UNITY_5_3_OR_NEWER
			EditorUtility.ClearProgressBar();
			#endif

			return ok;
		}

		/// <summary>
		/// Calculates the relation weights. wight from xml * number of words in path
		/// </summary>
		/// <param name="ct">Ct.</param>
		private static void CalculateRelationWeights (MintyTextsContainer ct)
		{
			foreach (var relation in ct.textRelations)
			{
				int relationWords = ct.GetWords(new TextPattern(new PathCondition(relation.path))).Count;
				relation.weight *= relationWords;
			}
		}


		/// <summary>
		/// Creates the opposite relations. 
		/// as a firxyt step: Every object-relation of a verb is also a verb-relation of the substantives in the object-relation
		/// </summary>
		/// <param name="ct">Ct.</param>
		private static void CreateOppositeRelations(MintyTextsContainer ct)
		{
			List<TextRelation> oppositeRelations = new List<TextRelation>();
			TextRelation newRelation;
			foreach (var relation in ct.textRelations)
			{
				WordType ownerType = MintyUtils.GetTypeFromPath(relation.ownerPaths[0]);

				if (ownerType == WordType.VERB)
				{
					switch (relation.type.baseType)
					{
						case RelationBaseType.OBJECT:

							foreach (string path in relation.ownerPaths)
							{
								newRelation = new TextRelation(RelationType.Get(RelationBaseType.VERB), path);
								newRelation.ownerPaths = new List<string>() { relation.path };
								oppositeRelations.Add(newRelation);
							}
							break;
					}
				}
			}

			foreach (var rel in oppositeRelations)
			{
				newRelation = rel;
				int index = ct.textRelations.IndexOf(rel);
				if (index < 0)
				{
					rel.num = ct.textRelations.Count;
					ct.textRelations.Add(rel);
				}
				else
				{
					newRelation = ct.textRelations[index];

					if (newRelation.ownerPaths == null)
						newRelation.ownerPaths = new List<string>();

					if (rel.ownerPaths != null && rel.ownerPaths.Count > 0)

					{
						foreach (string ownerPath in rel.ownerPaths)
						{
							if (!newRelation.ownerPaths.Contains(ownerPath))
					{
								newRelation.ownerPaths.Add(ownerPath);
							}
						}
					}
						
				}
			}
		}


		private static bool FindRhymes(MintyTextsContainer ct)
		{
			//initialize lastWordSentences Dictionary
			var lastWordSentences = ct.LastWordSentences;


			string log = "";
			int rhymingWordsWithoutSentence = 0;
			int n = 0;

			int rhymingWordsCounter = 0;
			foreach (WordFormContainer cont in ct.WordForms.Values)
			{
				//if ((n++)%1 != 0) continue;

				string aWord = cont.form;
				HashSet<string> lastWords = new HashSet<string>();
				var rhymes = ct.GetRhymes(aWord);
				if (rhymes != null && rhymes.Count > 0)
				{
					log += aWord + ":\n";
					rhymingWordsCounter++;

					string logLater = "";
					foreach (var wordPatterns in rhymes)
					{

						if (wordPatterns.Value.Count == 0)
						{
							logLater += "\t no pattern for " + wordPatterns.Key.Text + "\n";
						}

						foreach (var p in wordPatterns.Value)
						{
							string form = wordPatterns.Key.CreateForm(p).Text.ToLower();
							if (!lastWords.Contains(form))
							{
								logLater += "\t " + form + "\n";
								lastWords.Add(form);
							}

							if (!cont.rhymingForms.Contains(form))
							{
								cont.rhymingForms.Add(form);
							}

						}
					}


#if DEBUG

					if (false /*AppManager.Instance.HasFlag(test rhyming sentences at import)*/)
					{
						//the following block just for testing, how complete the sentences are to serve the wordforms of the rhyming words
						Dictionary<Sentence, Word> rhymeSentences;
						//							TextPattern rhymePattern;
						bool rhymeOK = ct.TryFindLastWordSentence(ct.words[cont.words[0]], aWord, new TextPattern(), out rhymeSentences, true);
						double reimdruck = (rhymeSentences == null || rhymeSentences.Count == 0) ? 100 : System.Math.Round(((double)rhymes.Count / (double)rhymeSentences.Count), 2);
						log += "\trhyme pressure: " + reimdruck + "\n";

						if (!rhymeOK)
						{
							rhymingWordsWithoutSentence++;
							var patternList = MintyTextsContainer.GetFittingPatterns(aWord, ct.words[cont.words[0]]);
							foreach (TextPattern rp in patternList)
							{
								log += "\t" + ct.words[cont.words[0]].wordType.ToString() + ", " + rp.ToString() + "\n";
							}
						}
						else
						{
							foreach (var sentenceWord in rhymeSentences)
							{
								sentenceWord.Key.AddConditionForNextProcessing(new WordCondition(sentenceWord.Value));
								log += "\texcample (" + sentenceWord.Value.Text + "): " + sentenceWord.Key.Process(null, null, null) + "\n";
								log += "\t\tlast word pattern: " + sentenceWord.Key.lastWordPattern.ToString() + "\n";
								log += "\t\t" + sentenceWord.Key.Text + "\n";

								sentenceWord.Key.Reset();
							}


						}
					}

					#endif

					log += logLater;

				}
				n++;

				if (ct.WordForms != null && ct.WordForms.Count > 0)
				{
					if (UpdateProgress(n, ct.WordForms.Count, "searching for rhymes ... (" + rhymingWordsCounter + ")"))
					{
						return false;
					}
				}
				else if (UpdateProgress(n, 0, "word_form_list is NUUULLLL!!!! ..."))
				{
					return false;
				}
			}

			log += rhymingWordsWithoutSentence + " rhyming words without sentence. (of +" + rhymingWordsCounter + " rhyming words)";

			#if UNITY_5_3_OR_NEWER
			//          EditorUtility.DisplayDialog("Rhymes", log, "ok");
			Logger.DebugL("end", "Rhymes:\n " + log);
			System.IO.File.WriteAllText("rhymes.txt", log);
			EditorUtility.ClearProgressBar();
			#else
			Logger.DebugL(ct, "Rhymes:\n " + log);
			#endif

			return true;
		}

		
		private static bool UpdateProgress(int i, int count, string text)
		{
#if UNITY_5_3_OR_NEWER
			return EditorUtility.DisplayCancelableProgressBar("Text import", text, (float)(i) / (float)count);

#else
			return false;
#endif
		}

		private static void ImportWords(MintyTextsContainer ct, XmlNode wordGroupNode, string path, HashSet<string> additionalGroups = null)
		{
			//WordGroup wg = new WordGroup(); 
			path += (path.Length == 0 ? "" : "/") + wordGroupNode.Name;

			List<string> groupsForAllChilds;
			if (wordGroupNode.TryGetAttributeList<string>("group", ',', out groupsForAllChilds))
			{
				if (additionalGroups == null)
				{
					additionalGroups = new HashSet<string>();
				}
				additionalGroups.UnionWith(groupsForAllChilds);
			}

			//read node
			foreach (XmlNode node in wordGroupNode.ChildNodes)
			{
				if (node.Name.ToLower() == "word")
				{
					if (ct.words == null)
						ct.words = new List<Word>();
					Word word = ImportWord(node, path, ct.words.Count, additionalGroups);
					if (!ct.words.Exists((w) =>
						w.Text == word.Text
						&& w.wordType == word.wordType
						&& w.topics == word.topics
						&& w.genus == word.genus))
					{
						ct.words.Add(word);
					}
				}
				else if (node.Name.ToLower() == "relation")
				{
					ImportTextRelation(node, path);
				}
				else
				{
					ImportWords(ct, node, path, additionalGroups);
				}
			}
			
//			if (relations != null && relations.Count>0){
//				WordGroup wg = ct.GetWordGroup (path);
//				if (wg == null) {
//					wg = ct.AddWordGroup (path);
//				}
//				wg.textRelations = relations;
//			}
		}

		public static void ImportTextRelation(XmlNode node, string owner)
		{
			if (node == null || node.Name.ToLower() != "relation")
				return;
			if (MintyTextsContainer.Instance.textRelations == null)
				MintyTextsContainer.Instance.textRelations = new List<TextRelation>();
			var relations = MintyTextsContainer.Instance.textRelations;
			List<RelationBaseType> types = XmlUtils.GetAttributeEnumList<RelationBaseType>(node, "type");
			if (types != null && types.Count > 0)
			{
				//relations = new List<TextRelation>();
				string relPath = TextProcessor.GetInside(node.InnerText, '[', ']');
				int praePos = node.InnerText.IndexOf('[');
				string praeposition = null;
				if (praePos > 0)
				{
					praeposition = node.InnerText.Substring(0, praePos - 1);
				}
				int doublePointPos = relPath.IndexOf(':');
				int startPos = "words/".Length;
				int len = doublePointPos >= 0 ? doublePointPos - startPos : relPath.Length - startPos;
				string relPathOnly = relPath.Substring(startPos, len);

				Casus casus = Casus.UNDEFINED;
				Genus genus = Genus.UNDEFINED;
				if (doublePointPos >= 0)
				{
					TextPattern pathPattern = TextPattern.CreateFromString(relPath.Substring(doublePointPos + 1));
					casus = pathPattern.Get<Casus>();
					genus = pathPattern.Get<Genus>();
				}
				if (casus == Casus.UNDEFINED)
				{
					casus = XmlUtils.GetAttributeEnum<Casus>(node, "casus");
				}
				if (genus == Genus.UNDEFINED)
				{
					genus = XmlUtils.GetAttributeEnum<Genus>(node, "genus");
				}

				float weight = XmlUtils.GetAttributeFloat(node, "probability", 1f);

				string name = XmlUtils.GetAttribute(node, "name");

				string needs = XmlUtils.GetAttribute(node, "needs");
				TextPattern needsPattern = null;
				if (needs != null && needs.Length > 0)
				{
					needsPattern = TextPattern.CreateFromString(needs);
					if (needsPattern.NumOfConditionTypes == 0)
					{
						Logger.LogError(needsPattern, "needs pattern is empty! \"" + needs + "\"");
						needsPattern = null;
					}
				}

				foreach (RelationBaseType baseType in types)
				{

					if (baseType == RelationBaseType.SUBJECT)
						casus = Casus.NOMINATIV;

					RelationType type = RelationType.Get(baseType, casus, genus);
					TextRelation rel = new TextRelation(type, relPathOnly);
					if (name != null)
					{
						rel.name = name;
						MintyTextsContainer.Instance.NamedRelations[name] = rel;
					}
					rel.needs = needsPattern;
					rel.praeposition = praeposition;
					rel.weight = weight;
					int index = relations.IndexOf(rel);
					if (index < 0)
					{
						rel.num = relations.Count;
						relations.Add(rel);
					}
					else
					{
						rel = relations[index];
					}

					
					//if (owner is Word)
					//{
					//	Word w = (Word)owner;
					//	if (rel.ownerWordIDs == null)
					//		rel.ownerWordIDs = new List<int>();
					//	if (!rel.ownerWordIDs.Contains(w.num))
					//		rel.ownerWordIDs.Add(w.num);
					//}
					//else if (owner is string)
					//{
						if (rel.ownerPaths == null)
							rel.ownerPaths = new List<string>();
						if (!rel.ownerPaths.Contains((string)owner))
						{
							rel.ownerPaths.Add((string)owner);
							//Logger.DebugL("", "Added Textrelation: "+rel);
						}
					//}
				}
			}
		}


		/// <summary>
		/// Adds the relations to words in groups.
		/// If a Word has a group and in this group (path) are possible relations,
		/// add this relations to the word
		/// </summary>
//		private static void AddRelationsToWordsInGroups()
//		{
//			var ct = MintyTextsContainer.Instance;
//			foreach (Word w in ct.words)
//			{
//				foreach (var group in w.Groups)
//				{
////					ct.textRelations.Find((r)=>group.BelongsToMe(r.path))
		//		}
		//	}
		//}

		public static Word ImportWord(XmlNode node, string path, int wordNum, HashSet<string> additionalGroups = null)
		{
			var text = GetMintyText(node);
			var fullPath = path + "/" + (text.IndexOf(',') > 0 ? text.Substring(0, text.IndexOf(',')):text).Trim();
			Word word = new Word(fullPath);
			word.num = wordNum;
					
			word.wordType = MintyUtils.GetTypeFromPath(path);

			
			string groups = XmlUtils.GetAttribute(node, "group");
			if (groups != null && groups.Length > 0)
			{
				if (additionalGroups == null)
				{
					additionalGroups = new HashSet<string>();
				}
				additionalGroups.UnionWith(groups.Split(','));
				foreach (string otherPath in additionalGroups)
				{
					word.paths.Add(otherPath.Substring("words/".Length));
				}
			}
			
			WordType wordType = XmlUtils.GetAttributeEnum<WordType>(node, "type");
			if (wordType != WordType.UNDEFINED)
			{
				word.wordType = wordType;
			}


			Article_Type article_type = XmlUtils.GetAttributeEnum<Article_Type>(node, "article_type");
			if (article_type != Article_Type.UNDEFINED_VALUE)
			{
				word.article_type = article_type;
			} 
			
			if (word.wordType == WordType.VERB)
			{
				VerbCategory verb_category = XmlUtils.GetAttributeEnum<VerbCategory>(node, "verb_category");
				if (verb_category != VerbCategory.UNDEFINED)
					word.verbCategory = verb_category;
			}
			else if (word.wordType == WordType.ARTICLE)
			{

				if (article_type == Article_Type.UNDEFINED_VALUE)
				{
					word.article_type = Article_Type.UNDEFINED_ARTICLE;
				}
			}
			
			SetMintyTextAttributes(word, node);



//			if (word.Get<WordType>() != WordType.SUBSTANTIV)
//			{
//				word.flags |= TextFlag.NO_ARTICLE;
//			}


			//find topics in path
			string[] pathParts = path.ToUpper().Split(new char[] { '/' });
			foreach (string pathPart in pathParts)
			{
//                        try {
				if (System.Enum.IsDefined(typeof(ComicTopic), pathPart))
				{
					System.Object enumObject = System.Enum.Parse(typeof(ComicTopic), pathPart);
					if (!System.Enum.IsDefined(typeof(ComicTopic), pathPart))
					{
						Logger.LogError("", "is defined is wrong: " + pathPart);
					}

					word.topics |= new TopicFlags((ComicTopic)enumObject);
//					if (word.topics == null)
//					{
////								word.topics = new ComicTopicEnumList();
					//	word.topics = new List<ComicTopic>();
					//}
					//if (!word.topics.Contains((ComicTopic)enumObject))
					//{
					//	word.topics.Add((ComicTopic)enumObject);
					//}
				}
				//catch (System.ArgumentException e ){
				//    if ( Enum.IsDefined( typeof(ComicTopic), pathPart )) {
				//        Debug.LogError("is defined is wrong: "+pathPart );
				//    }
				//    Debug.Log("exception ignored: " + pathPart + ": "+e);
				//}
			}
			
			//language (german) specific code
			string mainPart = word.Text;
			int dividerPos = TextProcessor.FirstDividerPos(mainPart, ':');
			if (dividerPos >= 0)
			{
				mainPart = word.Text.Substring(0, dividerPos).Trim(' ');
			}
			if (word.wordType == WordType.SUBSTANTIV)
			{
				Genus inlineGenus = Genus.UNDEFINED;
				if (mainPart.IndexOf("der ") >= 0)
					inlineGenus = Genus.MASCULINUM; 
				if (mainPart.IndexOf("die ") >= 0)
					inlineGenus = Genus.FEMININUM;
				if (mainPart.IndexOf("das ") >= 0)
					inlineGenus = Genus.NEUTRUM;
				
				if (inlineGenus != Genus.UNDEFINED)
				{
					word.genus = inlineGenus;
					word.Text = word.Text.Remove(0, 4);
				}
			}
			
			//special relations for this word node?
			foreach (XmlNode relNode in node.ChildNodes)
			{
				if (relNode.Name.ToLower() == ("relation"))
				{
					ImportTextRelation(relNode, fullPath);
				}
			}
			
			return word;
		}

		private static void SetMintyTextAttributes(MintyText s, XmlNode node)
		{
			
			s.weight = XmlUtils.GetAttributeFloat(node, "probability", 1f);
			s.sequence = XmlUtils.GetAttributeInteger(node, "sequence", -1);
			s.id = XmlUtils.GetAttribute(node, "id");
//			s.topics = new ComicTopicEnumList( XmlUtils.GetAttributeEnumList<ComicTopic>(node, "topic") );
			s.topics = XmlUtils.GetAttributeTopicFlags(node, "topic");
			string likeString = XmlUtils.GetAttribute(node, "like");
			s.like = GetIntervalValues(likeString);
			s.symbols = XmlUtils.GetAttributeEnumList<SymbolType>(node, "symbols");

			s.flags = XmlUtils.GetAttributeEnum<TextFlag>(node, "flags");
			
			s.genus = XmlUtils.GetAttributeEnum<Genus>(node, "genus");
			s.casus = XmlUtils.GetAttributeEnum<Casus>(node, "casus");
			s.person = XmlUtils.GetAttributeEnum<Person>(node, "person");
			s.numerus = XmlUtils.GetAttributeEnum<Numerus>(node, "numerus");
			s.speaker = XmlUtils.GetAttributeEnum<ActorType>(node, "speaker");
			s.personality = XmlUtils.GetAttributeEnum<Personality>(node, "personality");
			s.meaning = XmlUtils.GetAttributeEnum<TextMeaning>(node, "meaning");
			s.declinationType = XmlUtils.GetAttributeEnum<DeclinationType>(node, "Declination_Type");

			s.Text = GetMintyText(node);

//		if (s.flags != null) {
//			Logger.DebugL ("import", s.Text + s.flags.ToString ());
//		}

		}

		private static string GetMintyText(XmlNode node)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.CDATA)
				{
					string val = child.InnerText;
					if (val != null)
					{
						val = val.Trim();
						return val.Trim(' ');
					}

				}
				;
			}
			return "";
		}


		private static Interval GetIntervalValues(string valueString)
		{
			if (valueString == null || valueString.Length == 0)
				return default(Interval);
			
			
			Interval value;
			string[] stringValues = valueString.Split(new char[]{ ',', ';', ':' });
			if (stringValues.Length == 2)
			{
				try
				{
					value.from = float.Parse(stringValues[0]);
					value.to = float.Parse(stringValues[1]);
					return value;
				}
				catch
				{
				}
				;
			}
			
			try
			{
				value.from = float.Parse(valueString);
				value.to = float.Parse(valueString);
				return value;
			}
			catch
			{
			}
			;
			
			return default(Interval);
		}

		public static Sentence ImportSentence(XmlNode node)
		{
			
			Sentence sentence = new Sentence();
			
			SetMintyTextAttributes(sentence, node);
			
			sentence.bubbleType = XmlUtils.GetAttributeEnum<BubbleType>(node, "bubbleType");
			string responseString = XmlUtils.GetAttribute(node, "response");
			string responseTypes = XmlUtils.GetAttribute(node, "responseType");
//			if (sentence.topics != null && sentence.topics.Count>0) {
//						sentence.currentTopic = sentence.topics.Get()[0];
			//sentence.currentTopic = sentence.topics[0];
//			}
			
			//import responseTypes
			if (responseTypes != null && responseTypes != "")
			{
				foreach (string responseRaw in responseTypes.Split(new char[]{','}))
				{
					string response = responseRaw.Trim();
					sentence.AddResponseType(response);
				}
			}
			
			//import responsePointer
			if (responseString != null && responseString != "")
			{
				foreach (string responseRaw in responseString.Split(new char[]{','}))
				{
					string response = responseRaw.Trim();
					if (sentence.responses == null)
						sentence.responses = new List<string>();
					sentence.responses.Add(response);
				}
			}
			
			
			//is this obviously a question or an answer?
			if (sentence.meaning == TextMeaning.UNDEFINED)
			{
				if ((sentence.responseTypes == null || sentence.responseTypes.Count == 0)
				    && (sentence.responses != null && sentence.responses.Count > 0))
				{
				
					sentence.meaning = TextMeaning.QUESTION;
				}
				else if ((sentence.responseTypes != null && sentence.responseTypes.Count > 0)
				         && (sentence.responses == null || sentence.responses.Count == 0))
				{
				
					sentence.meaning = TextMeaning.ANSWER;
				}
			}

			//set story
			string nodeStoryNames = XmlUtils.GetAttribute(node, "story");
			if (nodeStoryNames != null)
			{
				foreach (string storyName in nodeStoryNames.Split(','))
				{
					if (sentence.storyNames == null)
					{
						sentence.storyNames = new List<string>();
					}
					if (!sentence.storyNames.Contains(storyName))
					{
						sentence.storyNames.Add(storyName);
					}
				}
			}

			// is this a sofisticated sentence? (Minty Script with Object and Subject tags)
			if (sentence.Text.Contains("subject") || sentence.Text.Contains("object"))
			{
				//if (sentence.topics == null)
					//sentence.topics = new List<ComicTopic>();
				sentence.topics |= new TopicFlags(ComicTopic.MINTYSCRIPT);
			}


			return sentence;	
		}

		private static void ImportNames(string namesTextFilePath)
		{
			string[] lines = File.ReadAllLines(namesTextFilePath);
			GenderType gender;
			string name;
			var ct = MintyTextsContainer.Instance;

			foreach (string line in lines)
			{
				if (line.EndsWith(","))
				{
					foreach (string cellString in line.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries))
					{
						string[] nameParts = cellString.Split(new char[]{ ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
						if (nameParts.Length > 1 && nameParts[0].Length > 0)
						{
							name = nameParts[0];
							if (nameParts[1].Length == 1)
							{
								gender = (nameParts[1][0] == '♂') ? GenderType.MALE : GenderType.FEMALE;
							}
							else
							{
								gender = Utils.RandomRange(0f, 1f) > 0.5f ? GenderType.MALE : GenderType.FEMALE;
							}

							ct.nameGenders[name] = gender;
						}
					}
				}
			}
		}
	}
}

