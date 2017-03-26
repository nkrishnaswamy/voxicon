using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;

public class Roll : Behavior {
	
	public float speed = 3.0f;
	
	Collider[] colliders;
	SphereCollider sphereCollider;

	// Use this for initialization
	public override void Start () {
		base.Start ();

		arity = 1;
		StopOnCollide = false;
		formula = "roll";
		type = BehaviorType.Roll;

		colliders = gameObject.GetComponents<Collider> ();
		sphereCollider = gameObject.GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (entity.CurrentBehaviorTypeProperty != type)
			return;

		if (Pause)
			return;

		entity.CurrentBehavior = this;

		transform.eulerAngles = new Vector3(0.0f,0.0f,(float)transform.eulerAngles.z+(speed*200.0f*Time.deltaTime));
		transform.position = new Vector3(transform.position.x - sphereCollider.radius*speed*Time.deltaTime,transform.position.y,transform.position.z);
	}

	public override void SetNewBehavior(List<string> args) {
		base.SetNewBehavior (args);
		DisableAllCollisions (colliders);
	}
}
