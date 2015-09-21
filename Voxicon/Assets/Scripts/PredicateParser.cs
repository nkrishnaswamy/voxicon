using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BehaviorTypes;

public class PredicateParser : MonoBehaviour {
	public bool DynamicIntroduction = false;
	public bool ShowTrails = false;

	public string inputString;
	public string InputStringProperty
	{
		get { return inputString; }
		set {
			inputString = value.Replace(" ",string.Empty);
			string[] inputs = inputString.Split (';');

			foreach (string input in inputs)
			{
				PromptBehavior(input);
			}
		}
	}

	object[] trails;

	private List<Behavior> currentBehaviors;
	public List<Behavior> CurrentBehaviors
	{
		get { return currentBehaviors; }
		set { currentBehaviors = value; }
	}

	// Use this for initialization
	void Start() {
		CurrentBehaviors = new List<Behavior> ();
		trails = Resources.FindObjectsOfTypeAll(typeof(TrailRenderer));
	}
	
	// Update is called once per frame
	void Update() {
		foreach (object trail in trails)
			((TrailRenderer)trail).enabled = ShowTrails;
	}

	void PromptBehavior(string inputString) {

		string pred = inputString.Split('(')[0];
		List<string> args = (inputString.Split('(')[1].Split(')')[0].Split(',')).OfType<string>().ToList();

		OldEntityClass entity = GameObject.Find(args[0]).GetComponent<OldEntityClass>();

		/*if (DynamicIntroduction) {
			object[] visibleObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (object obj in visibleObjects) {
				if (((GameObject)obj).activeInHierarchy)
				{
					Entity e = ((GameObject)obj).GetComponent<Entity>();
					MeshRenderer renderer = ((GameObject)obj).GetComponent<MeshRenderer>();
					if (renderer != null)
					{
						if (e != null)
						{
							if (!e.InUse)
								renderer.enabled = false;
						}
						else
							renderer.enabled = false;
					}
				}
			}

			foreach (string arg in args)
				GameObject.Find(arg).GetComponent<MeshRenderer>().enabled = true;
		}*/

		entity.PromptBehavior(pred, args);
	}
}
