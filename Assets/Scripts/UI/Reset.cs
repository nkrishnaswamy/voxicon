using UnityEngine;
using System.Collections;

using Global;

public class Reset : FontManager {

	public int fontSize = 12;

	protected GUIStyle buttonStyle = new GUIStyle ("Button");

	protected ExitToMenu exitToMenu;

	float fontSizeModifier;	
	public float FontSizeModifier {
		get { return fontSizeModifier; }
		set { fontSizeModifier = value; }
	}

	// Use this for initialization
	protected void Start () {
		exitToMenu = GameObject.Find ("BlocksWorld").GetComponent<ExitToMenu> ();

		buttonStyle = new GUIStyle ("Button");

		fontSizeModifier = (int)(fontSize / defaultFontSize);
		buttonStyle.fontSize = fontSize;
	}

	// Update is called once per frame
	protected void Update () {
	}	

	protected virtual void OnGUI () {
		if (GUI.Button (new Rect (10, Screen.height - ((10 + (int)(20*exitToMenu.FontSizeModifier)) + (5 + (int)(20*fontSizeModifier))),
			100*fontSizeModifier, 20*fontSizeModifier), "Reset", buttonStyle)) {
			StartCoroutine(SceneHelper.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name));
			return;
		}
	}
}
