using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputController : MonoBehaviour {
	public String inputString;
	String[] commands;
	EventManager eventManager;
	Macros macros;

	public Rect inputRect;

	void Start() {
		GameObject bc = GameObject.Find ("BehaviorController");
		eventManager = bc.GetComponent<EventManager> ();
		macros = bc.GetComponent<Macros> ();

		inputRect = new Rect (5, 5, 50, 25);
	}

	void Update() {
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.keyCode == KeyCode.Return) {
			if (inputString != "") {
				MessageReceived (inputString);
//				if (inputString == "reset") {
//					UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name);
//					return;
//				}
//
//				if (inputString.Count (x => x == '(') == inputString.Count (x => x == ')')) {
//					eventManager.ClearEvents ();
//					foreach (KeyValuePair<String,String> kv in macros.commandMacros) {	// if input is a macro
//						if (inputString == kv.Key) {									// sub in value
//							inputString = kv.Value;
//							break;
//						}
//					}
//					Debug.Log ("User entered: " + inputString);
//					OutputHelper.PrintOutput("");
//					commands = inputString.Split (';');
//					foreach (String commandString in commands) {
//						// add to queue
//						eventManager.QueueEvent (commandString);
//					}
//
//					eventManager.ExecuteNextCommand ();
//				}
				GUI.Label (inputRect, "Human:");
				inputString = GUI.TextField (inputRect, ""); 
			}
		}
		else {
			GUI.Label (new Rect (5, 5, 50, 25), "Human:");
			inputString = GUI.TextField (new Rect (55, 5, 300, 25), inputString);
		}

			/* DEBUG BUTTONS */

		if (GUI.Button (new Rect (10, Screen.height - 55, 100, 20), "Reset")) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name);
			return;
		}

		if (GUI.Button (new Rect (10, Screen.height - 30, 100, 20), "Disable Agent")) {
			eventManager.preds.DISABLE(new object[]{GameObject.Find("human1")});
			return;
		}
	}

	public void MessageReceived(String inputString) {
		if (inputString != "") {
			if (inputString == "reset") {
				UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name);
				return;
			}

			if (inputString.Count (x => x == '(') == inputString.Count (x => x == ')')) {
				eventManager.ClearEvents ();
				foreach (KeyValuePair<String,String> kv in macros.commandMacros) {	// if input is a macro
					if (inputString == kv.Key) {									// sub in value
						inputString = kv.Value;
						break;
					}
				}
				Debug.Log ("User entered: " + inputString);
				OutputHelper.PrintOutput ("");
				commands = inputString.Split (';');
				foreach (String commandString in commands) {
					// add to queue
					eventManager.QueueEvent (commandString);
				}

				eventManager.ExecuteNextCommand ();
			}
		}
	}
}
