using System;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
	
	public class CharacterWrapper : ICharacterData
	{
		private string nickName;
		private GenderType gender;
		//		private Personality personality;

		#if UNITY_5_3_OR_NEWER
		private string id;

		public string Id { get { return id; } }
		#endif
		public CharacterWrapper(string name, GenderType gender)
		{
			this.nickName = name;
			this.gender = gender;
		}

		public string NickName
		{ 
			get { return nickName; }
			set { nickName = value; }
		}

		public GenderType Gender
		{ 
			get { return gender; }
			set { gender = value; }
		}

		//		public Personality Personality
		//		{
		//			get { return personality; }
		//			set { personality = value; }
		//		}

		public BodyType BodyType { get; set; }

		public Emotion Emotion { get; set; }
		
		//		#if UNITY_5_3_OR_NEWER
		//		public Color SkinColor { get { return Color.white; } set { } }
		//
		//		public Wear Wear { get { return null; } set { } }
		//
		//		public Character Character { get { return null; } set { } }
		//
		//		private int mutalFriendCount;
		//
		//		public virtual int MutalFriendCount
		//		{
		//			get { return mutalFriendCount; }
		//			set { mutalFriendCount = value; }
		//		}
		//		#endif
	}
}

