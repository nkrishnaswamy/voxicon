using UnityEngine;
using System.Collections;

public class Help : MonoBehaviour {
	public Rect windowRect = new Rect(0, 0, 120, 20);
	public string stringToEdit = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
	public Vector2 scrollPosition;
	public bool render = false;
	bool isResizing = false;
	Rect resizeStart = new Rect();
	Vector2 minWindowSize = new Vector2(200,100);

	// Use this for initialization
	void Start () {
		windowRect = new Rect(50, 50, 200, 100);
	}
	
	// Update is called once per frame
	void Update () {

	}	

	void OnGUI () {
		if (GUI.Button (new Rect (Screen.width-50, Screen.height-30, 38, 22), "Help"))
			render = true;
	
		if (render) {
			//GUILayout automatically lays out the GUI window to contain all the text
			windowRect = GUILayout.Window (0, windowRect, DoMyWindow, "Help");
			//prevents GUI window from dragging off window screen
			windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
			windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);
			//Resizing GUI window
			windowRect = ResizeWindow (windowRect, ref isResizing, ref resizeStart, minWindowSize);
		} 
	}

	void DoMyWindow(int windowID){
		if (GUI.Button (new Rect (windowRect.width-25, 2, 23, 16), "X"))
			render = false;
		//makes GUI window scrollable
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		GUILayout.Label (stringToEdit);
		GUILayout.EndScrollView ();
		//makes GUI window draggable
		GUI.DragWindow (new Rect (0, 0, 10000, 20));
	}

	public static Rect ResizeWindow (Rect windowRect, ref bool isResizing, ref Rect resizeStart, Vector2 minWindowSize){
		Vector2 mouse = GUIUtility.ScreenToGUIPoint (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y));
		if (Event.current.type == EventType.mouseDown && windowRect.Contains (mouse)) {
			isResizing = true;
			resizeStart = new Rect (mouse.x, mouse.y, windowRect.width, windowRect.height);
		} else if (Event.current.type == EventType.mouseUp && isResizing) {
			isResizing = false;
		} else if (!Input.GetMouseButton (0)) {
			isResizing = false; 
		} else if (isResizing) {
			windowRect.width = Mathf.Max (minWindowSize.x, resizeStart.width + (mouse.x - resizeStart.x));
			windowRect.height = Mathf.Max (minWindowSize.y, resizeStart.height + (mouse.y - resizeStart.y));
			windowRect.xMax = Mathf.Min (Screen.width, windowRect.xMax); 
			windowRect.yMax = Mathf.Min (Screen.height, windowRect.yMax);  
		}
		return windowRect;
	}
}