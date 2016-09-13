using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Global;

public class InputController : MonoBehaviour {
	public String inputLabel;
	public String inputString;
	public int inputHeight = 25;
	public Rect inputRect;

	String[] commands;
	EventManager eventManager;
	Macros macros;

	PluginImport commBridge;
	ObjectSelector objSelector;

	String disableEnable;

	GUIStyle textAreaStyle = new GUIStyle();

	void Start() {
		GameObject bc = GameObject.Find ("BehaviorController");
		eventManager = bc.GetComponent<EventManager> ();
		macros = bc.GetComponent<Macros> ();

		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();

		commBridge = GameObject.Find ("CommunicationsBridge").GetComponent<PluginImport> ();

		//inputRect = new Rect (5, 5, 50, 25);
		inputRect = new Rect (5, 5, 365, inputHeight);
	}

	void Update() {
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.keyCode == KeyCode.Return) {
			if (inputString != "") {
				MessageReceived (inputString);

				// warning: switching to TextArea here (and below) seems to cause crash
				GUILayout.BeginArea (inputRect);
				GUILayout.BeginHorizontal();
				GUILayout.Label(inputLabel+":");
				inputString = GUILayout.TextField("", GUILayout.Width(300), GUILayout.ExpandHeight (false));
				GUILayout.EndHorizontal ();
				GUILayout.EndArea();

				//GUI.Label (inputRect, inputLabel+":");
				//inputString = GUI.TextField (inputRect, ""); 
			}
		}
		else {

			//GUI.Label (inputRect, inputLabel+":");
			//inputString = GUI.TextField (inputRect, inputString);

			GUILayout.BeginArea (inputRect);
			GUILayout.BeginHorizontal();
			GUILayout.Label(inputLabel+":");
			inputString = GUILayout.TextField(inputString, GUILayout.Width(300), GUILayout.ExpandHeight (false));
			GUILayout.EndHorizontal ();
			GUILayout.EndArea();
		}

			/* DEBUG BUTTONS */

		if (GUI.Button (new Rect (10, Screen.height - 80, 100, 20), "Reset")) {
			StartCoroutine(SceneHelper.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name));
			return;
		}

		disableEnable = (objSelector.disabledObjects.FirstOrDefault (t => t.tag == "Agent") == null) ? "Disable Agent" : "Enable Agent";
		if (GUI.Button (new Rect (10, Screen.height - 55, 100, 20), disableEnable)) {
			GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
			if (agent != null) {
				if (agent.activeInHierarchy) {
					eventManager.preds.DISABLE (new object[]{ agent });
				}
			}
			else {
				agent = objSelector.disabledObjects.First (t => t.tag == "Agent");
				eventManager.preds.ENABLE (new object[]{ agent });
			}
			return;
		}
	}

	public void MessageReceived(String inputString) {
		Regex r = new Regex(@".*\(.*\)");
		string functionalCommand = "";

		if (inputString != "") {
			if (inputString == "reset") {
				StartCoroutine(SceneHelper.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name));
				return;
			}

			if (inputString == "repeat") {
				GameObject.Find ("BlocksWorld").GetComponent<ScenarioManager> ().scenarioScript.SendMessage("Repeat");
				return;
			}

			Debug.Log ("User entered: " + inputString);
			if (!r.IsMatch (inputString)) { // is not already functional form
				// parse into functional form
				String[] inputs = inputString.Split(new char[]{'.',',','!'});
				List<String> commands = new List<String> ();
				foreach (String s in inputs) {
					if (s != String.Empty) {
						commands.Add(commBridge.NLParse(s.Trim().ToLower()));
					}
				}
				functionalCommand = String.Join (";", commands.ToArray());
			}
			else {
				functionalCommand = inputString;
			}

			if (functionalCommand.Count (x => x == '(') == functionalCommand.Count (x => x == ')')) {
				eventManager.ClearEvents ();
				foreach (KeyValuePair<String,String> kv in macros.commandMacros) {	// if input is a macro
					if (functionalCommand == kv.Key) {									// sub in value
						functionalCommand = kv.Value;
						break;
					}
				}
				Debug.Log ("Parsed as: " + functionalCommand);
				OutputHelper.PrintOutput (OutputController.Role.Affector,"");
				OutputHelper.PrintOutput (OutputController.Role.Planner,"");
				commands = functionalCommand.Split (';');
				foreach (String commandString in commands) {
					// add to queue
					eventManager.QueueEvent (commandString);
				}

				eventManager.ExecuteNextCommand ();
			}
		}
	}
}
