using System.Collections.Generic;
using System;
using System.Linq;
using Com.Gamegestalt.MintyScript;
using System.Text;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
    public class TextProcessor
    {
        #region fields

        public MintyText sentence;
        public TextPattern pattern;
        public ICharacterData meCharacter;
        public ICharacterData youCharacter;
        public ICharacterData other = null;

        private List<Word> chosenWords = new List<Word>();
        //        private string[] dependent_sentence_words;

        /** namedWords (variables) from another sentence **/
        private Dictionary<string,Word> dependent_sentence_NamedWords = new Dictionary<string, Word>();

        /** namedWords (variables) from this sentence (known, after complete processing) **/
        private Dictionary<string, Word> namedWords;
        /** namedNodes known after parsing **/
        private Dictionary<string,ParseTreeNode> namedNodes;
        /** dependencies known after parsing **/
        private Dictionary<string,List<ParseTreeNode>> dependencies;
        //        private List<ParseTreeNode> responsibleNodes = new List<ParseTreeNode>();
        //private Dictionary<string, Word> wordsToTake = new Dictionary<string, Word> ();

        private int pass = 0;

        private HashSet<ParseTreeNode> notProcessedLeafNodes;

        private ParseTreeNode parseInfoRoot;
        private Syntagma syntagma;

        #endregion

        #region static fields

        private static List<MintyText> isParsing = new List<MintyText>();

		public static bool IsParsing(MintyText mt)
		{
			return isParsing.Contains(mt);
		}

		#endregion

		#region main static methods

		public static string Process(MintyText ctext, TextPattern pattern, ICharacterData meCharacter, ICharacterData youCharacter, ICharacterData other, bool allSyntagmas=false)
        {
            if (ctext == null)
                return "";
			var startTime = DateTime.Now;

            TextProcessor processor = new TextProcessor();
            processor.sentence = ctext;
            processor.pattern = pattern;
            processor.meCharacter = meCharacter;
            processor.youCharacter = youCharacter;
            processor.other = other;
            if (ctext is Sentence && ((Sentence)ctext).next_processing_namedWords != null)
            {
                processor.dependent_sentence_NamedWords = ((Sentence)ctext).next_processing_namedWords;
//                if (((Sentence)ct).next_processing_sentence != null && !String.IsNullOrEmpty (((Sentence)ct).next_processing_sentence.last_processing_result)) {
//                    processor.dependent_sentence_words = (((Sentence)ct).next_processing_sentence.last_processing_result).Split(marks);
//                }
            }
            string result = "";
            if (allSyntagmas && ctext is Sentence)
            {
                ((Sentence)ctext).DoParse();
                if (((Sentence)ctext).ParseTree.Syntagmas != null)
                {
                    string oneResult = "";
                    for (int syntagmaNum= ((Sentence)ctext).ParseTree.Syntagmas.Count-1; syntagmaNum>=0 ; syntagmaNum--)
                    {
                        Syntagma synt = ((Sentence)ctext).ParseTree.Syntagmas[syntagmaNum];
                        processor.syntagma = synt;
                        oneResult = processor.GetProcessedText();
                        result += oneResult + (syntagmaNum>0?"\n":"");
                    }
                    ctext.last_processing_result = oneResult;
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = processor.GetProcessedText();
                ctext.last_processing_result = result;
            }

            if (ctext is Sentence)
            {
                ((Sentence)ctext).last_processing_words = processor.chosenWords;
                ((Sentence)ctext).last_processing_namedWords = processor.namedWords;
            }

			//Logger.DebugL(processor, "time for processing: " + (DateTime.Now - startTime).Milliseconds + "ms.\nsentence: " + ctext.Text);
            return result;
        }

        public static void ResetParsingData()
        {
            isParsing.Clear();
        }

        public static ParseTreeNode CreateParseTree(MintyText cText)
        {
            if (cText == null)
                return null;
            //do not create a tree for a text that is in creation-process
            if (!isParsing.Contains(cText))
            {
                isParsing.Add(cText);

                TextProcessor processor = new TextProcessor();
                processor.sentence = cText;
                processor.dependencies = new Dictionary<string, List<ParseTreeNode>>();
                processor.namedNodes = new Dictionary<string, ParseTreeNode>();
//                System.DateTime now = System.DateTime.Now;
                ParseTreeNode parseTree = processor.ParseText(cText.Text, 0);
//                if ((System.DateTime.Now-now).TotalMilliseconds > 2) {
//                    Logger.DebugL (cText, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for ParseText of \""+cText.Text+"\"\n");
//                }
                parseTree.cText = cText;

                //set links between parseNodes and its dependent nodes and store dependencies (variable names) in the sentence.
                foreach (var dependencyKeyVal in processor.dependencies)
                {
                    foreach (ParseTreeNode dependentNode in dependencyKeyVal.Value)
                    {
                        ParseTreeNode namedNode;
                        if (processor.namedNodes.TryGetValue(dependencyKeyVal.Key, out namedNode))
                        {
                            dependentNode.AddDependency(dependencyKeyVal.Key, namedNode);
                            if (dependentNode.name != null && !processor.namedNodes.ContainsKey(dependentNode.name))
                            {
                                processor.namedNodes.Add(dependentNode.name, dependentNode);
                            }


                            //NOTE: german language dependent part:
                            //if the dependent word is an adjective, it is also dependent on an article (its article_type)
                            if (dependentNode.WordType == WordType.ADJEKTIV)
                            {

                                if (dependentNode.BasicDependencyNode != null
                                    /*&& dependentNode.BasicDependencyNode.WordType == WordType.SUBSTANTIV*/)
                                {

                                    //find article!
                                    ParseTreeNode substantivNode = dependentNode.BasicDependencyNode;
                                    if (substantivNode.name != null && processor.dependencies.ContainsKey(substantivNode.name))
                                    {
                                        foreach (ParseTreeNode substantivDependentNode in  processor.dependencies [substantivNode.name])
                                        {
                                            if (substantivDependentNode.WordType == WordType.ARTICLE)
                                            {
                                                dependentNode.AddDependency(Constants.VAR_ARTICLE, substantivDependentNode);
                                            }
                                        }
                                    }
                                }
                            }

                            //NOTE: end of german language dependent part
                        }

                        //there is no node, which can solve the value for a dependent node,
                        //so --> if this is a sentence it is dependent on information from outside.
                        else
                        {
                            dependentNode.AddDependency(dependencyKeyVal.Key);


                            if (cText is Sentence)
                            {
                                Sentence s = (Sentence)cText;
                                if (s.dependencyList == null)
                                    s.dependencyList = new List<string>(1);
                                if (!s.dependencyList.Contains(dependencyKeyVal.Key))
                                {
                                    s.dependencyList.Add(dependencyKeyVal.Key);
                                }
                            }
                            else
                            {
                                Logger.LogError(cText, "Words must not include variable dependencies!");
                            }
                        }
                    }
                }


                //1. set root 2. calculate structure 3. find words 4. find relations
                parseTree.SetRoot();

                //store all variable names set in this sentence:
                if (cText is Sentence)
                {
                    Sentence s = (Sentence)cText;
                    //todo: it should iterate exactly over processor.namedWords, but namedWords are set after text-processing (not after parsing)
                    foreach (string variableName in processor.namedNodes.Keys)
                    {
                        if (s.variableNames == null)
                            s.variableNames = new List<string>();
                        if (!s.variableNames.Contains(variableName))
                        {
                            s.variableNames.Add(variableName);
                        }
                    }

                    if (parseTree.lastWordPattern != null)
                    {
                        s.lastWordPattern = parseTree.lastWordPattern.Values.ToList();
                    }
                    //Logger.DebugL ("", parseTree.ToString ()+" - LastWordPattern: "+parseTree.LastWordPattern);
                }


                isParsing.Remove(cText);
//                if ((System.DateTime.Now-now).TotalMilliseconds > 2) {
//                    Logger.DebugL (cText, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for total ParseText of \""+cText.Text+"\"\n");
//                }
                return parseTree;
            }
            else
            {
                Logger.LogError(cText, "CIRCLE in parsing comic texts! " + cText.ToString());
            }
            return null;
        }

        #endregion

        #region BUILD_PARSE_TREE

        private ParseTreeNode CreateParseNode(ParseNodeType type, string text, string name = null)
        {
            ParseTreeNode result = null;
            result = new ParseTreeNode(type, text, name);
            if (parseInfoRoot == null)
            {
                parseInfoRoot = result;
            }
            return result;
        }

        private ParseTreeNode ParseText(string text, int recursion)
        {
            if (text == null)
                return null;


            ParseTreeNode result = null;
            recursion++;

            if (text.Length == 0 || recursion > 50)
            {
                return CreateParseNode(ParseNodeType.TEXT, text);
            }


//            System.DateTime now = System.DateTime.Now;


            //split in parts
            result = CreateParseNode(ParseNodeType.PARTS, text);
            int bracketDeepness = 0;
            int posOpen = 0;
            int posClose = 0;
            string partText;
            List<ParseTreeNode> parts = new List<ParseTreeNode>(3);
            for (int n = 0; n < text.Length; n++)
            {
                if (text[n] == '[')
                {
                    bracketDeepness++;
                    if (bracketDeepness == 1 && n > posOpen)
                    {
                        //new part
                        posClose = n - 1;
                        partText = text.Substring(posOpen, posClose - posOpen + 1);
                        parts.Add(CreateParseTreeFromTextOnly(partText));
                        posOpen = n;
                    }
                }
                else if (text[n] == ']')
                {
                    bracketDeepness--;
                    if (bracketDeepness < 0)
                    {
                        Logger.LogError(this, "Minty Script Parse Error: Closing ] without opening one at " + n + " in sentence: " + sentence.Text);
                    }
                    else if (bracketDeepness == 0 && n > posOpen)
                    {
                        //new part
                        posClose = n;
                        partText = text.Substring(posOpen, posClose - posOpen + 1);
                        ParseTreeNode newNode = ParseToken(partText, recursion);

                        if (!string.IsNullOrEmpty(newNode.name))
                        {
                            if (namedNodes.ContainsKey(newNode.name))
                            {
                                //TODO: store multiple nodes, if there are more than one node with the same variableName.
                                //but make sure, that dynamically the right node is used (the chosen one), ...
                                Logger.LogWarning(this, "duplicate variable name: \"" + newNode.name + "\" in " + (sentence == null ? "" : "\nSentence: " + sentence.Text));
                            }
                            else
                            {
                                namedNodes.Add(newNode.name, newNode);
                            }
                        }

                        parts.Add(newNode);
                        posOpen = n + 1;
                    }
                }
                else if (n == text.Length - 1)
                {
                    if (n >= posOpen)
                    {

                        if (posOpen == 0 && recursion == 1)
                        {
                            //if this is the root (text is the whole content) do not search this as a word, ... -> it would find itself, ... (infinite poop)
                            if (sentence is Sentence)
                            {
                                parts.Add(CreateParseTreeFromTextOnly(text));
                            }
                            else
                            {
                                parts.Add(CreateParseNode(ParseNodeType.TEXT, text));
                            }
                        }
                        else
                        {
                            partText = text.Substring(posOpen);
                            parts.Add(CreateParseTreeFromTextOnly(partText));
                        }
                    }
                }
            }
            if (bracketDeepness > 0)
            {
                Logger.LogError(this, "Minty Script Parse Error: Too many opening brackets [ in sentence: " + sentence.Text);
            }

            if (parts.Count == 1)
            {
                result = parts[0];

            }
            else if (parts.Count > 1)
            {
                result.Parts = parts.ToArray();
            }
            else
            {
                result = CreateParseNode(ParseNodeType.EMPTY, "");
            }


//            //is there soemthing to process?
//            int startPos = text.IndexOf ('[');
//            if (startPos >= 0) {
//                int endPos = -1;
//
//                //find endPos of token
//                for (int i=startPos+1; i<text.Length; i++) {
//                    if (text[i] == '[') bracketDeepness++;
//                    if (text[i] == ']') {
//                        if (bracketDeepness==0) {
//                            endPos = i;
//                            break;
//                        }
//                        else bracketDeepness--;
//                    }
//                }
//
//                //is there a valid token?
//                if (endPos>=0) {
//                    ParseTreeNode[] parts = new ParseTreeNode[3];
//                    //part 0 is allways just normal text
//                    //part 1 is one (first) token (can include internal tokens)
//                    //part 2 is the rest
//
//                    if (startPos>0) {
//                        parts[0] = CreateParseTreeFromTextOnly (text.Substring (0, startPos));
//                    }
//                    if (endPos<text.Length-1) {
//                        string textPart = text.Substring (endPos+1);
//                        parts[2] = ParseText (textPart, recursion);
//                    }
//
//                    string tokenText = text.Substring (startPos, endPos-startPos+1);
//
//                    //if the whole text is framed by squared brackets --> return the Processed Text of this token
//                    if (parts[0]==null && parts[2]==null) {
//                        result = ParseToken (tokenText, recursion);
////                        if ((System.DateTime.Now-now).TotalMilliseconds > 2) {
////                            Logger.DebugL (this, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for ParseText of \""+text+"\"\n");
////                        }
//                        return result;
//                    }
//                    else {
//                        parts[1] = ParseToken (tokenText, recursion);
//
//                        if (result==null) result = new ParseTreeNode (ParseNodeType.PARTS, text);
//                        List<ParseTreeNode> partsList = new List<ParseTreeNode>();
//                        foreach (ParseTreeNode node in parts){
//                            if (node != null) partsList.Add(node);
//                        }
//                        result.Parts = partsList.ToArray();
//
//
//                    }
//                }
//                else {
//                    Logger.LogError (this,"parse Error in ComicTextProcessor, (check squared brackets) in: \""+text+"\"");
//                }
//            }
//            if (result == null){
//                //if this is the root (text is the whole content) do not search this as a word, ... -> it would find itself, ... (infinite poop)
//                if (recursion > 1){
//                    result = CreateParseTreeFromTextOnly (text);
//                }
//                else {
//                    result = new ParseTreeNode( ParseNodeType.TEXT, text);
//                }
//            }
//
//
////            if ((System.DateTime.Now-now).TotalMilliseconds > 2) {
////                Logger.DebugL (this, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for ParseText of \""+text+"\"\n");
////            }

            return result;
        }

   //     public static TokenType GetToken(string text)
   //     {
			//ComicEnums.Token
        //    foreach (string token in Constants.TOKENS)
        //    {
        //        if (text.StartsWith(token))
        //            return token;
        //    }
        //    return null;
        //}

        private static char[] marks = { ' ', ',', '.', ':', '?', '!', ';', '-', '(', ')', '[', ']' };

        private ParseTreeNode CreateParseTreeFromTextOnly(string justText)
        {

            if (justText == null || justText.Length == 0)
                return CreateParseNode(ParseNodeType.EMPTY, "");
            if (justText.Length == 1)
                return CreateParseNode(ParseNodeType.TEXT, justText);

            string wholeText = justText;


            List<ParseTreeNode> parts = new List<ParseTreeNode>();
//            List<Word> wordsInText = new List<Word>();
            string textPart = "";
            ParseTreeNode parseNode = null;

            int posWordDivider = 0;
            while (posWordDivider >= 0)
            {
                posWordDivider = justText.IndexOfAny(marks);
                if (posWordDivider == 0)
                {
                    textPart += justText[0].ToString();
                    justText = justText.Substring(1);
                    if (parseNode == null)
                        parseNode = CreateParseNode(ParseNodeType.TEXT, textPart);
                    parts.Add(parseNode);
                    parseNode = null;
                    textPart = "";
                }
                else
                {
                    //this is probably a word
                    string textWord = posWordDivider < 0 ? justText : justText.Substring(0, posWordDivider);
                    //is this word value for a variable?
                    string varName = null;
                    int equalPos = FirstDividerPos(textWord, '=');
                    if (equalPos >= 0)
                    {
                        varName = justText.Substring(0, equalPos).Trim(' ');
                        textWord = justText.Substring(equalPos + 1);

                        if (textPart.Length > 0)
                        {
                            parseNode.UnprocessedText = textPart;
                            parts.Add(parseNode);
                            parseNode = null;
                            textPart = "";
                        }
                        parseNode = CreateParseNode(ParseNodeType.PREDEFINED, textWord, varName);
                        parts.Add(parseNode);
                    }
                    else
                    {
                        textPart += textWord;
                    }



                    List<Word> fittingForms;
                    List<TextPattern> fittingPatterns;
                    if (MintyTextsContainer.Instance.TryFindFittingForms(textWord.ToLower(), out fittingForms, out fittingPatterns))
                    {
//                        TextPattern pattern = new TextPattern(new FixedTextCondition(textWord));

                        if (parseNode == null)
                            parseNode = CreateParseNode(ParseNodeType.PREDEFINED, textPart);

                        TextPattern pattern;
                        if (TextPattern.TryCreateFromIntersection(fittingPatterns, out pattern))
                        {
                            parseNode.pattern = (TextPattern)pattern.Clone();
                            parseNode.pattern.immutable = false;
                        }

                        if (parseNode.pattern == null)
                        {
                            parseNode.pattern = new TextPattern();
                        }

                        parseNode.pattern.AddConditionsWords(fittingForms);

                        foreach (Word fittingWord in fittingForms)
                        {
                            parseNode.AddAlternativeComicText(fittingWord);
                        }

                        if (!parts.Contains(parseNode))
                        {
                            parts.Add(parseNode);
                        }
                        parseNode = null;
                        textPart = "";
                    }

                    if (parseNode != null && parseNode.type == ParseNodeType.PREDEFINED)
                    {
                        parseNode = null;
                    }

                    if (posWordDivider > 0)
                    {
                        justText = justText.Substring(posWordDivider);
                    }

                }
            }

            if (textPart.Length > 0)
            {
                if (parseNode == null)
                    parseNode = CreateParseNode(ParseNodeType.TEXT, textPart);
                parseNode.UnprocessedText = textPart;
                parts.Add(parseNode);
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }
            else if (parts.Count > 1)
            {
                ParseTreeNode partsNode = CreateParseNode(ParseNodeType.PARTS, wholeText);
                partsNode.Parts = parts.ToArray();
                return partsNode;
            }
            return CreateParseNode(ParseNodeType.EMPTY, "");
        }




        private ParseTreeNode ParseToken(string text, int recursion)
        {
            MintyTextsContainer ct = MintyTextsContainer.Instance;
            if (ct == null)
            {
                Logger.LogError(this, "ComicTextsContainer not found. No processing!");
                return CreateParseNode(ParseNodeType.UNKNOWN, text);
            }

            int i = 0;
            string nodeName = null;
            ParseNodeType nodeType = ParseNodeType.UNKNOWN;
            string[] dependencyNodeNames = null;
            ParseTreeNode result = null;
			TokenType token;

            if (!MintyEnums.TryGetToken(text, out token))
            {
                //is this a named token?
                string withoutBrackets = text.Substring(1, text.Length - 2);
                int equalPos = FirstDividerPos(withoutBrackets, '=');
                if (equalPos >= 0)
                {
                    nodeName = withoutBrackets.Substring(0, equalPos).Trim(' ');
                    text = "[" + withoutBrackets.Substring(equalPos + 1).Trim(' ') + "]";
                    
					if (!MintyEnums.TryGetToken(text, out token))
					{
                        return CreateParseTreeFromTextOnly(withoutBrackets);
                    }
                }
            }

			if (token == default(TokenType))
			{
				//let the token unchanged: for example [e] 
				return CreateParseNode(ParseNodeType.TEXT, text);
			}
#if UNITY_5_3_OR_NEWER
		string tokenString = MintyEnums.Tokens.Reverse[token];
#else
			string tokenString = MintyEnums.TokensReverse[token];
#endif

			//names
			switch (token)
            {
                case TokenType.NAME_ME:
                    result = CreateParseNode(ParseNodeType.NAME_ME, text, nodeName);
                    break;

                case TokenType.NAME_YOU:
                    result = CreateParseNode(ParseNodeType.NAME_YOU, text, nodeName);
                    break;

                case TokenType.NAME_OTHER:
                    result = CreateParseNode(ParseNodeType.NAME_OTHER, text, nodeName);
                    break;



            //choose a word by wordpath
                case TokenType.WORDS:

                    string wordPath = text.Substring(1, text.Length - 2);

                    i = 0;

                    TextPattern pathPattern = TextPattern.CreateFromString(wordPath);
                    result = CreateParseNode(ParseNodeType.WORDS, text, nodeName);

					if (TryGetVariableNames(wordPath, out dependencyNodeNames))
					{
						foreach (string varName in dependencyNodeNames)
						{
							StoreDependentNode(varName, result);
						}
					}

                    result.pattern = pathPattern;
                    result.CalculateWordType();

                    break;



            //link to another sentence
                case TokenType.SENTENCE:
                    string sentencePatternString = text.Substring(MintyEnums.TokensReverse [TokenType.SENTENCE].Length + 1, text.Length - MintyEnums.TokensReverse[TokenType.SENTENCE].Length - 2);
                    if (sentencePatternString.Contains("\""))
                    {
                        string error = sentencePatternString + " must not contain \"";
                        Logger.LogError(this, error);
                        return CreateParseNode(ParseNodeType.TEXT, error);
                    }

                    result = CreateParseNode(ParseNodeType.SENTENCES, text, nodeName);

					TextPattern sentencePattern = new TextPattern();
					sentencePattern.SetFromString(sentencePatternString);

					if (!string.IsNullOrEmpty(nodeName))
					{
						sentencePattern.AddPatternConditions(new DefinesVariableCondition(new List<string>( new string[] { nodeName })));
					}

					List<Sentence> linkedSentences = ct.GetSentences(sentencePattern);
					result.pattern = sentencePattern;

                    if (linkedSentences != null && linkedSentences.Count > 0)
                    {
                        foreach (Sentence sentence in linkedSentences)
                        {

                            if (sentence != null && sentence.Text != null)
                            {
                                //ParseTreeNode sentenceParseTree = ParseText (sentence.Text, recursion);
                                result.AddAlternativeComicText(sentence);
                            }

                        }
                    }
                    break;



                case TokenType.TOPIC:
                    result = CreateParseNode(ParseNodeType.TOPIC, text, nodeName);

					if (TryGetVariableNames(text.Substring(tokenString.Length - 1), out dependencyNodeNames))
					{
						foreach (string varName in dependencyNodeNames)
						{
							StoreDependentNode(varName, result);
						}
					}

					break;



                case TokenType.SWITCH_GENUS:
                case TokenType.SWITCH_NUMERUS:
                case TokenType.SWITCH_CASUS:
				case TokenType.SWITCH_REFLEXIVE:
				case TokenType.SWITCH_LAST_OF_X:
                    {
                        int posQuestionMark = text.IndexOf('?');
                        if (posQuestionMark < 0)
                        {
                            Logger.LogError(this, "missing Question mark in: " + text + "\" in: " + sentence.Text);
                        }
                        else
                        {

                            string partsString = text.Substring(posQuestionMark + 1, text.Length - posQuestionMark - 2);
							switch (token)
							{
								case TokenType.SWITCH_GENUS:
									nodeType = ParseNodeType.SWITCH_GENUS;
									break;
								case TokenType.SWITCH_NUMERUS:
									nodeType = ParseNodeType.SWITCH_NUMERUS;
									break;
								case TokenType.SWITCH_CASUS:
									nodeType = ParseNodeType.SWITCH_CASUS;
									break;
								case TokenType.SWITCH_REFLEXIVE:
									nodeType = ParseNodeType.SWITCH_REFLEXIVE;
									break;
								case TokenType.SWITCH_LAST_OF_X:
									nodeType = ParseNodeType.SWITCH_LAST_OF_X;
									break;
							}

							char listSplitChar = Constants.NODE_TYPE_INFO[nodeType].delimiter[0];
							List<string> parts = Split(partsString, listSplitChar);
                            if (parts != null && parts.Count > 0)
                            {
                                
                                result = CreateParseNode(nodeType, text, nodeName);

                                for (i = 0; i < parts.Count; i++)
                                {
                                    string wordText = parts[i];

                                    ParseTreeNode alternativePart = ParseText(wordText, recursion);

                                    result.AddAlternativePart(alternativePart);
                                }

								if (TryGetVariableNames(text.Substring(tokenString.Length - 1), out dependencyNodeNames))
								{
									foreach (string varName in dependencyNodeNames)
									{
										StoreDependentNode(varName, result);
									}
								}

							}
                            else
                            {
                                Logger.LogError(this, "syntx error in: " + partsString + "\" in " + sentence.Text);
                            }
                        }

                    }
                    break;



                case TokenType.WORDX:
                case TokenType.SUBJECT:
                case TokenType.OBJECT:
				case TokenType.ADJECTIVE:
				case TokenType.ADVERB:
				case TokenType.VERB:

                    int posForm = text.IndexOf(':') + 1;
                    TextPattern formPattern = null;
                    if (posForm > 0)
                    {
                        formPattern = TextPattern.CreateFromString(text.Substring(posForm, text.Length - posForm - 1));
                    }


                    ParseNodeType type;
					WordType wordType = WordType.UNDEFINED;
                    switch (token)
                    {
                        case TokenType.WORDX:
                            type = ParseNodeType.WORDX;
                            break;
                        case TokenType.SUBJECT:
                            type = ParseNodeType.SUBJECT;
							wordType = WordType.SUBSTANTIV;
                            break;
                        case TokenType.OBJECT:
                            type = ParseNodeType.OBJECT;
							wordType = WordType.SUBSTANTIV;
                            break;
                        case TokenType.ADJECTIVE:
                            type = ParseNodeType.ADJECTIVE;
							wordType = WordType.ADJEKTIV;
                            //add gramma of dependent substantiv
                            //do that in ParseTreeNode.CalculateWordsAndPattern
                            break;
						case TokenType.VERB:
							type = ParseNodeType.VERB;
							wordType = WordType.VERB;
							break;
						case TokenType.ADVERB:
							type = ParseNodeType.ADVERB;
							wordType = WordType.ADVERB;
							break;
						default:
                            type = ParseNodeType.WORDX;
                            break;
                    }

                    result = CreateParseNode(type, text, nodeName);
                    result.pattern = formPattern;
					result.WordType = wordType;


					if (TryGetVariableNames(text.Substring(tokenString.Length - 1), out dependencyNodeNames))
					{
						foreach (string varName in dependencyNodeNames)
						{
							StoreDependentNode(varName, result);
						}
					}

					break;



            //random text of inline list
                case TokenType.RANDOM:
                    {
                        string partsString = text.Substring(tokenString.Length, text.Length - tokenString.Length - 1);
                        List<string> parts = Split(partsString, ';');
                        if (parts != null && parts.Count > 0)
                        {

                            result = CreateParseNode(ParseNodeType.ALTERNATIVES, text, nodeName);

                            bool atLeastOneWeightFound = false;
                            float[] weights = new float[parts.Count];
                            for (i = 0; i < parts.Count; i++)
                            {
                                string wordText = parts[i];

                                //test if there are word weights
                                float weight = 1;
                                int divPos = FirstDividerPos(wordText, ':');
                                if (divPos > 0)
                                {
                                    if (float.TryParse(wordText.Substring(0, divPos), out weight))
                                    {
                                        atLeastOneWeightFound = true;
                                        wordText = wordText.Substring(divPos + 1);
                                    }
                                }
                                weights[i] = weight;

                                ParseTreeNode alternativePart = ParseText(wordText, recursion);

                                result.AddAlternativePart(alternativePart);
                            }

                            if (atLeastOneWeightFound)
                                result.alternativePartsWeights = weights;

                        }
                    }
                    break;



            //sex dependent words
                case TokenType.SEX:
                    ICharacterData character;

                    List<string> sexParts = Split(text.Substring(text.IndexOf("?") + 1), ':');
                    string wordMale = sexParts[0];
                    string wordFemale = sexParts[1].Substring(0, sexParts[1].Length - 1);

                    if (text.IndexOf(tokenString + "You") >= 0)
                    {
                        character = youCharacter;
                        nodeType = ParseNodeType.SEX_ME;
                    }
                    else if (text.IndexOf(tokenString + "Me") >= 0)
                    {
                        character = meCharacter;
                        nodeType = ParseNodeType.SEX_ME;
                    }
                    else
                    {
                        character = other;
                        nodeType = ParseNodeType.SEX_ME;
                    }

                    result = CreateParseNode(nodeType, text, nodeName);

                    result.AddAlternativePart(ParseText(wordMale, recursion));
                    result.AddAlternativePart(ParseText(wordFemale, recursion));

                    break;



            //UPPER
                case TokenType.UPPER:
                    {
                        int posStart = tokenString.Length;
                        string t = text.Substring(posStart, text.Length - posStart - 1);
                        result = CreateParseNode(ParseNodeType.UP, text, nodeName);

                        result.AddAlternativePart(ParseText(t, recursion));
                    }
                    break;

            //LOWER
                case TokenType.LOWER:
                    {
                        int posStart = tokenString.Length;
                        string t = text.Substring(posStart, text.Length - posStart - 1);
                        result = CreateParseNode(ParseNodeType.LOW, text, nodeName);

                        result.AddAlternativePart(ParseText(t, recursion));
                    }
                    break;

            //INVISIBLE
                case TokenType.INVISIBLE:
                    {
                        int posStart = tokenString.Length;
                        string t = text.Substring(posStart, text.Length - posStart - 1);
                        result = CreateParseNode(ParseNodeType.INVISIBLE, text, nodeName);

                        result.AddAlternativePart(ParseText(t, recursion));
                    }
                    break;


            }//switch end


            if (result != null)
            {
                return result;
            }

            return CreateParseNode(ParseNodeType.TEXT, text);
        }

        #endregion







        #region PROCESS_TEXT

        private string GetProcessedText()
        {
            string text = sentence.Text;
            if (text == null || text.Length == 0)
                return "";

            //System.DateTime now = System.DateTime.Now;

            //creates the ParseTree
            //namedNodes = new Dictionary<string, ParseTreeNode>();
            //dependencies = new Dictionary<string, List<ParseTreeNode>>();
            sentence.DoParse();
            parseInfoRoot = sentence.ParseTree;

            notProcessedLeafNodes = new HashSet<ParseTreeNode>();
            namedWords = new Dictionary<string, Word>();
            chosenWords.Clear();

            //Logger.DebugL (this, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms for ParseTree of \""+text+"\"\n");

            bool choosePatternForWords = false;

            if (pattern != null)
            {
                choosePatternForWords = true;


                //choose best Syntagma (fitting best for words and topics tu use)
                if (parseInfoRoot.Syntagmas != null)
                {
                    float bestMatch = -Constants.MATCH_PERFECT;
                    List<Syntagma> bestSyntagmas = new List<Syntagma>(1);
                    foreach (Syntagma sy in parseInfoRoot.Syntagmas)
                    {
                        float match = 0;
                        foreach (ParseTreeNode node in sy.nodes)
                        {
                            Word bestMatchingWord = null;
                            float nodeMatch = MatchValue(pattern, node, sy, out bestMatchingWord);



                            //look, if this match is based on a fitting word. if so: check the context!
                            if (bestMatchingWord != null
                                && node.AncestorVariables != null
                                && node.AncestorVariables.Count > 0)
                            {
                                foreach (string varName in node.AncestorVariables)
                                {
                                    if (sy.variables.ContainsKey(varName) && sy.variables[varName] == node)
                                    {
                                        if (TestAndStoreRelations(bestMatchingWord, varName, sy.nodes, false))
                                        {
                                            match += nodeMatch;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        match += nodeMatch;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                match += nodeMatch;
                            }
                        }
                        if (match <= 0)
                        {
                            continue;
                        }

                        if (match > bestMatch)
                        {
                            bestSyntagmas.Clear();
                            bestMatch = match;
                        }
                        if (match >= bestMatch)
                        {
                            bestSyntagmas.Add(sy);
                        }
                    }

                    if (syntagma == null && bestSyntagmas.Count > 0)
                    {
                        syntagma = Syntagma.GetRandom(bestSyntagmas);
                    }
                }

            }


            //check, if all attributes (variables) exist
            List<string> externalVariables = GetExternalVariables();


            if (syntagma == null && parseInfoRoot.Syntagmas != null)
            {
                syntagma = Syntagma.GetRandom(parseInfoRoot.Syntagmas);
            }

            if (pattern != null)
            {
//                Logger.DebugL(this, "processing sentence with pattern: " + pattern + "\nin Syntagma: " + syntagma + "\nsentence: " + sentence.Text);
            }


            //set relational nodes for dependencies from another sentence
            if (syntagma != null)
            {
                if (externalVariables != null && externalVariables.Count > 0)
                {
                    foreach (string variableName in externalVariables)
                    {
                        Word externalWord = dependent_sentence_NamedWords[variableName];
                        TestAndStoreRelations(externalWord, variableName, syntagma.nodes);
                    }
                }
            }




            //central start of text-processing
            MintyText chosenText;
            string result = "";
            bool completed = false;
            pass = 0;

            while (!completed)
            {
                //calculate another pass (use words, that are chosen at the last pass, ...)
                pass++;
//                string oldResult = string.Copy(result);
                completed = TryProcessText(parseInfoRoot, out result, out chosenText, choosePatternForWords, 0);
                if (!completed && pass > 50)
                {
                    completed = true;

                    StringBuilder errorLog = new StringBuilder();
                    errorLog.AppendLine("Could not find all solutions (maybe crossing references?).");
                    errorLog.AppendLine("sentence: " + sentence.Text);
                    errorLog.AppendLine("syntagma: " + syntagma);
                    errorLog.AppendLine("result: " + result);
                    foreach (var notProcessedNode in notProcessedLeafNodes)
                    {
                        errorLog.AppendLine("not processed: "+notProcessedNode.ToString());
                    }

                    if (namedWords != null)
                    {
                        errorLog.Append("namedWords: ");
                        foreach (string depKey in namedWords.Keys)
                        {
                            errorLog.Append( depKey + " = " + namedWords[depKey]+", ");
                        }
                    }
                    if (this.dependent_sentence_NamedWords != null)
                    {
                        errorLog.Append("\ndependent_sentence_NamedWords: ");
                        foreach (string depKey in dependent_sentence_NamedWords.Keys)
                        {
                            errorLog.Append(depKey + " = " + dependent_sentence_NamedWords[depKey]+", ");
                        }
                    }

                    Logger.LogWarning(this, errorLog.ToString());
                    //Logger.DebugL(this, logVariables);
                }
            }
            //Logger.DebugL (this, ""+(int)(System.DateTime.Now-now).TotalMilliseconds+" ms total of \""+text+"\"\n");

            //remove white spaces and double spaces, ...
            result = result.Trim();
            result = result.Replace("  ", " ");

            //find "[xy]" and look if xy is a repetition with preceeding words
            int posA = result.IndexOf('[');
            int posE = result.IndexOf(']', Math.Max(0, posA));
            while (posA > 0 && posE > posA)
            {
                string inside = result.Substring(posA + 1, posE - posA - 1);
                string before = result.Substring(0, posA);
                if (before.EndsWith(inside))
                {
                    //delete brackets and inside text
                    result = before + (result.Length > posE ? result.Substring(posE + 1) : "");
                }
                else
                {
                    //delete just brackets
                    result = before + inside + (result.Length > posE ? result.Substring(posE + 1) : "");
                }
                posA = result.IndexOf('[');
                posE = result.IndexOf(']', Math.Max(0, posA));
            }

            //exclude duplicate punktuation marks and set sentence start letter to upper case
            if (result.Length > 1)
            {
                int i = 1;
                bool lastLetterWasSentenceEndMark = MintyUtils.IsSentenceEndMark(result[0]);
                while (i < result.Length)
                {
                    //duplicate
                    if (MintyUtils.IsPunctuationMark(result[i - 1]) && MintyUtils.IsPunctuationMark(result[i]))
                    {
                        result = result.Substring(0, i) + (i + 1 < result.Length ? result.Substring(i + 1) : "");
                    }

                    //upper starting letter
                    if (lastLetterWasSentenceEndMark)
                    {
                        if (i < result.Length)
                        {
                            result = result.Substring(0, i) + result[i].ToString().ToUpper() + (i + 1 < result.Length ? result.Substring(i + 1) : "");
                        }
                    }

                    if (i < result.Length && result[i] != ' ')
                        lastLetterWasSentenceEndMark = MintyUtils.IsSentenceEndMark(result[i]);
                    i++;
                }
            }

            //reset parse tree
            parseInfoRoot.Reset();

            //save actual length:
            if (sentence != null)
                sentence.processedLength = result.Length;

            return result;
        }


        private bool TryProcessText(ParseTreeNode parseInfo, out string result, out MintyText chosenText, bool choosePatternForWords, int recursion)
        {
            recursion++;
            chosenText = null;
            result = "";
            if (parseInfo == null || parseInfo.UnprocessedText.Length == 0)
            {
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }

            if (parseInfo.SolutionString != null)
            {
                chosenText = parseInfo.SolutionComicText;
                if (chosenText != null && parseInfo.name != null && chosenText is Word)
                {
                    if (!namedWords.ContainsKey(parseInfo.name))
                    {
                        namedWords.Add(parseInfo.name, (Word)chosenText);
                    }
                }
                result = parseInfo.SolutionString;
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }

            if (recursion > 1000)
            {
                Logger.LogError(this, "recursion overflow: " + recursion + " in " + sentence.Text);
                result = parseInfo.UnprocessedText;
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }


            string text = parseInfo.UnprocessedText;
            int partToForceTopic = -1;

            if (choosePatternForWords
                && parseInfo.Parts != null//&& parseInfo.Parts.Length > 1
                && pattern != null)
            {
                //sentence does not contain topic, or special word to use --> force one part to choose a matching word

                List<int> partsToChoose = new List<int>();
                float bestMatch = 0;
                for (int i = 0; i < parseInfo.Parts.Length; i++)
                {
                    ParseTreeNode partInfo = parseInfo.Parts[i];
                    Word bestMatchingWord;
                    float match = MatchValue(pattern, partInfo, syntagma, out bestMatchingWord);
                    if (match > bestMatch)
                    {
                        bestMatch = match;
                        partsToChoose.Clear();
                        partsToChoose.Add(i);
                    }
                    else if (match == bestMatch)
                    {
                        partsToChoose.Add(i);
                    }

                }
                if (partsToChoose.Count > 0)
                    partToForceTopic = partsToChoose[Utils.RandomRange(0, partsToChoose.Count)];
            }

            bool allPartsCompleted = true;

            if (parseInfo.Parts != null)
            {

//                ParseTreeNode partWordTopics = null;
                for (int i = 0; i < parseInfo.Parts.Length; i++)
                {
                    if (parseInfo.Parts[i] != null)
                    {
                        string partString;
                        MintyText oldChosenText = chosenText;
                        //TODO: remove this recursion by deciding, if this part is a token or just a text. or is there anything else?
                        if (!TryProcessToken(parseInfo.Parts[i], out partString, out chosenText, (partToForceTopic == i), recursion))
                        {
                            allPartsCompleted = false;
                        }
                        if (chosenText == null)
                            chosenText = oldChosenText;
                        result += partString;
                    }
                }

            }
            else
            {
                allPartsCompleted = TryProcessToken(parseInfo, out result, out chosenText, choosePatternForWords, recursion);
                //Logger.LogError (this,"parse Error in ComicTextProcessor, (check squared brackets) in: \""+text+"\"");
            }

            if (allPartsCompleted)
            {
                parseInfo.SetDynamicSolution(result, chosenText);
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }

            AddNotProcessedLeafNode(parseInfo);
            return false;
        }


        private bool TryProcessToken(ParseTreeNode parseInfo, out string result, out MintyText chosenText, bool choosePatternForWords, int recursion)
        {
            chosenText = null;
            result = "";
            if (parseInfo == null)
            {
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }
            MintyTextsContainer ct = MintyTextsContainer.Instance;
            if (ct == null)
            {
                Logger.LogError(this, "ComicTextsContainer not found. No text processing!");
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }
            if (parseInfo.SolutionString != null)
            {
                chosenText = parseInfo.SolutionComicText;

                if (chosenText != null && parseInfo.name != null && chosenText is Word)
                {
                    if (!namedWords.ContainsKey(parseInfo.name))
                    {
                        namedWords.Add(parseInfo.name, (Word)chosenText);
                    }
                }
                result = parseInfo.SolutionString;
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }

            string text = parseInfo.UnprocessedText;
            Dictionary<string,Word> variableWords = null;
            TextPattern formPattern;
            TokenType token;
            bool allPartsOK = true;

            int i = 0;

            #region TEXT
            switch (parseInfo.type)
            {
                case ParseNodeType.TEXT:
                    chosenText = parseInfo.SolutionComicText;
                    result = parseInfo.UnprocessedText;


                //it is necessary to set relations for other nodes (example: a dependent adjective has to be set)
                    SetRelationalNodes(parseInfo, (Word)chosenText);

                    break;
            #endregion



                #region NAME
                case ParseNodeType.NAME_ME:
                    if (meCharacter != null)
                        result = meCharacter.NickName;
                    break;

                case ParseNodeType.NAME_YOU:
                    if (youCharacter != null)
                        result = youCharacter.NickName;
                    break;

                case ParseNodeType.NAME_OTHER:
                    if (other != null)
                    {
                        result = other.NickName;
                    }
                    break;

                #endregion


                #region SPECIAL_WORD
            //topic
                case ParseNodeType.TOPIC:
                    Word specialWord = null;

//                patternWordName = GetInside (text.Substring (1), '(', ')');
//                patternword = GetNamedWord (patternWordName);
//                if (patternWordName.Length > 0 && patternword == null) {
//                    result = parseInfo.name == null ? text : "[" + parseInfo.name + "=" + text.Substring (1);
//                    return false;
//                }
                    if (!GetDependencyWords(parseInfo, out variableWords, choosePatternForWords, recursion))
                    {
                        AddNotProcessedLeafNode(parseInfo);
                        return false;
                    }

                    formPattern = parseInfo.pattern == null ? new TextPattern() : parseInfo.pattern;
                    if (variableWords != null)
                    {
                        formPattern.AddGramatikConditionsFrom(variableWords[parseInfo.BasicDependencyName]);
                    }
					//                else {
					//                    int posForm = text.IndexOf (':') + 1;
					//                    if (posForm > 0) {
					//                        formPattern.AddConditionString (text.Substring (posForm, text.Length - posForm - 1));
					//                    }
					//                }

					if (MintyEnums.TryGetToken(text, out token))
					{
						switch (token)
						{
							case TokenType.TOPIC:
								if (pattern == null || pattern.PossibleTopics.FlagsInt == 0)
								{
									specialWord = ct.GetRandomWord(new TextPattern(new PathCondition("substantiv")));
									Logger.LogWarning(this, "Topic not found. Set Pattern with topic for processing this sentence! " + (sentence == null ? "" : "\nSentence: " + sentence.Text));
								}
								else
								{
									TextPattern topicNamePattern = new TextPattern(new PathCondition("substantiv"));
									topicNamePattern.AddPatternConditions(new TopicCondition(pattern.PossibleTopics, -1, "name of topic"));
									specialWord = ct.GetRandomWord(topicNamePattern);
									if (specialWord.Text.IndexOf("error") >= 0)
									{
										specialWord = ct.GetRandomWord(new TextPattern(new PathCondition("substantiv")));
										Logger.LogWarning(this, "Topic not found. Set Pattern with topic for processing this sentence! " + (sentence == null ? "" : "\nSentence: " + sentence.Text));
									}
									pattern.RemovePatternType<TopicCondition>();
								}
								break;
						}
					}

                    if (specialWord != null)
                    {
                        specialWord = specialWord.CreateForm(formPattern);
                        if (!chosenWords.Contains(specialWord))
                        {
                            chosenWords.Add(specialWord);

                            if (parseInfo.name != null && !namedWords.ContainsKey(parseInfo.name))
                            {
                                namedWords.Add(parseInfo.name, specialWord);
                            }
                        }
                        chosenText = specialWord;
                        result = specialWord.Text;

                        //is if necessary to set relations for other nodes (example: a dependent adjective has to be set)
                        SetRelationalNodes(parseInfo, (Word)chosenText);
                    }
                    break;
                #endregion



                case ParseNodeType.WORDS:
                    allPartsOK = Process_WORDS(parseInfo, out result, out chosenText, choosePatternForWords, recursion);

                    break;



                #region WORDX
            //previous used named word word(x)
                case ParseNodeType.WORDX:
                    chosenText = null;
                    result = "";

                    if (!GetDependencyWords(parseInfo, out variableWords, choosePatternForWords, recursion))
                    {
                        AddNotProcessedLeafNode(parseInfo);
                        return false;
                    }
                    if (variableWords == null)
                    {
//                    Logger.LogError(this, "delete me!");
                    }
                    formPattern = TextPattern.CreateFromWord(variableWords[parseInfo.BasicDependencyName]);
                    formPattern.Concatenate(parseInfo.pattern, Constants.MATCH_SUPER);

                    if (parseInfo.pattern != null && parseInfo.pattern.HasCondition(typeof(EnumCondition<Article_Type>)))
                    {
                      formPattern.RemovePatternType<EnumCondition<DeclinationType>>();
                      DeclinationType declinationType = GrammaUtil.GetDeclinationType(parseInfo.pattern);
                      formPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));
                    }

                    chosenText = variableWords[parseInfo.BasicDependencyName].CreateForm(formPattern);
                    result = chosenText.Text;

                    if (parseInfo.name != null && !namedWords.ContainsKey(parseInfo.name))
                    {
                        namedWords.Add(parseInfo.name, (Word)chosenText);
                    }

                //is it necessary to set relations for other nodes (example: a dependent adjective has to be set)
                    SetRelationalNodes(parseInfo, (Word)chosenText);


                    break;





            //object(x), subject(x), adjective(x), ...
                case ParseNodeType.SUBJECT:
                case ParseNodeType.OBJECT:
				case ParseNodeType.ADJECTIVE:
				case ParseNodeType.ADVERB:
				case ParseNodeType.VERB:

                    allPartsOK = Process_Subject_Object(parseInfo, out result, out chosenText, choosePatternForWords, recursion);
                    break;

                #endregion


                #region SENTENCES
            //link to another sentence
                case ParseNodeType.SENTENCES:
                    allPartsOK = Process_Sentences(parseInfo, out result, out chosenText, choosePatternForWords, recursion);
                    break;

                #endregion


                #region SWITCH
            //gramatical switch of inline list
                case ParseNodeType.SWITCH_CASUS:
                case ParseNodeType.SWITCH_GENUS:
                case ParseNodeType.SWITCH_NUMERUS:
				case ParseNodeType.SWITCH_REFLEXIVE:
				case ParseNodeType.SWITCH_LAST_OF_X:
                    chosenText = null;
                    result = "";

					if (!GetDependencyWords(parseInfo, out variableWords, choosePatternForWords, recursion))
					{
						AddNotProcessedLeafNode(parseInfo);
						return false;
					}

					//information complete
					else
					{
						//int posInlineList = text.IndexOf('?') + 1;
						//List<string> parts = Split(text.Substring(posInlineList, text.Length - posInlineList - 1), ';');
						int partToTake = 0;
						//if (MintyEnums.Tokens.TryGetValue(text, out token))
						//{
							switch (parseInfo.type)
							{
								case ParseNodeType.SWITCH_GENUS:
									switch (variableWords[parseInfo.BasicDependencyName].genus)
									{
										case Genus.MASCULINUM:
											partToTake = 0;
											break;
										case Genus.FEMININUM:
											partToTake = 1;
											break;
										case Genus.NEUTRUM:
											partToTake = 2;
											break;
									}
									break;
								case ParseNodeType.SWITCH_NUMERUS:
									switch (variableWords[parseInfo.BasicDependencyName].numerus)
									{
										case Numerus.SINGULAR:
											partToTake = 0;
											break;
										case Numerus.PLURAL:
											partToTake = 1;
											break;
									}
									break;
								case ParseNodeType.SWITCH_CASUS:
									switch (variableWords[parseInfo.BasicDependencyName].casus)
									{
										case Casus.NOMINATIV:
											partToTake = 0;
											break;
										case Casus.GENETIV:
											partToTake = 1;
											break;
										case Casus.DATIV:
											partToTake = 2;
											break;
										case Casus.AKKUSATIV:
											partToTake = 3;
											break;
									}
									break;

								case ParseNodeType.SWITCH_REFLEXIVE:
									{
										Word w = variableWords[parseInfo.BasicDependencyName];
										if (w.wordType == WordType.VERB
											&& (w.verbCategory == VerbCategory.REFLEXIV || w.verbCategory == VerbCategory.REFLEXIV_ONLY))
										{
											partToTake = 0;
										}
										else
										{
											partToTake = 1;
										}
									}
									break;


								case ParseNodeType.SWITCH_LAST_OF_X:
								{
									Word w = variableWords[parseInfo.BasicDependencyName];
									int posInlineList = text.IndexOf('?');
									int equalSignPos = text.IndexOf("=", StringComparison.CurrentCulture) + 1;
									string compareString = text.Substring(equalSignPos, posInlineList - equalSignPos);
									partToTake = 1;
									foreach (string compareOrPart in compareString.Split('|'))
									{
										if (w.Text.EndsWith(compareOrPart.Trim(), StringComparison.CurrentCultureIgnoreCase))
										{
											partToTake = 0;
											break;
										}
									}
								}
								break;
							}
						//}

                        allPartsOK = TryProcessText(parseInfo.alternativeParts[partToTake], out result, out chosenText, false, recursion);
                        if (allPartsOK && chosenText != null && chosenText is Word && parseInfo.name != null)
                        {
                            namedWords[parseInfo.name] = (Word)chosenText;


                            //is if necessary to set relations for other nodes (example: a dependent adjective has to be set)
                            SetRelationalNodes(parseInfo, (Word)chosenText);
                        }
                    }
                    break;
                #endregion


                #region ALTERNATIVES
            //random text of inline list
                case ParseNodeType.ALTERNATIVES:
                    int chosenIndex = -1;

                    if (parseInfo.alternativeParts != null
                        && parseInfo.alternativeParts.Count > 0)
                    {

                        object storedIndexObject = null;

                        if (syntagma != null && syntagma.decissionValues.ContainsKey(parseInfo))
                        {
                            //index to choose for syntagma?
                            storedIndexObject = syntagma.decissionValues[parseInfo];

                        }
                        else
                        {
                            //stored index at previous processing?
                            storedIndexObject = parseInfo.GetDynamicData(Constants.KEY_INDEX);

                        }

                        if (storedIndexObject != null && storedIndexObject is int)
                        {
                            chosenIndex = (int)storedIndexObject;
                        }

                        if (chosenIndex < 0)
                        {
                            List<int> partsToChoose = new List<int>();


                            //find those alternatives that fit the pattern best
                            float bestMatch = 0;
                            for (int pNum = 0; pNum < parseInfo.alternativeParts.Count; pNum++)
                            {
                                if (choosePatternForWords)
                                {
                                    ParseTreeNode alternativeInfo = parseInfo.alternativeParts[pNum];
                                    Word bestMatchingWord;
                                    float match = MatchValue(pattern, alternativeInfo, syntagma, out bestMatchingWord);
                                    if (match > bestMatch)
                                    {
                                        bestMatch = match;
                                        partsToChoose.Clear();
                                        partsToChoose.Add(pNum);
                                    }
                                    else if (match == bestMatch)
                                    {
                                        partsToChoose.Add(pNum);
                                    }
                                }
                                else
                                {
                                    partsToChoose.Add(pNum);
                                }
                            }

                            //chosen text is randomly one of all that fits (the topic or the word)
                            //if there are just "links" to other "words" or "sentences", choose one by their weight
                            bool justComicTexts = true;
                            if (partsToChoose.Count > 0)
                            {
                                float[] wordWeights = new float[partsToChoose.Count];
                                int pNum = 0;
                                foreach (int wNum in partsToChoose)
                                {
                                    if (parseInfo.alternativePartsWeights == null)
                                    {
                                        wordWeights[pNum] = 1f;
                                        //                                    Logger.LogError (this, "delete me! parseInfo: "+parseInfo+" in "+parseInfoRoot.UnprocessedText);
                                        //                                    Logger.LogError (this, "delete me!: "+parseInfo.alternativePartsWeights);
                                    }
                                    else
                                    {
                                        wordWeights[pNum] = parseInfo.alternativePartsWeights[wNum];
                                    }

                                    //moved to parseInfo.StoreWeights
//                                if (parseInfo.alternativePartsWeights != null) {
//                                    wordWeights [pNum] = parseInfo.alternativePartsWeights [wNum];
//                                }
//                                else {
//                                    if (parseInfo.alternativeParts [wNum].alternativeComicTexts == null
//                                        || parseInfo.alternativeParts [wNum].alternativeComicTexts.Count==0){
//                                        justComicTexts = false;
//                                    }
//                                    wordWeights [pNum] = parseInfo.alternativeParts [wNum].ProbabilitySum;
//                                }


                                    pNum++;
                                }
                                chosenIndex = partsToChoose[MintyUtils.GetRandomIndex(wordWeights)];

//                            chosenIndex = justComicTexts ? partsToChoose [ComicUtils.GetRandomIndex (wordWeights)]
//                                : partsToChoose [Utils.RandomRange (0, partsToChoose.Count)];
                            }


                            if (chosenIndex < 0)
                            {
                                chosenIndex = Utils.RandomRange(0, parseInfo.alternativeParts.Count);
                            }

                            parseInfo.StoreDynamicData(Constants.KEY_INDEX, chosenIndex);
                        }

                        bool relationsOK = false;
                        i = 0;
                        while (!relationsOK && (i++) < 100)
                        {
                            allPartsOK = TryProcessText(parseInfo.alternativeParts[chosenIndex], out result, out chosenText, choosePatternForWords, recursion);

                            if (allPartsOK && chosenText != null && chosenText is Word && parseInfo.name != null)
                            {
                                namedWords[parseInfo.name] = (Word)chosenText;


                                //is if necessary to set relations for other nodes (example: a dependent adjective has to be set)
                                relationsOK = SetRelationalNodes(parseInfo, (Word)chosenText);
                            }
                        }

                    }
                    break;
                #endregion


                #region SEX
            //sex dependent words
                case ParseNodeType.SEX_ME:
                case ParseNodeType.SEX_OTHER:
                case ParseNodeType.SEX_YOU:

                    ICharacterData character;

//                    List<string> sexParts = Split(text.Substring(text.IndexOf("?") + 1), ':');
//                string wordMale = sexParts[0];
//                string wordFemale = sexParts[1].Substring (0, sexParts[1].Length-1);

                    if (text.IndexOf( MintyEnums.TokensReverse[TokenType.SEX] + "You") >= 0)
                        character = youCharacter;
                    else if (text.IndexOf(MintyEnums.TokensReverse[TokenType.SEX] + "Me") >= 0)
                        character = meCharacter;
                    else
                        character = other;

                    ParseTreeNode maleParseTree = null;
                    ParseTreeNode femaleParseTree = null;
                    if (parseInfo.alternativeParts.Count == 2)
                    {
                        maleParseTree = parseInfo.alternativeParts[0];
                        femaleParseTree = parseInfo.alternativeParts[1];
                    }



                    ParseTreeNode genderParseTree;
                    if (character != null)
                    {
                        genderParseTree = character.Gender == GenderType.MALE ? maleParseTree : femaleParseTree;
                    }
                    else
                    {
                        genderParseTree = maleParseTree;
                    }
                    allPartsOK = TryProcessText(genderParseTree, out result, out chosenText, choosePatternForWords, recursion);
                    if (allPartsOK && chosenText != null && chosenText is Word && parseInfo.name != null && !namedWords.ContainsKey(parseInfo.name))
                    {
                        namedWords.Add(parseInfo.name, (Word)chosenText);

                        //is if necessary to set relations for other nodes (example: a dependent adjective has to be set)
                        SetRelationalNodes(parseInfo, (Word)chosenText);
                    }
                    break;
                #endregion


                #region UP
            //UPPER
                case ParseNodeType.UP:
                    allPartsOK = TryProcessText(parseInfo.alternativeParts[0], out result, out chosenText, choosePatternForWords, recursion);
                    result = (result.Length == 0) ? "" : result[0].ToString().ToUpper() + result.Substring(1);
                    break;
                #endregion


                #region LOW
            //LOWER
                case ParseNodeType.LOW:
                    allPartsOK = TryProcessText(parseInfo.alternativeParts[0], out result, out chosenText, choosePatternForWords, recursion);
                    result = (result.Length == 0) ? "" : result[0].ToString().ToLower() + result.Substring(1);
                    break;
                #endregion


                #region PREDEFINED
                case ParseNodeType.PREDEFINED:
                    if (parseInfo.alternativeComicTexts != null && parseInfo.alternativeComicTexts.Count > 0)
                    {
//                        ComicText predefinedComicText = new List<ComicText>(parseInfo.alternativeComicTexts.Values)[Utils.RandomRange(0, parseInfo.alternativeComicTexts.Count)];
                        MintyTextsContainer.TryGetRandomElement<MintyText>(parseInfo.alternativeComicTexts.Values, out MintyText predefinedComicText);

                        if (parseInfo.name != null && predefinedComicText is Word)
                        {
                            if (!namedWords.ContainsKey(parseInfo.name))
                            {
                                namedWords.Add(parseInfo.name, (Word)predefinedComicText);
                            }
                        }
                        chosenText = predefinedComicText;
                        result = predefinedComicText.Text;

                        if (predefinedComicText is Word)
                        {
                            //necessary to set relations for other nodes (example: a dependent adjective has to be set)
                            SetRelationalNodes(parseInfo, (Word)chosenText);
                        }

                    }
                    break;
                #endregion


                #region INVISIBLE
            //INVISIBLE
                case ParseNodeType.INVISIBLE:
                    allPartsOK = TryProcessText(parseInfo.alternativeParts[0], out result, out chosenText, choosePatternForWords, recursion);
                    if (!allPartsOK)
                    {
                        result = "";//parseInfo.UnprocessedText;
                        AddNotProcessedLeafNode(parseInfo);
                        return false;
                    }
                    else
                    {
                        result = "";
                        if (parseInfo.name != null && chosenText is Word)
                        {
                            namedWords[parseInfo.name] = (Word)chosenText;
                        }

                        //necessary to set relations for other nodes (example: a dependent adjective has to be set)
                        SetRelationalNodes(parseInfo, (Word)chosenText);
                    }
                    break;
                #endregion



                default:
                    result = text;
                    break;
            }

            if (allPartsOK)
            {
                parseInfo.SetDynamicSolution(result, chosenText);
                RemoveNotProcessedLeafNode(parseInfo);
                return true;
            }

            AddNotProcessedLeafNode(parseInfo);
            return false;
        }

        private bool SetRelationalNodes(ParseTreeNode parseInfo, Word word)
        {
            bool hasRelations = false;
            foreach (string varName in parseInfo.AncestorVariables)
            {
                if (syntagma.variables.ContainsKey(varName)
                    && syntagma.variables[varName] == parseInfo)
                {

                    hasRelations = true;
                    break;
                }
            }

            if (hasRelations)
            {
                bool contextOK = true;
                var variableUsageInfos = new Dictionary<string, Dictionary<ParseTreeNode, List<TextRelation>>>();
                foreach (string varName in parseInfo.AncestorVariables)
                {
                    Dictionary<ParseTreeNode, List<TextRelation>> usageInfo;
                    if (!TestRelations(word, varName, syntagma.nodes, out usageInfo))
                    {
                        contextOK = false;
                        break;
                    }
                    else
                    {
                        variableUsageInfos[varName] = usageInfo;
                    }
                }
                if (contextOK)
                {
                    foreach (var nodesAndRelations in variableUsageInfos)
                    {
                        StoreRelationalNodes(ChooseRandomly(nodesAndRelations.Value));
                    }
                    return true;
                }
                return false;
            }

            return true;
        }

		private bool GetDependencyWords(ParseTreeNode parseInfo, out Dictionary<string, Word> variableWords, bool choosePatternForWords, int recursion)
        {
            variableWords = null;


            //decide if parseInfo is an independent node (expl: no verb for dependent objects, ...)
//            bool hasRelations = false;
//            foreach (string varName in parseInfo.AncestorVariables) {
//                if (syntagma.variables.ContainsKey (varName)
//                    && syntagma.variables [varName] == parseInfo) {
//
//                    hasRelations = true;
//                    break;
//                }
//            }
//            //debug
//            if (hasRelations != (parseInfo.HasRelations)) {
//                Logger.DebugL (this, "delete me!");
//            }

            //look if there are variables at the relational node to process before this node can decide for that relational node
            if (parseInfo.HasRelationsInSyntagma(syntagma))
            {
                foreach (ParseTreeNode node in syntagma.nodes)
                {
                    if (!string.IsNullOrEmpty(node.BasicDependencyName)
                        && syntagma.variables.ContainsKey(node.BasicDependencyName)
                        && syntagma.variables[node.BasicDependencyName] == parseInfo)
                    {

                        var additionalVariables = node.GetAdditionalDependencyNodes();
                        if (additionalVariables != null)
                        {
                            foreach (string varName in additionalVariables.Keys)
                            {
								Word variableWord;
                                if (!TryGetNamedWord(varName, out variableWord))
                                {
                                    return false;
                                }
                                else
                                {

                                    if (node.RelationType.casus != Casus.UNDEFINED)
                                    {
                                        TextPattern relationTypePattern = new TextPattern(
                                                                              new EnumCondition<Casus>(node.RelationType.casus)
                                                                          );

                                        variableWord = variableWord.CreateForm(relationTypePattern);
                                    }

                                    node.StoreDynamicData(Constants.KEY_WORD_TO_TAKE, variableWord);
                                }
                            }
                        }
                    }
                }
            }


            if (parseInfo.dependencies == null)
            {
                return true;
            }



            foreach (var nameNode in parseInfo.dependencies)
            {

                Word variableWord = null;
                if (nameNode.Key == Constants.VAR_ARTICLE)
                {
                    if (nameNode.Value.Processed)
                    {
                        variableWord = (Word)nameNode.Value.SolutionComicText;
                    }
                }
                else
                {
                    TryGetNamedWord(nameNode.Key, out variableWord);
                }

                if (variableWord == null)
                {
                    //is the node in the syntagma at all?
                    //if not - try to process it anyways, just to get the data.
                    if (syntagma == null || !syntagma.nodes.Contains(nameNode.Value))
                    {
                        string result;
                        MintyText chosenText;

                        if (TryProcessToken(nameNode.Value, out result, out chosenText, choosePatternForWords, recursion) && chosenText is Word)
                        {
                            variableWord = (Word)chosenText;
                        }
                    }
                }

                if (variableWord == null)
                {
                    return false;
                }
                else
                {
                    if (variableWords == null)
                    {
                        variableWords = new Dictionary<string, Word>(1);
                    }
                    variableWords[nameNode.Key] = variableWord;
                }

            }

            return true;
        }

        public static bool TestAndStoreRelations(Word word, string variableName,
                                         IEnumerable<ParseTreeNode> completeContext, bool storeResultInNodes = true)
        {

            Dictionary<ParseTreeNode, List<TextRelation>> usageInfo;
            if (TestRelations(word, variableName, completeContext, out usageInfo))
            {
                if (storeResultInNodes)
                {
                    StoreRelationalNodes(ChooseRandomly (usageInfo));
                }
                return true;
            }
            return false;
        }


        public static bool TestRelations(Word word, string variableName,
                                         IEnumerable<ParseTreeNode> completeContext,
                                         out Dictionary<ParseTreeNode, List<TextRelation>> usageInfo)
        {

            usageInfo = null;

            //1. test if trivial case:
            if (word == null || word.textRelations == null || word.textRelations.Count == 0)
            {
                return false;
            }


            //2. preparation

            //2.1 create list of dependent relational nodes
            List<ParseTreeNode> context = new List<ParseTreeNode>();
            foreach (ParseTreeNode relationalNode in completeContext)
            {
                if (relationalNode.BasicDependencyName == variableName
                    && relationalNode.RelationType != null)
                {
                    context.Add(relationalNode);
                }
            }
            if (context.Count == 0)
            {
                return true;
            }


            //2.2 create list of all permutations of the relations of word
            List<List<TextRelation>> permutations = new List<List<TextRelation>>(2);

            //TODO: for performance reasons, instead of all permutations,
            //we try just the (shuffeld) list of relations and its reverse order. that should solve most issues.
            var relations = new List<TextRelation>();
            foreach (int relNum in word.textRelations)
            {
                relations.Add(MintyTextsContainer.Instance.textRelations[relNum]);
            }
            permutations.Add(new List<TextRelation>(relations));
            //Shuffle (permutations [0]);

            if (relations.Count > 1)
            {
                permutations.Add(new List<TextRelation>());
                for (int i = permutations[0].Count - 1; i >= 0; i--)
                {
                    permutations[1].Add(permutations[0][i]);
                }
            }



            //3. iterate through relations
            foreach (List<TextRelation> permutation in permutations)
            {
                usageInfo = new Dictionary<ParseTreeNode, List<TextRelation>>();

                //test all relations
                foreach (TextRelation tr in permutation)
                {
                    tr.ContextOK(context, relations, ref usageInfo);
                }

                //all relational nodes with a relation?
                bool allNodesOK = true;
                foreach (ParseTreeNode relationalNode in context)
                {
                    if (!usageInfo.ContainsKey(relationalNode))
                    {
                        allNodesOK = false;
                        break;
                    }
                }

                if (allNodesOK)
                {
                    return true;
                }
            }

            return false;
        }

        private static void StoreRelationalNodes(Dictionary<ParseTreeNode, TextRelation> usageInfo)
        {
            //store relations in nodes!
            if (usageInfo == null)
            {
                return;
            }

            foreach (var keyValue in usageInfo)
            {
                keyValue.Key.StoreDynamicData(Constants.KEY_TEXT_RELATION, keyValue.Value);
            }
        }



        #endregion


        #region WORDS

        private bool Process_WORDS(ParseTreeNode parseInfo, out string result, out MintyText chosenText, bool choosePatternForWords, int recursion)
        {
            Dictionary<string,Word> variableWords = null;
            chosenText = null;
            result = "";

            if (!GetDependencyWords(parseInfo, out variableWords, choosePatternForWords, recursion))
            {
                return false;
            }

            bool allPartsOK = true;
            int i = 0;
            Word randomWord = null;
            //TextPattern relationPattern = null;
            TextPattern pathPattern = (parseInfo.pattern == null ? new TextPattern() : (TextPattern)parseInfo.pattern.Clone());

            if (variableWords != null)
            {
                Word basicDependencyWord = variableWords[parseInfo.BasicDependencyName];
                pathPattern.AddGramatikConditionsFrom(basicDependencyWord);

                Word article = null;

                //0. (german dependent) is this an adjective and its form is dependent on the article of the substantive as well?
                if (parseInfo.WordType == WordType.ADJEKTIV)
                {
                    if (basicDependencyWord.wordType == WordType.ARTICLE)
                    {
                        article = basicDependencyWord;
                    }
                    else
                    {
                        variableWords.TryGetValue(Constants.VAR_ARTICLE, out article);
                    }

                    DeclinationType declinationType;
  					if (article == null)
  					{
  						declinationType = DeclinationType.STRONG;
  					}
  					else
  					{
  						declinationType = GrammaUtil.GetDeclinationType(article);
  					}
  					pathPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));

                    if (article != null)
                    {
                        declinationType = GrammaUtil.GetDeclinationType(article);
                        pathPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));
                    }
                }
            }

            if (parseInfo.WordType == WordType.ADJEKTIV
                && pathPattern.Get<DeclinationType>() == DeclinationType.UNDEFINED
                && pathPattern.Get<Article_Type>() != Article_Type.UNDEFINED_VALUE)
            {
                DeclinationType declinationType = GrammaUtil.GetDeclinationType(pathPattern);
                pathPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));
            }

            if (parseInfo.HasDynamicData(Constants.KEY_DYNAMIC_PATTERN))
            {
                pathPattern.AddGramatikConditionsFrom((TextPattern)parseInfo.GetDynamicData(Constants.KEY_DYNAMIC_PATTERN));
            }

            //1. get list of words to use at this node:
            Dictionary<int,Word> wordsToUse = null;
            if (choosePatternForWords
                && pattern != null
                && pattern.WordsToInclude != null
                && pattern.WordsToInclude.Count > 0)
            {

                //float bestMatch = -Constants.MATCH_PERFECT;
                wordsToUse = new Dictionary<int, Word>();
                foreach (Word wordToUse in pattern.WordsToInclude)
                {
                    if (wordToUse != null
                        && parseInfo.alternativeComicTexts != null
                        && parseInfo.alternativeComicTexts.ContainsKey(wordToUse.num)
                        && parseInfo.NodeIs(wordToUse.textPosition)
                        && pathPattern.IsGrammarCompatible(TextPattern.CreateFromWord(wordToUse)))
                    {

                        wordsToUse.Add(wordToUse.num, wordToUse);

                    }
                }
            }


			//2. get list of topics to use at this node:
			TopicFlags topicsToUse = new TopicFlags();
            if (choosePatternForWords
                && pattern != null
                && pattern.PossibleTopics.FlagsInt > 0)
            {


                //2.1 are there words to use? --> just take best fitting words for one of the topics
                if (wordsToUse != null && wordsToUse.Count > 0)
                {
                    Dictionary<int,Word> wordsWithTopicToUse = new Dictionary<int, Word>();
                    TopicFlags foundTopics = new TopicFlags();
                    foreach (Word wordToUse in wordsToUse.Values)
                    {
						if (wordToUse.topics.ContainsAny(pattern.PossibleTopics))
						{
							wordsWithTopicToUse.Add(wordToUse.num, wordToUse);
							foundTopics |= (wordToUse.topics & pattern.PossibleTopics);
						}
					}

                    if (wordsWithTopicToUse.Count > 0)
                    {
                        wordsToUse = wordsWithTopicToUse;

						TopicCondition topicCondition;
						if (pattern.TryGetFirst<TopicCondition>(out topicCondition))
						{
							topicCondition.conditionTopics &= ~foundTopics;
						}


                        //List<PatternCondition> conditionsToDelete = new List<PatternCondition>();
                        //foreach (ComicTopic foundTopic in foundTopics)
                        //{
                        //    List<TopicCondition> condList;
                        //    if (pattern.TryGetPatternConditions<TopicCondition>(out condList))
                        //    {
                        //        foreach (var cond in condList)
                        //        {
                        //            if (((TopicCondition)cond).conditionTopic == foundTopic)
                        //            {
                        //                conditionsToDelete.Add(cond);
                        //            }
                        //        }
                        //    }
                        //    foreach (PatternCondition condToDelete in conditionsToDelete)
                        //    {
                        //        pattern.RemovePatternCondition(condToDelete);
                        //    }
                        //}
                    }
                }
                else
                {

                    //no words to use, so try to find all topics
                    topicsToUse = pattern.PossibleTopics;
                }
            }



            //3. decide if this is an independent node (no verb for dependent objects, ...)
            bool hasRelations = parseInfo.HasRelationsInSyntagma(syntagma);

            //4. store words to use in dependent nodes (subject, object, ...)
            if (hasRelations)
            {

                if (pattern != null
                    && pattern.WordsToInclude != null
                    && pattern.WordsToInclude.Count > 0)
                {

                    List<Word> wordsToRemove = new List<Word>();
                    foreach (Word w in pattern.WordsToInclude)
                    {
                        TextPattern wordPattern = TextPattern.CreateFromWord(w);
                        float bestMatch = -Constants.MATCH_PERFECT;
                        ParseTreeNode bestNode = null;
                        foreach (ParseTreeNode node in syntagma.nodes)
                        {
                            if (!string.IsNullOrEmpty(node.BasicDependencyName)
                                && syntagma.variables.ContainsKey(node.BasicDependencyName)
                                && syntagma.variables[node.BasicDependencyName] == parseInfo)
                            {

                                if (node.pattern != null
                                    && node.pattern.IsGrammarCompatible(wordPattern))
                                {

                                    float match = node.pattern.MatchValue(w);
                                    if (match > bestMatch)
                                    {
                                        bestMatch = match;
                                        bestNode = node;
                                    }
                                }
                            }
                        }

                        if (bestMatch > 0 && !bestNode.HasDynamicData(Constants.KEY_WORD_TO_TAKE))
                        {
                            //store the word (overwrites existing values with same key)
                            bestNode.StoreDynamicData(Constants.KEY_WORD_TO_TAKE, w);

                            if (bestNode.type == ParseNodeType.ADJECTIVE)
                            {
                                pathPattern.AddGramatikConditionsFrom(w);
                            }

                            wordsToRemove.Add(w);
                        }
                    }

                    if (wordsToRemove.Count > 0)
                    {
                        foreach (Word wordToRemove in wordsToRemove)
                        {
                            List<WordCondition> wordConditions;
                            if (pattern.TryGetPatternConditions<WordCondition>(out wordConditions))
                            {
                                foreach (var cond in wordConditions)
                                {
                                    if (cond.conditionWord == wordToRemove)
                                    {
                                        pattern.RemovePatternCondition(cond);
                                    }
                                }
                            }
                        }
                    }
                }
            }



            i = 0;
            //5. find random word

            TextPattern randomPattern = (TextPattern)pathPattern.Clone();
            if (topicsToUse.FlagsInt > 0)
            {
                randomPattern.AddPatternConditions(new TopicCondition (topicsToUse));
            }



            if (wordsToUse == null || wordsToUse.Count == 0)
            {
                //if there is no word to use, take any word, that fits the pattern

                List<Word> words = MintyTextsContainer.Instance.GetWords(randomPattern);
                if (words != null && words.Count > 0)
                {
                    wordsToUse = new Dictionary<int, Word>();
                    foreach (Word w in words)
                    {
                        wordsToUse[w.num] = w;
                    }
                }
            }

            //do while the word that was found is used allready (chosenWords)
            do
            {


                if (wordsToUse != null && wordsToUse.Count > 0)
                {

                    if (!hasRelations || parseInfo.WordType == WordType.ARTICLE)
                    {
                        //trivial case
                        MintyTextsContainer.TryGetRandomElement<Word>(wordsToUse.Values, out randomWord);

                    }
                    else
                    {
                        //relations to solve

                        bool oneWord = wordsToUse.Count == 1;
                        while (wordsToUse.Count > 0)
                        {
                            MintyTextsContainer.TryGetRandomElement<Word>(wordsToUse.Values, out Word wordToUse);
                            wordsToUse.Remove(wordToUse.num);


                            bool contextOK = true;
                            var variableUsageInfos = new Dictionary<string, Dictionary<ParseTreeNode, List<TextRelation>>>();
                            foreach (string varName in parseInfo.AncestorVariables)
                            {
                                Dictionary<ParseTreeNode, List<TextRelation>> usageInfo;
                                if (!TestRelations(wordToUse, varName, syntagma.nodes, out usageInfo))
                                {
                                    contextOK = false;
                                    break;
                                }
                                else
                                {
                                    variableUsageInfos[varName] = usageInfo;
                                }
                            }
                            if (contextOK)
                            {
                                foreach (var keyVal in variableUsageInfos)
                                {
                                    StoreRelationalNodes(ChooseRandomly (keyVal.Value));
                                }
                                randomWord = wordToUse;
                                break;
                            }

                            //reason can be: only one given word to use, which does not fit in the context.
                            //if this is the case: retry again after deleting this word.
                            if (oneWord)
                            {
                                oneWord = false;
                                var randomWords = MintyTextsContainer.Instance.GetWords(randomPattern);
                                if (randomWords != null && randomWords.Count > 0)
                                {
                                    foreach (Word w in randomWords)
                                    {
                                        wordsToUse[w.num] = w;
                                    }
                                }
                            }
//                            if (TestRelations (wordToUse, parseInfo.name, syntagma.nodes, true)) {
//                                randomWord = wordToUse;
//                                break;
//                            }
                        }
                    }
                }

                if (randomWord == null)
                {
					//emergency!
					if (MintyTextsContainer.TryGetRandomElement<Word>(MintyTextsContainer.Instance.GetWords(randomPattern), out randomWord))
					{ 
						Logger.LogWarning(this, "used just a random word. (not fitting the context?): " + randomWord.Text + " " + parseInfo + "\nsentence: " + sentence); 
					}
					else
					{
						Logger.LogError(this, "no word found!: "+randomPattern + "\n" + parseInfo + "\nsentence: " + sentence);
					}
				}
                //{
                //    Logger.LogError(this, "no words to choose from! " + parseInfo);
                //}

            } while (randomWord != null && chosenWords.Contains(randomWord) && (i++) <= 50);

            if (i >= 50)
            {
                Logger.DebugL(this, "no random word after " + i + " iterations. "+"node: "+parseInfo+ "\nsentence: " + sentence);
            }


            //6. remove random word as wordToUse in pattern
            if (randomWord != null && wordsToUse != null && wordsToUse.Count > 0 && pattern != null)
            {
                //delete wordToUse from pattern


                PatternCondition conditionToDelete = null;
                List<WordCondition> condList;
                if (pattern.TryGetPatternConditions<WordCondition>(out condList))
                    foreach (var cond in condList)
                    {
                        {
                            if (cond.conditionWord.num == randomWord.num)
                            {
                                conditionToDelete = cond;
                                break;
                            }
                        }
                    }
                if (conditionToDelete != null)
                {
                    pattern.RemovePatternCondition(conditionToDelete);
                }
            }


            //7. create form:
            if (pathPattern != null && randomWord != null)
            {
                if (variableWords != null
                    && variableWords.ContainsKey(Constants.VAR_ARTICLE))
                {

                    pathPattern.AddPatternConditions(new EnumCondition<Article_Type>(variableWords[Constants.VAR_ARTICLE].article_type));
                }

                randomWord = randomWord.CreateForm(pathPattern);
            }




            if (randomWord != null && randomWord.Text != null)
            {


                if (randomWord.IsLeaf)
                {
                    if (!chosenWords.Contains(randomWord)
                        && randomWord.wordType != WordType.ARTICLE)
                    {
                        chosenWords.Add(randomWord);
                    }



                    //if this is an article, dependent on an object --> let's see if there is a praeposition to use!
                    string praeposition = "";
                    if (randomWord.wordType == WordType.ARTICLE
                        && !string.IsNullOrEmpty(parseInfo.BasicDependencyName)
                        && syntagma.variables.ContainsKey(parseInfo.BasicDependencyName))
                    {

                        ParseTreeNode relatedNode = syntagma.variables[parseInfo.BasicDependencyName];
                        if (relatedNode != null)
                        {
                            object data = relatedNode.GetDynamicData(Constants.KEY_TEXT_RELATION);
                            if (data != null && data is TextRelation && ((TextRelation)data).praeposition != null)
                            {
                                praeposition = ((TextRelation)data).praeposition + " ";
                            }
                        }
                    }



                    result = praeposition + randomWord.Text;
                }
                else
                {
                    randomWord.DoParse();
                    allPartsOK = TryProcessText(randomWord.ParseTree, out result, out chosenText, choosePatternForWords, recursion);
                    if (allPartsOK)
                    {
                        randomWord.ParseTree.Reset();
                    }
                }
            }

            if (randomWord == null)
            {
                Logger.DebugL(this, "random word is null: " + parseInfo.UnprocessedText+"\nsentence: "+sentence.Text);
            }
            else
            {
                if (chosenText == null)
                    chosenText = randomWord;

                allPartsOK = true;

                if (parseInfo.name != null && !namedWords.ContainsKey(parseInfo.name))
                {
                    namedWords.Add(parseInfo.name, (Word)chosenText);
                }
            }

            return allPartsOK;
        }

        #endregion


        #region SUBJECT OBJECT

        private bool Process_Subject_Object(ParseTreeNode parseInfo, out string result, out MintyText chosenText, bool choosePatternForWords, int recursion)
        {

            bool allPartsOK = true;
            Dictionary<string, Word> variableWords = null;
            string text = parseInfo.UnprocessedText;
            chosenText = null;
            result = "";

            if (!GetDependencyWords(parseInfo, out variableWords, choosePatternForWords, recursion))
            {
                return false;
            }


            //find grammatical form
            TextPattern formPattern = (parseInfo.pattern == null ? new TextPattern() : (TextPattern)parseInfo.pattern.Clone());
            switch (parseInfo.type)
            {
                case ParseNodeType.SUBJECT:
                    formPattern.AddPatternConditions(new EnumCondition<Casus>(Casus.NOMINATIV));

                    break;
                case ParseNodeType.ADJECTIVE:
                    formPattern.AddGramatikConditionsFrom(variableWords[parseInfo.BasicDependencyName], false);


                    if (!formPattern.HasCondition(typeof(Article_Type)))
                    {
                        formPattern.AddPatternConditions(new EnumCondition<Article_Type>(Article_Type.UNDEFINED_ARTICLE));
                    }

                    //article
                    if (variableWords.Count > 1
                        && variableWords.ContainsKey(Constants.VAR_ARTICLE))
                    {
                        DeclinationType declinationType = GrammaUtil.GetDeclinationType(variableWords[Constants.VAR_ARTICLE]);
                        formPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));
                    }
                    else
                    {
                        DeclinationType declinationType = GrammaUtil.GetDeclinationType(formPattern);
                        formPattern.AddPatternConditions(new EnumCondition<DeclinationType>(declinationType));
                    }
                    break;
            }



            Word wordToUse = null;


            //if there is a word to use from pattern fitting the Position use it!
            if (choosePatternForWords && pattern != null && pattern.WordsToInclude != null)
            {
                float bestMatch = -Constants.MATCH_PERFECT;
                foreach (Word pw in pattern.WordsToInclude)
                {
                    if (parseInfo.NodeIs(pw.textPosition)
                        && formPattern.IsGrammarCompatible(TextPattern.CreateFromWord(pw)))
                    {

                        float match = formPattern.MatchValue(pw);
                        if (match > bestMatch)
                        {
                            wordToUse = pw;
                            bestMatch = match;
                        }
                    }
                }

                if (wordToUse != null)
                {
                    wordToUse = wordToUse.CreateForm(formPattern);
                    result = wordToUse.Text;


                    //this is to late at that moment. the BasicDependencyNode has been processed.
//                    //if adjective pattern has a grammatical form which is not set at BasicDependencyNode.pattern --> add that!
//                    if (parseInfo.type == ParseNodeType.ADJECTIVE
//                        && parseInfo.BasicDependencyNode != null)
//                    {
//                        parseInfo.BasicDependencyNode.StoreDynamicData(Constants.KEY_DYNAMIC_PATTERN, TextPattern.CreateFromWord(wordToUse));
//                    }
                }
            }


            if (wordToUse == null)
            {
                //word to choose "sent from another parseInfoNode"
                if (parseInfo.HasDynamicData(Constants.KEY_WORD_TO_TAKE))
                {
                    wordToUse = (Word)parseInfo.GetDynamicData(Constants.KEY_WORD_TO_TAKE);
                    if (!wordToUse.IsLeaf)
                    {
                        wordToUse.DoParse();
                        allPartsOK = TryProcessText(wordToUse.ParseTree, out result, out chosenText, choosePatternForWords, recursion);
                        if (allPartsOK)
                            wordToUse.ParseTree.Reset();
                        if (chosenText != null && chosenText is Word)
                            wordToUse = (Word)chosenText;
                    }
                    formPattern.AddGramatikConditionsFrom(wordToUse);
                    wordToUse = wordToUse.CreateForm(formPattern);
                    result = wordToUse.Text;

                    if (parseInfo.HasRelationsInSyntagma(syntagma))
                    {
                        //check context for this word:
                        foreach (string varName in parseInfo.AncestorVariables)
                        {
                            if (!TestAndStoreRelations(wordToUse, varName, syntagma.nodes))
                            {
                                Logger.LogWarning(this, "Context not ok for " + varName + "=" + wordToUse.Text + " at " + parseInfo);
                            }
                        }
                    }
                }
            }


            if (wordToUse == null && variableWords != null)
            {
                //textRelation


                //is there a TextRelation to take?
                if (parseInfo.HasDynamicData(Constants.KEY_TEXT_RELATION))
                {
                    var textRelationToTake = (TextRelation)parseInfo.GetDynamicData(Constants.KEY_TEXT_RELATION);

					var selectionPattern = TextPattern.CreateFromPaths(textRelationToTake.path);
					if (parseInfo.WordType == WordType.ADVERB
						&& (formPattern.Get<Komparation>() == Komparation.Komparativ || formPattern.Get<Komparation>() == Komparation.Superlativ))
					{
						selectionPattern.AddPatternConditions(new EnumCondition<TextFlag>(TextFlag.NO_COMPARATION, Constants.MATCH_PERFECT, true));
					}

					List<Word> words = MintyTextsContainer.Instance.GetWords(selectionPattern);
                    Dictionary<int, Word> fittingWords = new Dictionary<int, Word>();
                    foreach (Word w in words)
                    {
                        fittingWords[w.num] = w;
                    }

                    while (fittingWords.Count > 0)
                    {
                        MintyTextsContainer.TryGetRandomElement<Word>(fittingWords.Values, out wordToUse);
                        fittingWords.Remove(wordToUse.num);

                        if (!parseInfo.HasRelationsInSyntagma(syntagma))
                        {
                            break;
                        }
                        else
                        {

                            bool contextOK = true;
                            var variableUsageInfos = new Dictionary<string, Dictionary<ParseTreeNode, List<TextRelation>>>();
                            foreach (string varName in parseInfo.AncestorVariables)
                            {
                                Dictionary<ParseTreeNode, List<TextRelation>> usageInfo;
                                if (!TestRelations(wordToUse, varName, syntagma.nodes, out usageInfo))
                                {
                                    contextOK = false;
                                    break;
                                }
                                else
                                {
                                    variableUsageInfos[varName] = usageInfo;
                                }
                            }
                            if (contextOK)
                            {
                                foreach (var keyVal in variableUsageInfos)
                                {
                                    StoreRelationalNodes(ChooseRandomly (keyVal.Value));
                                }
                                break;
                            }

                        }
                    }


//                    formPattern.AddPatternConditions(new EnumCondition<WordType>(wordToUse.wordType, Constants.MATCH_SUPER));

                    if (wordToUse.wordType == WordType.PRONOMEN_REFLEXIV)
                    {
                        formPattern.AddPatternConditions(new EnumCondition<Person>(variableWords[parseInfo.BasicDependencyName].person));
                    }
                }
                else
                {
                    //no TextRelation has been set
                    Logger.LogError(this, "no relation set: " + parseInfo+" in "+sentence.Text+"\nsyntagma: "+syntagma);
                    wordToUse = MintyTextsContainer.Instance.GetRandomWord(formPattern);
                }

                if (wordToUse == null)
                {
                    Logger.LogError(this, "no relational word found: " + parseInfo + " in " + sentence.Text);
                }
                else
                {
                    wordToUse = wordToUse.CreateForm(formPattern);
                }
            }



            //Logger.DebugL (this, "wordToUse: " + wordToUse.Text+", wordForm: "+wordForm.ToString());
            //                    wordToUse = wordToUse.CreateForm (wordForm);
            if (wordToUse == null)
            {
                Logger.DebugL(this, "\nno word found: " + parseInfo + " in: " + parseInfo.root.UnprocessedText);
            }
            else
            {

                if (!wordToUse.IsLeaf)
                {
                    wordToUse.DoParse();
                    allPartsOK = TryProcessText(wordToUse.ParseTree, out result, out chosenText, choosePatternForWords, recursion);
                    if (allPartsOK)
                        wordToUse.ParseTree.Reset();
                    if (chosenText != null && chosenText is Word)
                        wordToUse = (Word)chosenText;
                }

                if (parseInfo.name != null)
                {
                    namedWords[parseInfo.name] = wordToUse;
                }

                chosenText = wordToUse;
                result = wordToUse.Text;
            }

            return allPartsOK;
        }

        #endregion

        #region process SENTENCE

        private bool Process_Sentences(ParseTreeNode parseInfo, out string result, out MintyText chosenText, bool choosePatternForWords, int recursion)
        {

            string text = parseInfo.UnprocessedText;
            chosenText = null;
            result = "";

            //wait until all named nodes of the syntagma are processed that do not depend ont the sentence itself to select best sentence and have variable names available.
            if (syntagma != null)
            foreach (var node in syntagma.nodes)
            {
                Dictionary<string, Word> variableWords;
                if (node != parseInfo
                	&& !GetDependencyWords(node, out variableWords, choosePatternForWords, recursion))
                {
                    return false;
                }
            }

            Dictionary<string, Word> namedWordsForSubsentence = new Dictionary<string, Word>();
            foreach (var nameWordPair in namedWords.Union(dependent_sentence_NamedWords))
            {
                namedWordsForSubsentence[nameWordPair.Key] = nameWordPair.Value;
            }

            Dictionary<int, Sentence> bestSentences = new Dictionary<int, Sentence>();
            TextPattern sentencePattern = (pattern == null ? new TextPattern() : (TextPattern)pattern.Clone());
			sentencePattern.Concatenate(parseInfo.pattern);
            if (namedWordsForSubsentence.Count > 0)
            {
                var varDepCo = new VariableDependencyCondition(new HashSet<string>(namedWordsForSubsentence.Keys));
                sentencePattern.AddPatternConditions(varDepCo);
            }
            //if (choosePatternForWords)
            //{
                if (parseInfo.alternativeComicTexts == null)
                {
                    if (choosePatternForWords) Logger.DebugL(this, "sentence link has null reference.\n" + sentence.Text);
                    return false;
                }



                float bestMatch = 0;
                foreach (MintyText s in parseInfo.alternativeComicTexts.Values)
                {
                    float match = sentencePattern.MatchValue(s);
                    if (match > bestMatch)
                    {
                        bestMatch = match;
                        bestSentences.Clear();
                        bestSentences.Add(s.num, (Sentence)s);
                }
#if UNITY_5_3_OR_NEWER
                                    else if (Mathf.Approximately(match, bestMatch))
#else
                else if (float.Equals(match, bestMatch))
#endif
                {
                    if (!bestSentences.ContainsKey(s.num))
                            bestSentences.Add(s.num, (Sentence)s);
                    }
                }
			//}
			//else
			//{
			//    if (parseInfo.alternativeComicTexts != null)
			//    {
			//        bestSentences = parseInfo.alternativeComicTexts;
			//    }
			//}

			if (MintyTextsContainer.TryGetRandomElement<Sentence>(bestSentences.Values, out Sentence linkedSentence))

			{
				linkedSentence.next_processing_namedWords = namedWordsForSubsentence;
				linkedSentence.SetPatternForNextProcessing(sentencePattern);
				result = linkedSentence.Process(meCharacter, youCharacter, other);
				parseInfo.SetDynamicSolution(result, linkedSentence);
				if (linkedSentence.last_processing_namedWords != null)
				{
					foreach (var usedWord in linkedSentence.last_processing_namedWords)
					{
						if (!namedWords.ContainsKey(usedWord.Key))
						{
							namedWords[usedWord.Key] = usedWord.Value;
						}
					}
				}
				linkedSentence.Reset();

				//linkedSentence.DoParse();
				//allPartsOK = ProcessText(linkedSentence.ParseTree, out result, out chosenText, choosePatternForWords, recursion);
				//if (allPartsOK)
				//{
				//    linkedSentence.ParseTree.Reset();
				//    //linkedSentence.Reset();
				//}
				chosenText = linkedSentence;
				if (parseInfo.name != null && !namedWords.ContainsKey(parseInfo.name))
				{
					//namedWords.Add (parseInfo.name, linkedSentence);
					Logger.LogWarning(this, "Sentence link can not (yet) have a variable name.");
				}

				return true;
			}
			else
			{
				chosenText = MintyText.ErrorText<Sentence>("Error finding sentence");
				return false;
			}


		}



        #endregion

        //        public ICharacterData Other
        //        {
        //            get
        //            {
        //                if (other == null)
        //                {
        //                    ICharacterData best = null;
        //
        //                    if (best == null)
        //                    {
        //                        best = new CharacterWrapper("Barbara", Gender.FEMALE);
        //                        best.BodyType = BodyType.FEMALE_PYKNIC;
        //                    }
        //                    other = best;
        //
        //                }
        //                return other;
        //            }
        //        }


        #region private methods



        private List<string> GetExternalVariables()
        {
            //if processing a sentence, every entry in dependency list must have an entry in dependent_sentence_NamedWords
            List<string> externalVariables = new List<string>();

            if (sentence is Sentence && ((Sentence)sentence).dependencyList != null)
            {
                List<string> missingVariables = new List<string>();
                foreach (string variable in ((Sentence)sentence).dependencyList)
                {
                    if (dependent_sentence_NamedWords == null
                        || !dependent_sentence_NamedWords.ContainsKey(variable))
                    {

                        missingVariables.Add(variable);
                    }
                    else
                    {

                        externalVariables.Add(variable);
                    }
                }

                if (missingVariables.Count > 0)
                {
                    foreach (string missingVariable in missingVariables)
                    {

                        Word namedWord;
                        if (TryGetNamedWord(missingVariable, out namedWord))
                        {
                            dependent_sentence_NamedWords[missingVariable] = namedWord;
                            externalVariables.Add(missingVariable);
                        }

                    }
                }
            }

            return externalVariables;
        }

        private void StoreDependentNode(string dependencyNodeName, ParseTreeNode processedWords)
        {
            if (dependencyNodeName.Length > 0)
            {
//                processedWords.BasicDependencyName = dependencyNodeName;

                List<ParseTreeNode> dependentNodes;
                if (!dependencies.TryGetValue(dependencyNodeName, out dependentNodes))
                {
                    dependentNodes = new List<ParseTreeNode>();
                    dependencies.Add(dependencyNodeName, dependentNodes);
                }
                if (!dependentNodes.Contains(processedWords))
                {
                    dependentNodes.Add(processedWords);
                }
            }
        }

		/// <summary>
		/// Trys to get a named word (variable).
		/// 1. from processed named nodes of the syntagma
		/// 2. from subsentence (sentence-nodes) in the syntagma
		/// 3. from named words not in the syntagma (hä?)
		/// 4. from processed dependent sentences (previously processed sentences)
		/// 5. guess from name (if intended)
		/// </summary>
		/// <returns>The Word.</returns>
		/// <param name="name">Name.</param>
		private bool TryGetNamedWord(string name, out Word word, bool doGuessIfNecessary = false)
        {
            word = null;
            ParseTreeNode varNode;
			if (syntagma != null)
			{
				//1. from processed nodes
				if (syntagma.variables.TryGetValue(name, out varNode))
				{
					if (varNode.Processed)
					{
						if (varNode.SolutionComicText is Word)
						{
							word = (Word)varNode.SolutionComicText; ;
							return true;
						}
						else if (varNode.SolutionComicText is Sentence)
						{
							if (((Sentence)varNode.SolutionComicText).last_processing_namedWords != null
								&& ((Sentence)varNode.SolutionComicText).last_processing_namedWords.TryGetValue(name, out word))
							{
								return true;
							}
						}
					}
					else if (namedWords.TryGetValue(name, out word))
					{
						//how can this be?
						return true;
					}
					else
					{
						//not yet rendered
						return false;
					}
				}

				//2.from subsentence
			}

            if (namedWords.TryGetValue(name, out word))
            {
                return true;
            }



            if (dependent_sentence_NamedWords.TryGetValue(name, out word))
            {
                return true;
            }

			if (doGuessIfNecessary)
			{
				//try to find a "similar" word like "o" for "o1"
				string similarName = null;
				if (name.Contains('o'))
				{
					similarName = "o";
				}
				else if (name.Contains('v'))
				{
					similarName = "v";
				}
				else

					if (name.Contains('a'))
				{
					similarName = "a";
				}
				else if (name.Contains('s'))
				{
					similarName = "s";
				}

				if (similarName != null && dependent_sentence_NamedWords.TryGetValue(name, out word))
				{
					return true;
				}

				//try to guess
				TextPattern pathPattern = null;
				if (name.Contains('o'))
				{
					pathPattern = new TextPattern(
											new PathCondition("substantiv/person"),
											new PathCondition("substantiv/nature"),
											new PathCondition("substantiv/thing")
										);

				}
				else if (name.Contains('v'))
				{
					pathPattern = new TextPattern(
											new PathCondition("verb")
										);
				}
				else if (name.Contains('a'))
				{
					pathPattern = new TextPattern(
											new PathCondition("adjective")
										);
				}
				else if (name.Contains('s'))
				{
					pathPattern = new TextPattern(
							new PathCondition("substantiv/person"),
							new PathCondition("substantiv/nature/animals")
							);
				}
				if (pathPattern != null)
				{
					word = MintyTextsContainer.Instance.GetRandomWord(pathPattern);

					if (word != null)
					{
						return true;
					}
				}
			}


            return false;
        }

        /// <summary>
        /// looks how goog a node fits a pattern
        /// </summary>
        /// <returns>The matching-value (the better the higher value).</returns>
        /// <param name="pattern">Pattern of topics, words, textpositions, ....</param>
        /// <param name="parseTree">Parse tree.</param>
        /// <param name="bestMatchingWord">returns the best matching word, if there are words in the pattern.</param>
        private float MatchValue(TextPattern pattern, ParseTreeNode parseTree, Syntagma syntagma, out Word bestMatchingWord)
        {
            bestMatchingWord = null;
            if (parseTree == null || pattern == null)
                return 0;
            float match = 0;

            //1. look if there is a variable at the node that points directly to the content of that node
            if (parseTree.type == ParseNodeType.WORDX
                || parseTree.GetAdditionalDependencyNodes() != null)
            {

                return 0;
            }

            //2. look how good that node fits topics in tha pattern
            float topicsMatch = 0f;
            TopicFlags possibleTopics = pattern.PossibleTopics;

			if (!(pattern.PossibleTopics & parseTree.topics).IsEmpty())
			{
				topicsMatch += Constants.MATCH_GOOD;
			}
			//if (possibleTopics != null && possibleTopics.Count > 0
            //    && parseTree.topics != null && parseTree.topics.Count > 0)
            //{
            //    foreach (ComicTopic topic in possibleTopics)
            //    {
            //        if (parseTree.topics.ContainsKey((int)topic))
            //        {
            //            topicsMatch += Constants.MATCH_GOOD;
            //            break;
            //        }
            //    }
            //}
            match += topicsMatch;

            //3. look how good words in the pattern fit in the node
            //the best match counts
            float bestWordMatch = 0;
            Word w;
            Word rawWord;
            List<WordCondition> wordsToInclude;
            if (pattern.TryGetPatternConditions<WordCondition>(out wordsToInclude))
            {
                foreach (var wCond in wordsToInclude)
                {
                    w = wCond.conditionWord;
                    rawWord = MintyTextsContainer.Instance.words[w.num];
                    float wordMatch = 0f;

                    if (parseTree.words != null && parseTree.words.Count > 0)
                    {
                        if (parseTree.NodeIs(w.textPosition)
                            && w != null
                            && parseTree.words.ContainsKey(syntagma)
                            && parseTree.words[syntagma].Contains(w))
                        {

                            wordMatch = wCond.impact * (wCond.not ? -1 : 1);
                            Word form;
                            foreach (ParseTreeNode syntagmaNode in syntagma.nodes)
                            {
                                TextPattern lastWordPattern;
                                if (parseTree.lastWordPattern != null
                                    && parseTree.lastWordPattern.TryGetValue(syntagmaNode, out lastWordPattern)
                                    && rawWord.TryCreateForm(lastWordPattern, out form)
                                    && string.Equals(form.Text, w.Text, StringComparison.CurrentCultureIgnoreCase))
                                {

                                    wordMatch += wCond.impact * (wCond.not ? -2 : 2);
                                }
                            }

//                            break;
                        }
                    }
                    //this words fit a fixed text?
                    else if (parseTree.pattern != null
                             && parseTree.pattern.HasCondition(typeof(FixedTextCondition)))
                    {
                        List<FixedTextCondition> condList;
                        if (parseTree.pattern.TryGetPatternConditions<FixedTextCondition>(out condList))
                        {
                            foreach (var fixedTextCondition in condList)
                            {
                                if (fixedTextCondition.MatchValue(w) > 0)
                                {
                                    wordMatch += wCond.impact * (wCond.not ? -3 : 3);
                                }
                            }
                        }
                    }


                    if (wordMatch > 0
                        && w.textPosition != TextPosition.UNDEFINED
                        && parseTree.NodeIs(w.textPosition))
                    {
                        wordMatch += wCond.impact * (wCond.not ? -0.5f : 0.5f);
                    }

                    if (wordMatch > bestWordMatch)
                    {
                        bestWordMatch = wordMatch;
                        bestMatchingWord = w;
                    }

                }
            }
            else
            {
                //TODO: check: this block will never be reached?
//                if (wordsToInclude != null && wordsToInclude.Count > 0
//                    && parseTree.lastWordPattern != null && parseTree.lastWordPattern.HasCondition(typeof(FixedTextCondition)))
//                {
//                    foreach (Word w in wordsToInclude) {
//                        float wordMatch = 0f;
//                        if (parseTree.NodeIs (w.textPosition)) {
//                            string fixedWord = ((FixedTextCondition)parseTree.LastWordPattern.GetPatternConditions (typeof(FixedTextCondition)) [0]).text;
//                            if (string.Equals (fixedWord, w.Text, StringComparison.CurrentCultureIgnoreCase)) {
//
//                                wordMatch = 2*Constants.MATCH_SUPER;
//
//                                if (w.textPosition != TextPosition.UNDEFINED && parseTree.NodeIs (w.textPosition)) {
//                                    wordMatch += Constants.MATCH_GOOD +Constants.MATCH_MINIMAL;
//                                }
//
//                                if (wordMatch > bestWordMatch) {
//                                    bestWordMatch = wordMatch;
//                                }
//                                break;
//                            }
//                        }
//                    }

//                }
            }


            match += bestWordMatch;

            //if this node is a relation node, it's possible words are stored in the related node.
            //give it matchingpoints for the right position!
//            if (wordsToInclude != null) {
//                foreach (PatternCondition wCond in wordsToInclude) {
//                    Word w = ((WordCondition)wCond).conditionWord;
//                    if (w.textPosition != TextPosition.UNDEFINED && parseTree.NodeIs (w.textPosition)) {
//                        match += Constants.MATCH_MINIMAL + Constants.MATCH_GOOD;
//                        break;
//                    }
//                }
//            }

//            Logger.DebugL (this,"match: "+match+" "+pattern+" -- "+(parseTree.Words==null?0:parseTree.Words.Count) + " words in: "+parseTree);
            return match;
        }

        private void AddNotProcessedLeafNode(ParseTreeNode parseInfo)
        {
            if (parseInfo.IsLeaf && !notProcessedLeafNodes.Contains(parseInfo))
            {
                notProcessedLeafNodes.Add(parseInfo);
            }
        }


        private void RemoveNotProcessedLeafNode(ParseTreeNode parseInfo)
        {
            if (notProcessedLeafNodes.Contains(parseInfo))
            {
                notProcessedLeafNodes.Remove(parseInfo);
            }
        }

		#endregion

		#region static methods



		private static Dictionary<ParseTreeNode, TextRelation> ChooseRandomly(Dictionary<ParseTreeNode, List<TextRelation>> allNodesAndRelations)
		{
			Dictionary<ParseTreeNode, TextRelation> result = new Dictionary<ParseTreeNode, TextRelation>();
			if (allNodesAndRelations != null && allNodesAndRelations.Count > 0)
			{
				foreach (var nodeRelations in allNodesAndRelations)
				{
					if (nodeRelations.Value != null && nodeRelations.Value.Count > 0)
					{
						//result.Add(nodeRelations.Key, nodeRelations.Value[Utils.RandomRange(0, nodeRelations.Value.Count)]);
						if (MintyTextsContainer.TryGetRandomElement<TextRelation>(nodeRelations.Value, out TextRelation relation))
						{
							result.Add(nodeRelations.Key, relation); 
						}
					}
				}
			}
			return result;
		}

		public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Utils.RandomRange(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool TryGetVariableNames(string text, out string[] result)
        {
			result = null;
			var inside = GetInside(text, '(', ')');
			if (string.IsNullOrEmpty(inside))
			{
				return false;
			}
			result = inside.Split(',');
			return true;
        }

        public static string GetInside(string text, char li, char re, int startpos = 0)
        {
            int liPos = FirstDividerPos(text, li, startpos);
            int rePos = FirstDividerPos(text, re, liPos);
            if (liPos >= 0 && rePos >= 0 && liPos < rePos)
            {
                return text.Substring(liPos + 1, rePos - liPos - 1);
            }
            return null;
        }

        public static string GetOutside(string text, char li, char re, int startpos = 0)
        {
//            string outside = "";
            List<char> chars = new List<char>(text.Length);
            bool inside = false;
            foreach (char c in text)
            {
                if (c == li)
                    inside = true;
                else if (c == re)
                    inside = false;
                else if (!inside)
                {
                    chars.Add(c);
//                    outside += c;
                }
            }

            return new string(chars.ToArray());
        }

        public static int FirstDividerPos(string text, char devider, int startpos = 0)
        {
            if (text == null || text.Length == 0)
                return -1;
            startpos = startpos < 0 || startpos >= text.Length ? 0 : startpos;
            int bracketsCounter = 0;
            for (int i = startpos; i < text.Length; i++)
            {
                if (text[i] == devider && bracketsCounter == 0)
                {
                    return i;
                }
                if (devider != '[' && devider != ']')
                {
                    if (text[i] == '[')
                        bracketsCounter++;
                    if (text[i] == ']')
                        bracketsCounter--;
                }
            }
            return -1;
        }


        public static List<string> Split(string text, char devider)
        {
            if (text == null || text.Length == 0)
                return null;
            List<string> list = new List<string>();
            int startpos = 0;
            int bracketsCounter = 0;
            int i = 0;
            for (i = 0; i < text.Length; i++)
            {
                if (text[i] == devider && bracketsCounter == 0)
                {
                    list.Add(text.Substring(startpos, i - startpos));
                    startpos = i + 1;
                }
                if (text[i] == '[')
                    bracketsCounter++;
                if (text[i] == ']')
                    bracketsCounter--;
            }
            if (i > startpos - 1)
                list.Add(text.Substring(startpos, i - startpos));

            return list;
        }

        //returns the number of characters, which are not inside brackets []
        private static int GetTextLen(string text)
        {
            int textLen = 0;
            int numBrackets = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[')
                    numBrackets++;
                else if (text[i] == ']')
                    numBrackets--;
                else if (numBrackets == 0)
                    textLen++;
            }
            return textLen;
        }

        #endregion
    }

    public class ComperatorComicTopics : IComparer<ComicTopic>
    {

        public int Compare(ComicTopic ct1, ComicTopic ct2)
        {
            int cti1 = (int)ct1;
            int cti2 = (int)ct2;
            return cti1.CompareTo(cti2);
        }

        private static ComperatorComicTopics _comparer;

        public static ComperatorComicTopics Comperator
        {
            get
            {
                if (_comparer == null)
                    _comparer = new ComperatorComicTopics();
                return _comparer;
            }
        }
    }

}
