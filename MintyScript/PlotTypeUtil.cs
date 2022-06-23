using System;
using System.Collections.Generic;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
namespace Com.Gamegestalt.MintyScript
{
	class PlotTypeUtil
	{
		public static PoseType[] listeningPoses =
			{
				PoseType.ZUHOEREN, PoseType.DONTSHOOT, PoseType.HUCH, PoseType.BETEN, PoseType.SKEPSIS
			};
		public static PoseType[] dancePoses =
			{
			PoseType.HIPHOP, PoseType.KICK, PoseType.CAPER, 
			PoseType.DANCE, PoseType.DANCE_ANGRY, PoseType.DANCE_CONFIDENT, PoseType.DANCE_HAPPY, PoseType.DANCE_SAD
		};
		public static List<PoseType> actorOnGroundPoses = new List<PoseType>()
		{
			PoseType.SITZENDELANGEWEILE, PoseType.SITZENDETRAUER, 
			PoseType.ELEGANTEVERBEUGUNG, PoseType.TIEFEVERBEUGUNG,
			PoseType.ANBETEN, PoseType.SCHOCKLIEGEND
		};


		internal static T RandomEnum<T>()
		{
			 var enums = Enum.GetValues(typeof(T));
			if (enums == null && enums.Length == 0)
			{
				return default(T);
			}

			return (T)enums.GetValue(Utils.RandomRange(0, enums.Length));
		}

		private static T RandomEnum<T>(float[] probabilities)
		{
			var enums = Enum.GetValues(typeof(T));

			if (enums.Length != probabilities.Length)
			{
				Logger.LogWarning("", "RandomEnum: enum length != probabilities[] length");
			}

			int minEnd = Math.Min(enums.Length, probabilities.Length);
			float sum = 0;
			foreach (float fl in probabilities)
				sum += fl;
			float rand = Utils.RandomRange(0f, sum);
			sum = 0;
			for (int i = 0; i < minEnd; i++)
			{
				sum += probabilities[i];
				if (rand < sum)
				{
					return (T)enums.GetValue(i);
				}
			}
			return (T)enums.GetValue(0);
		}

	}
}
