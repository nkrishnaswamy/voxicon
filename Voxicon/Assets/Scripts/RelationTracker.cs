using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RelationTracker : MonoBehaviour {

	public Hashtable relations = new Hashtable();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// for each relation
		// assume they still hold
		// unless break condition is met
		List<object> toRemove = new List<object>();

		foreach (DictionaryEntry pair in relations)
		{
			if (!IsSatisfied((pair.Value as string),(pair.Key as List<GameObject>))) {
				toRemove.Add (pair.Key);
			}
		}

		foreach (object key in toRemove) {
			RemoveRelation (key as List<GameObject>);
		}
	}

	public void AddNewRelation (List<GameObject> objs, string relation) {
		relations.Add(objs,relation);	// add key-val pair or modify value if key already exists
	}

	public void RemoveRelation (List<GameObject> objs) {
		relations.Remove(objs);
	}

	bool IsSatisfied(string relation, List<GameObject> objs) {
		bool satisfied = true;

		if (relation == "support") {	// x support y - binary relation
			if (Vector3.Dot (objs [0].transform.up, Vector3.up) <= 0.0f) {	// --> get support axis info from habitat
				if (Vector3.Dot (objs [1].transform.up, Vector3.up) < 0.5f) {	// --> get support axis info from habitat
					// break relation
					objs [1].transform.parent = null;
					objs [1].GetComponent<Voxeme> ().enabled = true;
					objs [1].GetComponent<Voxeme> ().supportingSurface = null;
					objs [1].GetComponent<Rigging> ().ActivatePhysics (true);
					satisfied = false;
				}
			}
		}

		return satisfied;
	}
}
