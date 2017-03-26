using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;

public class Behavior : Predicate {
	[HideInInspector]
	public BehaviorType type = BehaviorType.None;

	private bool stopOnCollide;
	public bool StopOnCollide
	{
		get { return stopOnCollide; }
		set { stopOnCollide = value; }
	}

	private bool pause = true;
	public bool Pause
	{
		get { return pause; }
		set { pause = value; }
	}

	private PredicateParser controller;
	public PredicateParser Controller
	{
		get { return controller; }
		set { controller = value; }
	}

	// Use this for initialization
	public override void Start () {
		base.Start ();
		Controller = GameObject.Find ("MinSimController").GetComponent<PredicateParser>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public virtual void SetNewBehavior(List<string> args) {
		string pred;
		
		for (int i = 1; i < entity.CurrentBehavior.Arguments.Count; i++)
			entity.CurrentBehavior.Arguments [i].EndBehavior ();
		entity.EndBehavior();
		entity.InUse = true;
		entity.CurrentBehaviorTypeProperty = type;
		Arguments.Clear();
		Arguments.Add(entity);

		for (int i = 1; i < arity; i++) {
			try
			{
				OldEntityClass e = GameObject.Find(args[i]).GetComponent<OldEntityClass>();
				e.InUse = true;
				e.CurrentBehaviorTypeProperty = type;
				Arguments.Add(e);
			}
			catch (Exception)
			{
				Arguments.Add(null);
			}
		}
		
		CalculatePredicateFormula (out pred);

		Debug.Log (pred);

		if (CheckForCompleteness ())
			pause = false;

		Controller.CurrentBehaviors.Add (this);
	}

	public bool CheckForCompleteness() {
		bool complete = true;

		foreach (OldEntityClass entity in Arguments) {
			if (entity == null) {
				complete = false;
				break;
			}
		}

		return complete;
	}

	public void DisableAllCollisions(Collider[] selfColliders) {
		Collider[] otherColliders = GameObject.FindObjectsOfType<Collider>();
		foreach (Collider c1 in selfColliders) {
			foreach (Collider c2 in otherColliders) {
				if (c1 != c2)
					Physics.IgnoreCollision(c1, c2);
			}
		}
	}

	public void EnableCollisions(Collider[] colliders) {
		foreach (Collider c1 in colliders) {
			foreach (Collider c2 in colliders) {
				if (c1 != c2)
					Physics.IgnoreCollision(c1, c2, false);
			}
		}
	}

	public void EnableAllCollisions(Collider[] selfColliders) {
		Collider[] otherColliders = GameObject.FindObjectsOfType<Collider>();
		foreach (Collider c1 in selfColliders) {
			foreach (Collider c2 in otherColliders) {
				if (c1 != c2)
					Physics.IgnoreCollision(c1, c2, false);
			}
		}
	}
}
