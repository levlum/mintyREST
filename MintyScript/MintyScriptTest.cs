using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using TMPro.EditorUtilities;
using System.Numerics;
#endif
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif


namespace Com.Gamegestalt.MintyScript
{
	#if UNITY_5_3_OR_NEWER && !MintyScriptHelper
	public class MintyScriptTest: MonoBehaviour
	{

		void Start()
		{
			Run();
		}

		
		
#else
	public static class MintyScriptTest
	{
		#endif
		public static void Run()
		{

//			return;
				
			Logger.DebugL("Test", "Start Test");


			var ct = MintyTextsContainer.Instance;

            CharacterWrapper me = new CharacterWrapper("Lev", GenderType.MALE);
            CharacterWrapper you = new CharacterWrapper("Martina", GenderType.FEMALE);
            CharacterWrapper other = new CharacterWrapper("Lautsi", GenderType.MALE);

			List<Word> words = null;
			List<TextPattern> patterns = null;



// #if UNITY_EDITOR && !MintyScriptHelper
			//EditorCoroutine.StartCoroutine(TestRunner(500, () => Logger.DebugL(null, "End Test")));
			//EditorCoroutine.StartCoroutine(ProcessAllSentencesAndSyntagmas(() => Logger.DebugL(null, "End Test")));

			Word w = ct.GetWords(new TextPattern(new FixedTextCondition("glauben")))[0];
			Sentence testS = new Sentence();
			testS.Text = "Weil [sentence:topic=deep_statements]";
			StringBuilder stringBuilder = new StringBuilder();


			//foreach (var relation in ct.textRelations)
			//{
			//	if (relation.needs != null && relation.needs.HasFlag(TextFlag.SINGLE))
			//	{
			//		stringBuilder.AppendLine("relation wich needs to be SINGLE: "+relation);
			//	}
			//}

			for (int i= 0; i<10; i++)
			{
				//testS.SetPatternForNextProcessing(new TextPattern(new WordCondition(w)));
			    stringBuilder.AppendLine (i+" "+testS.Process(me, you, other, true));
			    testS.Reset();
			}

			File.WriteAllText("TMintyTestResult.txt", stringBuilder.ToString());
			//Logger.DebugL(null, stringBuilder.ToString());
			Logger.DebugL(null, "End Test");
// #endif

        }

#if UNITY_5_3_OR_NEWER && !MintyScriptHelper
        private static IEnumerator ProcessAllSentencesAndSyntagmas (Action onComplete = null)
        {
            var ct = MintyTextsContainer.Instance;
            StringBuilder resultText = new StringBuilder();
            CharacterWrapper me = new CharacterWrapper("Lev", GenderType.MALE);
            CharacterWrapper you = new CharacterWrapper("Martina", GenderType.FEMALE);
            CharacterWrapper other = new CharacterWrapper("Lautsi", GenderType.MALE);

            float count = ct.Sentences.Count;
            float i = 0f;

            Dictionary<string, Word> last_processing_namedWords = null;
            foreach (var sentence in ct.Sentences)
            {
                resultText.AppendLine((i>0?"\n":"") +sentence.Text);
                sentence.next_processing_namedWords = last_processing_namedWords;
                resultText.AppendLine(sentence.Process(me, you, other, true));
#if UNITY_EDITOR
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Process all Sentences", sentence.Text, (float)(i) / (float)count))
                {
                    break;
                }
#endif
                last_processing_namedWords = sentence.last_processing_namedWords;
                sentence.Reset();
                i += 1f;
                yield return null;
            }

            File.WriteAllText("all sentences and syntagmas.txt", resultText.ToString());
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }

		private static IEnumerator TestRunner(int iterations, Action onComplete = null)
        {
            var ct = MintyTextsContainer.Instance;
            SortedDictionary<int, Dictionary<Sentence, List<string>>> useCounts = new SortedDictionary<int, Dictionary<Sentence, List<string>>>();

            int finishedCounter = 0;
            for (int i=0; i< iterations; i++)
            {
                MintyTextsContainer.RhymingResult rhymeResult = new MintyTextsContainer.RhymingResult();
				//var mb = new GameObject("invisible runner", typeof(EmptyMonoBehaviour)).GetComponent<EmptyMonoBehaviour>();

#if UNITY_EDITOR
				yield return EditorCoroutine.StartCoroutine(ct.GetRhymingSentences(rhymeResult,null,null,(result)=> {
                    if (result.found)
                    {
                        Dictionary<Sentence, List<string>> sentencesUsedX;
                        List<string> usages = null;

                        if (useCounts.TryGetValue(result.s1.usageCount, out sentencesUsedX))
                        {
                            usages = sentencesUsedX[result.s1];
                            sentencesUsedX.Remove(result.s1);
                        }

                        result.s1.usageCount++;

                        if (!useCounts.TryGetValue(result.s1.usageCount, out sentencesUsedX))
                        {
                            sentencesUsedX = new Dictionary<Sentence, List<string>>();

                            useCounts[result.s1.usageCount] = sentencesUsedX;
                        }


                        if (usages == null && !sentencesUsedX.TryGetValue(result.s1, out usages))
                        {
                            usages = new List<string>();
                        }
                        sentencesUsedX[result.s1] = usages;

                        result.s1.SetPatternForNextProcessing(new TextPattern(new WordCondition(result.w1)));
                        usages.Add(MintyUtils.AddDotAndBigStartingLetters(result.s1.Process(null, null, null)));
                        result.s1.Reset();
                    }
                    finishedCounter++;
                }));
#endif

			}


            while (finishedCounter < iterations)
            {
                yield return null;
            }

            StringBuilder resultText = new StringBuilder();
            foreach (var sameUseCounts in useCounts)
            {
                if (sameUseCounts.Value.Count > 0)
                {
                    resultText.AppendLine("\n"+sameUseCounts.Key + " times used:");
                    foreach (var oneSentence in sameUseCounts.Value)
                    {
                        resultText.AppendLine("\t" + oneSentence.Key.Text);
                        foreach (string usage in oneSentence.Value)
                        {
                            resultText.AppendLine("\t\t" + usage);
                        }
                    }
                }
            }

            File.WriteAllText("rhyme sentence usage.txt", resultText.ToString());

            if (onComplete!=null)
            {
                onComplete.Invoke();
            }
        }

#endif

    }
}

