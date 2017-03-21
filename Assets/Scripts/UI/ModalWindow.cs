using UnityEngine;
using System.Collections;

public class ModalWindow : FontManager
{
	public Rect windowRect;
	public Vector2 scrollPosition;
	public bool isResizing = false;
	public Rect resizeStart = new Rect();
	public Vector2 minWindowSize;
	public string windowTitle;
	public bool persistent;
	public int id;

	public bool render = false;
	public virtual bool Render {
		get { return render; }
		set { render = value; }
	}

	protected ModalWindowManager windowManager;

	// Use this for initialization
	protected virtual void Start () {
		windowManager = gameObject.GetComponent<ModalWindowManager> ();

		if (!windowManager.windowManager.ContainsKey (id)) {
			windowManager.RegisterWindow (this);
		}
		else {
			Debug.Log ("ModalWindow of id " + id.ToString () + "already exists on this object!");
			Destroy(this);
		}
	}

	// Update is called once per frame
	void Update () {
	}

	protected virtual void OnGUI() {
		if (Render) {
			//GUILayout automatically lays out the GUI window to contain all the text
			windowRect = GUILayout.Window (id, windowRect, DoModalWindow, windowTitle);
			//prevents GUI window from dragging off window screen
			windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
			windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);
			//Resizing GUI window
			windowRect = ResizeWindow (windowRect, ref isResizing, ref resizeStart, minWindowSize);
		} 
	}

	public static Rect ResizeWindow (Rect windowRect, ref bool isResizing, ref Rect resizeStart, Vector2 minWindowSize){
		Vector2 mouse = GUIUtility.ScreenToGUIPoint (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y));
		if (Event.current.type == EventType.mouseDown && windowRect.Contains (mouse)) {
			isResizing = true;
			resizeStart = new Rect (mouse.x, mouse.y, windowRect.width, windowRect.height);
		}
		else if (Event.current.type == EventType.mouseUp && isResizing) {
			isResizing = false;
		}
		else if (!Input.GetMouseButton (0)) {
			isResizing = false; 
		}
		else if (isResizing) {
			windowRect.width = Mathf.Max (minWindowSize.x, resizeStart.width + (mouse.x - resizeStart.x));
			windowRect.height = Mathf.Max (minWindowSize.y, resizeStart.height + (mouse.y - resizeStart.y));
			windowRect.xMax = Mathf.Min (Screen.width, windowRect.xMax); 
			windowRect.yMax = Mathf.Min (Screen.height, windowRect.yMax);  
		}
		return windowRect;
	}

	public virtual void DoModalWindow(int windowID){
		//Debug.Log (windowID);
		if (GUI.Button (new Rect (windowRect.width - 25, 2, 23, 16), "X")) {
			if (persistent) {
				Render = false;
			}
			else {
				windowManager.UnregisterWindow (this);
				Destroy (this);
			}
		}
	}
}

