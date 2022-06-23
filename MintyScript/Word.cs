using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using Com.Gamegestalt.MintyScript;








#else
using System.Linq;
#endif


namespace Com.Gamegestalt.MintyScript
{

	[System.Serializable]
	public class Word : MintyText, ICloneable
	{
		public List<string> paths;

		[System.NonSerialized]
		private List<WordGroup>
			groups;

		public List<WordGroup> Groups
		{
			get
			{
				if (groups == null)
				{
					groups = new List<WordGroup>();
					foreach (string path in paths)
					{
						WordGroup wg;
						if (MintyTextsContainer.Instance.WordGroups.TryGetValue(path, out wg))
						{
							groups.Add(wg);
						}
					}
						
				}
				return groups;
			}
		}

		public bool BelongsTo(string[] path)
		{
			foreach (WordGroup wg in Groups)
			{
				if (wg.BelongsToMe(path))
					return true;
			}
			return false;
		}

		public List<int> textRelations;
			

		[System.NonSerialized]
		public Word helper;

		[System.NonSerialized]
		private string form = null;

		public bool IsFormed
		{
			get { return form != null; }
		}

		public override string Text
		{
			get
			{
				if (form != null)
					return form;
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		public Word(string path)
		{
			this.paths = new List<string>();
			paths.Add(path);
		}

		public static Word CreateSearchWord(int num)
		{
			Word w = new Word("");
			w.num = num;
			return w;
		}


		public object Clone()
		{
			Word clone = new Word(paths[0]);
			clone.CopyValues(this);
			return clone;
		}

		public override void CopyValues(MintyText original)
		{
			base.CopyValues(original);
			if (original is Word)
			{
				Word originalWord = (Word)original;
				paths = originalWord.paths;
				if (originalWord.groups != null)
					groups = originalWord.groups;
				if (originalWord.form != null)
					form = string.Copy(originalWord.form);
					
				textRelations = originalWord.textRelations;
					
			}
		}

		public bool IsLeaf
		{
			get
			{
				return (text == null || text.Length == 0 || text.IndexOf('[') < 0);
			}
		}

		/// <summary>
		/// Gets all related words.
		/// </summary>
		/// <returns>
		/// All related words.
		/// </returns>
		/// <param name='type'>
		/// RelationType.
		/// </param>
		/// <param name='context'>
		/// parseTree of the sentence in which the relation has to be found.
		/// </param>
		public Dictionary<TextRelation, List<Word>> GetAllRelatedWords(RelationType type, IEnumerable<ParseTreeNode> context, bool isProcessing, params PatternCondition[] conditions)
		{
			TextPattern pattern = new TextPattern();
			if (conditions != null && conditions.Length > 0)
				pattern.AddPatternConditions(conditions);
			var relations = GetRelationPaths(type);
			var allRelations = new List<TextRelation>(textRelations.Count);
			foreach (int relNum in textRelations)
			{
				allRelations.Add(MintyTextsContainer.Instance.textRelations[relNum]);
			}
			if (relations != null && relations.Count > 0)
			{
				var result = new Dictionary<TextRelation, List<Word>>();
				Dictionary<ParseTreeNode, List<TextRelation>> usageInfo = new Dictionary<ParseTreeNode, List<TextRelation>>();
				foreach (TextRelation tr in relations)
				{

					//test if all needs for relation fit the sentence
					if (!tr.ContextOK(context, allRelations, ref usageInfo))
					{
						continue;
					}
				}

				foreach (var nodeRelations in usageInfo)
				{
					foreach (var tr in nodeRelations.Value)
					{
						var patternForRelation = pattern.Clone();
						pattern.AddPatternConditions(new PathCondition(tr.path));
						var words = MintyTextsContainer.Instance.GetWords(pattern);
						if (words != null && words.Count > 0)
						{
							result[tr] = words;
						}
					}
				}


				return result;
			}
				
				
			//Logger.DebugL( this, Text+": CreateRelationPaths ("+type+", "+pattern+") == null, it has no relationis?");
				
			return null;
		}

		//context: list of object-subject-nodes of the sentence in which the relation has to be found
		public Word GetRelatedWord(RelationType type, IEnumerable<ParseTreeNode> context, out TextRelation chosenTextRelation)
		{
			chosenTextRelation = null;
			var words = GetAllRelatedWords(type, context, true);
			if (words != null && words.Count > 0)
			{
				#if UNITY_5_3_OR_NEWER
				List<TextRelation> relations = new List<TextRelation>(words.Keys);
				TextRelation randomRelation = relations[Utils.RandomRange(0, words.Keys.Count)];
				#else
				TextRelation randomRelation = words.Keys.ElementAt(Utils.RandomRange(0, words.Keys.Count));
				#endif

				//TODO: take that relations more likely that have necessary needed relations or that are needed by other relatons


				chosenTextRelation = randomRelation;
				MintyTextsContainer.TryGetRandomElement<Word>(words[randomRelation], out Word result);
				return result;
			}
				
			Logger.LogError(this, "No related word of type \"" + type.ToString() + "\" for \"" + Text + "\" found." + (context == null ? "" : "\n" + context));
			return this;
				
		}

		/// <summary>
		/// Gets the relation paths for a RelationType.
		/// </summary>
		/// <returns>The relation paths.</returns>
		/// <param name="type">Type.</param>
		public List<TextRelation> GetRelationPaths(RelationType type)
		{
			var rPaths = new List<TextRelation>();
				
			if (textRelations != null)
			{
				foreach (int relNum in textRelations)
				{
					if (MintyTextsContainer.Instance.textRelations[relNum].type.Equals(type))
					{
						rPaths.Add(MintyTextsContainer.Instance.textRelations[relNum]);
					}
				}
			}
				
			return rPaths;
		}

		private static Casus[] casusList =
			{
				Casus.NOMINATIV,
				Casus.GENETIV,
				Casus.DATIV,
				Casus.AKKUSATIV
			};
		private static Komparation[] komparationList =
			{
				Komparation.Positiv,
				Komparation.Komparativ,
				Komparation.Superlativ
			};
		private static Genus[] genusList =
			{
				Genus.MASCULINUM,
				Genus.FEMININUM,
				Genus.NEUTRUM
			};
		private static Numerus[] numerusList = { Numerus.SINGULAR, Numerus.PLURAL };
		private static Person[] personList = { Person.P1, Person.P2, Person.P3 };
		private static Tempus[] tempList =
			{
				Tempus.INFINITIV,
				Tempus.IMPERATIV,
				Tempus.PRAESENS,
				Tempus.PAST
			};
		private static Article_Type[] articleTypesList =
			{
				Article_Type.DEFINED_ARTICLE,
				Article_Type.UNDEFINED_ARTICLE
			};
		private static DeclinationType[] declinationTypesList =
			{
				DeclinationType.STRONG,
				DeclinationType.WEAK,
				DeclinationType.MIXED
			};

		private static Dictionary<WordType, List<TextPattern>> ALL_FORMS_PATTERNS = new Dictionary<WordType, List<TextPattern>>();
			
		//*language (german) specific method!! *//
		public List<TextPattern> CreateAllFormPatterns()
		{
			List<TextPattern> result = null;
				
			if (ALL_FORMS_PATTERNS.TryGetValue(wordType, out result))
			{
				if (result.Count > 0)
				{
					return result;
				}
			}

			switch (wordType)
			{
				case WordType.SUBSTANTIV:

					result = new List<TextPattern>();

					foreach (Numerus num in numerusList)
					{
						foreach (Casus cas in casusList)
						{
							result.Add(new TextPattern(
									new EnumCondition<WordType>(WordType.SUBSTANTIV),
									new EnumCondition<Numerus>(num),
									new EnumCondition<Casus>(cas))
                                    { immutable = true}
                            );
						}
					}
					break;

				case WordType.ARTICLE:
					result = new List<TextPattern>();
					foreach (Numerus num in numerusList)
					{
						foreach (Casus cas in casusList)
						{
							result.Add(new TextPattern(
									new EnumCondition<WordType>(WordType.ARTICLE),
									new EnumCondition<Numerus>(num),
									new EnumCondition<Casus>(cas))
                            { immutable = true }
                            );
						}
					}
					break;
					
				case WordType.VERB:
					result = new List<TextPattern>();
					foreach (Tempus temp in tempList)
					{
						if (temp == Tempus.INFINITIV)
						{
							result.Add(new TextPattern(
									new EnumCondition<WordType>(WordType.VERB),
									new EnumCondition<Tempus>(temp))
                            { immutable = true }
                            );
						}
						else
						{
							foreach (Numerus num in numerusList)
							{
								foreach (Person pers in personList)
								{
                                    result.Add(new TextPattern(
                                            new EnumCondition<WordType>(WordType.VERB),
                                            new EnumCondition<Tempus>(temp),
                                            new EnumCondition<Numerus>(num),
                                            new EnumCondition<Person>(pers))
                                    { immutable = true}
                                    );
								}
							}
						}
					}
					break;
					
				case WordType.ADJEKTIV:
					result = new List<TextPattern>();
					foreach (DeclinationType declinationType in declinationTypesList)
					{
						foreach (Komparation komp in komparationList)
						{
							//it is a specital form of adjective to have NO casus, genus, ...
							result.Add(new TextPattern(
									new EnumCondition<WordType>(WordType.ADJEKTIV),
									new EnumCondition<Komparation>(komp))
                            { immutable = true }
                            );

							foreach (Numerus num in numerusList)
							{
								foreach (Casus cas in casusList)
								{
									foreach (Genus gen in genusList)
									{
										result.Add(new TextPattern(
												new EnumCondition<WordType>(WordType.ADJEKTIV),
												new EnumCondition<DeclinationType>(declinationType),
												new EnumCondition<Komparation>(komp),
												new EnumCondition<Numerus>(num),
												new EnumCondition<Casus>(cas),
												new EnumCondition<Genus>(gen))
                                        { immutable = true }
                                        );
									}
								}
							}
						}
					}
					break;


				default: 
					if (wordType != WordType.UNDEFINED)
					{
						result = new List<TextPattern>();
						result.Add(new TextPattern(
								new EnumCondition<WordType>(wordType))
                        { immutable = true }
                        );
					}
					break;

			}

			if (result == null)
			{
				result = new List<TextPattern>(0);
			}
			else
			{
				ALL_FORMS_PATTERNS[wordType] = result;
			}


			return result;
		}
						
		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		public string[]
			_forms = null;
		#if UNITY_5_3_OR_NEWER
		[SerializeField]
		#endif
		public string
			_radical = null;

		public string[] Forms
		{
			get
			{
				if (_forms == null)
				{
					GetEndingsAndRadical(out _forms, out _radical);
				}
				return _forms;
			}
		}

		public string Radical
		{
			get
			{
				if (_radical == null)
				{
					GetEndingsAndRadical(out _forms, out _radical);
				}
				return _radical;
			}
		}
			
		//*language (german) specific method!! *//
		private void GetEndingsAndRadical(out string[] forms, out string radical)
		{
			MintyTextsContainer ct = MintyTextsContainer.Instance;
			//Logger.DebugL(this, "ComicTextsContainer: "+ct);
			long key = 0;
			radical = null;
			string[] formsArray = null;
			string endings = "";
			Word endingsWord = null;
			TextPattern endingsPattern = null;
				
			int posEndings = text.IndexOf(";");


			switch (wordType)
			{
					
					
			//SUBSTANTIV
					
					
				case WordType.SUBSTANTIV:
					if (posEndings < 0)
					{
						//regular substantiv
						radical = string.Copy(text);
						
						if (genus == Genus.FEMININUM && radical.EndsWith("in"))
						{
							key = MintyUtils.GetKey(6, (int)wordType, 1, (int)genus);
							if (!ct.cache_endings.TryGetValue(key, out formsArray))
							{
								endingsPattern = new TextPattern(
									new PathCondition("special/gramatik"),
									new EnumCondition<WordType>(WordType.NOWORD), new EnumCondition<Genus>(genus),
									new TopicCondition(new TopicFlags (ComicTopic.GRAMMATICAL), 2, "substantive", Constants.MATCH_SUPER)
								);
							}
						}
						else if (genus == Genus.FEMININUM
						         && (!radical.EndsWith("e") && !radical.EndsWith("el")))
						{
							key = MintyUtils.GetKey(6, (int)wordType, 2, (int)genus);
							if (!ct.cache_endings.TryGetValue(key, out formsArray))
							{
								endingsPattern = new TextPattern(
									new PathCondition("special/gramatik"),
									new EnumCondition<WordType>(WordType.NOWORD), new EnumCondition<Genus>(genus),
									new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 3, "substantive", Constants.MATCH_SUPER)
								);
							}
						}
						else if (genus == Genus.NEUTRUM)
						{
							bool isAdjectivic = false;
							foreach (WordGroup wg in Groups)
							{	
								if (wg.PathContains("adjectivic"))
								{
									isAdjectivic = true;
									break;
								}
							}
							if (isAdjectivic)
							{
								key = MintyUtils.GetKey(6, (int)wordType, 3, (int)genus);
								if (!ct.cache_endings.TryGetValue(key, out formsArray))
								{
									endingsPattern = new TextPattern(
										new PathCondition("special/gramatik"),
										new EnumCondition<WordType>(WordType.NOWORD), new EnumCondition<Genus>(genus),
										new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 2, "substantive", Constants.MATCH_SUPER)
									);
								}
							}
						} 
						if (formsArray == null && endingsPattern == null)
						{
							key = MintyUtils.GetKey(6, (int)wordType, 4, (int)genus);
							if (!ct.cache_endings.TryGetValue(key, out formsArray))
							{
								endingsPattern = new TextPattern(
									new PathCondition("special/gramatik"),
									new EnumCondition<WordType>(WordType.NOWORD), new EnumCondition<Genus>(genus),
									new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 1, "substantive", Constants.MATCH_SUPER)
								);
							}
						}
						
						if (formsArray == null && endingsPattern != null)
						{
							endingsWord = MintyTextsContainer.Instance.GetRandomWord(endingsPattern);
							formsArray = endingsWord.text.Split(new char[] { ';', ',' });
							ct.cache_endings.Add(key, formsArray);
						}
						//endings = endingsWord.Text;
						
						
					}
					else
					{
						//irregular substantiv
						endings = text.Substring(posEndings + 1);
						formsArray = endings.Split(new char[] { ';', ',' });
						radical = text.Substring(0, posEndings);
					}
					
					if (formsArray.Length != 8)
					{
						Logger.LogError(this, "Text XML has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}
					break;
					
					
			//VERB
					
				case WordType.VERB:
					if (posEndings < 0)
					{
						//regular substantiv
						radical = string.Copy(text);
						int seq = radical.EndsWith("ier") ? 2 : 1;
						key = MintyUtils.GetKey(6, (int)wordType, 0, seq);
						if (!ct.cache_endings.TryGetValue(key, out formsArray))
						{
							endingsPattern = new TextPattern(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), seq, "verb forms", Constants.MATCH_SUPER)
							);
							endingsWord = MintyTextsContainer.Instance.GetRandomWord(endingsPattern);
							formsArray = endingsWord.Text.Split(new char[]{ ';', ',' });
							ct.cache_endings.Add(key, formsArray);
						}

					}
					else
					{
						//irregular substantiv
						endings = text.Substring(posEndings + 1);
						formsArray = endings.Split(new char[]{ ';', ',' });
						radical = text.Substring(0, posEndings);
					}
					//endingsArray = endings.Split(new char[] { ';', ',' });
					if (formsArray.Length != 11)
					{
						Logger.LogError(this, "Text XML has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;


				//ADVERB
				case WordType.ADVERB:
					if (posEndings < 0)
					{
						//regular
						radical = string.Copy(text);
						int seq = 1;
						char lastChar = radical[radical.Length - 1];
						if (lastChar == 'e')
							seq = 2;
						else if (lastChar == 't' || lastChar == 's')
							seq = 3;

						key = MintyUtils.GetKey(6, (int)wordType, 0, seq);
						if (!ct.cache_endings.TryGetValue(key, out formsArray))
						{
							endingsPattern = new TextPattern(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), seq, "comparatives_adverb", Constants.MATCH_SUPER));

							endingsWord = MintyTextsContainer.Instance.GetRandomWord(endingsPattern);
							formsArray = endingsWord.Text.Split(new char[] { ';', ',' });
							ct.cache_endings.Add(key, formsArray);
						}

					}
					else
					{
						//irregular
						endings = text.Substring(posEndings + 1);
						formsArray = endings.Split(new char[] { ';', ',' });
						radical = text.Substring(0, posEndings);
					}

					if (formsArray.Length != 2)
					{
						Logger.LogError(this, "Text XML has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;

				//ADJECTIVE
				case WordType.ADJEKTIV:
					if (posEndings < 0)
					{
						//regular
						radical = string.Copy(text);
						int seq = 1;
						char lastChar = radical[radical.Length - 1];
						if (lastChar == 'e')
							seq = 2;
						else if (lastChar == 't' || lastChar == 's')
							seq = 3;
						
						key = MintyUtils.GetKey(6, (int)wordType, 0, seq);
						if (!ct.cache_endings.TryGetValue(key, out formsArray))
						{
							endingsPattern = new TextPattern(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), seq, "comparatives", Constants.MATCH_SUPER));
							
							endingsWord = MintyTextsContainer.Instance.GetRandomWord(endingsPattern);
							formsArray = endingsWord.Text.Split(new char[] { ';', ',' });
							if (wordType == WordType.ADVERB)
							{
								formsArray[1] = "am " + formsArray[1];
							}
							ct.cache_endings.Add(key, formsArray);
						}

					}
					else
					{
						//irregular
						endings = text.Substring(posEndings + 1);
						formsArray = endings.Split(new char[] { ';', ',' });
						radical = text.Substring(0, posEndings);
					}
					
					if (formsArray.Length != 2)
					{
						Logger.LogError(this, "Text XML has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;




				//ARTICLE

				case WordType.ARTICLE:
					if (posEndings < 0)
					{
						//regular
						radical = string.Copy(text);
						
						key = MintyUtils.GetKey(6, (int)wordType, 1, (int)genus);
						if (!ct.cache_endings.TryGetValue(key, out formsArray))
						{
							endingsPattern = new TextPattern(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), -1, "article casus", Constants.MATCH_SUPER),
								new EnumCondition<Genus>(genus)
							);
							
							endingsWord = MintyTextsContainer.Instance.GetRandomWord(endingsPattern);
							formsArray = endingsWord.Text.Split(new char[] { ';', ',' });
							ct.cache_endings.Add(key, formsArray);
						}
						
					}
					else
					{
						//irregular
						endings = text.Substring(posEndings + 1);
						formsArray = endings.Split(new char[] { ';', ',' });
						radical = text.Substring(0, posEndings);
					}
					if (formsArray.Length != 7)
					{
						Logger.LogError(this, "Text XML has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;



			//PRONOMEN

				case WordType.PRONOMEN_PERSONAL:
					formsArray = text.Split(';', ',');

					if (formsArray.Length != 24)
					{
						Logger.LogError(this, "Text XML (PRONOMEN_PERSONAL) has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;

				case WordType.PRONOMEN_REFLEXIV:
					formsArray = text.Split(';', ',');

					if (formsArray.Length != 12)
					{
						Logger.LogError(this, "Text XML (PRONOMEN_REFLEXIV) has a broken word definition: " + Text + " (" + paths[0] + ")" + " (" + endings + ")");
					}

					break;
			}


			if (formsArray == null)
				formsArray = new string[]{ };


			if (radical == null)
			{
				forms = formsArray;
			}
			else
			{
				forms = new string[formsArray.Length];
				for (int i = 0; i < formsArray.Length; i++)
				{
					
					forms[i] = formsArray[i].Replace("-", radical);
				}
			}
		}

		/**language (german) specific method!! **/
		public Word CreateForm(TextPattern pattern)
		{
			Word form;
			TryCreateForm(pattern, out form);
			return form;
		}

		/**language (german) specific method!! **/
		public bool TryCreateForm(TextPattern pattern, out Word form)
		{
//			if (pattern.Genus != Genus.UNDEFINED && pattern.Genus != genus && type != WordType.ADJEKTIV) {
//				//not possible to create the right form ...
//				//Logger.LogError (this, "Can not build " + pattern.Genus + " form of " + this);
//				return (Word)Clone ();
//			}
				
			bool ok = true;
			form = (Word)Clone();
			MintyTextsContainer ct = MintyTextsContainer.Instance;



			//if the pattern contains a fixedTextCondition, find fitting Pattern first!
			List<FixedTextCondition> condList;
			if (pattern.TryGetPatternConditions<FixedTextCondition>(out condList))
			{
				foreach (var cond in condList)
				{
					string formToBuild = cond.text;
					var fittingPatterns = MintyTextsContainer.GetFittingPatterns(formToBuild, this);
					if (fittingPatterns.Count > 0)
					{
						return TryCreateForm(fittingPatterns[Utils.RandomRange(0, fittingPatterns.Count)], out form);
					}
				}

				return false;
			}



			//look if something is not possible to form
			switch (wordType)
			{

				case WordType.SUBSTANTIV:
					if (pattern.Get<Genus>() != Genus.UNDEFINED && pattern.Get<Genus>() != genus)
						ok = false;
					break;

				case WordType.ARTICLE:
					if (pattern.Get<Genus>() != Genus.UNDEFINED && pattern.Get<Genus>() != genus)
						ok = false;
					break;

				case WordType.PRONOMEN_PERSONAL:
					if (pattern.Get<Genus>() != Genus.UNDEFINED && pattern.Get<Genus>() != genus)
					{

						ok = false;
					}
					break;

			}


			//set default forms:
			switch (wordType)
			{
				case WordType.SUBSTANTIV: 
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Casus>() != Casus.UNDEFINED)
						form.casus = pattern.Get<Casus>();

					if (form.numerus == Numerus.UNDEFINED)
						form.numerus = Numerus.SINGULAR;
					if (form.casus == Casus.UNDEFINED)
						form.casus = Casus.NOMINATIV;
					break;

				case WordType.ARTICLE:
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Casus>() != Casus.UNDEFINED)
						form.casus = pattern.Get<Casus>();
					if (pattern.Get<Article_Type>() != Article_Type.UNDEFINED_VALUE)
						form.article_type = pattern.Get<Article_Type>();

					if (form.numerus == Numerus.UNDEFINED)
						form.numerus = Numerus.SINGULAR;
					if (form.casus == Casus.UNDEFINED)
						form.casus = Casus.NOMINATIV;
					break;

				case WordType.PRONOMEN_PERSONAL:
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Casus>() != Casus.UNDEFINED)
						form.casus = pattern.Get<Casus>();
					if (pattern.Get<Genus>() != Genus.UNDEFINED)
						form.genus = pattern.Get<Genus>();
					if (pattern.Get<Person>() != Person.UNDEFINED)
						form.person = pattern.Get<Person>();

					if (form.numerus == Numerus.UNDEFINED)
						form.numerus = Numerus.SINGULAR;
					if (form.casus == Casus.UNDEFINED)
						form.casus = Casus.NOMINATIV;
					if (form.genus == Genus.UNDEFINED)
						form.genus = Genus.MASCULINUM;
					if (form.person == Person.UNDEFINED)
						form.person = Person.P1;
					break;

				case WordType.PRONOMEN_REFLEXIV:
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Casus>() != Casus.UNDEFINED)
						form.casus = pattern.Get<Casus>();
					if (pattern.Get<Genus>() != Genus.UNDEFINED)
						form.genus = pattern.Get<Genus>();
					if (pattern.Get<Person>() != Person.UNDEFINED)
						form.person = pattern.Get<Person>();

					if (form.numerus == Numerus.UNDEFINED)
						form.numerus = Numerus.SINGULAR;
					if (form.casus == Casus.UNDEFINED)
						form.casus = Casus.AKKUSATIV;
					if (form.genus == Genus.UNDEFINED)
						form.genus = Genus.MASCULINUM;
					if (form.person == Person.UNDEFINED)
						form.person = Person.P1;
					break;

				case WordType.ADVERB:
					if (pattern.Get<Komparation>() != Komparation.UNDEFINED)
						form.komparation = pattern.Get<Komparation>();

					if (form.komparation == Komparation.UNDEFINED)
						form.komparation = Komparation.Positiv;
					break;

				case WordType.ADJEKTIV:
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Casus>() != Casus.UNDEFINED)
						form.casus = pattern.Get<Casus>();
					if (pattern.Get<Komparation>() != Komparation.UNDEFINED)
						form.komparation = pattern.Get<Komparation>();
					if (pattern.Get<Genus>() != Genus.UNDEFINED)
						form.genus = pattern.Get<Genus>();
					if (pattern.Get<DeclinationType>() != DeclinationType.UNDEFINED)
						form.declinationType = pattern.Get<DeclinationType>();
					if (pattern.Get<Article_Type>() != Article_Type.UNDEFINED_VALUE)
						form.article_type = pattern.Get<Article_Type>();

					if (form.komparation == Komparation.UNDEFINED)
						form.komparation = Komparation.Positiv;


					if (form.genus != Genus.UNDEFINED || form.casus != Casus.UNDEFINED || form.numerus != Numerus.UNDEFINED)
					{
						if (form.numerus == Numerus.UNDEFINED)
						{
							form.numerus = Numerus.SINGULAR;
						}
						if (form.casus == Casus.UNDEFINED)
						{
							form.casus = Casus.NOMINATIV;
						}
					}

					if (form.declinationType == DeclinationType.UNDEFINED)
					{
						form.declinationType = GrammaUtil.GetDeclinationType(form);
					}
					break;

				case WordType.VERB:
					if (pattern.Get<Numerus>() != Numerus.UNDEFINED)
						form.numerus = pattern.Get<Numerus>();
					if (pattern.Get<Tempus>() != Tempus.UNDEFINED)
						form.tempus = pattern.Get<Tempus>();
					if (pattern.Get<Person>() != Person.UNDEFINED)
						form.person = pattern.Get<Person>();

					if (form.tempus == Tempus.UNDEFINED)
					{
						if (form.numerus == Numerus.UNDEFINED
						    && form.person == Person.UNDEFINED)
						{

							form.tempus = Tempus.INFINITIV;
						}
						else
						{
							form.tempus = Tempus.PRAESENS;
						}
					}
					break;
			}




				
			string radical = Radical;
			string endings = "";
			int index = 0;

				
			switch (wordType)
			{
				
					
			//SUBSTANTIV
					
					
				case WordType.SUBSTANTIV:
//				string ending = "";
					if (form.numerus == Numerus.PLURAL)
					{
						switch (form.casus)
						{
							case Casus.GENETIV:
								form.form = Forms[5];
								break;
							case Casus.DATIV:
								form.form = Forms[6];
								break;
							case Casus.AKKUSATIV:
								form.form = Forms[7];
								break;
							default:
								form.form = Forms[4];
								break; //NOMINATIV and default
						}
					}
					else
					{
						//SINGULAR
						switch (form.casus)
						{
							case Casus.GENETIV:
								form.form = Forms[1];
								break;
							case Casus.DATIV:
								form.form = Forms[2];
								break;
							case Casus.AKKUSATIV:
								form.form = Forms[3];
								break;
							default:
								form.form = Forms[0];
								break; //in the case of NOMINATIV and if the pattern does not have casus information
						}
					}

					break;
				
				
			//VERB
				
				case WordType.VERB:

					if (pattern.Form == Form.STEM)
					{
						form.form = Radical;
					}
					else if (form.tempus == Tempus.INFINITIV
					         || (form.tempus == Tempus.UNDEFINED
					         && form.numerus == Numerus.UNDEFINED
					         && form.person == Person.UNDEFINED))
					{
						//form.form = endingsArray[0].Replace("-", radical);
						form.form = Forms[0];
					}
					else if (form.tempus == Tempus.IMPERATIV)
					{
						if (form.numerus == Numerus.PLURAL)
						{
							form.form = Forms[10];
						}
						else
						{
							form.form = Forms[9];
						}
					}
					else if (form.tempus == Tempus.PRAESENS || form.tempus == Tempus.UNDEFINED)
					{
//					ending = "";
						if (form.numerus == Numerus.PLURAL)
						{
							switch (form.person)
							{
								case Person.P2:
									form.form = Forms[6];
									break;
								case Person.P3:
									form.form = Forms[7];
									break;
								default:
									form.form = Forms[5];
									break; //P1 and default
							}
						}
						else
						{
							//SINGULAR
							switch (form.person)
							{
								case Person.P2:
									form.form = Forms[3];
									break;
								case Person.P3:
									form.form = Forms[4];
									break;
								default:
									form.form = Forms[2];
									break; //P1 and default
							}
						}

						
						
						
						if (pattern.Form == Form.HELPER)
						{

							TextPattern helperPattern = new TextPattern(
								                            new EnumCondition<Person>(form.person),
								                            new EnumCondition<Numerus>(form.numerus),
								                            new EnumCondition<Tempus>(form.tempus)
							                            );

							//not to get helper verb for an helper verb, ...
							if (Forms[1] == this.id)
							{
								form.helper = CreateForm(helperPattern);
							}
							else
							{
								helperPattern.AddPatternConditions(
									new PathCondition("special/gramatik"),
									new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 1, Forms[1])
								);

								form.helper = ct.GetRandomWord(helperPattern);
								//								ct.cache_endings.Add( helperPattern, form.helper);
								//							}
								//}
							}
							form = form.helper;
						}
						else
						{
							form.helper = null;
						}
					}
					else if (form.tempus == Tempus.PAST)
					{
						form.form = Forms[8];


						TextPattern helperPattern = new TextPattern(
							                            new EnumCondition<Person>(form.person),
							                            new EnumCondition<Numerus>(form.numerus),
							                            new EnumCondition<Tempus>(Tempus.PRAESENS)
						                            );

						//not to get helper verb for an helper verb, ...
						if (Forms[1] == this.id)
						{
							form.helper = CreateForm(helperPattern);
						}
						else
						{		
							helperPattern.AddPatternConditions(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 1, Forms[1])
							);

							form.helper = ct.GetRandomWord(helperPattern);
							//							Cache_endings.Add( helperPattern, form.helper);
							//						}
						}
						
						if (pattern.Form == Form.HELPER)
						{
							form = form.helper;
						}
					}
					else if (form.tempus == Tempus.FUTUR)
					{
						form.form = Forms[0];


						TextPattern helperPattern = new TextPattern(
							                            new EnumCondition<Person>(form.person),
							                            new EnumCondition<Numerus>(form.numerus),
							                            new EnumCondition<Tempus>(Tempus.PRAESENS)
						                            );

						//not to get helper verb for an helper verb, ...
						if (this.id == "werden")
						{
							form.helper = CreateForm(helperPattern);
						}
						else
						{	
							helperPattern.AddPatternConditions(
								new PathCondition("special/gramatik"),
								new TopicCondition(new TopicFlags(ComicTopic.GRAMMATICAL), 1, "werden")
							);
							
							//						if (!Cache_endings.TryGetValue( helperPattern, out form.helper)){
							form.helper = ct.GetRandomWord(helperPattern);
							//							Cache_endings.Add( helperPattern, form.helper);
							//						}
						}
						
						if (pattern.Form == Form.HELPER)
						{
							form = form.helper;
						}
					}
					break;



				//ADVERB
				case WordType.ADVERB:

					if ((flags & TextFlag.NO_COMPARATION)!=0)
					{
						form.form = radical;
					}
					else if (form.komparation == Komparation.Komparativ)
					{
						form.form = Forms[0];
					}
					else if (form.komparation == Komparation.Superlativ)
					{
						form.form = Forms[1];
					}
					else
					{
						form.form = radical;
					}
					break;

				//ADJECTIVE

				case WordType.ADJEKTIV:
					

					if (form.komparation == Komparation.Komparativ)
					{
						form.form = Forms[0];
					}
					else if (form.komparation == Komparation.Superlativ)
					{
						form.form = Forms[1];
					}
					else
					{
						form.form = radical;
					}
					
					if (pattern.Get<DeclinationType>() != DeclinationType.UNDEFINED
					    && pattern.Get<Form>() != Form.INFINITIV
					    && (form.casus != Casus.UNDEFINED || form.genus != Genus.UNDEFINED || form.numerus != Numerus.UNDEFINED))
					{
					
						//correct ending for casus, genus and numerus:
						
						//key = ComicUtils.GetKey( 6, (int)type, 0, seq);
						long key = MintyUtils.GetKey(6, (int)form.wordType, 1, 0, (int)form.declinationType);
						string[] endingsArray;
						if (!ct.cache_endings.TryGetValue(key, out endingsArray))
						{
							TextPattern endingsPattern = new TextPattern(
								                             new PathCondition("special/gramatik"),
								                             new EnumCondition<DeclinationType>(form.declinationType, Constants.MATCH_SUPER)
							                             );
							Word endingsWord = ct.GetRandomWord(endingsPattern);

							endings = endingsWord.Text;
							endingsArray = endings.Split(',', ';', ':');
							ct.cache_endings.Add(key, endingsArray);
						}
						
						//fall1:fall2:fall3:fall4
						//fall = masculin singular, feminin singular, neutrum singular;  masculin plural, feminin plural, neutrum plural; 
						index = Math.Max(0, (int)form.casus - 1) * 6 + Math.Max(0, (int)form.numerus - 1) * 3 + Math.Max(0, (int)form.genus - 1);
						if (index < endingsArray.Length)
						{
							if (form.form[form.form.Length - 1] == 'e')
								form.form = form.form.Substring(0, form.form.Length - 1);
							//Logger.DebugL(this, pattern+"  index: "+index+" end: "+end);
							form.form = endingsArray[index].Replace("-", form.form);
						
						}

						//adjective normal form
					}
					else if (form.komparation == Komparation.Superlativ)
					{
						form.form += "n";
					}
					break;
					
					
			//ARTICLE
					
				case WordType.ARTICLE:

					if (pattern.Get<Article_Type>() == Article_Type.NO_ARTICLE)
					{
						form.form = "";
						break;
					}
					
					switch (form.casus)
					{
						
						case Casus.GENETIV:
							switch (form.numerus)
							{
								case Numerus.PLURAL:
									form.form = Forms[4];
									break;
								default:
									form.form = Forms[0];
									break;
							}
							break;
						
						case Casus.DATIV:
							switch (form.numerus)
							{
								case Numerus.PLURAL:
									form.form = Forms[5];
									break;
								default:
									form.form = Forms[1];
									break;
							}
							break;
						
						case Casus.AKKUSATIV:
							switch (form.numerus)
							{
								case Numerus.PLURAL:
									form.form = Forms[6];
									break;
								default:
									form.form = Forms[2];
									break;
							}
							break;
						
						default:
							switch (form.numerus)
							{
								case Numerus.PLURAL:
									form.form = Forms[3];
									break;
								default:
									form.form = radical;
									break;
							}
							break;
					}
					break;



			//PRONOMEN

				case WordType.PRONOMEN_PERSONAL:

					index = ((int)form.numerus - 1) * 12 + ((int)form.person - 1) * 4 + ((int)form.casus - 1);
					form.form = Forms[index];
					break;

				case WordType.PRONOMEN_REFLEXIV:

					index = ((int)form.numerus - 1) * 6 + ((int)form.person - 1) * 2 + ((int)form.casus - 3);
					form.form = Forms[index];
					break;
			}
				
				
				
			return ok;
		}
	}



	public class WordGroup
	{
		private string wholePath;
		public string[] path;
		public List<int> words = null;
		public HashSet<int> allWords = null;

		public WordGroup(string path)
		{
			if (path == null)
			{
				throw new Exception("word-path can not be NULL");
			}
				
			wholePath = path;
			this.path = path.Split('/');
		}
		//
		//        protected WordGroup(SerializerInStream stream) {
		//            Deserialize(stream);
		//        }
		//        public static IPMSerializeableDynamic SerializerCreate(SerializerInStream instream) {
		//            return new WordGroup(instream);
		//        }
		//        public virtual void Serialize(SerializerOutStream stream) {
		//            stream.Write(wholePath);
		//            stream.WriteStringArray(path);
		//            stream.WriteSerializeableList<Word>(words);
		//			SerializerUtils.WriteTextRelationList (stream, textRelations);
		//
		//        }
		//        public virtual void Deserialize(SerializerInStream stream) {
		//            wholePath = stream.ReadString();
		//            stream.ReadStringArray(out path);
		//            words = stream.ReadSerializeableList<Word>();
		//			textRelations = SerializerUtils.ReadTextRelationList (stream);
		//        }
			
		public string Name
		{
			get
			{
				return (path == null || path.Length == 0) ? "" : path[path.Length - 1];
			}
		}
		//
		//		public bool IsChildOf(string[] otherPath){
		//			Logger.DebugL (this, "this wholePath: "+wholePath+" path.length: "+path.Length+" otherPath.Length: "+otherPath.Length+ "  otherpath[0]:"+otherPath[0]);
		//			//return (otherPath.Length == path.Length+1 && BelongsToMe(otherPath));
		//			if (otherPath.Length+1 == path.Length){
		//				Logger.DebugL (this, otherPath[otherPath.Length-1]+" belongs to "+Name+"?");
		//				return BelongsToMe(otherPath);
		//			}
		//			return false;
		//		}
			
		//		public HashSet<int> AllWords {
		//			get {
		//				if (_allWords == null){
		//					if (words == null) _allWords = new HashSet<int>();
		//					else _allWords = new HashSet<int>(words);
		//
		//					foreach (WordGroup wg in ComicTextsContainer.Get.WordGroups.Values){
		//						if (wg.words != null && wg.BelongsToMe (path)){
		//							_allWords.UnionWith (wg.words);
		//						}
		//					}
		//					Logger.DebugL(this, "Created AllWordsSet for \""+Name+"\" with "+_allWords.Count+" words." );
		//				}
		//				return _allWords;
		//			}
		//		}
			
		//true if otherPath starts with this path
		public bool BelongsToMe(string[] otherPath)
		{
			return MintyUtils.BelongsTo(path, otherPath);
		}
			
		//true if this path starts with otherPath
		public bool BelongsTo(string[] otherPath)
		{
			return MintyUtils.BelongsTo(otherPath, path);
		}

		public bool PathContains(string part)
		{
			for (int i = 0; i < path.Length; i++)
			{
				if (path[i] == part)
					return true;
			}
			return false;
		}
			
		//		public string GetKey () {return wholePath;}
		public override string ToString()
		{
			return string.Format("words/{0}", wholePath);
		}
	}

	[System.Serializable]
	public class WordFormContainer : IComparable
	{
			
		public string form;
		public List<int> words;
		public List<String> rhymingForms;

		//		private List<WordFormContainer>
		//			rhyming_words_list;
		//
		//		public  DictList<string, WordFormContainer> RhymingWords {
		//			get {
		//				if (_rhyming_words_dict == null) {
		//					_rhyming_words_dict = new DictList<string, WordFormContainer> (ref rhyming_words_list);
		//				}
		//				return _rhyming_words_dict;
		//			}
		//		}
		//
		//		[System.NonSerialized]
		//		private DictList <string, WordFormContainer>
		//			_rhyming_words_dict = null;

		public WordFormContainer(string form)
		{
			this.form = form;
			words = new List<int>();
			rhymingForms = new List<string>();
		}

			
		public int CompareTo(object other)
		{
			if (other == null || !(other is WordFormContainer))
				return 1;
			return form.CompareTo(((WordFormContainer)other).form);
		}

		public string GetKey()
		{
			return form;
		}
	}
	
	//	public class WordTextComparer : IComparer<Word> {
	//
	//		public int Compare(Word w1, Word w2) {
	//			if (w1 == null || w2 == null) return 1;
	//			return w1.Text.CompareTo(w2.Text);
	//		}
	//
	//		private static WordTextComparer _comparer;
	//		public static WordTextComparer Comperator {
	//			get {
	//				if (_comparer == null ) _comparer = new WordTextComparer();
	//				return _comparer;
	//			}
	//		}
	//	}
}

