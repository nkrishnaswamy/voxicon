using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Global;

public class VoxemeInspector : MonoBehaviour {
	public int inspectorWidth = 200;
	public int inspectorHeight = 300;
	public int inspectorMargin = 150;

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
	
	GUIStyle listStyle = new GUIStyle ();
	Texture2D tex;
	Color[] colors;
	
	// Markup vars
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
	
	int mlAddComponent = -1;
	List<int> mlRemoveComponent = new List<int>();
	int mlComponentCount = 0;
	List<string> mlComponents = new List<string>();
	
	string[] mlConcavityOptions = new string[]{"Concave","Flat","Convex"};
	int mlConcavitySelectVisible = -1;
	int mlConcavitySelected = -1;
	string mlConcavity = "";
	
	bool mlRotatSymX = false;
	bool mlRotatSymY = false;
	bool mlRotatSymZ = false;
	bool mlReflSymXY = false;
	bool mlReflSymXZ = false;
	bool mlReflSymYZ = false;
	
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
	
	// Use this for initialization
	void Start () {
		colors = new Color[]{Color.white,Color.white,Color.white,Color.white};
		tex = new Texture2D (2, 2);
		
		// Make a GUIStyle that has a solid white hover/onHover background to indicate highlighted items
		listStyle.normal.textColor = Color.white;
		tex.SetPixels(colors);
		tex.Apply();
		listStyle.hover.background = tex;
		listStyle.onHover.background = tex;
		listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void OnGUI () {
		if (DrawInspector) {
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
			}

			GUILayout.BeginArea (inspectorRect, GUI.skin.window);
			scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);

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
					loadedObject = new Voxeme ();
				}
			}
#endif
#if UNITY_WEBPLAYER*/
			// Resources load here
			TextAsset markup = Resources.Load (inspectorObject.name) as TextAsset;
			if (markup != null) {
				if (!ObjectLoaded (markup.text)) {
					loadedObject = LoadMarkup (markup.text);
					markupCleared = false;
				}
			}
			else {
				if (!markupCleared) {
					InitNewMarkup ();
					loadedObject = new VoxML ();
				}
			}
//#endif
			
			inspectorStyle = GUI.skin.box;
			inspectorStyle.wordWrap = true;
			inspectorStyle.alignment = TextAnchor.MiddleLeft;
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("LEX");
			GUILayout.BeginHorizontal (inspectorStyle);
			GUILayout.Label ("Pred");
			GUILayout.Box (mlPred, GUILayout.Width (100));
			GUILayout.EndHorizontal ();
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Type");
			
			GUILayout.BeginVertical ();
			for (int i = 0; i < mlTypeCount; i++) {
				GUILayout.BeginHorizontal ();

				GUILayout.Box (mlTypes [i], GUILayout.Width (70), GUILayout.ExpandWidth (true));

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

			GUILayout.Box (mlHead, GUILayout.Width (70), GUILayout.ExpandWidth (true));
			
			GUILayout.EndHorizontal ();
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Components");
			GUILayout.EndHorizontal ();
			
			for (int i = 0; i < mlComponentCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlComponents [i], GUILayout.Width (115));
				GUILayout.EndHorizontal ();
			}

			GUILayout.EndVertical ();
			
			GUILayout.BeginHorizontal (inspectorStyle);
			GUILayout.Label ("Concavity");

			GUILayout.Box (mlConcavity, GUILayout.Width (70), GUILayout.ExpandWidth (true));

			GUILayout.EndHorizontal ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("Rotational Symmetry");
			GUILayout.BeginHorizontal ();
			GUILayout.Toggle (mlRotatSymX, "X");
			GUILayout.Toggle (mlRotatSymY, "Y");
			GUILayout.Toggle (mlRotatSymZ, "Z");
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("Reflectional Symmetry");
			GUILayout.BeginHorizontal ();
			GUILayout.Toggle (mlReflSymXY, "XY");
			GUILayout.Toggle (mlReflSymXZ, "XZ");
			GUILayout.Toggle (mlReflSymYZ, "YZ");
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("HABITAT");
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Intrinsic");
			
			GUILayout.EndHorizontal ();
			
			for (int i = 0; i < mlIntrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlIntrHabitats [i].Split (new char[]{'='}) [0], GUILayout.Width (50));
				GUILayout.Box (mlIntrHabitats [i].Split (new char[]{'='}) [1], GUILayout.Width (60));
				GUILayout.EndHorizontal ();
			}
			
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Extrinsic");
			
			GUILayout.EndHorizontal ();
			
			for (int i = 0; i < mlExtrHabitatCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlExtrHabitats [i].Split (new char[]{'='}) [0], GUILayout.Width (50));
				GUILayout.Box (mlExtrHabitats [i].Split (new char[]{'='}) [1], GUILayout.Width (60));
				GUILayout.EndHorizontal ();
			}

			GUILayout.EndVertical ();
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("AFFORD_STR");
			
			GUILayout.BeginVertical (inspectorStyle);
			
			for (int i = 0; i < mlAffordanceCount; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (mlAffordances [i], GUILayout.Width (115));
				GUILayout.EndHorizontal ();
			}
			
			GUILayout.EndVertical ();
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (inspectorStyle);
			GUILayout.Label ("EMBODIMENT");
			GUILayout.BeginHorizontal (inspectorStyle);
			GUILayout.Label ("Scale");

			GUILayout.Box (mlScale, GUILayout.Width (70), GUILayout.ExpandWidth (true));

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal (inspectorStyle);
			GUILayout.Label ("Movable");
			GUILayout.Toggle (mlMovable, "");
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();

			GUILayout.EndScrollView ();
			GUILayout.EndArea ();

			Vector2 textDimensions = GUI.skin.label.CalcSize (new GUIContent (inspectorObject.name));
			GUI.Label (new Rect (((2 * inspectorPositionAdjX + inspectorWidth) / 2) - textDimensions.x / 2, inspectorPositionAdjY, textDimensions.x, 25), inspectorObject.name);
		}
	}
	
	void InitNewMarkup() {
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
	
	VoxML LoadMarkup(GameObject obj) {
		VoxML voxml = new VoxML();
		
		try {
			voxml = VoxML.Load (obj.name + ".xml");
			
			// assign VoxML values
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
			mlHead = voxml.Type.Head;
			mlComponents = new List<string>();
			foreach (Component c in voxml.Type.Components) {
				mlComponents.Add (c.Value);
			}
			mlComponentCount = mlComponents.Count;
			mlConcavity = voxml.Type.Concavity;
			
			List <string> rotatSyms = new List<string>(voxml.Type.RotatSym.Split (new char[]{','}));
			mlRotatSymX = (rotatSyms.Contains("X"));
			mlRotatSymY = (rotatSyms.Contains("Y"));
			mlRotatSymZ = (rotatSyms.Contains("Z"));
			
			List<string> reflSyms = new List<string>(voxml.Type.ReflSym.Split (new char[]{','}));
			mlReflSymXY = (reflSyms.Contains ("XY"));
			mlReflSymXZ = (reflSyms.Contains ("XZ"));
			mlReflSymYZ = (reflSyms.Contains ("YZ"));
			
			// HABITAT
			mlIntrHabitats = new List<string>();
			foreach (Intr i in voxml.Habitat.Intrinsic) {
				mlIntrHabitats.Add (i.Name + "=" + i.Value);
			}
			mlIntrHabitatCount = mlIntrHabitats.Count;
			mlExtrHabitats = new List<string>();
			foreach (Extr e in voxml.Habitat.Extrinsic) {
				mlExtrHabitats.Add (e.Name + "=" + e.Value);
			}
			mlExtrHabitatCount = mlExtrHabitats.Count;
			
			// AFFORD_STR
			mlAffordances = new List<string>();
			foreach (Affordance a in voxml.Afford_Str.Affordances) {
				mlAffordances.Add (a.Formula);
			}
			mlAffordanceCount = mlAffordances.Count;
			
			// EMBODIMENT
			mlScale = voxml.Embodiment.Scale;
			mlMovable = voxml.Embodiment.Movable;
		}
		catch (FileNotFoundException ex) {
		}
		
		return voxml;
	}

	VoxML LoadMarkup(string text) {
		VoxML voxml = new VoxML();
		
		try {
			voxml = VoxML.LoadFromText (text);
			
			// assign VoxML values
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
			mlHead = voxml.Type.Head;
			mlComponents = new List<string>();
			foreach (Component c in voxml.Type.Components) {
				mlComponents.Add (c.Value);
			}
			mlComponentCount = mlComponents.Count;
			mlConcavity = voxml.Type.Concavity;
			
			List <string> rotatSyms = new List<string>(voxml.Type.RotatSym.Split (new char[]{','}));
			mlRotatSymX = (rotatSyms.Contains("X"));
			mlRotatSymY = (rotatSyms.Contains("Y"));
			mlRotatSymZ = (rotatSyms.Contains("Z"));
			
			List<string> reflSyms = new List<string>(voxml.Type.ReflSym.Split (new char[]{','}));
			mlReflSymXY = (reflSyms.Contains ("XY"));
			mlReflSymXZ = (reflSyms.Contains ("XZ"));
			mlReflSymYZ = (reflSyms.Contains ("YZ"));
			
			// HABITAT
			mlIntrHabitats = new List<string>();
			foreach (Intr i in voxml.Habitat.Intrinsic) {
				mlIntrHabitats.Add (i.Name + "=" + i.Value);
			}
			mlIntrHabitatCount = mlIntrHabitats.Count;
			mlExtrHabitats = new List<string>();
			foreach (Extr e in voxml.Habitat.Extrinsic) {
				mlExtrHabitats.Add (e.Name + "=" + e.Value);
			}
			mlExtrHabitatCount = mlExtrHabitats.Count;
			
			// AFFORD_STR
			mlAffordances = new List<string>();
			foreach (Affordance a in voxml.Afford_Str.Affordances) {
				mlAffordances.Add (a.Formula);
			}
			mlAffordanceCount = mlAffordances.Count;
			
			// EMBODIMENT
			mlScale = voxml.Embodiment.Scale;
			mlMovable = voxml.Embodiment.Movable;
		}
		catch (FileNotFoundException ex) {
		}
		
		return voxml;
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
