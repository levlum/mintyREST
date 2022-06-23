#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;
using Com.Gamegestalt.MintyScript;

public interface ICharacterData
{


	string NickName { get; set; }

	GenderType Gender { get; set; }

	BodyType BodyType { get; set; }

	//	Personality Personality { get; set; }

	Emotion Emotion { get; set; }
}
