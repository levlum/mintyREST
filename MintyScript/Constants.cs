
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Com.Gamegestalt.MintyScript
{
	public static class Constants
	{
		public const string NO_VARIABLE = "NO_VARIABLE";

		public const float COMIC_PANEL_BORDER_THICKNESS = 4.5f;
	
		public const string PRODUCT_ID_FOR_FULL_CONTENT = "FU";
		public const int NICK_NAME_MAX_LENGTH = 12;

		#if UNITY_5_3_OR_NEWER
		//	public static Color ALERTBOX_COLOR_GREEN = new Color(110.0f/255, 189.0f/255, 68.0f/255, 1);
		public static Color ALERTBOX_COLOR_GREEN = new Color(64.0f / 255, 130.0f / 255, 87.0f / 255, 1);
		public static Color ALERTBOX_COLOR_ORANGE = new Color(219.0f / 255, 94.0f / 255, 0.0f / 255, 1);
		#endif

		public const string MATERIAL_FAKE_SHADOW = "Core/Materials/charShadowMat";
	
		public const float PAYMENT_JOB_TIMEOUT_IN_SECONDS = 7;
		public const int PROFILE_PHOTO_WIDTH = 266;
		public const int PROFILE_PHOTO_HEIGHT = 266;
		public const int THUMBNAIL_WIDTH = 200;
		public const int THUMBNAIL_HEIGHT = 98;
	
		#if UNITY_5_3_OR_NEWER
		public static Color TRANSPARENT_COMIC_BACKGROUND_COLOR = new Color(220 / 255.0f, 250 / 255.0f, 250 / 255.0f, 1);
		#endif

		public const float MIN_TIME_TO_RATE_COMIC = 4.0f;
		// users must take at least this many seconds to actually read the comics before rating them
		public const int ALLOWED_RATINGS_BELOW_MIN_TIME = 2;
		// number of consecutive ratings it takes below MIN_TIME_TO_RATE_COMIC before user doesn't receive any more money.
	
	
		//public const string TUTORIAL_INVENTORY_ITEM_ID = "-417792000";
		public const string PREFAB_MAP_TILE_MATERIAL = "Core/Materials/MapTileMaterial";
		public const string COMIC_SLOT_MACHINE_MATERIAL = "Comic/SlotMachineEffect/ComicSlotMachineMaterial";
		public const string GUI_COMIC_NARRATION_BOX_COLOR = "[000000]";
		//"[198098]";
		public const string GUI_COMIC_REGULAR_COLOR = "[000000]";
		public const string GUI_COMIC_BOLD_COLOR = "[218097]";
		#if UNITY_5_3_OR_NEWER
		public static Color GUI_TEXT_COLOR = new Color(66.0f / 255, 90.0f / 255, 117.0f / 255, 1);
		public static Color COLOR_BUTTON_OFF = new Color(131.0f / 255, 216.0f / 255, 247.0f / 255, 1);
		public static Color COLOR_BUTTON_ON = new Color(255.0f / 255, 227.0f / 255, 87.0f / 255, 1);
		#endif
		public const string CAM_LONG_SHOT = "cam_totale";
		public const string CAM_BIRD = "cam_bird";
		public const string CAM_WORMS_EYE = "cam_frosch";
		public const string CAM_WORMS_EYE_KNEE = "cam_frosch_nah";
		public const string CAM_KNEE_SHOT = "cam_halbtotale";
		public const string CAM_AMERICAN = "cam_american";
		public const string CAM_BIRD_KNEE = "cam_bird_near";
		public const string CAM_CLOSE_SHOT = "cam_nah";
		public const string CAM_PHOTO = "cam_photo";
	

		public const string SHADER_CLOTHING_REPLACE_COLOR_WITH_COLOR = "Papermint/ClothingReplaceColorWithColor";

		public const int CHARACTER_LAYER = 8;

		public const bool COMIC_CREATE_HIGHRES = true;

		public const string BUNDLE_OUTFIT_DEFAULTWEAR = "DEFAULTWEAR";

		public const string BUNDLE_OUTFIT_ICON_PREFIX = "ICON_";
		public const string BUNDLE_OUTFIT_ONLOAD_PREFIX = "ONLOAD_";

		public const string BUNDLE_OUTFIT_COLOR_PREFIX = "OUTFNOAL_";
		public const string BUNDLE_EXTERNAL_IMAGES = "EXTIMG";

		public const string BUNDLE_SCENE_OBSTACLE_TEXTURES = "SCN_OBST_TEX";
		public const string BUNDLE_FILE_SCENE_OBSTACLE_TEXTURES = "obstacles_tex";


		public const string BUNDLE_OBSTACLE_ICON_TEXTURES = "SCN_OBST_ICON";
		public const string BUNDLE_FILE_OBSTACLE_ICON_TEXTURES = "obstacles_icons";

		public const string BUNDLE_WALLPAPER_TEXTURES = "WALLPAPER";
		public const string BUNDLE_FILE_WALLPAPER_TEXTURES = "wallpapers_tex";

		public const string BUNDLE_SOUNDS = "SOUNDS";
		public const string BUNDLE_FILE_SOUNDS = "sounds";


		public const string BUNDLE_GUI = "GUI";
		public const string BUNDLE_FILE_GUI = "gui";

		public const string BUNDLE_OUTFIT_MAN = "OUTMAN";
		public const string BUNDLE_FILE_OUTFIT_MAN = "outfit_man";

		public const string BUNDLE_OBSTACLE_MAN = "OBSTMAN";
		public const string BUNDLE_FILE_OBSTACLE_MAN = "obstacles_man";

		public const string BUNDLE_LOC_MAN = "LOCMAN";
		public const string BUNDLE_FILE_LOC_MAN = "text_man";

		public const string BUNDLE_CHARACTER_MAN = "CHARMAN";
		public const string BUNDLE_FILE_CHARACTER_MAN = "characters_man";

		public const string BUNDLE_ANIMATION_MAN = "ANIMAN";
		public const string BUNDLE_FILE_ANIMATION_MAN = "animations_man";

		public const string BUNDLE_ICON_MAN = "ICOMAN";
		public const string BUNDLE_FILE_ICON_MAN = "icons_man";

		public const string BUNDLE_SOUND_MAN = "SOUNDMAN";
		public const string BUNDLE_FILE_SOUND_MAN = "sound_man";

		public const string BUNDLE_SCENE_OBSTACLES = "SCN_OBST";
		public const string BUNDLE_FILE_SCENE_OBSTACLES = "obstacles";

		public const string BUNDLE_COMIC_ICONS = "COMICON";
		public const string BUNDLE_CHARACTER_DEFAULT_TEXTURES = "DEFAULTTEX";
		public const string BUNDLE_ANIMATION = "ANI_";

		public const string BUNDLE_SCENE_TILES = "SCN_TILE_";
		public const string BUNDLE_FILE_SCENE_TILES = "tiles_";

		public const string BUNDLE_SCENE = "SCN_";
		public const string BUNDLE_FILE_SCENE = "sc_";

		public const string BUNDLE_COMIC_PICTURE = "COMICPIC";

		public const string PREFAB_POPUP_PARTICLES = "FX/PopupParticles";

		public const string PREFAB_SINGLE_POPUP_PARTICLE = "FX/SinglePopupParticle";

		public const string MATERIAL_TRANS_UNLIT = "Core/Materials/SingleSidedTransparent";
		public const string MATERIAL_SINGLE_COLORED = "Core/Materials/SingleColored";

		public const string MATERIAL_DOUBLE_SIDED_CUTOUT = "Core/Materials/DoubleSided";


		public const string PREFAB_UNIT_PLANE = "Core/UnitPlanePrefab";

		public const string TEXTURES_OUTFIT_PATH = "Character/Outfits/";
		public const string CHARACTER_ANIMATION_PATH = "Character/Animations/";
	
		public const string PREFAB_GLOBAL_REFERENCES = "Globals";

		public const string PREFAB_OUTFIT_MANAGER = "Character/OutfitManager";
		public const string PREFAB_ICON_MANAGER = "Textures/IconManager";
		public const string PREFAB_OBSTACLE_MANAGER = "Obstacles/ObstacleManager";
		public const string PREFAB_LOCALIZATION_MANAGER = "Text/LocalizationManager";
		public const string PREFAB_ANIMATION_MANAGER = "Character/AnimationManager";
		public const string PATH_CHARACTER = "ResourcesExternal/Character/";
		public const string PATH_TEXT = "ResourcesExternal/Text/";
		public const string FILE_ARTICLE_TEXTS = PATH_TEXT + "article_loc_data.xml";

		public const string PREFAB_SOUND_MANAGER = "Audio/SoundManager";

		public const string OBSTACLES_RESOURCES_FOLDER = "Obstacles/";

		public const string PREFAB_MAP_MANAGER = "Map/MapManager";


		public const string IMPORT_NAV_MESH_PARENT = "NavigationMesh";


		public const string GAMEOBJECT_TILE_MANAGER = "TileManager";

		public const string GAMEOBJECT_WEAR_FABRIK = "Fabrik/WearFabrik";

		public const string GAMEOBJECT_GUI = "GUI";

		public const string GAMEOBJECT_GUI_CAMERA = "GUI/Camera";

		public const string GAMEOBJECT_SCENEMANAGER = "SceneManager";

		public const string GAMEOBJECT_FLOOR_SHADOW = "FloorShadow";

		public const string GAMEOBJECT_GAME_MANAGER = "GameManager";

		public const string GAMEOBJECT_STREAMING_MAP = "StreamingMap";


		//public const string GAMEOBJECT_MAINLIGHT = "Game/Directional light";

		//public const string GAMEOBJECT_COMICLIGHT = "Game/Comic light";


		public const string GAMEOBJECT_SCENE_FLOOR = "Floor";

		public const string GAMEOBJECT_SCENE_OBSTACLES = "Obstacles";
		public const float OBSTACLE_PARENT_Y_OFFSET = 0.01f;
		// small offset to avoid z-fighting between floor and obstacles
	
		public const string GAMEOBJECT_SCENE_CEILING = "Ceiling";

		public const string GAMEOBJECT_DEBUG_VIEW = "DebugView";


		public const string PREFAB_SCENE_FLOOR = "Resources/Core/Floor";

		public const string PREFAB_SCENE_CEILING = "Resources/Core/Ceiling";



		//public const string PREFAB_CHARACTER_A_F = "Character/Prefabs/f_a_prefab";

		//public const string PREFAB_CHARACTER_A_M = "Character/Prefabs/m_a_prefab";

		//public const string PREFAB_CHARACTER_L_F = "Character/Prefabs/f_l_prefab";

		//public const string PREFAB_CHARACTER_L_M = "Character/Prefabs/m_l_prefab";

		//public const string PREFAB_CHARACTER_P_F = "Character/Prefabs/f_p_prefab";

		//public const string PREFAB_CHARACTER_P_M = "Character/Prefabs/m_p_prefab";




	


		internal const string TEXTURES_CHARACTER_DEFAULT = "Character/Materials/";

		public const string PREFAB_EMOTION_PANEL = "GUI/SelectEmotionPanel";



		public const string SHADER_OBSTACLES_DEFAULT = "Papermint/DoubleSided";

		public const string SHADER_SINGLE_SIDED_OPAQUE = "Papermint/SingleSidedOpaque";

		public const string SHADER_SINGLE_SIDED_OPAQUE_DESATURATED = "Papermint/SingleSidedOpaqueDesaturate";

		public const string TRANSPARENT_GUI = "Unlit/Transparent2";
	
		public const string SHADER_SINGLE_SIDED_TRANSPARENT = "Papermint/SingleSidedTransparent";

		public const string SHADER_SINGLE_SIDED_TRANSPARENT_TINT = "Papermint/SingleSidedTransparentTint";

		public const string SHADER_SINGLE_SIDED_TRANSPARENT_COLOR_REPLACE = "Papermint/SingleSidedTransparentColorReplace";

		public const string SHADER_FLOOR = "Papermint/FloorShader";


		public const string PREFAB_SCENE_MANAGER = "Core/SceneManager";

		public const string PREFAB_FLOOR_SHADOW = "Resources/Core/FloorShadow";

		public const string PREFAB_GAME_MANAGER = "Core/GameManager";



		public const string PREFAB_DEFAULT_DEBUG = "ResourcesExternal/Utilities/DebugView";


		//	public const string DEFAULT_PLAYER_PREFAB = "Character/Prefabs/a_f_prefab";

		public const string DEFAULT_OUTFIT_TEXTURES = "Resources/Character/Outfits";
		public const string DEFAULT_ICON_TEXTURES = "Textures/Icons";


		public const string BUNDLES_PATH = "Bundles/";

		public static float RATING_MINZE_REWARD = 1.0f;
		public static float RATING_DELTA = 0.5f;

		public const string GUI_CHARACTER_SELECTION_WITH_COMIC_PREFAB = "GUI/CharacterSelection/CharacterSelectionWithComic";

		public const int ERROR_TEXT = -1001;


	

		// -------------------- Move Tool

	

		//MoveToolRotateIndicator_off


	


	


	

		// -------------------- Comic

		//public const string GAMEOBJECT_CREATE_COMIC = "ComicStripCam";



		public static Dictionary<ParseNodeType, NodeTypeInfo> NODE_TYPE_INFO = new Dictionary<ParseNodeType, NodeTypeInfo>()
		{
			{ ParseNodeType.ADJECTIVE, new NodeTypeInfo(){ header = "adjective", format = "[{0}{1}({2})]", isLeaf = true } },
			{ ParseNodeType.ADVERB, new NodeTypeInfo(){ header = "adverb", format = "[{0}{1}({2})]", isLeaf = true } },
			{ ParseNodeType.ALTERNATIVES, new NodeTypeInfo(){ header = "?", format = "[{0}{1}{2}]", isLeaf = false, delimiter = ";" } },
			{ ParseNodeType.EMPTY, new NodeTypeInfo(){ header = "", format = "", isLeaf = true } },
			{ ParseNodeType.INVISIBLE, new NodeTypeInfo(){ header = "invisible", format = "[{0}{1}{2}]", isLeaf = true } },
			{ ParseNodeType.LOW, new NodeTypeInfo(){ header = "Low", format = "[{0}{1}{2}]", isLeaf = true } },
			{ ParseNodeType.NAME_ME, new NodeTypeInfo(){ header = "nameMe", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.NAME_OTHER, new NodeTypeInfo(){ header = "nameOther", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.NAME_YOU, new NodeTypeInfo(){ header = "nameYou", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.OBJECT, new NodeTypeInfo(){ header = "object", format = "[{0}{1}({2})]", isLeaf = true } },
			{ ParseNodeType.PARTS, new NodeTypeInfo(){ header = "", format = "", isLeaf = false } },
			{ ParseNodeType.PREDEFINED, new NodeTypeInfo(){ header = "PREDEFINED", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.SENTENCES, new NodeTypeInfo(){ header = "sentence", format = "[{0}{1}:{2}]", isLeaf = true } },
			{ ParseNodeType.SEX_ME, new NodeTypeInfo(){ header = "sexMe", format = "[{0}{1}?{2}]", isLeaf = false, delimiter = ":" } },
			{ ParseNodeType.SEX_OTHER, new NodeTypeInfo(){ header = "sexOther", format = "[{0}{1}?{2}]", isLeaf = false, delimiter = ":" } },
			{ ParseNodeType.SEX_YOU, new NodeTypeInfo(){ header = "sexYou", format = "[{0}{1}?{2}]", isLeaf = false, delimiter = ":" } },
			{ ParseNodeType.SUBJECT, new NodeTypeInfo(){ header = "subject", format = "[{0}{1}({2})]", isLeaf = true } },
			{ ParseNodeType.SWITCH_CASUS, new NodeTypeInfo(){ header = "Casus", format = "[{0}{1}({2})]", isLeaf = false, delimiter = ";" } },
			{ ParseNodeType.SWITCH_GENUS, new NodeTypeInfo(){ header = "Genus", format = "[{0}{1}({2})]", isLeaf = false, delimiter = ";" } },
			{ ParseNodeType.SWITCH_NUMERUS, new NodeTypeInfo(){ header = "Numerus", format = "[{0}{1}({2})]", isLeaf = false, delimiter = ";" } },
			{ ParseNodeType.SWITCH_REFLEXIVE, new NodeTypeInfo(){ header = "Reflexive", format = "[{0}{1}({2})]", isLeaf = false, delimiter = ";" } },
			{ ParseNodeType.SWITCH_LAST_OF_X, new NodeTypeInfo(){ header = "lastOf", format = "[{0}{1}({2})]", isLeaf = false, delimiter = ":" } },
			{ ParseNodeType.TEXT, new NodeTypeInfo(){ header = "", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.TOPIC, new NodeTypeInfo(){ header = "topic", format = "[{0}{1}]", isLeaf = true } },
			{ ParseNodeType.UNKNOWN, new NodeTypeInfo(){ header = "", format = "", isLeaf = true } },
			{ ParseNodeType.UP, new NodeTypeInfo(){ header = "Up", format = "[{0}{1}{2}]", isLeaf = true } },
			{ ParseNodeType.VERB, new NodeTypeInfo(){ header = "verb", format = "[{0}{1}({2})]", isLeaf = true } },
			{ ParseNodeType.WORDS, new NodeTypeInfo(){ header = "words", format = "[{0}{1}/{2}]", isLeaf = true } },
			{ ParseNodeType.WORDX, new NodeTypeInfo(){ header = "word", format = "[{0}{1}({2})]", isLeaf = true } },
		};

	
		
		public const float MATCH_MINIMAL = 1f;
		public const float MATCH_GOOD = 100f;
		public const float MATCH_SUPER = 1000f;
		public const float MATCH_PERFECT = 10000f;

		public const int KEY_TEXT_RELATION = 1;
		public const int KEY_WORD_TO_TAKE = 3;
		public const int KEY_INDEX = 4;
		public const int KEY_WEIGHT = 5;
		public const int KEY_DYNAMIC_PATTERN = 6;

		public const string VAR_ARTICLE = "var_article";

		// Array of Components for various Papermint objects (plant, character, etc.)

		//public static System.Type[] PAPERMINT_OBJECTS = new System.Type[]{

		//    typeof(Building),

		//    typeof(Character),

		//    typeof(Plant),

		//    typeof(Furniture)

		//};

	}



	#region enums

	public enum Language
	{
		GERMAN,
		ENGLISH
	}

	#region Gender
	public enum GenderType
	{

		MALE = 0,

		FEMALE = 1

	}
	#endregion

	#region Emotion
	public enum Emotion
	{

		ZORNIG_11,
		//11

		WUETEND_12,
		//12

		SCHLECHTGELAUNT_13,
		//13

		TRAURIG_14,
		//14

		VERLETZT_15,
		//15

		BOCKIG_21,
		//21

		GRANTIG_22,
		//22

		GEREIZT_23,
		//23

		NIEDERGESCHLAGEN_24,
		//24

		GEKRAENKT_25,
		//25

		ERSTAUNT_31,
		//31

		ANGEREGT_32,
		//32

		NEUTRAL_33,
		//33

		GELANGWEILT_34,
		//34

		DISTANZIERT_35,
		//35

		ERREGT_41,
		//41

		FRECH_42,
		//42

		AMUESIERT_43,
		//43

		HEITER_44,
		//44

		ZUFRIEDEN_45,
		//45

		BEGEISTERT_51,
		//51

		LUSTIG_52,
		//52

		FROEHLICH_53,
		//53

		ERGRIFFEN_54,
		//54

		GLUECKLICH_55
		//55

	}
	#endregion

	#region BodyType
	public enum BodyType
	{

		FEMALE_ATHLETIC,

		FEMALE_LEPTOSOMIC,

		FEMALE_PYKNIC,

		MALE_ATHLETIC,

		MALE_LEPTOSOMIC,

		MALE_PYKNIC

	}
	#endregion

	#region PoseType
	public enum PoseType
	{

		HUCH,

		SCHLEICH,

		ERSCHRECKEN,

		ERSCHRECKTWERDEN,

		HANDREICHEN,

		KAMPF,

		ZEHENSPITZEN,

		ELEGANTEVERBEUGUNG,

		PISTOLENDUELL,

		HIPHOP,
	
		DANCE,
		DANCE_ANGRY,
		DANCE_CONFIDENT,
		DANCE_HAPPY,
		DANCE_SAD,

		TIEFEVERBEUGUNG,

		BETEN,

		KICK,

		DONTSHOOT,

		STOP,

		VERZWEIFLUNG,

		SITZENDETRAUER,

		WINKEN,

		VERLIEBT,

		SCHLIMMERFINGER,

		CAPER,

		ZOMBIE,

		ANBETEN,

		SCHOCKLIEGEND,

		WATSCHE,

		HAENDEHOCH,

		NEIN,

		AUFMUCKEN,

		KNIEN,

		LOUGH_ACTIVE,

		LACHEN,

		ZUHOEREN,

		SKEPSIS,
		SITZENDELANGEWEILE,

		KISS

	}
	#endregion

	#region SymbolType
	public enum SymbolType
	{

		ZUHOEREN_SCHWARZORANGE_PAPERMINT,
		ZUHOEREN_ORANGE_PAPERMINT,
		ZUHOEREN_MULTICOLOR_PAPERMINT,
		ZUHOEREN_SCHWARZ_PAPERMINT,
		FARBPALETTE_3FARBEN_PAPERMINT,
		FARBPALETTE_4FARBEN_PAPERMINT,
		FARBPALETTE_5FARBEN_PAPERMINT,
		CHECK_PAPERMINT,
		FERNGLAS_PAPERMINT,
		MITTELPUNKT_SCHWARZ_PAPERMINT,
		MITTELPUNKT_ORANGE_PAPERMINT,
		FREUDENSPRUNG_PAPERMINT,
		UNDERDRESSED_PAPERMINT,
		GELDSEGEN_PAPERMINT,
		KLEINES_GESCHENK_ORANGE_PAPERMINT,
		GROSSES_GESCHENK_ORANGE_PAPERMINT,
		GESCHENK_SCHWARZ_ORANGE_PAPERMINT,
		GESCHENK_SCHWARZ_PAPERMINT,
		LILA_ALIENSCHAEDEL_PAPERMINT,
		STAR_PAPERMINT,
		LIEBE_PAPERMINT,
		HAND_PAPERMINT,
		ROSE_PAPERMINT,
		HERZ_PAPERMINT,
		FEUER_PAPERMINT,
		ENGEL_PAPERMINT,
		KAROTTE_PAPERMINT,
		BLUME_PAPERMINT,
		MOND_PAPERMINT,
		HAUS_PAPERMINT,
		WURST_PAPERMINT,
		SONNE_PAPERMINT,
		GEHIRN_PAPERMINT,
		REGEN_PAPERMINT,
		MANN_PAPERMINT,
		GELDSCHEINE_PAPERMINT,
		HANF_PAPERMINT,
		BILDUNG_PAPERMINT,
		KLECKS_PAPERMINT,
		KUEKEN_PAPERMINT,
		KONDOM_PAPERMINT,
		YINYANG_PAPERMINT,
		SCHAEDEL_PAPERMINT,
		PISTOLE_PAPERMINT,
		FRAU_PAPERMINT,
		SANDUHR_PAPERMINT,
		RAGE_PAPERMINT,
		SAUER_PAPERMINT,
		UNGLUECKLICH_PAPERMINT,
		VERZWEIFELT_PAPERMINT,
		TRAURIG_PAPERMINT,
		SKEPSIS_PAPERMINT,
		KONZENTRATION_PAPERMINT,
		MUEDE_PAPERMINT,
		SCHLAFEN_PAPERMINT,
		ERSTAUNT_PAPERMINT,
		PEINLICH_PAPERMINT,
		UNBEEINDRUCKT_PAPERMINT,
		SCHLAFZIMMERBLICK_PAPERMINT,
		MEDITATION_PAPERMINT,
		FREUDENTRAENEN_PAPERMINT,
		VERLIEBT_PAPERMINT,
		FREUDE_PAPERMINT,
		LASZIV_PAPERMINT,
		ZUFRIEDEN_PAPERMINT,
		BLOEDELN_PAPERMINT,
		ZWINKERN_PAPERMINT,
		GLUECKLICH_PAPERMINT,
		GERUEHRT_PAPERMINT,
		GENIESSEN_PAPERMINT,
		UHR_PAPERMINT,
		NUMMER1_LILA_PAPERMINT,
		NUMMER1_GELB_PAPERMINT,
		NUMMER1_GRUEN_PAPERMINT,
		NUMMER1_ROSA_PAPERMINT,
		NUMMER1_ROT_PAPERMINT,
		PLUS_PAPERMINT,
		HERZ_ANDERS_PAPERMINT,
		HAMMER_PAPERMINT,
		ZWEI_HERZEN_PAPERMINT,
		STAR_ANDERS_PAPERMINT,
		EI_PAPERMINT,
		PFLANZE_PAPERMINT,
		HACKERL_PAPERMINT,
		FEUER_ANDERS_PAPERMINT,
		KLECKS_ANDERS_PAPERMINT,
		TRAURIGER_SMILEY_PAPERMINT,
		TOTENSCHADEL_PAPERMINT,
		OEL_PAPERMINT,
		BOMBE_PAPERMINT,
		FRAGEZEICHEN_PAPERMINT,
		RUFZEICHEN_PAPERMINT,
		MISTKUEBEL_PAPERMINT,
		WUERFEL_PAPERMINT,
		ZAHNRAD_PAPERMINT,
		SMILEY_PAPERMINT,
		HOCHZEIT_PAPERMINT,
		FLUESTERN_PAPERMINT,
		INFORMATION_PAPERMINT,
		BRIEF_OFFEN_PAPERMINT,
		BRIEF_GESCHLOSSEN_PAPERMINT,
		GELD_PAPERMINT,
		SCHWANGER_PAPERMINT,
		RUBIN_PAPERMINT,
		FUNK_PAPERMINT,
		X_PAPERMINT,
		GLOCKE_PAPERMINT,
		FRAGEZEICHEN_ANDERS_PAPERMINT,
		INFORMATION_ANDERS_PAPERMINT,
		BRIEF_GESCHLOSSEN_ANDERS_PAPERMINT,
		ZAHNRAD_ANDERS_PAPERMINT,
		THUMBSUP_PAPERMINT,
		KAMERA_PAPERMINT,
		SPRECHBLASE_PAPERMINT,
		KLEIDERHAKEN_PAPERMINT,
		BRIEFLOS_PAPERMINT,
		BLABLABLA_PAPERMINT,
		TRITT_PAPERMINT,
		LILA_X_PAPERMINT,
		FARBPALETTE,
		FARBPALETTE_ANDERS,
		RUCKSACK,
		BALL,
		BASKETBALL,
		BEACHBALL,
		FOOTBALL,
		SPRINGENDER_FUSSBALL,
		FUSSBALL,
		TENNISBALL,
		BASKETBALLNETZ,
		BOWLING,
		BOXHANDSCHUH,
		PINSEL,
		CAMPING_ZELT,
		SCHACH_LAEUFER,
		SCHACH_TURM,
		SCHACH_SPRINGER,
		SCHACH_KOENIG,
		SCHACH_BAUER,
		SCHACH_DAME,
		POKAL_1,
		POKAL_2,
		POKAL_3,
		FILMKLAPPE,
		FILMKAMERA,
		FILMROLLE,
		FOTOFILM,
		FILMBAND,
		FILMBAND_ANDERS,
		ANGELHAKEN,
		FISCH_UND_HAKEN,
		FAHNE,
		RENNFAHNE,
		CONTROLLER,
		EINSAME_INSEL,
		FOOTBALL_HELM,
		REITEN,
		HUFEISEN,
		HUFEISEN_ANDERS,
		HEISSLUFTBALLON,
		MARKER,
		THEATERMASKE,
		THEATERMASKE_ANDERS,
		MEDAILLE_1,
		MEDAILLE_2,
		MEDAILLE_3,
		TISCHTENNIS,
		FALLSCHIRM,
		FALLSCHIRM_ANDERS,
		PARTY,
		BALLET,
		BASEBALL,
		BASEBALL_ANDERS,
		BASKETBALL_SPORT,
		RADFAHREN,
		BREAKDANCE,
		GITARRESPIELEN,
		GITARRESPIELEN_ANDERS,
		WANDERN,
		HOCKEY,
		EISHOCKEY,
		KARATE,
		LAUFEN,
		LAUFEN_ANDERS,
		SKIFAHREN_1,
		SKIFAHREN_2,
		SKIFAHREN_3,
		SNOWBARDEN,
		FUSSBALLSPIELEN,
		TENNISSPIELEN,
		PINGPONG,
		TENNISSCHLAEGER,
		FEDERBALLSCHLAEGER,
		FERNBEDIENUNG,
		SCHLEIFE,
		SCHLEIFE_ANDERS,
		SEGELBOOT_1,
		SEGELBOOT_2,
		SEGELSCHIFF_1,
		SEGELSCHIFF_2,
		ROLLERBLADESCHUH,
		SKATEBOARD,
		SKI,
		BASEBALLSCHLAEGER,
		BILLIARD,
		CRICKET,
		DART,
		GEWICHTHEBEN,
		IPHONE,
		IPOD_1,
		IPOD_2,
		MEGAPHON,
		AKKORDEON,
		KLARINETTE,
		NOTENSCHLUESSEL,
		TROMMEL_1,
		TROMMEL_2,
		TROMMEL_3,
		MUSIKNOTE_1,
		MUSIKNOTE_2,
		MUSIKNOTE_3,
		GITARRE_1,
		GITARRE_2,
		GITARRE_3,
		HANDHARFE,
		STANDHARFE,
		MIKROPHON,
		KLAVIER,
		SAXOPHON,
		LAUTSPRECHER,
		TAMBURIN,
		TROMPETE_1,
		HORN,
		TROMPETE_2,
		TUBA_1,
		TUBA_2,
		VIOLINE_1,
		VIOLINE_2,
		VIOLINE_3,
		AMEISE,
		BAER,
		ADLER_1,
		ADLER_2,
		ADLER_3,
		ENTE,
		KRANICH,
		SCHWALBE,
		TAUBE,
		KUEKEN,
		SCHMETTERLING_1,
		SCHMETTERLING_2,
		SCHMETTERLING_3,
		DROMEDAR,
		KAMEL,
		KATZE_1,
		KATZE_2,
		KATZE_3,
		KATZE_GLUECKLICH,
		KATZE_VERLETZT,
		PFOTENABDRUCK_KATZE,
		KUH,
		KRABBE,
		REH,
		HIRSCH,
		HIRSCH_ANDERS,
		BRACHIOSAURUS,
		TRICERATOPS,
		STEGADON,
		HUND_1,
		HUND_2,
		HUND_3,
		PFOTENABDRUCK_HUND,
		DRACHE_1,
		DRACHE_2,
		DRACHE_3,
		DRACHE_4,
		LIBELLE,
		ENTE_ANDERS,
		ELEPHANT,
		ELEPHANT_ANDERS,
		FISCH_1,
		FISCH_2,
		FISCH_3,
		FISCH_4,
		FROSCH,
		GIRAFFE,
		GIRAFFE_ANDERS,
		NASHORN,
		NASHORN_ANDERS,
		PFERD,
		PFERD_ANDERS,
		KAENGURUH,
		MARIENKAEFER_1,
		MARIENKAEFER_2,
		MARIENKAEFER_3,
		MARIENKAEFER_4,
		MARIENKAEFER_5,
		LOEWE_MAENNLICH,
		LOEWE_WEIBLICH,
		EIDECHSE,
		EIDECHSE_ANDERS,
		HUMMER,
		AFFE,
		ELCH,
		PANDA,
		PFOTENABDRUCK,
		PINGUIN,
		HASE,
		SEEPFERDCHEN,
		MUSCHEL_1,
		MUSCHEL_2,
		MUSCHEL_3,
		MUSCHEL_4,
		SCHNECKE,
		KOBRA,
		SPINNE,
		EICHHOERNCHEN,
		SCHILDKROETE,
		EINHORN,
		EINHORN_ANDERS,
		WOLF,
		GOLDFISCHGLAS,
		PFOTENABDRUCK_ANDERS,
		SPINNENNETZ,
		KAFFEETASSE,
		FLASCHE,
		SEKT,
		KAFFEETASSE_ANDERS,
		WEINGLAS,
		SEKTGLAS,
		COCKTAIL,
		EI,
		APFEL,
		BANANE,
		HEIDELBEERE,
		BROT,
		KAROTTE,
		KAESE,
		KAESE_ANDERS,
		KIRSCHE,
		KIRSCHE_ANDERS,
		MAIS,
		BURGERMENUE,
		ZITRUSFRUCHT,
		WEINTRAUBE,
		BURGER,
		CHILI,
		PILZ,
		BIRNE,
		APFELKUCHEN,
		ANANAS,
		PIZZA,
		ERDBEERE,
		TOMATE,
		HERD,
		BESTECK,
		MIKROWELLE,
		ESSEN_VERBOTEN,
		KRUG,
		TOPF,
		NUDELWALKER,
		TOASTER,
		EICHEL,
		KAKTUS,
		FEUER,
		LAGERFEUER,
		BLUME_1,
		BLUME_2,
		BLUME_3,
		BLUME_4,
		KLEEBLATT,
		BLUMENTOPF,
		ROSE,
		GRAS,
		WASSERTROPFEN,
		WUESTE,
		WUESTE_ANDERS,
		CANNABIS,
		AHORN,
		BLATT_1,
		BLATT_2,
		BLATT_3,
		LORBEERKRANZ,
		BLITZ,
		BLITZ_ANDERS,
		MOND_1,
		MOND_2,
		MOND_3,
		MOND_4,
		MOND_5,
		PLANET,
		SATURN_1,
		SATURN_2,
		REGENWOLKE,
		REGENWOLKE_ANDERS,
		REGENTROPFEN_1,
		REGENTROPFEN_2,
		REGENTROPFEN_3,
		SCHNEEFLOCKE_1,
		SCHNEEFLOCKE_2,
		SCHNEEFLOCKE_3,
		SCHNEEFLOCKE_4,
		STERN_1,
		STERN_2,
		STERN_3,
		STERN_4,
		STERN_5,
		STERN_6,
		ELEKTROSTERN,
		SONNE_1,
		SONNE_2,
		SONNE_3,
		SONNENAUFGANG,
		BAUM,
		TANNE,
		TANNE_ANDERS,
		BONSAI,
		PALME,
		PALME_ANDERS,
		ENGEL,
		ENGEL_ANDERS,
		AQUARIUS,
		SCHUETZE,
		ARIES,
		STIER,
		KREBS_1,
		CAPRICORN,
		KREBS_2,
		FISCHE,
		GEMINI,
		LEO,
		LIBRA,
		LOEWE,
		PISCES,
		WIDDER,
		SAGITTARIUS,
		WAAGE,
		SKORPION,
		SCORPIO,
		STEINBOCK,
		TAURUS,
		ZWILLINGE,
		JUNGFRAU,
		VIRGO,
		WASSERMANN,
		SCHUETZE_ANDERS,
		STIER_ANDERS,
		KREBS_3,
		FISCHE_ANDERS,
		ZWILLINGE_ANDERS,
		LOEWE_ANDERS,
		WIDDER_ANDERS,
		WAAGE_ANDERS,
		SKORPION_ANDERS,
		STEINBOCK_ANDERS,
		JUNGFRAU_ANDERS,
		WASSERMANN_ANDERS,
		BALLOON,
		GLOCKE,
		BUCH_GESCHLOSSEN,
		BUCH_OFFEN,
		BUCHSTAPEL,
		SCHLOSS,
		CHINESISCHES_SYMBOL_1,
		CHINESISCHES_SYMBOL_2,
		CHINESISCHES_SYMBOL_3,
		CHINESISCHES_SYMBOL_4,
		KIRCHE,
		KRONE_1,
		KRONE_2,
		KRONE_3,
		KRONE_4,
		AEGYPTISCHES_AUGE,
		GESCHENK_1,
		GESCHENK_2,
		GESCHENK_3,
		GESCHENK_4,
		GLOBUS_1,
		ERDBALL_1,
		GLOBUS_2,
		FLEDERMAUS,
		KUERBIS,
		HERZ,
		HERZEN,
		HERZ_GEBROCHEN,
		SCHLUESSE_ZUM_HERZEN,
		HERZ_ZELDA,
		GLOCKEN,
		KERZE_WEIHNACHTEN,
		ZUCKERSTANGE,
		WEIHNACHTSBAUM,
		AMOR,
		OSTERHASE,
		OSTEREI,
		GEIST,
		LEBKUCHENMANN,
		PILGERHUT,
		MISTELZWEIG,
		CHRISTBAUMKUGEL,
		WEIHNACHTSMANN,
		SCHNEEMANN,
		WEIHNACHTSSTRUMPF,
		WEIHNACHTSBAUM_ANDERS,
		TRUTHAHN,
		HERZ_PFEIL,
		WEIHNACHTSKRANZ,
		MALTESERKREUZ,
		MENORAH,
		SICHELMOND_STERN,
		KREUZ,
		DAVIDSTERN,
		SCHRIFTROLLE,
		SCHAEDEL,
		ALIEN,
		UFO,
		ERDBALL_2,
		YINYANG,
		AT,
		RUFZEICHEN_1,
		RUFZEICHEN_2,
		KAUFMAENNISCHES_UND,
		FRAGEZEICHEN,
		AKKUANZEIGE,
		FERNGLAS,
		PINSEL_FARBE,
		FARBROLLER,
		TASCHENRECHNER,
		KALENDER,
		EINKAUFSWAGEN,
		CHARTS,
		WECKER,
		UHR,
		LAPTOP,
		COMPUTERMAUS,
		USB,
		PFUND,
		DOLLAR,
		EURO,
		YEN,
		CD,
		DISKETTE_1,
		DISKETTE_2,
		DOKUMENTE,
		STECKER,
		BRIEF,
		TANKSTELLE,
		ZAHNRAD,
		HAUS,
		SANDUHR,
		SCHLUESSEL,
		SCHLUESSEL_ANDERS,
		SCHLUESSELBUND,
		LEITER,
		LEUCHTENDE_GLUEHBIRNE,
		GLUEHBIRNE,
		SCHLOSS_ZU,
		SCHLOSS_OFFEN,
		BRIEFKASTEN_ZU,
		BRIEFKISTEN_OFFEN,
		MIKROSKOP,
		BOHRTURM,
		BUEROKLAMMER,
		TINTENFUELLER,
		BLEISTIFT,
		MALSTIFT,
		HANDY,
		TELEFON,
		SATELLITENSCHUESSEL,
		SCHERE,
		DAUMEN_RUNTER,
		DAUMEN_RAUF,
		PINNADEL,
		AXT,
		ZANGE,
		SEITENSCHNEIDER,
		HAMMER,
		SPITZHACKE,
		SCHRAUBENZIEHER,
		SCHRAUBE,
		SCHAUFEL,
		SCHWERT,
		SCHRAUBENSCHLUESSEL,
		MISTKUEBEL,
		ANKER_1,
		ANKER_2,
		HAKERL,
		SPRECHBLASE,
		SPRECHBLASEN,
		WEIBLICH,
		MAENNLICH,
		PEACE,
		POWER,
		PUZZLESTEIN,
		FLEURDELIS,
		TANKANZEIGE,
		WINDROSE,
		STEUERRAD,
		SPACESHUTTLE,
		TRAKTOR,
		BAGGER,
		FLUGZEUG_1,
		FLUGZEUG_2,
		FLUGZEUG_3,
		AMBULANZ,
		FAHRRAD,
		MOTORRAD,
		MOTORBOOT,
		OMNIBUS,
		AUTO_1,
		AUTO_2,
		HELIKOPTER_1,
		HELIKOPTER_2,
		JET,
		POLIZEIWAGEN,
		ZUG,
		LASTWAGEN,
		PLANWAGEN,
		KINDERWAGEN,
		BETT,
		GEHIRN,
		KAMERA,
		KERZE,
		KISTE,
		DIAMANT,
		DIPLOM,
		WASSERHAHN,
		FUSSABDRUCK_LINKS,
		FUSSABDRUCK_RECHTS,
		SONNENBRILLE,
		HAARBUERSTE,
		BEHINDERT,
		PEACE_HANDZEICHEN,
		HAENDESCHUETTELN,
		DAMENHUT,
		ZYLINDER,
		DIPLOMHUT,
		SCHUTZHELM,
		LAMPE,
		MAGNET,
		FUSSABDRUECKE,
		MANN,
		FRAU,
		RING,
		SCHAUCKELPFERD,
		TELESKOP,
		FERNSEHER,
		REGENSCHIRM,
		GIESSKANNE,
		BRUNNEN,
		FACEBOOK,
		BOMBE,
		BOMBE_ANDERS,
		TRANK,
		AESCULAP,
		AESCULAP_ANDERS,
		VERBOT,
		VERBOT_ANDERS,
		VERBOT_RAUCHEN,
		ATOM,
		RECYCLING,
		STETOSKOP,
		THERMOMETER,
		BIOHAZARD,
		GIFT,
		STRAHLUNG,
		STRAHLUNG_ANDERS,
		STOPSCHILD
	}
	#endregion

	#region SymbolTypeGroups
	public class SymbolTypeGroup
	{
		public static SymbolType Random(List<SymbolType> symbols)
		{
			return symbols[Utils.RandomRange(0, symbols.Count)];
		}

		private static List<SymbolType> love = null;

		public static List<SymbolType> LOVE
		{
			get
			{
				if (love == null)
					love = new List<SymbolType>
					{
						SymbolType.BLUME_4,
						SymbolType.HERZ,
						SymbolType.HERZEN,
						SymbolType.HERZ_PFEIL,
						SymbolType.HERZ_ZELDA,
						SymbolType.MAGNET
					};
				return love;
			}
		}

		private static List<SymbolType> question = null;

		public static List<SymbolType> QUESTION
		{
			get
			{
				if (question == null)
					question = new List<SymbolType>
					{
						SymbolType.FRAGEZEICHEN
					};
				return question;
			}
		}

		private static List<SymbolType> agreement = null;

		public static List<SymbolType> AGREEMENT
		{
			get
			{
				if (agreement == null)
					agreement = new List<SymbolType>
					{
						SymbolType.HAENDESCHUETTELN,
						SymbolType.HAKERL
					};
				return agreement;
			}
		}

		private static List<SymbolType> grumble = null;

		public static List<SymbolType> GRUMBLE
		{
			get
			{
				if (grumble == null)
					grumble = new List<SymbolType>
					{
						SymbolType.BOXHANDSCHUH,
						SymbolType.BLITZ,
						SymbolType.BLITZ_ANDERS,
						SymbolType.GIFT,
						SymbolType.BOMBE,
						SymbolType.BOMBE_ANDERS,
						SymbolType.RUFZEICHEN_1,
						SymbolType.RUFZEICHEN_2,
						SymbolType.STOPSCHILD
					};
				return grumble;
			}
		}

		private static List<SymbolType> music = null;

		public static List<SymbolType> MUSIC
		{
			get
			{
				if (music == null)
					music = new List<SymbolType>
					{
						SymbolType.MUSIKNOTE_1,
						SymbolType.MUSIKNOTE_2,
						SymbolType.MUSIKNOTE_3
					};
				return music;
			}
		}
	}
	
	#endregion



	#region CategoryGarment

	public enum CategoryGarment
	{
		UNDEFINED,
		ACCESSORY,
		TROUSERS,
		TOP,
		DRESS,
		UNDERWEAR,
		HAIRSTYLE,
		SPECIAL,
		SHOE
	}

	#endregion

	#region ComicTopic
	/// <summary>

	/// Comic Topic. IMPORTANT: if you change that enum, reflect it in "PMEnum" (in Scripts/Utilities)!
	/// 
	/// </summary>
	public enum ComicTopic
	{
		UNDEFINED,
		ALTERNATIVES,
		BIRTHDAY,
		BODY,
		CATS,
		DOGS,
		RABBITS,
		CINEMA,
		CRIME,
		DEATH,
		DISEASE,
		DISPUTE,
		DREAMS,
		FAMILY,
		JOKE,
		KISSING,
		EDUCATION,
		COMPLIMENTS,
		HAPPINESS,
		SADNESS,
		LUXURY,
		MONOLOGUE,
		EMOTION,
		MUSIC,
		NATURE,
		NO_INFO,
		NO_RESPONSE,
		NOITEM,
		PHILOSOPHY,
		POLITICS,
		PSYCHOLOGY,
		RELATIONSHIP,
		RELIGION,
		SCIFI,
		SEX,
		SPECIAL,
		SPECIALSTARTERS,
		STATEMENTS,
		TOILET,
		TRAVEL,
		WEDDING,
		CHRISTMAS,
		NEW_YEAR,
		EASTERN,
		WORK,
		AGE,
		ORACLE,
		SPORT,
		DEEP_STATEMENTS,
		INDIRECT,
		FOOD,
		ALCOHOL,
		SCIENCE,
		NO_TOPIC,
		ART,
		ASTROLOGY,
		QUOTATIONS,
		LIFESTYLE,
		GENDER,
		ANIMALS,
		FRIENDSHIP,
		MADNESS,
		GAMES,
		PARTY,
		MY_TOPIC,
		LIVING,
		MARRIAGE,
		MEDIA,
		SADOMASO,
		POETRY,
		MINTYSCRIPT,
		GRAMMATICAL
	}
	#endregion

	#region ObstacleCategory

	public enum ObstacleCategory
	{
		CULTURE,
		HOBBY,
		LIFESTYLE,
		PERSONAL,
		RELATIONSHIP,
		WISDOM,
		WALLPAPER,
		//    FLOOR,

	}
	#endregion

	#region ObstacleSubCategory

	public enum ObstacleSubCategory
	{
		LIFESTYLE,
		CULTURE,
		OCCASION,
		FEELINGS,
		WISDOM,
		HOBBY,
		RELATIONSHIP,
		SEX,
		NATURE,
		RED,
		ORANGE,
		TAN,
		BROWN,
		YELLOW,
		DARKGREEN,
		YELLOWGREEN,
		GREEN,
		PEACHPUFF,
		SKYBLUE,
		AQUAMARINE,
		BLUE,
		CYAN,
		GOLD,
		MAGENTA,
		MAROON,
		ORCHID,
		PINK,
		PURPLE,
		SALMON,
		VIOLET,
		WALLPAPER_TEST_1,
		LIFE
	}
	#endregion

	#region ColorValues

	public class ColorValues
	{
		private static Dictionary<ObstacleSubCategory,uint> _values = null;

		public static Dictionary<ObstacleSubCategory,uint> values
		{
			get
			{
				if (_values == null)
				{
					_values = new Dictionary<ObstacleSubCategory, uint>();
					_values.Add(ObstacleSubCategory.BLUE, 0x0000ff);
					_values.Add(ObstacleSubCategory.RED, 0xff0000);
					_values.Add(ObstacleSubCategory.ORANGE, 0xffa500);
					_values.Add(ObstacleSubCategory.TAN, 0xd2b48c);
					_values.Add(ObstacleSubCategory.BROWN, 0xcd853f);
					_values.Add(ObstacleSubCategory.YELLOW, 0xffff00);
					_values.Add(ObstacleSubCategory.DARKGREEN, 0x006400);
					_values.Add(ObstacleSubCategory.YELLOWGREEN, 0x9acd32);
					_values.Add(ObstacleSubCategory.GREEN, 0x00ff00);
					_values.Add(ObstacleSubCategory.PEACHPUFF, 0xffdab9);
					_values.Add(ObstacleSubCategory.SKYBLUE, 0x87ceeb);
					_values.Add(ObstacleSubCategory.AQUAMARINE, 0x7fffd4);
					_values.Add(ObstacleSubCategory.CYAN, 0x00ffff);
					_values.Add(ObstacleSubCategory.GOLD, 0xffd700);
					_values.Add(ObstacleSubCategory.MAGENTA, 0xff00ff);
					_values.Add(ObstacleSubCategory.MAROON, 0xb03060);
					_values.Add(ObstacleSubCategory.ORCHID, 0xda70d6);
					_values.Add(ObstacleSubCategory.PINK, 0xffc0cb);
					_values.Add(ObstacleSubCategory.PURPLE, 0xa020f0);
					_values.Add(ObstacleSubCategory.SALMON, 0xfa8072);
					_values.Add(ObstacleSubCategory.VIOLET, 0xee82ee);
				}
				return _values;
			}
		}
	}

	#endregion

	#if minty_toons
	
	













#region InventoryCategories
	public class InventoryCategory
	{
		public ObstacleCategory cat;
		public ObstacleSubCategory subcat;
		public ComicTopic topic;

		public InventoryCategory(ObstacleCategory cat, ObstacleSubCategory sub, ComicTopic topic)
		{
			this.cat = cat;
			this.subcat = sub;
			this.topic = topic;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			// If parameter cannot be cast to Point return false.
			InventoryCategory p = obj as InventoryCategory;
			if ((System.Object)p == null)
			{
				return false;
			}
			return (p.cat == this.cat && p.subcat == this.subcat && p.topic == this.topic);
		}

		public override string ToString()
		{
			return cat + "/" + subcat + "/" + topic;
		}

		public static InventoryCategory[] INVENTORY_CATEGORIES = new InventoryCategory[]
		{
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.RELATIONSHIP, ComicTopic.RELATIONSHIP),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.RELATIONSHIP, ComicTopic.COMPLIMENTS),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.SEX, ComicTopic.KISSING),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.SEX, ComicTopic.SEX),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.SEX, ComicTopic.SADOMASO),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.RELATIONSHIP, ComicTopic.MARRIAGE),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.RELATIONSHIP, ComicTopic.DISPUTE),
			new InventoryCategory(ObstacleCategory.RELATIONSHIP, ObstacleSubCategory.RELATIONSHIP, ComicTopic.FRIENDSHIP),
		
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.FEELINGS, ComicTopic.DREAMS),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.FEELINGS, ComicTopic.HAPPINESS),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.FEELINGS, ComicTopic.SADNESS),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.LIFE, ComicTopic.FAMILY),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.LIFE, ComicTopic.AGE),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.LIFE, ComicTopic.GENDER),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.LIFE, ComicTopic.DISEASE),
			new InventoryCategory(ObstacleCategory.PERSONAL, ObstacleSubCategory.LIFE, ComicTopic.DEATH),
		
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.LUXURY),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.LIVING),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.BODY),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.WORK),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.CRIME),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.TRAVEL),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.PARTY),
			new InventoryCategory(ObstacleCategory.LIFESTYLE, ObstacleSubCategory.LIFESTYLE, ComicTopic.JOKE),
		
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.ART),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.CINEMA),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.MUSIC),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.MEDIA),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.OCCASION, ComicTopic.BIRTHDAY),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.OCCASION, ComicTopic.WEDDING),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.OCCASION, ComicTopic.CHRISTMAS),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.OCCASION, ComicTopic.NEW_YEAR),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.OCCASION, ComicTopic.EASTERN),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.FOOD),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.ALCOHOL),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.TOILET),
			new InventoryCategory(ObstacleCategory.CULTURE, ObstacleSubCategory.CULTURE, ComicTopic.POETRY),

			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.PSYCHOLOGY),
			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.PHILOSOPHY),
			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.SCIENCE),
			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.RELIGION),
			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.EDUCATION),
			new InventoryCategory(ObstacleCategory.WISDOM, ObstacleSubCategory.WISDOM, ComicTopic.POLITICS),
		
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.NATURE, ComicTopic.CATS),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.NATURE, ComicTopic.DOGS),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.NATURE, ComicTopic.RABBITS),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.NATURE, ComicTopic.ANIMALS),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.NATURE, ComicTopic.NATURE),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.HOBBY, ComicTopic.GAMES),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.HOBBY, ComicTopic.SPORT),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.HOBBY, ComicTopic.SCIFI),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.HOBBY, ComicTopic.MADNESS),
			new InventoryCategory(ObstacleCategory.HOBBY, ObstacleSubCategory.HOBBY, ComicTopic.ASTROLOGY),


		};


		public static InventoryCategory GetInventoryCategory(ObstacleCategory cat, ObstacleSubCategory subcat, ComicTopic comicTopic)
		{
			for (int i = 0; i < INVENTORY_CATEGORIES.Length; i++)
			{
				if (INVENTORY_CATEGORIES[i].cat == cat &&
				    INVENTORY_CATEGORIES[i].subcat == subcat &&
				    INVENTORY_CATEGORIES[i].topic == comicTopic)
				{
					return INVENTORY_CATEGORIES[i];
				}
			}
			return null;
		}
	}




	













#endregion
	

	#endif
	
	#region BubbleType
	public enum BubbleType
	{
		SAY,
		THINK,
		SAYSYMBOL,
		THINKSYMBOL
	}
	
	#endregion

	#if minty_toons
	













#region Level
	public enum Level
	{
		UNKNOWN,
		TEST,
		UTOPIA,
		BAR02,
		FLAT02,
		CHARACTER_SELECTION,
		PLANET,
		WINTERFLAT_10,
		WINTERDISKO,
		SPACEDISKO,
		FAMILYFLAT03,
		DISCO01,
		SHOP01,
		STUDIO_FLAT_05,
		WOHNUNG01,
		BOUTIQUE01,
		CITYHALL,
		GAMEHALL_BLINDDATE,
		GAMEHALL_PLUSMINUS,
		LOTTOKIOSK,
		NECKERMANN,
		DECORATE,
		WAISENHAUS,

	}

	













#endregion
	
	#endif

	#endregion

}