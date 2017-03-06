using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Global;
using VideoCapture;

public class Launcher : FontManager {
	public int fontSize = 12;

	string ip;
	string parserUrl;
	string inPort;
	string sriUrl;
	bool makeLogs;
	bool captureVideo;
	VideoCaptureMode videoCaptureMode;
	bool resetScene;
	VideoCaptureFilenameType prevVideoCaptureFilenameType;
	VideoCaptureFilenameType videoCaptureFilenameType;
	string customVideoFilenamePrefix;
	string videoCaptureDB;

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

		GUI.Label (new Rect (bgLeft + 10, bgTop + 125, 90*fontSizeModifier, 40*fontSizeModifier), "Parser URL");
		parserUrl = GUI.TextField (new Rect (bgLeft+100, bgTop+125, 150, 25*fontSizeModifier), parserUrl);
		GUI.Label (new Rect (bgLeft + 10, bgTop + 150, 300, 50), "(Leave empty to use simple regex parser)");

		GUI.Label (new Rect (bgLeft + 10, bgTop + 180, 90*fontSizeModifier, 25*fontSizeModifier), "Capture Video");
		captureVideo = GUI.Toggle (new Rect (bgLeft+100, bgTop+180, 150, 25*fontSizeModifier), captureVideo, string.Empty);

		if (captureVideo) {
			string warningText = "Enabling this option may affect performance";
			GUI.TextArea (new Rect (bgLeft + 10, bgTop + 205, GUI.skin.label.CalcSize (new GUIContent (warningText)).x+10, 20*fontSizeModifier),
				warningText);

			GUI.Label (new Rect (bgLeft + 15, bgTop + 230, GUI.skin.label.CalcSize (new GUIContent ("Video Capture Mode")).x+10, 20*fontSizeModifier),
				"Video Capture Mode");

			string[] videoCaptureModeLabels = new string[]{ "Manual", "Full-Time", "Per Event" };
			videoCaptureMode = (VideoCaptureMode)GUI.SelectionGrid (
				new Rect (bgLeft + 15, bgTop + 250, 150, 20*fontSizeModifier*videoCaptureModeLabels.Length),
				(int)videoCaptureMode, videoCaptureModeLabels, 1, "toggle");

			if (videoCaptureMode == VideoCaptureMode.PerEvent) {
				GUI.Label (new Rect (bgLeft + 40, bgTop + 310, 120*fontSizeModifier, 40*fontSizeModifier), "Reset Scene Between Events");
				resetScene = GUI.Toggle (new Rect (bgLeft + 25, bgTop + 317, 150, 25 * fontSizeModifier), resetScene, string.Empty);
			}

			GUI.Label (new Rect (bgLeft + 15 + 150*fontSizeModifier, bgTop + 230, GUI.skin.label.CalcSize (new GUIContent ("Capture Filename Type")).x+10, 20*fontSizeModifier),
				"Capture Filename Type");

			prevVideoCaptureFilenameType = videoCaptureFilenameType;
			string[] videoCaptureFilenameTypeLabels = new string[]{ "Flashback Default", "Event String", "Custom" };
			videoCaptureFilenameType = (VideoCaptureFilenameType)GUI.SelectionGrid (
				new Rect (bgLeft + 15 + 150*fontSizeModifier, bgTop + 250, 150, 20*fontSizeModifier*videoCaptureFilenameTypeLabels.Length),
				(int)videoCaptureFilenameType, videoCaptureFilenameTypeLabels, 1, "toggle");

			// EventString can only be used with PerEvent
			if (videoCaptureMode != VideoCaptureMode.PerEvent) {
				if (videoCaptureFilenameType == VideoCaptureFilenameType.EventString) {
					videoCaptureFilenameType = prevVideoCaptureFilenameType;
				}
			}

			if (videoCaptureFilenameType == VideoCaptureFilenameType.Custom) {
				customVideoFilenamePrefix = GUI.TextArea (new Rect (bgLeft + 15 + 160*fontSizeModifier, bgTop + 315, 150, 25*fontSizeModifier),
					customVideoFilenamePrefix);
			}

			GUI.Label (new Rect (bgLeft + 15, bgTop + 355, 120*fontSizeModifier, 25*fontSizeModifier), "Video Database File");
			videoCaptureDB = GUI.TextField (new Rect (bgLeft+140*fontSizeModifier, bgTop+355, 150, 25*fontSizeModifier), videoCaptureDB);
			GUI.Label (new Rect (bgLeft + 15, bgTop + 380, 300, 50), "(Leave empty to omit videos from database)");
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
		parserUrl = PlayerPrefs.GetString("Parser URL");
		makeLogs = (PlayerPrefs.GetInt("Make Logs") == 1);
		captureVideo = (PlayerPrefs.GetInt("Capture Video") == 1);
		videoCaptureMode = (VideoCaptureMode)PlayerPrefs.GetInt("Video Capture Mode");
		resetScene = (PlayerPrefs.GetInt("Reset Between Events") == 1);
		videoCaptureFilenameType = (VideoCaptureFilenameType)PlayerPrefs.GetInt("Video Capture Filename Type");
		customVideoFilenamePrefix = PlayerPrefs.GetString("Custom Video Filename Prefix");
		videoCaptureDB = PlayerPrefs.GetString("Video Capture DB");
	}
	
	void SavePrefs() {
		PlayerPrefs.SetString("Listener Port", inPort);
		PlayerPrefs.SetString("SRI URL", sriUrl);
		PlayerPrefs.GetString("Parser URL", parserUrl);
		PlayerPrefs.SetInt("Make Logs", System.Convert.ToInt32(makeLogs));
		PlayerPrefs.SetInt("Capture Video", System.Convert.ToInt32(captureVideo));
		PlayerPrefs.SetInt("Video Capture Mode", System.Convert.ToInt32(videoCaptureMode));
		PlayerPrefs.SetInt("Reset Between Events", System.Convert.ToInt32(resetScene));
		PlayerPrefs.SetInt("Video Capture Filename Type", System.Convert.ToInt32(videoCaptureFilenameType));
		PlayerPrefs.SetString("Custom Video Filename Prefix", customVideoFilenamePrefix);
		PlayerPrefs.SetString("Video Capture DB", videoCaptureDB);
	}
}

