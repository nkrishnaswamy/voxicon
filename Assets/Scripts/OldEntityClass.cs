using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;
using SurfaceContactTypes;

public class OldEntityClass : MonoBehaviour {

	private Behavior previousBehavior = null;

	[HideInInspector]
	private Behavior currentBehavior = null;
	public Behavior CurrentBehavior
	{
		get { return currentBehavior; }
		set {
			if (currentBehavior != value){
				previousBehavior = currentBehavior;
				currentBehavior = value;
			}
		}
	}

	[HideInInspector]
	public BehaviorType currentBehaviorType = BehaviorType.None;
	public BehaviorType CurrentBehaviorTypeProperty
	{
		get { return currentBehaviorType; }
		set {
			currentBehaviorType = value;
			if (GetBehaviorByType(currentBehaviorType) != null) {
				CurrentBehavior = GetBehaviorByType(currentBehaviorType);
			}
		}
	}

	[HideInInspector]
	private SurfaceContactType contactType = SurfaceContactType.None;
	public SurfaceContactType ContactType
	{
		get { return contactType; }
		set {
			if (contactType != value){
				contactType = value;
			}
		}
	}

	PredicateParser controller;
	MeshRenderer renderer;

	private bool inUse;
	public bool InUse
	{
		get { return inUse; }
		set { inUse = value; }
	}

	private Vector3 homeCoords;
	public Vector3 HomeCoords
	{
		get { return homeCoords; }
		set { homeCoords = value; }
	}

	private Vector3 homeRot;
	public Vector3 HomeRot
	{
		get { return homeRot; }
		set { homeRot = value; }
	}
	
	// Use this for initialization
	public virtual void Start () {
		Debug.Log ("E " + gameObject);
		Debug.Log ("E " + GetComponent<Collider>());
		HomeCoords = transform.position;
		HomeRot = transform.eulerAngles;
		controller = GameObject.Find ("MinSimController").GetComponent<PredicateParser>();
		renderer = gameObject.GetComponent<MeshRenderer> ();
		InUse = false;
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (controller.DynamicIntroduction) {
			if (!InUse) {
				if (renderer != null)
					renderer.enabled = false;
			}
			else {
				if (renderer != null)
					renderer.enabled = true;
			}
		}
		else {
			if (renderer != null)
				renderer.enabled = true;
		}
	}
	
	public void PromptBehavior(string pred, List<string> args) {
		Behavior behavior = GetBehaviorByName (pred);

		if (behavior != null) {
			CurrentBehaviorTypeProperty = currentBehaviorType = behavior.type;
			currentBehavior.SetNewBehavior (args);
		}
		else {
			Debug.Log("Behavior " + pred + " not found on entity " + gameObject.name);
		}

	}
	
	public void EndBehavior() {
		if (currentBehavior != null) {
			if (currentBehavior.Controller.CurrentBehaviors.Contains (previousBehavior)) {
				foreach (OldEntityClass e in previousBehavior.Arguments)
					Debug.Log ("Removing " + previousBehavior.formula + " (" + e + ") ");
				currentBehavior.Controller.CurrentBehaviors.Remove (previousBehavior);
			}

			if (currentBehavior.Controller.CurrentBehaviors.Contains (currentBehavior)) {
				foreach (OldEntityClass e in currentBehavior.Arguments)
					Debug.Log ("Removing " + currentBehavior.formula + " (" + e + ") ");
				currentBehavior.Controller.CurrentBehaviors.Remove (currentBehavior);
			}

			if (previousBehavior != null) {
				foreach (OldEntityClass arg in previousBehavior.Arguments) {
					bool argInUse = false;
					foreach (Behavior beh in currentBehavior.Controller.CurrentBehaviors) {
						if (beh.Arguments.Contains (arg)) {
							argInUse = true;
							break;
						}
					}
					arg.InUse = argInUse;
				}
			}

			foreach (OldEntityClass arg in currentBehavior.Arguments) {
				bool argInUse = false;
				foreach (Behavior beh in currentBehavior.Controller.CurrentBehaviors) {
					if (beh.Arguments.Contains (arg)) {
						argInUse = true;
						break;
					}
				}
				arg.InUse = argInUse;
			}
		}

		currentBehaviorType = BehaviorType.None;
		currentBehavior = null;
		InUse = false;
	}

	public void Reset() {
		EndBehavior ();
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.enabled = true;
		transform.position = HomeCoords;
		transform.eulerAngles = HomeRot;
	}
	
	Behavior GetBehaviorByType(BehaviorType btype) {
		Behavior behavior = null;
		Behavior[] behaviors = GetComponents<Behavior> ();

		foreach (Behavior beh in behaviors)
		{
			if (beh.type == btype)
			{
				behavior = beh;
				break;
			}
		}

		return behavior;
	}

	Behavior GetBehaviorByName(string bname) {
		Behavior behavior = null;
		Behavior[] behaviors = GetComponents<Behavior> ();

		foreach (Behavior beh in behaviors)
		{
			if (beh.formula == bname)
			{
				behavior = beh;
				break;
			}
		}
		
		return behavior;
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log (gameObject.name + " Collide " + collision.gameObject.name);

		if (currentBehavior != null)
		{
			if (currentBehavior.StopOnCollide)
			{
				EndBehavior();
			}
		}
	}
}
