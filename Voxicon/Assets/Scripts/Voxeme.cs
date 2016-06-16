using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Global;
using MajorAxes;

public class Voxeme : MonoBehaviour {

	[HideInInspector]
	public VoxML voxml = new VoxML();

	Rigging rigging;

	public Queue<Vector3> interTargetPositions = new Queue<Vector3> ();
	public Vector3 targetPosition;
	public Vector3 targetRotation;
	public Vector3 targetScale;
	public float moveSpeed = 1.0f;
	public float turnSpeed = 2.5f;
	//public MajorAxis majorAxis;

	public float minYBound;

	public GameObject supportingSurface = null;

	public bool isGrasped = false;

	// Use this for initialization
	void Start () {
		// load in VoxML knowledge
		TextAsset markup = Resources.Load (gameObject.name) as TextAsset;
		if (markup != null) {
			voxml = VoxML.LoadFromText (markup.text);
		}

		// get movement blocking
		minYBound = Helper.GetObjectWorldSize(gameObject).min.y;

		// get rigging components
		rigging = gameObject.GetComponent<Rigging> ();

		targetPosition = transform.position;
		targetRotation = transform.eulerAngles;
		targetScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if (interTargetPositions.Count == 0) {	// no queued path
			if (!Helper.VectorIsNaN (targetPosition)) {	// has valid destination
				if (!isGrasped) {
					if (transform.position != targetPosition) {
						Vector3 offset = MoveToward (targetPosition);

						if (offset.sqrMagnitude <= 0.01f) {
							transform.position = targetPosition;
						}
					}
				}
				else {
					GameObject reachObj = GameObject.Find ("ReachObject");
					if (reachObj.transform.position != targetPosition) {
						Vector3 offset = MoveToward (targetPosition);

						if (offset.sqrMagnitude <= 0.01f) {
							reachObj.transform.position = targetPosition;
						}
					}
				}
			}
			else {	// cannot execute motion
				OutputHelper.PrintOutput("I'm sorry, I can't do that.");
				targetPosition = transform.position;
			}
		}
		else {
			Vector3 interimTarget = interTargetPositions.Peek ();
			if (!isGrasped) {
				if (transform.position != interimTarget) {
					Vector3 offset = MoveToward (interimTarget);

					if (offset.sqrMagnitude <= 0.001f) {
						transform.position = interimTarget;
						interTargetPositions.Dequeue ();
					}
				}
			}
			else {
				GameObject reachObj = GameObject.Find ("ReachObject");
				if (reachObj.transform.position != interimTarget) {
					Vector3 offset = MoveToward (interimTarget);

					if (offset.sqrMagnitude <= 0.01f) {
						reachObj.transform.position = interimTarget;
						interTargetPositions.Dequeue ();
					}
				}
			}
		}

		if (rigging != null) {
			if (rigging.usePhysicsRig) {
				return;
			}
		}

		if (!Helper.VectorIsNaN (targetRotation)) {	// has valid target
			if (!isGrasped) {
				if (transform.rotation != Quaternion.Euler (targetRotation)) {
					if ((Mathf.Deg2Rad * Quaternion.Angle (transform.rotation, Quaternion.Euler (targetRotation))) > 0.01f) {
						//transform.eulerAngles = Vector3.MoveTowards (transform.eulerAngles, targetRotation, Time.deltaTime * turnSpeed);
						//transform.eulerAngles = Vector3.Slerp (transform.eulerAngles, targetRotation, Time.deltaTime * turnSpeed);
						transform.rotation = Quaternion.Slerp (transform.rotation, 
							Quaternion.Euler (targetRotation), Time.deltaTime * turnSpeed);
					} else {
						//transform.eulerAngles = targetRotation;
						transform.rotation = Quaternion.Euler (targetRotation);
					}
				}
			}
		}
		else {	// cannot execute motion
			OutputHelper.PrintOutput("I'm sorry, I can't do that.");
			targetPosition = transform.position;
		}

		if ((transform.localScale != targetScale) && (!isGrasped)) {
			Vector3 offset = transform.localScale - targetScale;
			Vector3 normalizedOffset = Vector3.Normalize (offset);
	
			transform.localScale = new Vector3 (transform.localScale.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				transform.localScale.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				transform.localScale.z - normalizedOffset.z * Time.deltaTime * moveSpeed);
	
			if (offset.sqrMagnitude <= 0.01f) {
				transform.localScale = targetScale;
			}
		}

		RaycastHit[] hits;

		hits = Physics.RaycastAll (transform.position, AxisVector.negYAxis);
		List<RaycastHit> hitList = new List<RaycastHit> ((RaycastHit[])hits);
		hits = hitList.OrderBy (h => h.distance).ToArray ();
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.GetComponent<BoxCollider> () != null) {
				if (hit.collider.gameObject.GetComponent<BoxCollider> ().enabled) {
					supportingSurface = hit.collider.gameObject;
					break;
				}
			}
		}

		if (supportingSurface != null) {
			//Debug.Log (supportingSurface.name);
			// add check for SupportingSurface component
			Renderer[] renderers = supportingSurface.GetComponentsInChildren<Renderer> ();
			Bounds surfaceBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > surfaceBounds.max.y) {
					surfaceBounds = renderer.bounds;
				}
			}

			Vector3 currentMin = gameObject.transform.position;
			renderers = gameObject.GetComponentsInChildren<Renderer> ();
			Bounds objectBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > objectBounds.max.y) {
					objectBounds = renderer.bounds;
				}

				if (renderer.bounds.min.y < currentMin.y) {
					currentMin = renderer.bounds.min;
				}
			}

			if (transform.position.y < transform.position.y + (minYBound - objectBounds.min.y)) {
				transform.position = new Vector3 (transform.position.x,
					transform.position.y + (minYBound - objectBounds.min.y),
					transform.position.z);
			}

			/*if (supportingSurface.GetComponent<SupportingSurface> ().surfaceType == SupportingSurface.SupportingSurfaceType.Concave) {
				/*if (objectBounds.min.y < surfaceBounds.min.y) {
				transform.position = new Vector3 (transform.position.x,
				                                  transform.position.y + (surfaceBounds.min.y - objectBounds.min.y),
				                                  transform.position.z);
			}*/
				/*if (currentMin.y < surfaceBounds.min.y) {
					transform.position = new Vector3 (transform.position.x,
						transform.position.y + (surfaceBounds.min.y - currentMin.y),
						transform.position.z);
				}
			} else {
				/*if (objectBounds.min.y < surfaceBounds.max.y) {
				transform.position = new Vector3 (transform.position.x,
			                         transform.position.y + (surfaceBounds.max.y - objectBounds.min.y),
			                         transform.position.z);
			}*/
				/*if (currentMin.y < surfaceBounds.max.y) {
					transform.position = new Vector3 (transform.position.x,
						transform.position.y + (surfaceBounds.max.y - currentMin.y),
						transform.position.z);
				}
			}*/
		}

		// check relationships

	}

	Vector3 MoveToward(Vector3 target) {
		if (!isGrasped) {
			Vector3 offset = transform.position - target;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			if (rigging.usePhysicsRig) {
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
						transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
						transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
				}
			}

			transform.position = new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			//GameObject.Find ("ReachObject").transform.position = transform.position;

			return offset;
		}
		else {
			GameObject reachObj = GameObject.Find ("ReachObject");
			GameObject grasperCoord = GameObject.Find ("GrasperCoord");


			Vector3 offset = reachObj.transform.position - target;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			/*if (rigging.usePhysicsRig) {
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
						transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
						transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
				}
			}*/

			reachObj.transform.position = new Vector3 (reachObj.transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				reachObj.transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				reachObj.transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			return offset;
		}

		
	}

	void OnCollisionEnter(Collision other) {
		if (other.gameObject.tag == "MainCamera") {
			return;
		}
	}
}
