using UnityEngine;
using System.Collections;

public class Help : MonoBehaviour {
	public Rect windowRect = new Rect (30,30,20,20);
	public string stringToEdit = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
	public Vector2 scrollPosition;
	public bool render = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}	

	void OnGUI () {
		if (GUI.Button (new Rect (Screen.width-50, Screen.height-30, 38, 22), "Help"))
			render = true;
//			print ("You clicked the help button!");

		if (render) {
			//GUILayout automatically lays out the GUI window to contain all the text
			windowRect = GUILayout.Window (0, windowRect, DoMyWindow, "Help");
			//prevents GUI window from dragging off window screen
			windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
			windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);
		} 
	}

	void DoMyWindow(int windowID){
		if (GUI.Button (new Rect (95, 2, 23, 16), "X"))
			render = false;
//			print ("You closed the help window");
		//makes GUI window scrollable
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, GUILayout.Width (100), GUILayout.Height (100));
		GUILayout.Label (stringToEdit);
		GUILayout.EndScrollView ();
		//makes GUI window draggable
		GUI.DragWindow (new Rect (0, 0, 10000, 20));
	}
}