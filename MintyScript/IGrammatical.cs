using System;
using System.Collections.Generic;

namespace Com.Gamegestalt.MintyScript
{
	public interface IGrammatical
	{
		T Get<T>() where T:struct, IConvertible;

		//		Genus GetGenus();
		//
		//		Numerus GetNumerus();
		//
		//		Casus GetCasus();
		//
		//		Person GetPerson();
		//
		//		Komparation GetKomparation();
		//
		//		Tempus GetTempus();
		//
		//		TextFlag GetFlags();
		//
		//		WordType GetWordType();
		//
		//		Article_Type GetArticle_Type();
		//
		//		VerbCategory GetVerbCategory();
	}

	public static class GrammaUtil
	{
		public static bool IsGrammatical(object grammaClass)
		{
			return grammaClass is Genus
			|| grammaClass is Numerus
			|| grammaClass is Casus
			|| grammaClass is Person
			|| grammaClass is Komparation
			|| grammaClass is Tempus
			//|| grammaClass is TextFlag
			|| grammaClass is WordType
			|| grammaClass is Article_Type
			|| grammaClass is VerbCategory
			|| grammaClass is DeclinationType;
		}

		/// <summary>
		/// Gets the type of the declination for adjectives (dependent on the article of the noun).
		/// german specific
		/// </summary>
		/// <returns>The declination type.</returns>
		/// <param name="article">Article.</param>
		public static DeclinationType GetDeclinationType(IGrammatical article)
		{
			if (article == null
			    || (article.Get<Article_Type>() == Article_Type.UNDEFINED_VALUE && article.Get<DeclinationType>() == DeclinationType.UNDEFINED))
			{
				return DeclinationType.UNDEFINED;
			}

//			if (article.Get<DeclinationType>() != DeclinationType.UNDEFINED)
//			{
//				return article.Get<DeclinationType>();
//			}

			if (article.Get<Article_Type>() == Article_Type.DEFINED_ARTICLE)
				//TODO: jener, jene, jenes, jene
//				jeder, jede, jedes, jede
//				mancher, manche, manches, manche
//				solcher, solche, solches, solche
//				welcher, welche, welches, welche
//				derjenige, diejenige, dasjenige, diejenigen
//				derselbe, dieselbe, dasselbe, dieselben
//				oder die unbestimmte Numerale...
//
//				aller, alle, alles, alle
//				beide
//				sämtliche
			{
				return DeclinationType.WEAK;
			}

			if ((article.Get<Article_Type>() == Article_Type.UNDEFINED_ARTICLE && article.Get<Numerus>() == Numerus.PLURAL)
			    || article.Get<Article_Type>() == Article_Type.NO_ARTICLE)
				//TODO: eine Kardinalzahl vorangeht: zwei, drei, vier...
				//ein unkonjugiertes Pronomen vorangeht: manch, solch, welch, allerlei, mancherlei, andere, folgende, verschiedene
				//ein unkonjugiertes unbestimmtes Numerale oder Pronomen vorangeht: viel, wenig, etwas, mehr, genug, einige, etliche, mehrere
			{
				return DeclinationType.STRONG;
			}

			return DeclinationType.MIXED;
		}

		//		public static Type[] grammaticalEnums = { typeof(Genus) };

	}
}

