using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Predicate : MonoBehaviour {

	[HideInInspector]
	public OldEntityClass entity;

	public string formula;
	public int arity;
	
	List<OldEntityClass> arguments = new List<OldEntityClass>();
	public List<OldEntityClass> Arguments
	{
		get { return arguments; }
		set {
			arguments = value;
			Debug.Log(arguments);
		}
	}

	// Use this for initialization
	public virtual void Start () {
		arity = 1;

		entity = gameObject.GetComponent<OldEntityClass>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void CalculatePredicateFormula(out string pred) {
		string temp = formula + "(";

		temp += Arguments[0].gameObject.name;

		for (int i = 1; i < Arguments.Count; i++) {
			if (Arguments[i] != null) {
				temp += ","+Arguments[i].gameObject.name;
			}
			else{
				temp += ",_";
			}
		}

		if (arity > Arguments.Count)
		{
			for (int i = Arguments.Count; i < arity; i++)
				temp += ",_";
		}

		temp += ")";

		pred = temp;
	}
}
