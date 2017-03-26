﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CupsGame : DemoScript {
	RelationTracker relationTracker;
	EventManager eventManager;

	bool gameOver = false;

	// Use this for initialization
	void Start () {
		relationTracker = GameObject.Find ("BehaviorController").GetComponent<RelationTracker> ();
		eventManager = GameObject.Find ("BehaviorController").GetComponent<EventManager> ();

		eventManager.EventComplete += ChoiceMade;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void ChoiceMade(object sender, EventArgs e) {
		if (gameOver) {
			return;
		}

		string predicate = ((EventManagerArgs)e).EventString.Split ('(') [0];
		string argument = ((EventManagerArgs)e).EventString.Split ('(') [1].Split(',')[0];
		if (predicate == "lift") {
			bool winCondition = false;
			foreach (DictionaryEntry pair in relationTracker.relations) {
				List<GameObject> objs = (pair.Key as List<GameObject>);
				if ((objs [0] == GameObject.Find ("ball")) && (objs [1] == GameObject.Find (argument))) {
					string rel = (pair.Value as string);
					if (rel.Contains ("under")) {
						winCondition = true;
					}
				}
			}

			if (winCondition) {
				OutputHelper.PrintOutput (OutputController.Role.Planner, "You win!");
				gameOver = true;
			}
			else {
				OutputHelper.PrintOutput (OutputController.Role.Planner, "Ha!  I win!");
				foreach (DictionaryEntry pair in relationTracker.relations) {
					List<GameObject> objs = (pair.Key as List<GameObject>);
					if (objs [0] == GameObject.Find ("ball")) {
						string rel = (pair.Value as string);
						if (rel.Contains ("under")) {
							eventManager.InsertEvent(string.Format("lift({0})",objs [1].name),0);
							eventManager.ExecuteNextCommand ();
						}
					}
				}
				gameOver = true;
			}
		}
	}
}
