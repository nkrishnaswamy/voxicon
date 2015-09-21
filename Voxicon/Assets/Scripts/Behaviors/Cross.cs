using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using BehaviorTypes;
using SurfaceContactTypes;

public class Cross : Behavior {

	enum Nonant
	{
		TopLeft,
		TopCenter,
		TopRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		BottomLeft,
		BottomCenter,
		BottomRight
	};
	
	public float speed = 3.0f;
	float xRot = 0.0f;

	Collider[] colliders;
	SphereCollider sphereCollider;

	float surfaceBottomThirdBound, surfaceTopThirdBound,
			surfaceLeftThirdBound, surfaceRightThirdBound;

	Nonant nonant, destNonant;

	Vector3 destination;

	object[] lines;
	bool showLines = false;
	
	// Use this for initialization
	public override void Start () {
		base.Start ();

		lines = gameObject.GetComponents<LineRenderer>();
		((LineRenderer)lines [0]).enabled = false;

		arity = 2;
		StopOnCollide = false;
		formula = "cross";
		type = BehaviorType.Cross;

		colliders = gameObject.GetComponents<Collider> ();
		sphereCollider = gameObject.GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (entity.CurrentBehaviorTypeProperty != type) {
			((LineRenderer)lines [0]).enabled = false;
			return;
		}

		if (Pause)
			return;
		
		if (CheckNonant () == destNonant) {
			showLines = false;
			entity.EndBehavior ();
		}

		if (showLines)
			((LineRenderer)lines [0]).enabled = true;
		else
			((LineRenderer)lines [0]).enabled = false;
		
		Vector3 offset = transform.position - destination;
		offset = new Vector3 (offset.x, 0.0f, offset.z);
		Vector3 normalizedOffset = Vector3.Normalize(offset);

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

		// compute dimensions and destination
		surfaceBottomThirdBound = (float)(Arguments [1].GetComponent<Collider>().bounds.size.z / 3.0 + Arguments [1].GetComponent<Collider>().bounds.min.z);
		surfaceTopThirdBound = (float)(Arguments [1].GetComponent<Collider>().bounds.size.z / 3.0 * 2 + Arguments [1].GetComponent<Collider>().bounds.min.z);
		surfaceLeftThirdBound = (float)(Arguments [1].GetComponent<Collider>().bounds.size.x / 3.0 + Arguments [1].GetComponent<Collider>().bounds.min.x);
		surfaceRightThirdBound = (float)(Arguments [1].GetComponent<Collider>().bounds.size.x / 3.0 * 2 + Arguments [1].GetComponent<Collider>().bounds.min.x);

		((LineRenderer)lines[0]).SetPosition (0, new Vector3 (Arguments [1].GetComponent<Collider>().bounds.min.x, 0.0f, surfaceTopThirdBound));
		((LineRenderer)lines[0]).SetPosition (1, new Vector3 (Arguments [1].GetComponent<Collider>().bounds.max.x, 0.0f, surfaceTopThirdBound));
		((LineRenderer)lines[0]).SetPosition (2, new Vector3 (Arguments [1].GetComponent<Collider>().bounds.max.x, 0.0f, surfaceBottomThirdBound));
		((LineRenderer)lines[0]).SetPosition (3, new Vector3 (Arguments [1].GetComponent<Collider>().bounds.min.x, 0.0f, surfaceBottomThirdBound));
		((LineRenderer)lines[0]).SetPosition (4, new Vector3 (Arguments [1].GetComponent<Collider>().bounds.min.x, 0.0f, Arguments [1].GetComponent<Collider>().bounds.min.z));
		((LineRenderer)lines[0]).SetPosition (5, new Vector3 (surfaceLeftThirdBound, 0.0f, Arguments [1].GetComponent<Collider>().bounds.min.z));
		((LineRenderer)lines[0]).SetPosition (6, new Vector3 (surfaceLeftThirdBound, 0.0f, Arguments [1].GetComponent<Collider>().bounds.max.z));
		((LineRenderer)lines[0]).SetPosition (7, new Vector3 (surfaceRightThirdBound, 0.0f, Arguments [1].GetComponent<Collider>().bounds.max.z));
		((LineRenderer)lines[0]).SetPosition (8, new Vector3 (surfaceRightThirdBound, 0.0f, Arguments [1].GetComponent<Collider>().bounds.min.z));
		((LineRenderer)lines[0]).enabled = true;
		showLines = true;
		
		if (transform.position.z < surfaceBottomThirdBound) {
			if (transform.position.x < surfaceLeftThirdBound)
				destNonant = Nonant.TopRight;
			else if (transform.position.x > surfaceRightThirdBound)
				destNonant = Nonant.TopLeft;
			else
				destNonant = Nonant.TopCenter;
		} else if (transform.position.z > surfaceTopThirdBound) {
			if (transform.position.x < surfaceLeftThirdBound)
				destNonant = Nonant.BottomRight;
			else if (transform.position.x > surfaceRightThirdBound)
				destNonant = Nonant.BottomLeft;
			else
				destNonant = Nonant.BottomCenter;
		} else {
			if (transform.position.x < surfaceLeftThirdBound)
				destNonant = Nonant.MiddleRight;
			else if (transform.position.x > surfaceRightThirdBound)
				destNonant = Nonant.MiddleLeft;
			else
				destNonant = Nonant.MiddleCenter;
		}

		switch (destNonant) {
			case Nonant.TopRight:
				destination = new Vector3(surfaceRightThirdBound,0.0f,surfaceTopThirdBound);
				break;

			case Nonant.TopLeft:
				destination = new Vector3(surfaceLeftThirdBound,0.0f,surfaceTopThirdBound);
				break;

			case Nonant.TopCenter:
				destination = new Vector3(0.0f,0.0f,surfaceTopThirdBound);
				break;

			case Nonant.BottomRight:
				destination = new Vector3(surfaceRightThirdBound,0.0f,surfaceBottomThirdBound);
				break;

			case Nonant.BottomLeft:
				destination = new Vector3(surfaceLeftThirdBound,0.0f,surfaceBottomThirdBound);
				break;

			case Nonant.BottomCenter:
				destination = new Vector3(0.0f,0.0f,surfaceBottomThirdBound);
				break;

			case Nonant.MiddleRight:
				destination = new Vector3(surfaceRightThirdBound,0.0f,0.0f);
				break;

			case Nonant.MiddleLeft:
				destination = new Vector3(surfaceLeftThirdBound,0.0f,0.0f);
				break;

			case Nonant.MiddleCenter:
			default:
				break;
		}
	}

	Nonant CheckNonant(){
		if (transform.position.z < surfaceBottomThirdBound) {
			if (transform.position.x < surfaceLeftThirdBound)
				return Nonant.BottomLeft;
			else if (transform.position.x > surfaceRightThirdBound)
				return Nonant.BottomRight;
			else
				return Nonant.BottomCenter;
		} else if (transform.position.z > surfaceTopThirdBound) {
			if (transform.position.x < surfaceLeftThirdBound)
				return Nonant.TopLeft;
			else if (transform.position.x > surfaceRightThirdBound)
				return Nonant.TopRight;
			else
				return Nonant.TopCenter;
		} else {
			if (transform.position.x < surfaceLeftThirdBound)
				return Nonant.MiddleLeft;
			else if (transform.position.x > surfaceRightThirdBound)
				return Nonant.MiddleRight;
			else
				return Nonant.MiddleCenter;
		}
	}
}
