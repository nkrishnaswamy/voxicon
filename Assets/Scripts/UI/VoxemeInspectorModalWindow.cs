﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Global;
using Vox;

public class VoxemeInspectorModalWindow : ModalWindow {
	public int inspectorWidth = 230;
	public int inspectorHeight = 300;
	public int inspectorMargin = 150;

	string inspectorTitle = "";
	public string InspectorTitle {
		get { return inspectorTitle; }
		set { inspectorTitle = value; }
	}

	Vector2 scrollPosition;
	public Vector2 ScrollPosition {
		get { return scrollPosition; }
		set { scrollPosition = value; }
	}
	
	Rect inspectorRect = new Rect(0,0,0,0);
	public Rect InspectorRect {
		get { return inspectorRect; }
		set { inspectorRect = value; }
	}
	
	Vector2 inspectorPosition;
	public Vector2 InspectorPosition {
		get { return inspectorPosition; }
		set { inspectorPosition = value; }
	}
	
	float inspectorPositionAdjX;
	float inspectorPositionAdjY;
	GUIStyle inspectorStyle;
	string[] inspectorMenuItems = {"Reify As...", "View/Edit Markup", "Modify", "Delete"};
	
	int inspectorChoice = -1;
	public int InspectorChoice {
		get { return inspectorChoice; }
		set { inspectorChoice = value; }
	}
	
	GameObject inspectorObject;
	public GameObject InspectorObject {
		get { return inspectorObject; }
		set { inspectorObject = value; }
	}

	string inspectorVoxeme;
	public string InspectorVoxeme {
		get { return inspectorVoxeme; }
		set { inspectorVoxeme = value; }
	}
	
	string newName = "";
	string xScale = "1", yScale = "1", zScale = "1";
	
	bool drawInspector;
	public bool DrawInspector {
		get { return drawInspector; }
		set {
			drawInspector = value;
			if (!drawInspector) {
				inspectorRect = new Rect(0,0,0,0);
				scrollPosition = new Vector2(0,0);
			}
		}
	}

	bool editable;
	
	GUIStyle listStyle = new GUIStyle ();
	Texture2D tex;
	Color[] colors;
	
	// Markup vars
	// ENTITY
	VoxEntity.EntityType mlEntityType = VoxEntity.EntityType.None;

	// LEX
	string mlPred = "";
	
	string[] mlTypeOptions = new string[]{"physobj","human","artifact"};
	List<int> mlTypeSelectVisible = new List<int>(new int[]{-1});
	List<int> mlTypeSelected = new List<int>(new int[]{-1});
	int mlAddType = -1;
	List<int> mlRemoveType = new List<int>(new int[]{-1});
	int mlTypeCount = 1;
	List<string> mlTypes = new List<string>(new string[]{""});

	// TYPE
	string[] mlHeadOptions = new string[]{"cylindroid", "ellipsoid", "rectangular_prism", "toroid", "pyramidoid", "sheet"};
	int mlHeadSelectVisible = -1;
	int mlHeadSelected = -1;
	string mlHead = "";
	string mlHeadReentrancy = "";
	
	int mlAddComponent = -1;
	List<int> mlRemoveComponent = new List<int>();
	int mlComponentCount = 0;
	List<string> mlComponents = new List<string>();
	List<string> mlComponentReentrancies = new List<string>();
	
	string[] mlConcavityOptions = new string[]{"Concave","Flat","Convex"};
	int mlConcavitySelectVisible = -1;
	int mlConcavitySelected = -1;
	string mlConcavity = "";
	string mlConcavityReentrancy = "";
	
	bool mlRotatSymX = false;
	bool mlRotatSymY = false;
	bool mlRotatSymZ = false;
	bool mlReflSymXY = false;
	bool mlReflSymXZ = false;
	bool mlReflSymYZ = false;

	int mlArgCount = 0;
	List<string> mlArgs = new List<string>();

	int mlSubeventCount = 0;
	List<string> mlSubevents = new List<string>();

	string mlClass = "";
	string mlValue = "";
	string mlConstr = "";
	
	// HABITAT
	int mlAddIntrHabitat = -1;
	List<int> mlRemoveIntrHabitat = new List<int>();
	int mlIntrHabitatCount = 0;
	List<string> mlIntrHabitats = new List<string> ();
	
	int mlAddExtrHabitat = -1;
	List<int> mlRemoveExtrHabitat = new List<int>();
	int mlExtrHabitatCount = 0;
	List<string> mlExtrHabitats = new List<string> ();
	
	// AFFORD_STR
	int mlAddAffordance = -1;
	List<int> mlRemoveAffordance = new List<int>();
	int mlAffordanceCount = 0;
	List<string> mlAffordances = new List<string>();
	
	// EMBODIMENT
	string[] mlScaleOptions = new string[]{"<agent","agent",">agent"};
	int mlScaleSelectVisible = -1;
	int mlScaleSelected = -1;
	string mlScale = "";
	
	bool mlMovable = false;
	
	bool markupCleared = false;
	VoxML loadedObject = new VoxML();

	public override bool Render {
		get { return render; }
		set { render = value; }
	}

	// Use this for initialization
	protected override void Start () {
		colors = new Color[]{Color.white,Color.white,Color.white,Color.white};
		tex = new Texture2D (2, 2);
		
		// Make a GUIStyle that has a solid white hover/onHover background to indicate highlighted items
		listStyle.normal.textColor = Color.white;
		tex.SetPixels(colors);
		tex.Apply();
		listStyle.hover.background = tex;
		listStyle.onHover.background = tex;
		listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

		//Render = true;

		editable = (PlayerPrefs.GetInt ("Make Voxemes Editable") == 1);

		base.Start ();

		windowManager.OnNewModalWindow (this, new ModalWindowEventArgs(id));
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	protected override void OnGUI () {
		/*if (DrawInspector) {
			inspectorPositionAdjX = inspectorPosition.x;
			inspectorPositionAdjY = inspectorPosition.y;
			if (inspectorPosition.x + inspectorWidth > Screen.width) {
				if (inspectorPosition.y > Screen.height - inspectorMargin) {
					inspectorPositionAdjX = inspectorPosition.x - inspectorWidth;
					inspectorPositionAdjY = inspectorPosition.y - inspectorHeight;
					inspectorRect = new Rect (inspectorPosition.x - inspectorWidth, inspectorPosition.y - inspectorHeight, inspectorWidth, inspectorHeight);
				}
				else
				if (inspectorPosition.y + inspectorHeight > Screen.height) {
					inspectorPositionAdjX = inspectorPosition.x - inspectorWidth;
					inspectorRect = new Rect (inspectorPosition.x - inspectorWidth, inspectorPosition.y, inspectorWidth, Screen.height - inspectorPosition.y);
				}
				else {
					inspectorPositionAdjX = inspectorPosition.x - inspectorWidth;
					inspectorRect = new Rect (inspectorPosition.x - inspectorWidth, inspectorPosition.y, inspectorWidth, inspectorHeight);
				}
			}
			else
			if (inspectorPosition.y > Screen.height - inspectorMargin) {
				inspectorPositionAdjY = inspectorPosition.y - inspectorHeight;
				inspectorRect = new Rect (inspectorPosition.x, inspectorPosition.y - inspectorHeight, inspectorWidth, inspectorHeight);
			}
			else
			if (inspectorPosition.y + inspectorHeight > Screen.height) {
				inspectorRect = new Rect (inspectorPosition.x, inspectorPosition.y, inspectorWidth, Screen.height - inspectorPosition.y);
			}
			else {
				inspectorRect = new Rect (inspectorPosition.x, inspectorPosition.y, inspectorWidth, inspectorHeight);
			}*/

/*#if UNITY_EDITOR || UNITY_STANDALONE
			if (File.Exists (inspectorObject.name + ".xml")) {
				if (!ObjectLoaded (inspectorObject)) {
					loadedObject = LoadMarkup (inspectorObject);
					markupCleared = false;
				}
			}
			else {
				if (!markupCleared) {
					InitNewMarkup ();
					loadedObject = new VoxML ();
				}
			}
#endif
#if UNITY_WEBPLAYER*/
			// Resources load here
//			TextAsset markup = Resources.Load (InspectorVoxeme) as TextAsset;
//			if (markup != null) {
//				if (!ObjectLoaded (markup.text)) {
//					loadedObject = LoadMarkup (markup.text);
//					windowTitle = InspectorVoxeme;
//					markupCleared = false;
//				}
//			}
			if (File.Exists (string.Format("{0}/{1}",Data.voxmlDataPath,string.Format("{0}.xml", InspectorVoxeme)))) {
				using (StreamReader sr = new StreamReader (string.Format("{0}/{1}",Data.voxmlDataPath,string.Format("{0}.xml", InspectorVoxeme)))) {
					String markup = sr.ReadToEnd ();
					if (!ObjectLoaded (markup)) {
						loadedObject = LoadMarkup (markup);
						windowTitle = InspectorVoxeme.Substring(InspectorVoxeme.LastIndexOf('/') + 1);
						markupCleared = false;
					}
				}
			}
			else {
				if (!markupCleared) {
					InitNewMarkup ();
					//loadedObject = new VoxML ();
		
					switch (InspectorVoxeme.Remove(InspectorVoxeme.LastIndexOf('/'))) {
					case "objects":
						mlEntityType = VoxEntity.EntityType.Object;
						break;

					case "programs":
						mlEntityType = VoxEntity.EntityType.Program;
						break;

					case "attributes":
						mlEntityType = VoxEntity.EntityType.Attribute;
						break;

					case "relations":
						mlEntityType = VoxEntity.EntityType.Relation;
						break;

					case "functions":
						mlEntityType = VoxEntity.EntityType.Function;
						break;

					default:
						break;
					}

					windowTitle = InspectorVoxeme.Substring(InspectorVoxeme.LastIndexOf('/') + 1);
				}
			}
//#endif
		//}

		base.OnGUI ();
	}

	public override void DoModalWindow(int windowID) {
		base.DoModalWindow (windowID);

		switch (mlEntityType) {
		case	VoxEntity.EntityType.Object:
			DisplayObjectMarkup ();
			break;

		case	VoxEntity.EntityType.Program:
			DisplayProgramMarkup ();
			break;

		case	VoxEntity.EntityType.Attribute:
			DisplayAttributeMarkup ();
			break;

		case	VoxEntity.EntityType.Relation:
			DisplayRelationMarkup ();
			break;

		case	VoxEntity.EntityType.Function:
			DisplayFunctionMarkup ();
			break;

		default:
			break;
		}

		if (editable) {
			if (GUILayout.Button ("Save")) {
				SaveMarkup(InspectorVoxeme, mlEntityType);
			}
			else if (GUILayout.Button ("Import")) {
				ImportMarkup(InspectorVoxeme, mlEntityType);
			}
		}

		Vector2 textDimensions = GUI.skin.label.CalcSize (new GUIContent (inspectorTitle));
		GUI.Label (new Rect (((2 * inspectorPosition.x + inspectorWidth) / 2) - textDimensions.x / 2, inspectorPosition.y, textDimensions.x, 25), inspectorTitle);
	}

	void DisplayObjectMarkup() {
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

		inspectorStyle = GUI.skin.box;
		inspectorStyle.wordWrap = true;
		inspectorStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("LEX");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Pred");

		if (editable) {
			mlPred = GUILayout.TextField (mlPred, 25, GUILayout.Width (inspectorWidth - 100));
		} else {
			GUILayout.Box (mlPred, GUILayout.Width (inspectorWidth - 100));
		}

		GUILayout.EndHorizontal ();
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Type");

		GUILayout.BeginVertical ();

		if (editable) {
			for (int i = 0; i < mlTypeCount; i++) {
				GUILayout.BeginHorizontal ();
				if (mlTypeSelectVisible [i] == 0) {
					GUILayout.BeginVertical (inspectorStyle);
					mlTypeSelected [i] = -1;
					mlTypeSelected [i] = GUILayout.SelectionGrid (mlTypeSelected [i], mlTypeOptions, 1, listStyle, GUILayout.Width (70), GUILayout.ExpandWidth (true));
					if (mlTypeSelected [i] != -1) {
						mlTypes [i] = mlTypeOptions [mlTypeSelected [i]];
						mlTypeSelectVisible [i] = -1;
					}
					GUILayout.EndVertical ();
				} else {
					mlTypeSelectVisible [i] = GUILayout.SelectionGrid (mlTypeSelectVisible [i], new string[]{ mlTypes [i] }, 1, GUI.skin.button, GUILayout.Width (70), GUILayout.ExpandWidth (true));
				}
				if (i != 0) { // can't remove first type
					mlRemoveType [i] = GUILayout.SelectionGrid (mlRemoveType [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
				}
				GUILayout.EndHorizontal ();
			}
		} else {
			for (int i = 0; i < mlTypeCount; i++) {
				GUILayout.BeginHorizontal ();

				GUILayout.Box (mlTypes [i], GUILayout.Width (inspectorWidth - 130), GUILayout.ExpandWidth (true));

				GUILayout.EndHorizontal ();
			}
		}
		GUILayout.EndVertical ();

		GUILayout.EndHorizontal ();

		if (editable) {
			mlAddType = GUILayout.SelectionGrid (mlAddType, new string[]{ "+" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));

			if (mlAddType == 0) {	// add new type
				mlTypeCount++;
				mlTypes.Add ("");
				mlTypeSelectVisible.Add (-1);
				mlTypeSelected.Add (-1);
				mlRemoveType.Add (-1);
				mlAddType = -1;
			}

			for (int i = 0; i < mlTypeCount; i++) {
				if (mlRemoveType [i] == 0) {
					mlRemoveType [i] = -1;
					mlTypes.RemoveAt (i);
					mlRemoveType.RemoveAt (i);
					mlTypeCount--;
				}
			}
		}

		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("TYPE");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Head");

		GUILayout.BeginHorizontal (inspectorStyle);
		if (editable) {
			if (mlHeadSelectVisible == 0) {
				GUILayout.BeginVertical (inspectorStyle);
				mlHeadSelected = -1;
				mlHeadSelected = GUILayout.SelectionGrid (mlHeadSelected, mlHeadOptions, 1, listStyle, GUILayout.Width (60), GUILayout.ExpandWidth (true));
				if (mlHeadSelected != -1) {
					mlHead = mlHeadOptions [mlHeadSelected];
					mlHeadSelectVisible = -1;
				}
				GUILayout.EndVertical ();
				mlHeadReentrancy = GUILayout.TextField (mlHeadReentrancy, 25, GUILayout.Width (20));
			}
			else {
				mlHeadSelectVisible = GUILayout.SelectionGrid (mlHeadSelectVisible, new string[]{ mlHead }, 1, GUI.skin.button, GUILayout.Width (60), GUILayout.ExpandWidth (true));
				mlHeadReentrancy = GUILayout.TextField (mlHeadReentrancy, 25, GUILayout.Width (20));
			}
		}
		else {
			GUILayout.Box (mlHead, GUILayout.Width (inspectorWidth - 130), GUILayout.ExpandWidth (true));
			GUILayout.Box (mlHeadReentrancy, GUILayout.Width (20), GUILayout.ExpandWidth (true));
		}
		GUILayout.EndHorizontal ();

		GUILayout.EndHorizontal ();
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Components");

		if (editable) {
			mlAddComponent = GUILayout.SelectionGrid (mlAddComponent, new string[]{ "+" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
		}

		GUILayout.EndHorizontal ();

		if (editable) {
			if (mlAddComponent == 0) {	// add new component
				mlComponentCount++;
				mlComponents.Add ("");
				mlComponentReentrancies.Add ("");
				mlAddComponent = -1;
				mlRemoveComponent.Add (-1);
			}
		}

		GUILayout.BeginVertical (inspectorStyle);
		for (int i = 0; i < mlComponentCount; i++) {
			string componentName = mlComponents [i].Split (new char[]{ '[' }) [0];
			//TextAsset ml = Resources.Load (componentName) as TextAsset;
			if (File.Exists (string.Format ("{0}/{1}", Data.voxmlDataPath, string.Format ("objects/{0}.xml", componentName)))) {
				using (StreamReader sr = new StreamReader (
					                         string.Format ("{0}/{1}", Data.voxmlDataPath, string.Format ("objects/{0}.xml", componentName)))) {
					String ml = sr.ReadToEnd ();
					if (ml != null) {
						float textSize = GUI.skin.label.CalcSize (new GUIContent (mlComponents [i])).x;
						float padSize = GUI.skin.label.CalcSize (new GUIContent (" ")).x;
						int padLength = (int)(((inspectorWidth - 85) - textSize) / (int)padSize);

						GUILayout.BeginHorizontal (inspectorStyle);
						bool componentButton = GUILayout.Button (mlComponents [i].PadRight (padLength + mlComponents [i].Length - 3), GUILayout.Width (inspectorWidth - 85));
						mlComponentReentrancies [i] = GUILayout.TextField (mlComponentReentrancies [i], 25, GUILayout.Width (20));
						if (editable) {
							mlRemoveComponent.Add (-1);
							mlRemoveComponent [i] = GUILayout.SelectionGrid (mlRemoveComponent [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
						}
						GUILayout.EndHorizontal ();
				
						if (componentButton) {
//							if (ml != null) {
								LoadMarkup (ml);
								inspectorTitle = mlPred;
//							}
//							else {
//								if (editable) {
//									mlComponents [i] = GUILayout.TextField (mlComponents [i], 25, GUILayout.Width (inspectorWidth - 85));
//									mlComponentReentrancies [i] = GUILayout.TextField (mlComponents [i], 25, GUILayout.Width (20));
//									mlRemoveComponent.Add (-1);
//									mlRemoveComponent [i] = GUILayout.SelectionGrid (mlRemoveComponent [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
//								}
//								else {
//									GUILayout.Box (mlComponents [i], GUILayout.Width (inspectorWidth - 85));
//									GUILayout.Box (mlComponentReentrancies [i], GUILayout.Width (20), GUILayout.ExpandWidth (true));
//								}
							}
//						}
					}
					else {
						if (editable) {
							GUILayout.BeginHorizontal (inspectorStyle);
							mlComponents [i] = GUILayout.TextField (mlComponents [i], 25, GUILayout.Width (inspectorWidth - 85));
							mlComponentReentrancies [i] = GUILayout.TextField (mlComponents [i], 25, GUILayout.Width (20));
							mlRemoveComponent.Add (-1);
							mlRemoveComponent [i] = GUILayout.SelectionGrid (mlRemoveComponent [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
							GUILayout.EndHorizontal ();
						}
						else {
							GUILayout.Box (mlComponents [i], GUILayout.Width (inspectorWidth - 85));
							GUILayout.Box (mlComponentReentrancies[i], GUILayout.Width (20), GUILayout.ExpandWidth (true));
						}
					}
				}
			}
			else {
				if (editable) {
					GUILayout.BeginHorizontal (inspectorStyle);
					mlComponents [i] = GUILayout.TextField (mlComponents [i], 25, GUILayout.Width (inspectorWidth - 85));
					mlComponentReentrancies [i] = GUILayout.TextField (mlComponentReentrancies [i], 25, GUILayout.Width (20));
					mlRemoveComponent.Add (-1);
					mlRemoveComponent [i] = GUILayout.SelectionGrid (mlRemoveComponent [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
					GUILayout.EndHorizontal ();
				}
				else {
					GUILayout.Box (mlComponents [i], GUILayout.Width (inspectorWidth - 85));
					GUILayout.Box (mlComponentReentrancies[i], GUILayout.Width (20), GUILayout.ExpandWidth (true));
				}
			}
		}
		GUILayout.EndVertical ();

		if (editable) {
			for (int i = 0; i < mlComponentCount; i++) {
				if (mlRemoveComponent [i] == 0) {
					mlRemoveComponent [i] = -1;
					mlComponents.RemoveAt (i);
					mlComponentReentrancies.RemoveAt (i);
					mlRemoveComponent.RemoveAt (i);
					mlComponentCount--;
				}
			}
		}

		GUILayout.EndVertical ();

		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Concavity");

		if (editable) {
			if (mlConcavitySelectVisible == 0) {
				GUILayout.BeginVertical (inspectorStyle);
				mlConcavitySelected = -1;
				mlConcavitySelected = GUILayout.SelectionGrid (mlConcavitySelected, mlConcavityOptions, 1, listStyle, GUILayout.Width (70), GUILayout.ExpandWidth (true));
				if (mlConcavitySelected != -1) {
					mlConcavity = mlConcavityOptions [mlConcavitySelected];
					mlConcavitySelectVisible = -1;
				}
				GUILayout.EndVertical ();
				mlConcavityReentrancy = GUILayout.TextField (mlConcavityReentrancy, 25, GUILayout.Width (20));
			}
			else {
				mlConcavitySelectVisible = GUILayout.SelectionGrid (mlConcavitySelectVisible, new string[]{ mlConcavity }, 1, GUI.skin.button, GUILayout.Width (70), GUILayout.ExpandWidth (true));
				mlConcavityReentrancy = GUILayout.TextField (mlConcavityReentrancy, 25, GUILayout.Width (20));
			}
		}
		else {
			GUILayout.Box (mlConcavity, GUILayout.Width (inspectorWidth - 130), GUILayout.ExpandWidth (true));
			GUILayout.Box (mlConcavityReentrancy, GUILayout.Width (20), GUILayout.ExpandWidth (true));
		}

		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("Rotational Symmetry");
		GUILayout.BeginHorizontal ();

		if (editable) {
			mlRotatSymX = GUILayout.Toggle(mlRotatSymX,"X");
			mlRotatSymY = GUILayout.Toggle(mlRotatSymY,"Y");
			mlRotatSymZ = GUILayout.Toggle(mlRotatSymZ,"Z");
		}
		else {
			GUILayout.Toggle (mlRotatSymX, "X");
			GUILayout.Toggle (mlRotatSymY, "Y");
			GUILayout.Toggle (mlRotatSymZ, "Z");
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("Reflectional Symmetry");
		GUILayout.BeginHorizontal ();

		if (editable) {
			mlReflSymXY = GUILayout.Toggle(mlReflSymXY,"XY");
			mlReflSymXZ = GUILayout.Toggle(mlReflSymXZ,"XZ");
			mlReflSymYZ = GUILayout.Toggle(mlReflSymYZ,"YZ");
		}
		else {
			GUILayout.Toggle (mlReflSymXY, "XY");
			GUILayout.Toggle (mlReflSymXZ, "XZ");
			GUILayout.Toggle (mlReflSymYZ, "YZ");
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("HABITAT");
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Intrinsic");

		if (editable) {
			mlAddIntrHabitat = GUILayout.SelectionGrid (mlAddIntrHabitat, new string[]{ "+" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));

			if (mlAddIntrHabitat == 0) {	// add new intrinsic habitat formula
				mlIntrHabitatCount++;
				mlIntrHabitats.Add ("Name=Formula");
				mlAddIntrHabitat = -1;
			}
		}

		GUILayout.EndHorizontal ();

		if (editable) {
			for (int i = 0; i < mlIntrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				mlIntrHabitats [i] = GUILayout.TextField (mlIntrHabitats [i].Split (new char[]{ '=' }) [0], 25, GUILayout.Width (50)) + "=" +
				GUILayout.TextField (mlIntrHabitats [i].Split (new char[]{ '=' }) [1], 25, GUILayout.Width (60));
				mlRemoveIntrHabitat.Add (-1);
				mlRemoveIntrHabitat [i] = GUILayout.SelectionGrid (mlRemoveIntrHabitat [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
				GUILayout.EndHorizontal ();
			}

			for (int i = 0; i < mlIntrHabitatCount; i++) {
				if (mlRemoveIntrHabitat [i] == 0) {
					mlRemoveIntrHabitat [i] = -1;
					mlIntrHabitats.RemoveAt (i);
					mlRemoveIntrHabitat.RemoveAt (i);
					mlIntrHabitatCount--;
				}
			}
		}
		else {
			for (int i = 0; i < mlIntrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlIntrHabitats [i].Split (new char[]{ '=' }, 2) [0], GUILayout.Width (inspectorWidth - 150));
				GUILayout.Box (mlIntrHabitats [i].Split (new char[]{ '=' }, 2) [1], GUILayout.Width (inspectorWidth - 140));
				GUILayout.EndHorizontal ();
			}
		}

		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Extrinsic");

		if (editable) {
			mlAddExtrHabitat = GUILayout.SelectionGrid (mlAddExtrHabitat, new string[]{ "+" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));

			if (mlAddExtrHabitat == 0) {	// add new extrinsic habitat formula
				mlExtrHabitatCount++;
				mlExtrHabitats.Add ("Name=Formula");
				mlAddExtrHabitat = -1;
			}
		}

		GUILayout.EndHorizontal ();

		if (editable) {
			for (int i = 0; i < mlExtrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				mlExtrHabitats [i] = GUILayout.TextField (mlExtrHabitats [i].Split (new char[]{ '=' }) [0], 25, GUILayout.Width (50)) + "=" +
				GUILayout.TextField (mlExtrHabitats [i].Split (new char[]{ '=' }) [1], 25, GUILayout.Width (60));
				mlRemoveExtrHabitat.Add (-1);
				mlRemoveExtrHabitat [i] = GUILayout.SelectionGrid (mlRemoveExtrHabitat [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
				GUILayout.EndHorizontal ();
			}

			for (int i = 0; i < mlExtrHabitatCount; i++) {
				if (mlRemoveExtrHabitat [i] == 0) {
					mlRemoveExtrHabitat [i] = -1;
					mlExtrHabitats.RemoveAt (i);
					mlRemoveExtrHabitat.RemoveAt (i);
					mlExtrHabitatCount--;
				}
			}
		}
		else {
			for (int i = 0; i < mlExtrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlExtrHabitats [i].Split (new char[]{ '=' }, 2) [0], GUILayout.Width (inspectorWidth - 150));
				GUILayout.Box (mlExtrHabitats [i].Split (new char[]{ '=' }, 2) [1], GUILayout.Width (inspectorWidth - 140));
				GUILayout.EndHorizontal ();
			}
		}

		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("AFFORD_STR");

		GUILayout.BeginVertical (inspectorStyle);

		if (editable) {
			for (int i = 0; i < mlAffordanceCount; i++) {
				GUILayout.BeginHorizontal ();
				mlAffordances [i] = GUILayout.TextField (mlAffordances [i], 50, GUILayout.Width (115));
				mlRemoveAffordance.Add (-1);
				mlRemoveAffordance [i] = GUILayout.SelectionGrid (mlRemoveAffordance [i], new string[]{ "-" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));
				GUILayout.EndHorizontal ();
			}

			for (int i = 0; i < mlAffordanceCount; i++) {
				if (mlRemoveAffordance [i] == 0) {
					mlRemoveAffordance [i] = -1;
					mlAffordances.RemoveAt (i);
					mlRemoveAffordance.RemoveAt (i);
					mlAffordanceCount--;
				}
			}

			mlAddAffordance = GUILayout.SelectionGrid (mlAddAffordance, new string[]{ "+" }, 1, GUI.skin.button, GUILayout.ExpandWidth (true));

			if (mlAddAffordance == 0) {	// add new affordance
				mlAffordanceCount++;
				mlAffordances.Add ("");
				mlAddAffordance = -1;
			}
		}
		else {
			for (int i = 0; i < mlAffordanceCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlAffordances [i], GUILayout.Width (inspectorWidth - 85));
				GUILayout.EndHorizontal ();
			}
		}

		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("EMBODIMENT");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Scale");

		if (editable) {
			if (mlScaleSelectVisible == 0) {
				GUILayout.BeginVertical (inspectorStyle);
				mlScaleSelected = -1;
				mlScaleSelected = GUILayout.SelectionGrid (mlScaleSelected, mlScaleOptions, 1, listStyle, GUILayout.Width (70), GUILayout.ExpandWidth (true));
				if (mlScaleSelected != -1) {
					mlScale = mlScaleOptions [mlScaleSelected];
					mlScaleSelectVisible = -1;
				}
				GUILayout.EndVertical ();
			}
			else {
				mlScaleSelectVisible = GUILayout.SelectionGrid (mlScaleSelectVisible, new string[]{ mlScale }, 1, GUI.skin.button, GUILayout.Width (70), GUILayout.ExpandWidth (true));
			}
		}
		else {
			GUILayout.Box (mlScale, GUILayout.Width (inspectorWidth - 130), GUILayout.ExpandWidth (true));
		}

		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Movable");

		if (editable) {
			mlMovable = GUILayout.Toggle (mlMovable, "");
		}
		else {
			GUILayout.Toggle(mlMovable,"");
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("PARTICIPATION");
		GUILayout.BeginVertical (inspectorStyle);
		object[] programs = Directory.GetFiles (string.Format ("{0}/programs", Data.voxmlDataPath));
		//object[] assets = Resources.LoadAll ("Programs");
		foreach (object program in programs) {
			if (program != null) {
				List<string> participations = new List<string> ();
				foreach (string affordance in mlAffordances) {
					if (affordance.Contains (((string)program).Substring (((string)program).LastIndexOf ('/') + 1).Split ('.') [0])) {
						if (!participations.Contains (((string)program).Substring (((string)program).LastIndexOf ('/') + 1).Split ('.') [0])) {
							participations.Add (((string)program).Substring (((string)program).LastIndexOf ('/') + 1).Split ('.') [0]);
						}
					}
				}

				foreach (string p in participations) {
					using (StreamReader sr = new StreamReader (
						                        string.Format ("{0}/{1}", Data.voxmlDataPath, string.Format ("programs/{0}.xml", p)))) {
						//TextAsset ml = Resources.Load ("Programs/" + p) as TextAsset;
						String ml = sr.ReadToEnd ();
						if (ml != null) {
							float textSize = GUI.skin.label.CalcSize (new GUIContent (p)).x;
							float padSize = GUI.skin.label.CalcSize (new GUIContent (" ")).x;
							int padLength = (int)(((inspectorWidth - 85) - textSize) / (int)padSize);
							if (GUILayout.Button (p.PadRight (padLength + p.Length - 3), GUILayout.Width (inspectorWidth - 85))) {
								if (ml != null) {
									VoxemeInspectorModalWindow newInspector = gameObject.AddComponent<VoxemeInspectorModalWindow> ();
									//LoadMarkup (ml.text);
									//newInspector.DrawInspector = true;
									newInspector.windowRect = new Rect (inspectorRect.x + 25, inspectorRect.y + 25, inspectorWidth, inspectorHeight);
									//newInspector.InspectorTitle = mlComponents [i];
									newInspector.InspectorVoxeme = "Programs/" + p;
									newInspector.Render = true;
								}
								else {
								}
							}
						}
						else {
							GUILayout.Box ((string)program, GUILayout.Width (inspectorWidth - 85));
						}
					}
				}
			}
		}

		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.EndScrollView ();
	}

	void DisplayProgramMarkup() {
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

		inspectorStyle = GUI.skin.box;
		inspectorStyle.wordWrap = true;
		inspectorStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("LEX");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Pred");
		GUILayout.Box (mlPred, GUILayout.Width (inspectorWidth-100));
		GUILayout.EndHorizontal ();
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Type");

		GUILayout.BeginVertical ();
		for (int i = 0; i < mlTypeCount; i++) {
			GUILayout.BeginHorizontal ();

			GUILayout.Box (mlTypes [i], GUILayout.Width (inspectorWidth - 130), GUILayout.ExpandWidth (true));

			GUILayout.EndHorizontal ();
		}
		GUILayout.EndVertical ();

		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("TYPE");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Head");

		GUILayout.Box (mlHead, GUILayout.Width (inspectorWidth-130), GUILayout.ExpandWidth (true));

		GUILayout.EndHorizontal ();
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Args");
		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical (inspectorStyle);
		for (int i = 0; i < mlArgCount; i++) {
			GUILayout.Box (mlArgs [i], GUILayout.Width (inspectorWidth - 85));
		}
		GUILayout.EndVertical ();

		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Body");
		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical (inspectorStyle);
		for (int i = 0; i < mlSubeventCount; i++) {
		GUILayout.Box (mlSubevents [i], GUILayout.Width (inspectorWidth - 85));
		}
		GUILayout.EndVertical ();

		GUILayout.EndVertical ();

		GUILayout.EndVertical ();

		GUILayout.EndScrollView ();
	}

	void DisplayAttributeMarkup() {
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

		inspectorStyle = GUI.skin.box;
		inspectorStyle.wordWrap = true;
		inspectorStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.BeginVertical (inspectorStyle);

		GUILayout.EndVertical ();

		GUILayout.EndScrollView ();
	}

	void DisplayRelationMarkup() {
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

		inspectorStyle = GUI.skin.box;
		inspectorStyle.wordWrap = true;
		inspectorStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("LEX");
		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Pred");
		GUILayout.Box (mlPred, GUILayout.Width (inspectorWidth-100));
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.Label ("TYPE");

		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Class");
		GUILayout.Box (mlClass, GUILayout.Width (inspectorWidth-130), GUILayout.ExpandWidth (true));
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Value");
		GUILayout.Box (mlValue, GUILayout.Width (inspectorWidth-130), GUILayout.ExpandWidth (true));
		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical (inspectorStyle);
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Args");
		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical (inspectorStyle);
		for (int i = 0; i < mlArgCount; i++) {
			GUILayout.Box (mlArgs [i], GUILayout.Width (inspectorWidth - 85));
		}
		GUILayout.EndVertical ();

		GUILayout.EndVertical ();

		GUILayout.BeginHorizontal (inspectorStyle);
		GUILayout.Label ("Constr");
		GUILayout.Box (mlConstr, GUILayout.Width (inspectorWidth-130), GUILayout.ExpandWidth (true));
		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();

		GUILayout.EndScrollView ();
	}

	void DisplayFunctionMarkup() {
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

		inspectorStyle = GUI.skin.box;
		inspectorStyle.wordWrap = true;
		inspectorStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.BeginVertical (inspectorStyle);

		GUILayout.EndVertical ();

		GUILayout.EndScrollView ();
	}
	
	void InitNewMarkup() {
		// ENTITY
		mlEntityType = VoxEntity.EntityType.None;

		// LEX
		mlPred = "";
		
		mlTypeSelectVisible = new List<int>(new int[]{-1});
		mlTypeSelected = new List<int>(new int[]{-1});
		mlAddType = -1;
		mlRemoveType = new List<int>(new int[]{-1});
		mlTypeCount = 1;
		mlTypes = new List<string>(new string[]{""});
		
		// TYPE
		mlHeadSelectVisible = -1;
		mlHeadSelected = -1;
		mlHead = "";
		
		mlAddComponent = -1;
		mlRemoveComponent = new List<int>();
		mlComponentCount = 0;
		mlComponents = new List<string>();
		
		mlConcavitySelectVisible = -1;
		mlConcavitySelected = -1;
		mlConcavity = "";
		
		mlRotatSymX = false;
		mlRotatSymY = false;
		mlRotatSymZ = false;
		mlReflSymXY = false;
		mlReflSymXZ = false;
		mlReflSymYZ = false;

		mlArgCount = 0;
		mlArgs = new List<string>();

		mlSubeventCount = 0;
		mlSubevents = new List<string>();

		mlClass = "";
		mlValue = "";
		mlConstr = "";
		
		// HABITAT
		mlAddIntrHabitat = -1;
		mlRemoveIntrHabitat = new List<int>();
		mlIntrHabitatCount = 0;
		mlIntrHabitats = new List<string> ();
		
		mlAddExtrHabitat = -1;
		mlRemoveExtrHabitat = new List<int>();
		mlExtrHabitatCount = 0;
		mlExtrHabitats = new List<string> ();
		
		// AFFORD_STR
		mlAddAffordance = -1;
		mlRemoveAffordance = new List<int>();
		mlAffordanceCount = 0;
		mlAffordances = new List<string>();
		
		// EMBODIMENT
		mlScaleSelectVisible = -1;
		mlScaleSelected = -1;
		mlScale = "";
		
		mlMovable = false;
		
		markupCleared = true;
	}
	
	void ImportMarkup(string cloneToPath, VoxEntity.EntityType entityType) {
		ImportMarkupModalWindow importWindow = gameObject.AddComponent<ImportMarkupModalWindow> ();
		//LoadMarkup (ml.text);
		//newInspector.DrawInspector = true;
		float xPos = InspectorPosition.x + 235 > Screen.width ? InspectorPosition.x - 235 : InspectorPosition.x + 235;
		importWindow.windowRect = new Rect (xPos, InspectorPosition.y, 230, (InspectorPosition.y + 300 > Screen.height) ? Screen.height - InspectorPosition.y : 300);
		importWindow.windowTitle = "Import VoxML";
		importWindow.entityType = entityType;
		importWindow.ItemSelected += ImportVoxML;
		importWindow.Render = true;
	}

	void ImportVoxML(object sender, EventArgs e) {
		if (File.Exists (((ImportMarkupEventArgs)e).ImportPath)) {
			using (StreamReader sr = new StreamReader (((ImportMarkupEventArgs)e).ImportPath)) {
				String markup = sr.ReadToEnd ();
				if (!ObjectLoaded (markup)) {
					loadedObject = LoadMarkup (markup);
					//windowTitle = InspectorVoxeme.Substring (InspectorVoxeme.LastIndexOf ('/') + 1);
				}
			}
		}
	}

	void SaveMarkup(string markupPath, VoxEntity.EntityType entityType) {
		VoxML voxml = new VoxML ();
		voxml.Entity.Type = entityType;

		// assign VoxML values
		// PRED
		voxml.Lex.Pred = mlPred;
		voxml.Lex.Type = System.String.Join ("*", mlTypes.ToArray ());

		// TYPE
		voxml.Type.Head = (mlHeadReentrancy != string.Empty) ? mlHead + string.Format ("[{0}]", mlHeadReentrancy) : mlHead;
		for (int i = 0; i < mlComponentCount; i++) {
			voxml.Type.Components.Add (new VoxTypeComponent ());
			voxml.Type.Components [i].Value = (mlComponentReentrancies [i] != string.Empty) ? 
				mlComponents [i] + string.Format("[{0}]",mlComponentReentrancies[i]) :  mlComponents [i];
		}
		voxml.Type.Concavity = (mlConcavityReentrancy != string.Empty) ? mlConcavity + string.Format ("[{0}]", mlConcavityReentrancy) :
			mlConcavity;

		List<string> rotatSyms = new List<string> ();
		if (mlRotatSymX) {
			rotatSyms.Add ("X");
		}
		if (mlRotatSymY) {
			rotatSyms.Add ("Y");
		}
		if (mlRotatSymZ) {
			rotatSyms.Add ("Z");
		}
		voxml.Type.RotatSym = System.String.Join (",", rotatSyms.ToArray ());

		List<string> reflSyms = new List<string> ();
		if (mlReflSymXY) {
			reflSyms.Add ("XY");
		}
		if (mlReflSymXZ) {
			reflSyms.Add ("XZ");
		}
		if (mlReflSymYZ) {
			reflSyms.Add ("YZ");
		}
		voxml.Type.ReflSym = System.String.Join (",", reflSyms.ToArray ());

		// HABITAT
		for (int i = 0; i < mlIntrHabitatCount; i++) {
			voxml.Habitat.Intrinsic.Add (new VoxHabitatIntr ());
			voxml.Habitat.Intrinsic [i].Name = mlIntrHabitats [i].Split (new char[]{ '=' }) [0];
			voxml.Habitat.Intrinsic [i].Value = mlIntrHabitats [i].Split (new char[]{ '=' }) [1];
		}
		for (int i = 0; i < mlExtrHabitatCount; i++) {
			voxml.Habitat.Extrinsic.Add (new VoxHabitatExtr ());
			voxml.Habitat.Extrinsic [i].Name = mlExtrHabitats [i].Split (new char[]{ '=' }) [0];
			voxml.Habitat.Extrinsic [i].Value = mlExtrHabitats [i].Split (new char[]{ '=' }) [1];
		}

		// AFFORD_STR
		for (int i = 0; i < mlAffordanceCount; i++) {
			voxml.Afford_Str.Affordances.Add (new VoxAffordAffordance ());
			voxml.Afford_Str.Affordances [i].Formula = mlAffordances [i];
		}

		// EMBODIMENT
		voxml.Embodiment.Scale = mlScale;
		voxml.Embodiment.Movable = mlMovable;


		voxml.Save (Data.voxmlDataPath + "/" + markupPath + ".xml");
		//		voxml.SaveToServer (obj.name + ".xml");
	}

	VoxML LoadMarkup(GameObject obj) {
		VoxML voxml = new VoxML();
		
		try {
			voxml = VoxML.Load (obj.name + ".xml");

			AssignVoxMLValues(voxml);
		}
		catch (FileNotFoundException ex) {
		}

		return voxml;
	}

	VoxML LoadMarkup(VoxML v) {
		VoxML voxml = new VoxML();

		try {
			voxml = v;

			AssignVoxMLValues(voxml);
		}
		catch (FileNotFoundException ex) {
		}

		return voxml;
	}

	VoxML LoadMarkup(string text) {
		VoxML voxml = new VoxML();
		
		try {
			voxml = VoxML.LoadFromText (text);

			AssignVoxMLValues(voxml);
		}
		catch (FileNotFoundException ex) {
		}
		
		return voxml;
	}

	void AssignVoxMLValues(VoxML voxml) {
		
		// assign VoxML values
		// ENTITY
		mlEntityType = voxml.Entity.Type;

		// PRED
		mlPred = voxml.Lex.Pred;
		mlTypes = new List<string>(voxml.Lex.Type.Split (new char[]{'*'}));
		mlTypeCount = mlTypes.Count;
		mlTypeSelectVisible = new List<int>(new int[]{-1});
		mlTypeSelected = new List<int>(new int[]{-1});
		mlRemoveType = new List<int>(new int[]{-1});
		for (int i = 0; i < mlTypeCount; i++) {
			mlTypeSelectVisible.Add (-1);
			mlTypeSelected.Add (-1);
			mlRemoveType.Add (-1);
		}

		// TYPE
		mlHead = voxml.Type.Head.Split('[')[0];
		mlHeadReentrancy = voxml.Type.Head.Contains("[") ? voxml.Type.Head.Split ('[') [1].Replace("]","") : "";
		mlComponents = new List<string>();
		foreach (VoxTypeComponent c in voxml.Type.Components) {
			mlComponents.Add (c.Value.Split('[')[0]);
			mlComponentReentrancies.Add(c.Value.Contains("[") ? c.Value.Split('[') [1].Replace("]","") : "");
		}
		mlComponentCount = mlComponents.Count;
		mlConcavity = voxml.Type.Concavity.Split('[')[0];
		mlConcavityReentrancy = voxml.Type.Concavity.Contains("[") ? voxml.Type.Concavity.Split ('[') [1].Replace("]","") : "";

		List <string> rotatSyms = new List<string>(voxml.Type.RotatSym.Split (new char[]{','}));
		mlRotatSymX = (rotatSyms.Contains("X"));
		mlRotatSymY = (rotatSyms.Contains("Y"));
		mlRotatSymZ = (rotatSyms.Contains("Z"));

		List<string> reflSyms = new List<string>(voxml.Type.ReflSym.Split (new char[]{','}));
		mlReflSymXY = (reflSyms.Contains ("XY"));
		mlReflSymXZ = (reflSyms.Contains ("XZ"));
		mlReflSymYZ = (reflSyms.Contains ("YZ"));

		mlArgs = new List<string>();
		foreach (VoxTypeArg a in voxml.Type.Args) {
			mlArgs.Add (a.Value);
		}
		mlArgCount = mlArgs.Count;

		mlSubevents = new List<string>();
		foreach (VoxTypeSubevent e in voxml.Type.Body) {
			mlSubevents.Add (e.Value);
		}
		mlSubeventCount = mlSubevents.Count;

		mlClass = voxml.Type.Class;
		mlValue = voxml.Type.Value;
		mlConstr = voxml.Type.Constr;

		// HABITAT
		mlIntrHabitats = new List<string>();
		foreach (VoxHabitatIntr i in voxml.Habitat.Intrinsic) {
			mlIntrHabitats.Add (i.Name + "=" + i.Value);
		}
		mlIntrHabitatCount = mlIntrHabitats.Count;
		mlExtrHabitats = new List<string>();
		foreach (VoxHabitatExtr e in voxml.Habitat.Extrinsic) {
			mlExtrHabitats.Add (e.Name + "=" + e.Value);
		}
		mlExtrHabitatCount = mlExtrHabitats.Count;

		// AFFORD_STR
		mlAffordances = new List<string>();
		foreach (VoxAffordAffordance a in voxml.Afford_Str.Affordances) {
			mlAffordances.Add (a.Formula);
		}
		mlAffordanceCount = mlAffordances.Count;

		// EMBODIMENT
		mlScale = voxml.Embodiment.Scale;
		mlMovable = voxml.Embodiment.Movable;
	}
	
	bool ObjectLoaded(GameObject obj) {
		bool r = false;

		try {
			r = ((VoxML.Load (obj.name + ".xml")).Lex.Pred == loadedObject.Lex.Pred);
		}
		catch (FileNotFoundException ex) {
		}
		
		return r;
	}

	bool ObjectLoaded(string text) {
		bool r = false;

		try {
			r = ((VoxML.LoadFromText (text)).Lex.Pred == loadedObject.Lex.Pred);
		}
		catch (FileNotFoundException ex) {
		}

		return r;
	}
}
