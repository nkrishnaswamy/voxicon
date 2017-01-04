using UnityEngine;
using System.Collections;

using Global;

public class ExitToMenu : FontManager {

	public int fontSize = 12;

	GUIStyle buttonStyle = new GUIStyle ("Button");

	float fontSizeModifier;
	[HideInInspector]
	public float FontSizeModifier {
		get { return fontSizeModifier; }
		set { fontSizeModifier = value; }
	}

	// Use this for initialization
	void Start () {
		buttonStyle = new GUIStyle ("Button");
		FontSizeModifier = (int)(fontSize / defaultFontSize);
		buttonStyle.fontSize = fontSize;
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI () {
		if (GUI.Button (new Rect (10, Screen.height - (10 + (int)(20*fontSizeModifier)), (int)(100*fontSizeModifier), (int)(20*fontSizeModifier)), "Exit to Menu", buttonStyle)) {
			StartCoroutine(SceneHelper.LoadScene ("VoxSimMenu"));
			return;
		}
	}
}
