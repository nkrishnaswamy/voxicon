﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Global;

public class ScriptSelector : Reset {
	List<string> availableScripts = new List<string>();

	bool chooseScene;
	public bool ChooseScene {
		get { return chooseScene; }
		set {
			chooseScene = value;
			if (!chooseScene) {
				rect = new Rect(0,0,0,0);
				scrollPosition = new Vector2(0,0);
				choice = -1;
			}
		}
	}

	Vector2 scrollPosition;
	public Vector2 ScrollPosition {
		get { return scrollPosition; }
		set { scrollPosition = value; }
	}

	Rect rect = new Rect(0,0,0,0);
	public Rect Rect {
		get { return rect; }
		set { rect = value; }
	}

	int choice = -1;
	public int Choice {
		get { return choice; }
		set { choice = value; }
	}
		
	ScenarioManager scenarioManager;

	// Use this for initialization
	void Start () {
		base.Start ();

		scenarioManager = GameObject.Find ("BlocksWorld").GetComponent<ScenarioManager> ();

		List<GameObject> scripts = new List<GameObject> ();
		foreach (Transform child in transform) {
			scripts.Add (child.gameObject);
			availableScripts.Add (child.gameObject.name);
		}

		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}

		GameObject go = transform.Find (availableScripts [0]).gameObject;
		go.SetActive (true);
		scenarioManager.scenarioScript = go;
	}

	// Update is called once per frame
	void Update () {
	}

	protected override void OnGUI () {
		if (GUI.Button (new Rect (10, Screen.height - ((10 + (int)(20 * exitToMenu.FontSizeModifier)) + (5 + (int)(20 * FontSizeModifier))),
			    100 * FontSizeModifier, 20 * FontSizeModifier), "Load...", buttonStyle)) {
			ChooseScene = true;
		}

		if (ChooseScene) {
			float width = 0;
			foreach (string sceneName in availableScripts) {
				if (buttonStyle.CalcSize (new GUIContent (sceneName)).x > width) {
					width = buttonStyle.CalcSize (new GUIContent (sceneName)).x;
				}
			}

			rect = new Rect (120*FontSizeModifier,Screen.height - ((10 + (int)(20 * exitToMenu.FontSizeModifier)) + (20 + (int)(80 * FontSizeModifier))),
				width+60,120 * FontSizeModifier);
			GUILayout.BeginArea (rect, GUI.skin.window);
			scrollPosition = GUILayout.BeginScrollView (scrollPosition, false, false);
			GUILayout.BeginVertical (GUI.skin.box);
			choice = GUILayout.SelectionGrid (choice, availableScripts.ToArray(), 1, buttonStyle, GUILayout.ExpandWidth (true));
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();

			if (choice > -1) {
				foreach (Transform child in transform) {
					child.gameObject.SetActive (false);
				}

				GameObject go = transform.Find (availableScripts [choice]).gameObject;
				go.SetActive (true);
				scenarioManager.scenarioScript = go;
				ChooseScene = false;
				return;
			}
		}
	}
}
