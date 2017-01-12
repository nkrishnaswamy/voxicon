using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;
using SurfaceContactTypes;

public class Reach : Behavior {

	public float speed = 3.0f;
	float xRot = 0.0f;

	Collider[] colliders;
	SphereCollider sphereCollider;

	Vector3 destination;
	
	// Use this for initialization
	public override void Start () {
		base.Start ();

		arity = 2;
		StopOnCollide = true;
		formula = "reach";
		type = BehaviorType.Reach;

		colliders = gameObject.GetComponents<Collider> ();
		sphereCollider = gameObject.GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (entity.CurrentBehaviorTypeProperty != type)
			return;

		if (Pause)
			return;

		Vector3 offset = transform.position - destination;
		offset = new Vector3 (offset.x, 0.0f, offset.z);
		Vector3 normalizedOffset = Vector3.Normalize(offset);

		if (offset.magnitude < 0.1f)
			entity.EndBehavior();

		SurfaceContactType contactType = entity.ContactType;

		if ((contactType == SurfaceContactType.Point) || (contactType == SurfaceContactType.Edge)) {
			xRot -= (speed * Time.deltaTime*200.0f);
			transform.rotation = Quaternion.Slerp(transform.rotation,
			    Quaternion.LookRotation(normalizedOffset)*Quaternion.Euler(new Vector3(xRot,0.0f,0.0f)),
				Time.deltaTime * speed*200.0f);
		}

		if (sphereCollider != null)
			transform.position = new Vector3 (transform.position.x - normalizedOffset.x * sphereCollider.radius * Time.deltaTime * speed, transform.position.y,
				transform.position.z - normalizedOffset.z * sphereCollider.radius * Time.deltaTime * speed);
		else
			transform.position = new Vector3 (transform.position.x - normalizedOffset.x * 0.5f * Time.deltaTime * speed, transform.position.y,
			    transform.position.z - normalizedOffset.z * 0.5f * Time.deltaTime * speed);
	}

	public override void SetNewBehavior(List<string> args) {
		base.SetNewBehavior(args);

		object[] entities = Resources.FindObjectsOfTypeAll (typeof(OldEntityClass));

		foreach (object entity in entities) {
			if (((OldEntityClass)entity).CurrentBehaviorTypeProperty != this.type && !((OldEntityClass)entity).InUse)
				DisableAllCollisions(new[]{((OldEntityClass)entity).gameObject.GetComponent<Collider>()});
		}

		List<Collider> colls = new List<Collider>();

		foreach (string arg in args) {
			Collider c = GameObject.Find(arg).GetComponent<Collider>();
			if (c != null)
				colls.Add(c);
		}

		EnableCollisions (colls.ToArray());

		// compute destination
		GameObject target = GameObject.Find (args [1]);
		destination = new Vector3 (target.transform.position.x, 0.0f, target.transform.position.z);
	}
}
