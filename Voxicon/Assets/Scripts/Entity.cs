using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MajorAxes;

public class Entity : MonoBehaviour {

	public Vector3 targetPosition;
	public Vector3 targetRotation;
	public Vector3 targetScale;
	public float moveSpeed = 1.0f;
	public float turnSpeed = 100.0f;
	public MajorAxis majorAxis;

	// Use this for initialization
	void Start () {
		targetPosition = transform.position;
		targetRotation = transform.eulerAngles;
		targetScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position != targetPosition) {
			Vector3 offset = transform.position - targetPosition;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			transform.position = new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
			                                  transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
			                                  transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			if (offset.sqrMagnitude <= 0.01f) {
				transform.position = targetPosition;
			}
		}

		if (transform.eulerAngles != targetRotation) {

			if (Vector3.Distance (transform.eulerAngles, targetRotation) > 0.1f) {
				transform.eulerAngles = Vector3.MoveTowards (transform.eulerAngles, targetRotation, Time.deltaTime * turnSpeed);
			} else {
				transform.eulerAngles = targetRotation;
			}
		}

		if (transform.localScale != targetScale) {
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
		List<RaycastHit> hitList = new List<RaycastHit>((RaycastHit[])hits);
		hits = hitList.OrderBy (h => h.distance).ToArray ();
		GameObject supportingSurface = null;
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.GetComponent<SupportingSurface> () != null) {
				if (hit.collider.gameObject.GetComponent<SupportingSurface> ().enabled) {
					supportingSurface = hit.collider.gameObject;
					break;
				}
			}
		}
		//Debug.Log (hitInfo.collider.gameObject.name);

		if (supportingSurface != null) {
			// add check for SupportingSurface component
			Renderer[] renderers = supportingSurface.GetComponentsInChildren<Renderer> ();
			Bounds surfaceBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > surfaceBounds.max.y) {
					surfaceBounds = renderer.bounds;
				}
			}

			renderers = gameObject.GetComponentsInChildren<Renderer> ();
			Bounds objectBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > objectBounds.max.y) {
					objectBounds = renderer.bounds;
				}
			}

			if (transform.position.y < transform.position.y + (surfaceBounds.max.y - objectBounds.min.y)) {
				transform.position = new Vector3 (transform.position.x,
			                                 transform.position.y + (surfaceBounds.max.y - objectBounds.min.y),
			                                 transform.position.z);
			}
		}
	}
}
