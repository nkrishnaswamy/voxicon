using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Global;

public class Launcher : FontManager {
	public int fontSize = 12;

	string ip;
	string inPort;
	string sriUrl;
	bool makeLogs;
	bool captureVideo;

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

	GUIStyle labelStyle = new GUIStyle ("Label");
	GUIStyle textFieldStyle = new GUIStyle ("TextField");
	GUIStyle buttonStyle = new GUIStyle ("Button");

	float fontSizeModifier;
	
	// Use this for initialization
	void Start () {
		labelStyle = new GUIStyle ("Label");
		textFieldStyle = new GUIStyle ("TextField");
		buttonStyle = new GUIStyle ("Button");
		fontSizeModifier = (int)(fontSize / defaultFontSize);
		LoadPrefs ();
		
#if UNITY_EDITOR
		string scenesDirPath = Application.dataPath + "/Scenes/";
		string [] fileEntries = Directory.GetFiles(Application.dataPath+"/Scenes/","*.unity");
		foreach (string s in fileEntries) {
			string sceneName = s.Remove(0,scenesDirPath.Length).Replace(".unity","");
			if (!sceneName.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name)) {
				availableScenes.Add(sceneName);
			}
		}
#endif 
#if UNITY_STANDALONE || UNITY_WEBPLAYER
		TextAsset scenesList = (TextAsset)Resources.Load("ScenesList", typeof(TextAsset));
		string[] scenes = scenesList.text.Split ('\n');
		foreach (string s in scenes) {
			if (s.Length > 0) {
				availableScenes.Add(s);
			}
		}
#endif

		listItems = availableScenes.ToArray ();

		// get IP address
		foreach (IPAddress ipAddress in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList) {
			if (ipAddress.AddressFamily.ToString() == "InterNetwork") {
				//Debug.Log(ipAddress.ToString());
				ip = ipAddress.ToString ();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI () {
		bgLeft = Screen.width/6;
		bgTop = Screen.height/12;
		bgWidth = 4*Screen.width/6;
		bgHeight = 10*Screen.height/12;
		
		GUI.Box (new Rect (bgLeft, bgTop, bgWidth, bgHeight), "");
		
		GUI.Label (new Rect (bgLeft + 10, bgTop + 35, 90*fontSizeModifier, 25*fontSizeModifier), "Listener Port");
		inPort = GUI.TextField (new Rect (bgLeft+100, bgTop+35, 60, 25*fontSizeModifier), inPort);

		GUI.Button (new Rect (bgLeft + 165, bgTop + 35, 10, 10), new GUIContent ("*", "IP: " + ip));
		if (GUI.tooltip != string.Empty) {
			GUI.TextArea (new Rect (bgLeft + 175, bgTop + 35, GUI.skin.label.CalcSize (new GUIContent ("IP: "+ip)).x+10, 20), GUI.tooltip);
		}

		GUI.Label (new Rect (bgLeft + 10, bgTop + 65, 90*fontSizeModifier, 25*fontSizeModifier), "SRI URL");
		sriUrl = GUI.TextField (new Rect (bgLeft+100, bgTop+65, 150, 25*fontSizeModifier), sriUrl);

		GUI.Label (new Rect (bgLeft + 10, bgTop + 95, 90*fontSizeModifier, 25*fontSizeModifier), "Make Logs");
		makeLogs = GUI.Toggle (new Rect (bgLeft+100, bgTop+95, 150, 25*fontSizeModifier), makeLogs, string.Empty);

		GUI.Label (new Rect (bgLeft + 10, bgTop + 125, 90*fontSizeModifier, 25*fontSizeModifier), "Capture Video");
		captureVideo = GUI.Toggle (new Rect (bgLeft+100, bgTop+125, 150, 25*fontSizeModifier), captureVideo, string.Empty);

		if (captureVideo) {
			string warningText = "Enabling this option may affect performance";
			GUI.TextArea (new Rect (bgLeft + 10, bgTop + 145, GUI.skin.label.CalcSize (new GUIContent (warningText)).x+10, 20),
				warningText);
		}

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
				StartCoroutine(SceneHelper.LoadScene (sceneSelected));
			}
		}

		textDimensions = GUI.skin.label.CalcSize (new GUIContent ("VoxSim"));
		GUI.Label (new Rect (((2 * bgLeft + bgWidth) / 2) - textDimensions.x / 2, bgTop, textDimensions.x, 25), "VoxSim");
	}
	
	void LoadPrefs() {
		inPort = PlayerPrefs.GetString("Listener Port");
		sriUrl = PlayerPrefs.GetString("SRI URL");
		makeLogs = (PlayerPrefs.GetInt("Make Logs") == 1);
		captureVideo = (PlayerPrefs.GetInt("Capture Video") == 1);
	}
	
	void SavePrefs() {
		PlayerPrefs.SetString("Listener Port", inPort);
		PlayerPrefs.SetString("SRI URL", sriUrl);
		PlayerPrefs.SetInt("Make Logs", System.Convert.ToInt32(makeLogs));
		PlayerPrefs.SetInt("Capture Video", System.Convert.ToInt32(captureVideo));
	}
}

