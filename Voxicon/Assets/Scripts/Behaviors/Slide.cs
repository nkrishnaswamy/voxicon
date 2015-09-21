using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;

public class Slide : Behavior {
	
	public float speed = 1.5f;


	Collider[] colliders;
	
	// Use this for initialization
	public override void Start () {
		base.Start ();

		arity = 1;
		StopOnCollide = false;
		formula = "slide";
		type = BehaviorType.Slide;

		colliders = gameObject.GetComponents<Collider> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (entity.CurrentBehaviorTypeProperty != type)
			return;

		if (Pause)
			return;

		entity.CurrentBehavior = this;
		
		transform.position = new Vector3(transform.position.x - Time.deltaTime*speed,transform.position.y,transform.position.z);
	}

	public override void SetNewBehavior(List<string> args) {
		base.SetNewBehavior (args);
		DisableAllCollisions(colliders);
	}
}
