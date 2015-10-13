using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Launcher : MonoBehaviour {
	
	string inPort;
	
	int bgLeft = Screen.width/6;
	int bgTop = Screen.height/12;
	int bgWidth = 4*Screen.width/6;
	int bgHeight = 10*Screen.height/12;
	
	Vector2 scrollPosition;
	
	string[] listItems;
	
	List<string> availableScenes = new List<string>();
	
	int selected = -1;
	string sceneSelected = "";
	
	object[] scenes;
	
	GUIStyle customStyle;
	
	// Use this for initialization
	void Start () {
		LoadPrefs ();
		
#if UNITY_EDITOR
		string scenesDirPath = Application.dataPath + "/Scenes/";
		string [] fileEntries = Directory.GetFiles(Application.dataPath+"/Scenes/");
		foreach (string s in fileEntries) {
			if (!s.EndsWith(".meta")) {
				string sceneName = s.Remove(0,scenesDirPath.Length).Replace(".unity","");
				if (!sceneName.Equals("VoxiconMenu")) {
					availableScenes.Add(sceneName);
				}
			}
		}
#else
#endif
		listItems = availableScenes.ToArray ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI () {
		bgLeft = Screen.width/6;
		bgTop = Screen.height/12;
		bgWidth = 4*Screen.width/6;
		bgHeight = 10*Screen.height/12;
		
		GUI.Box (new Rect (bgLeft, bgTop, bgWidth, bgHeight), "Voxicon");
		
		GUI.Label (new Rect (bgLeft + 10, bgTop + 35, 90, 25), "Listener Port");
		inPort = GUI.TextField (new Rect (bgLeft+100, bgTop+35, 60, 25), inPort);
		
		GUILayout.BeginArea(new Rect(13*Screen.width/24, bgTop + 35, 3*Screen.width/12, 3*Screen.height/6), GUI.skin.window);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false); 
		GUILayout.BeginVertical(GUI.skin.box);
		
		customStyle = GUI.skin.button;
		//customStyle.active.background = Texture2D.whiteTexture;
		//customStyle.onActive.background = Texture2D.whiteTexture;
		//customStyle.active.textColor = Color.black;
		//customStyle.onActive.textColor = Color.black;
		
		selected = GUILayout.SelectionGrid(selected, listItems, 1, customStyle, GUILayout.ExpandWidth(true));
		
		if (selected >= 0) {
			sceneSelected = listItems [selected];
		}
		
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		
		Vector2 textDimensions = GUI.skin.label.CalcSize(new GUIContent("Scenes"));
		
		GUI.Label (new Rect (2*Screen.width/3 - textDimensions.x/2, bgTop + 35, textDimensions.x, 25), "Scenes");
		
		if (GUI.Button (new Rect (Screen.width / 2 - 50, bgTop + bgHeight - 60, 100, 50), "Launch")) {
			if (sceneSelected != "") {
				SavePrefs ();
				Application.LoadLevel(sceneSelected);
			}
		}
	}
	
	void LoadPrefs() {
		inPort = PlayerPrefs.GetString("Listener Port");
	}
	
	void SavePrefs() {
		PlayerPrefs.SetString("Listener Port", inPort);
	}
}

